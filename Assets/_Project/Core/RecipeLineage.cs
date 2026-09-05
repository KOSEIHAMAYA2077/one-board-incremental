using System;
using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public readonly struct RecipeReward
    {
        public RecipeReward(int target, int projectile, int gold) { Target = target; Projectile = projectile; Gold = gold; }
        public int Target { get; }
        public int Projectile { get; }
        public int Gold { get; }
    }
    public sealed class RecipeLineage
    {
        public readonly List<RoutingShot> Projectiles = new List<RoutingShot>();
        public readonly HashSet<int> RewardVisited = new HashSet<int>();
        public readonly HashSet<int> EffectVisited = new HashSet<int>();
        public readonly List<RecipeReward> Rewards = new List<RecipeReward>();
        public int YieldGold { get; private set; }
        public int LimitedCount { get; private set; }
        public bool Complete { get; private set; }
        public int ActiveCount { get { var count = 0; foreach (var p in Projectiles) if (p.Alive) count++; return count; } }
        public RecipeLineage(RecipeBullet bullet, int primer, SimVector2 position, SimVector2 velocity)
        {
            if (primer < 0 || primer > 2) throw new ArgumentOutOfRangeException(nameof(primer));
            Projectiles.Add(new RoutingShot(position, velocity) { Lineage = this, Bullet = bullet, Primer = primer, PiercesLeft = 2 + primer });
            Projectiles[0].Trace.Add(position);
        }
        public void End()
        { foreach (var shot in Projectiles) shot.Alive = false; Complete = true; }
        public void Tick(double seconds, IRoutingQuery query)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Complete) return;
            var existing = Projectiles.Count;
            // Appended children run this tick with only their parent's unspent distance.
            for (var i = 0; i < Projectiles.Count; i++) Step(Projectiles[i], i < existing ? seconds : 0, query);
            Complete = ActiveCount == 0;
        }
        private void Step(RoutingShot shot, double seconds, IRoutingQuery query)
        {
            if (!shot.Alive) return;
            shot.Age += seconds;
            var distance = shot.Velocity.Magnitude * seconds + shot.PendingDistance;
            shot.PendingDistance = 0;
            for (var contact = 0; shot.Alive && distance > 0.00001 && contact < 8; contact++)
            {
                var hit = query.Cast(shot, distance);
                if (!hit.Hit) { shot.Position += shot.Velocity.Normalized * distance; distance = 0; break; }
                shot.Position += shot.Velocity.Normalized * hit.Distance;
                distance = Math.Max(0, distance - hit.Distance);
                shot.Trace.Add(shot.Position); shot.Contacts.Add(hit.TargetId);
                if (hit.Kind == BoardPieceKind.Mirror)
                {
                    if (shot.ReflectionsLeft <= 0) { shot.Alive = false; break; }
                    shot.ReflectionsLeft--; shot.Mirrors.Add(hit.TargetId);
                    shot.Velocity = SimVector2.Reflect(shot.Velocity, hit.Normal);
                    shot.Position = hit.Point + hit.Normal * (RoutingShot.Radius + 0.3);
                    distance = Math.Max(0, distance - 0.3); continue;
                }
                if (hit.Kind == BoardPieceKind.Collector && RewardVisited.Add(hit.TargetId))
                {
                    var reflection = shot.Mirrors.Count == 0 ? 1 : shot.Mirrors.Count == 1 ? 1.5 : 2;
                    var reward = (int)Math.Floor(2 * reflection * shot.RewardToken * (shot.Bullet == RecipeBullet.Pierce ? 1.25 : 1));
                    shot.Gold += reward; YieldGold += reward; shot.HitTarget = hit.TargetId; shot.RewardToken = 1;
                    Rewards.Add(new RecipeReward(hit.TargetId, shot.SpawnSequence, reward));
                }
                if (hit.Kind == BoardPieceKind.Amplifier && EffectVisited.Add(hit.TargetId))
                { shot.RewardToken = Math.Min(4, shot.RewardToken * 2); shot.Amplifiers.Add(hit.TargetId); }

                // A guard only lets this projectile leave the collider it just entered.
                // Reward/effect visits never remove a collider's later physical behavior.
                shot.ExitGuards.Add(hit.TargetId);
                if (shot.Bullet == RecipeBullet.Split) Split(shot, distance);
                else if (hit.Kind == BoardPieceKind.Collector)
                {
                    if (shot.Bullet == RecipeBullet.Pierce && shot.PiercesLeft > 0) shot.PiercesLeft--;
                    else shot.Alive = false;
                }
            }
            if (shot.Alive) shot.PendingDistance = distance;
            if (shot.Position.X < -100 || shot.Position.X > 1700 || shot.Position.Y < -100 || shot.Position.Y > 1000) shot.Alive = false;
        }
        private void Split(RoutingShot parent, double distance)
        {
            parent.Alive = false;
            var requested = 2 + parent.Primer;
            var permitted = parent.Depth >= 3 ? 0 : Math.Min(requested, 64 - Projectiles.Count);
            LimitedCount += requested - permitted;
            for (var i = 0; i < permitted; i++)
            {
                var child = new RoutingShot(parent.Position, AimCalculator.RotateDegrees(parent.Velocity, -15 + 30.0 * i / (requested - 1)) * parent.Velocity.Magnitude)
                {
                    Lineage = this, Bullet = RecipeBullet.Split, Primer = parent.Primer, Depth = parent.Depth + 1,
                    ReflectionsLeft = parent.ReflectionsLeft, RewardToken = parent.RewardToken,
                    SpawnSequence = Projectiles.Count, PendingDistance = distance
                };
                child.ExitGuards.UnionWith(parent.ExitGuards);
                child.Mirrors.UnionWith(parent.Mirrors); child.Amplifiers.UnionWith(parent.Amplifiers);
                child.Trace.Add(child.Position); Projectiles.Add(child);
            }
        }
    }
}
