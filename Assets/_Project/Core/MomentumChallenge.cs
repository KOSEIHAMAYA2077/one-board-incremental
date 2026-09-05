using System;
using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public sealed partial class MomentumSimulation
    {
        public MomentumProgress Progress { get; }
        public int Stage { get; private set; }
        public int ChallengeMagazines { get; private set; }
        public int ChallengeLimit { get; private set; }
        public MomentumChallengeState ChallengeState { get; private set; }
        public int ChallengeGold { get; private set; }
        public int RemainingTargets => Targets.FindAll(t => t.Alive).Count;
        public bool CanConfigure => Progress != null && !Bursting && Balls.Count == 0;
        private int _volleyGun;
        private MomentumMod _volleyMods;
        private double _volleyPower;
        private double _volleyExpires;
        private int _volleySpawned;
        public int SuppressedSplits { get; private set; }
        public double FlightRemaining => Math.Max(0, _volleyExpires - Time);
        private readonly List<MomentumBall> _children = new List<MomentumBall>();
        public bool StartChallenge(int stage)
        {
            if (!CanConfigure || stage < 0 || stage > Progress.highestStage) return false;
            Stage = stage; ChallengeMagazines = 0; ChallengeGold = 0;
            ChallengeLimit = Progress.MagazineLimit; ChallengeState = MomentumChallengeState.Active;
            _readyAt = Time;
            foreach (var t in Targets)
            {
                t.Hp = 0; t.GoldenMarked = false; t.RadiusScale = .75;
                t.Armored = t.Id > (stage == 1 ? 6 : stage == 2 ? 8 : 10);
                t.HpOverride = (t.Armored ? 60 : 30) + stage * (t.Armored ? 25 : 20);
                t.RewardBonus = stage * 2;
            }
            RefillTargets();
            // A complete encounter is required; never grant a cheap mastery for failed placement.
            if (Targets.Exists(t => !t.Alive)) throw new InvalidOperationException("Challenge placement incomplete");
            return true;
        }
        private void PrepareVolley()
        {
            _volleyGun = Progress.gun; _volleyMods = (MomentumMod)Progress.equippedMods;
            _volleyPower = 1 + Progress.powerLevel * .1;
            _volleyExpires = Time + 10; _volleySpawned = 0;
            _firing = new MomentumAmmo[_volleyGun == 0 ? 6 : 18];
            ChallengeMagazines++;
        }
        private void ConfigureBall(MomentumBall ball, int number)
        {
            ball.Mods = _volleyMods;
            ball.ExpiresAt = _volleyExpires; _volleySpawned++;
            ball.Radius = _volleyGun == 0 ? 9 : 5;
            ball.DamageScale = (_volleyGun == 0 ? 48.0 : 17.0) / 40 * _volleyPower;
            if ((ball.Mods & MomentumMod.Power) != 0) ball.DamageScale *= 1.25;
            if ((ball.Mods & MomentumMod.Overcharge) != 0) ball.DamageScale *= 1.2;
            ball.ResistanceScale = (ball.Mods & MomentumMod.Pierce) != 0 ? .25 : 1;
            ball.Golden = (ball.Mods & MomentumMod.Golden) != 0 && number % 4 == 0;
            var speed = (_volleyGun == 0 ? 780 : 1050) * ((ball.Mods & MomentumMod.Speed) != 0 ? 1.2 : 1);
            ball.Velocity = ball.Velocity.Normalized * speed;
        }
        public bool Recall()
        {
            if (Progress == null || Editing || !Bursting && Balls.Count == 0) return false;
            if (Bursting) { _firing = null; _readyAt = Time + .8; }
            foreach (var ball in Balls) ball.Alive = false;
            Balls.Clear(); _children.Clear();
            ResolveChallenge(); return true;
        }
        private void ResolveChallenge()
        {
            if (Progress == null || ChallengeState != MomentumChallengeState.Active || Bursting || Balls.Count != 0 || _children.Count != 0) return;
            if (RemainingTargets == 0 && ChallengeMagazines > 0)
            {
                ChallengeState = MomentumChallengeState.Cleared;
                Progress.Complete(Stage, ChallengeMagazines);
            }
            else if (ChallengeMagazines >= ChallengeLimit) ChallengeState = MomentumChallengeState.Failed;
        }
        private void ApplyDamage(MomentumTarget target, MomentumBall ball, double damage)
        {
            if (!target.Alive) return;
            if (ball.Golden) target.GoldenMarked = true;
            var applied = Math.Min(target.Hp, damage);
            target.Hp = Math.Max(0, target.Hp - damage);
            Stats.Record(Time, applied, damage, ball.Ammo);
            Events.Add(new MomentumEvent("hit", target.Position, damage, target.Id));
            if (target.Alive) return;
            var reward = target.Reward * (target.GoldenMarked ? 2 : 1);
            if (Progress == null) _gold += reward; else { Progress.gold += reward; ChallengeGold += reward; }
            DestroyedCount++;
            Events.Add(new MomentumEvent("destroy", target.Position, reward, target.Id));
        }
        private void TriggerEffects(MomentumBall ball, MomentumTarget target, double damage)
        {
            if ((ball.Mods & MomentumMod.Blast) != 0 && !ball.HasBlasted && ball.HitTargets.Count >= 3)
            {
                ball.HasBlasted = true;
                foreach (var other in Targets) if (other.Id != target.Id && (other.Position - target.Position).Magnitude <= 100 + other.Radius)
                    ApplyDamage(other, ball, damage * .75);
                Events.Add(new MomentumEvent("blast", target.Position, damage * .75));
            }
            if (!ball.Alive || (ball.Mods & MomentumMod.Split) == 0 || ball.HasSplit) return;
            // No generation limit. Safety caps preserve the parent and its future damage.
            if (_volleySpawned + 2 > 1024 || Balls.Count + _children.Count + RemainingInBurst + 1 > 256)
            { SuppressedSplits++; return; }
            ball.HasSplit = true;
            for (var i = 0; i < 2; i++)
            {
                var child = new MomentumBall { Id = ++_nextBallId, MagazineId = ball.MagazineId, Ammo = ball.Ammo,
                    Position = ball.Position, Velocity = AimCalculator.RotateDegrees(ball.Velocity, i == 0 ? -18 : 18),
                    Radius = Math.Max(2, ball.Radius * .85), DamageScale = ball.DamageScale * .55, ResistanceScale = ball.ResistanceScale,
                    Mods = ball.Mods, Golden = ball.Golden, Generation = ball.Generation + 1, Boosted = ball.Boosted, ExpiresAt = ball.ExpiresAt };
                foreach (var id in ball.Exiting) child.Exiting.Add(id);
                _children.Add(child);
                _volleySpawned++;
            }
            ball.Alive = false;
            Events.Add(new MomentumEvent("split", ball.Position, 2));
        }
    }
}
