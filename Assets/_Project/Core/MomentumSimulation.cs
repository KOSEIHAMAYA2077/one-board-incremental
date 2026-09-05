using System;
using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public enum MomentumAmmo { Normal, Pierce }

    public sealed class MomentumTarget
    {
        public int Id; public bool Armored; public SimVector2 Position;
        public double Hp;
        public double MaximumHp => Armored ? 90 : 50;
        public double Radius => Armored ? 42 : 34;
        public double Resistance => Armored ? 420 : 150;
        public int Reward => Armored ? 7 : 3;
        public bool Alive => Hp > 0;
    }
    public sealed class MomentumObstacle { public int Id; public SimVector2 Position; public const double Radius = 26; }
    public sealed class MomentumBall
    {
        public int Id, MagazineId;
        public MomentumAmmo Ammo;
        public SimVector2 Position, Velocity;
        public bool Alive = true, Boosted;
        public double PendingTime;
        public readonly HashSet<int> Exiting = new HashSet<int>();
        public double Speed => Velocity.Magnitude;
    }
    public readonly struct MomentumEvent
    {
        public MomentumEvent(string kind, SimVector2 position, double amount, int target = 0)
        { Kind = kind; Position = position; Amount = amount; Target = target; }
        public string Kind { get; }
        public SimVector2 Position { get; }
        public double Amount { get; }
        public int Target { get; }
    }
    public static class MomentumRules
    {
        public const double LaunchSpeed = 900, MaximumSpeed = 1800, Drag = 120, StopSpeed = 80, Radius = 8;
        public const double Left = 320, Right = 1560, Top = 120, Bottom = 760, ZoneRadius = 65;
        public static readonly SimVector2 Gun = new SimVector2(940, 725);
        public static double BaseDamage(MomentumAmmo ammo) => ammo == MomentumAmmo.Normal ? 40 : 26;
        public static double ResistanceFactor(MomentumAmmo ammo) => ammo == MomentumAmmo.Normal ? 1 : .25;
        public static double Damage(MomentumAmmo ammo, double impactSpeed) => BaseDamage(ammo) * Math.Min(Math.Max(impactSpeed, 0) / LaunchSpeed, 2);
        public static double ExitSpeed(MomentumAmmo ammo, double impactSpeed, double resistance) => Math.Max(0, impactSpeed - resistance * ResistanceFactor(ammo));
        public static SimVector2 ZoneAt(double time) => new SimVector2(940 + 320 * Math.Sin(.8 * time), 550);
        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        // Earliest time of impact of two circles under constant relative velocity.
        public static bool SweepCircle(SimVector2 relativePosition, SimVector2 relativeVelocity, double radius, double duration, out double time)
        {
            time = 0;
            var c = relativePosition.SqrMagnitude - radius * radius;
            if (c <= 0) return true;
            var a = relativeVelocity.SqrMagnitude;
            if (a < 1e-12) return false;
            var b = SimVector2.Dot(relativePosition, relativeVelocity);
            if (b >= 0) return false;
            var discriminant = b * b - a * c;
            if (discriminant < 0) return false;
            time = (-b - Math.Sqrt(discriminant)) / a;
            return time >= 0 && time <= duration;
        }
    }

    public sealed class MomentumSimulation
    {
        public const uint DefaultSeed = 20260905;
        public readonly List<MomentumTarget> Targets = new List<MomentumTarget>();
        public readonly List<MomentumObstacle> Obstacles = new List<MomentumObstacle>();
        public readonly List<MomentumBall> Balls = new List<MomentumBall>();
        public readonly List<MomentumEvent> Events = new List<MomentumEvent>();
        public MomentumBoardLayout Layout { get; }
        public MomentumCombatStats Stats { get; } = new MomentumCombatStats();
        private readonly DeterministicRandom _layoutRandom, _shotRandom;
        private MomentumAmmo[] _magazine = { MomentumAmmo.Normal, MomentumAmmo.Pierce, MomentumAmmo.Normal };
        private MomentumAmmo[] _firing;
        private SimVector2 _aim;
        private int _nextBallId, _nextSlot;
        private double _nextShotTime, _readyAt;
        public uint Seed { get; }
        public double Time { get; private set; }
        public int Gold { get; private set; }
        public int MagazineCount { get; private set; }
        public int FiredCount { get; private set; }
        public int DestroyedCount { get; private set; }
        public int BoostCount { get; private set; }
        public bool Editing { get; private set; }
        public IReadOnlyList<MomentumAmmo> Magazine => Array.AsReadOnly(_magazine);
        public bool Bursting => _firing != null;
        public int RemainingInBurst => _firing == null ? 0 : 3 - _nextSlot;
        public bool Ready => !Editing && !Bursting && Time >= _readyAt;
        public double ReloadRemaining => Math.Max(0, _readyAt - Time);
        public SimVector2 ZonePosition => Layout.ZoneAt(Time);

        public MomentumSimulation(uint seed = DefaultSeed, MomentumBoardLayout layout = null)
        {
            Layout = layout ?? MomentumBoardLayout.Landscape;
            Seed = seed; _layoutRandom = new DeterministicRandom(seed); _shotRandom = new DeterministicRandom(seed ^ 0x93A5612Bu);
            for (var i = 0; i < 2; i++)
                Obstacles.Add(new MomentumObstacle { Id = 101 + i, Position = Layout.ObstaclePosition(i, _layoutRandom.NextUnitDouble()) });
            for (var i = 0; i < 7; i++) Targets.Add(new MomentumTarget { Id = i + 1, Armored = i >= 4 });
            RefillTargets();
        }
        public void SetEditing(bool editing) { Editing = editing; }
        public bool SetMagazine(IReadOnlyList<MomentumAmmo> slots)
        {
            if (!Editing || slots == null || slots.Count != 3) return false;
            foreach (var ammo in slots) if (ammo != MomentumAmmo.Normal && ammo != MomentumAmmo.Pierce) return false;
            _magazine = new[] { slots[0], slots[1], slots[2] }; return true;
        }
        public bool TryFire(SimVector2 aim)
        {
            if (!Ready || !MomentumRules.Finite(aim.X) || !MomentumRules.Finite(aim.Y) || (aim - Layout.Gun).Magnitude < 30) return false;
            RefillTargets();
            _aim = (aim - Layout.Gun).Normalized;
            _firing = (MomentumAmmo[])_magazine.Clone(); _nextSlot = 0; _nextShotTime = Time; MagazineCount++;
            FireNext(); return true;
        }
        private void FireNext()
        {
            Balls.Add(new MomentumBall { Id = ++_nextBallId, MagazineId = MagazineCount, Ammo = _firing[_nextSlot++],
                Position = Layout.Gun, Velocity = AimCalculator.RotateDegrees(_aim, _shotRandom.NextSignedOffset(2)) * MomentumRules.LaunchSpeed });
            FiredCount++;
            if (_nextSlot == 3) { _firing = null; _readyAt = Time + .8; }
            else _nextShotTime += .12; // Preserve cadence instead of accumulating fixed-tick rounding.
        }
        public void Tick(double seconds)
        {
            if (!MomentumRules.Finite(seconds) || seconds <= 0 || seconds > .05) throw new ArgumentOutOfRangeException(nameof(seconds), "Use fixed ticks up to 0.05 seconds.");
            Events.Clear();
            if (Editing) return;
            Time += seconds;
            Stats.Advance(Time);
            if (Bursting && Time + 1e-9 >= _nextShotTime) FireNext();
            foreach (var ball in Balls) if (ball.Alive) Step(ball, seconds);
            Balls.RemoveAll(b => !b.Alive);
        }
        public bool PositionAvailable(SimVector2 position, double radius, int ignoredTarget = 0)
        {
            if (!MomentumRules.Finite(position.X) || !MomentumRules.Finite(position.Y) ||
                position.X - radius < Layout.Left + 20 || position.X + radius > Layout.Right - 20 ||
                position.Y - radius < Layout.Top + 20 || position.Y + radius > Layout.TargetBottom) return false;
            foreach (var target in Targets)
                if (target.Id != ignoredTarget && target.Alive && (position - target.Position).Magnitude < radius + target.Radius + 22) return false;
            foreach (var obstacle in Obstacles)
                if ((position - obstacle.Position).Magnitude < radius + MomentumObstacle.Radius + 22) return false;
            foreach (var ball in Balls)
                if (ball.Alive && (position - ball.Position).Magnitude < radius + MomentumRules.Radius + 80) return false;
            return true;
        }
        private void RefillTargets()
        {
            foreach (var target in Targets)
            {
                if (target.Alive) continue;
                for (var attempt = 0; attempt < 200; attempt++)
                {
                    var p = Layout.TargetCandidate(_layoutRandom.NextUnitDouble(), _layoutRandom.NextUnitDouble());
                    if (!PositionAvailable(p, target.Radius, target.Id)) continue;
                    target.Position = p; target.Hp = target.MaximumHp; break;
                }
            }
        }
        private struct Contact
        {
            public bool Hit; public double Time; public int Priority, Id;
            public SimVector2 Normal; public MomentumTarget Target;
        }
        private static void Consider(ref Contact best, double time, int priority, int id, SimVector2 normal, MomentumTarget target = null)
        {
            if (!best.Hit || time < best.Time - 1e-7 || Math.Abs(time - best.Time) <= 1e-7 && (priority < best.Priority || priority == best.Priority && id < best.Id))
                best = new Contact { Hit = true, Time = time, Priority = priority, Id = id, Normal = normal, Target = target };
        }
        private Contact Query(MomentumBall ball, double remaining, double elapsedTime, double tickStart, double tickDuration)
        {
            var best = new Contact();
            foreach (var target in Targets)
            {
                var delta = ball.Position - target.Position; var radius = target.Radius + MomentumRules.Radius;
                if (!target.Alive) { ball.Exiting.Remove(target.Id); continue; }
                if (ball.Exiting.Contains(target.Id))
                { if (delta.Magnitude > radius + .1) ball.Exiting.Remove(target.Id); else continue; }
                if (MomentumRules.SweepCircle(delta, ball.Velocity, radius, remaining, out var t))
                    Consider(ref best, t, 0, target.Id, (delta + ball.Velocity * t).Normalized, target);
            }
            foreach (var obstacle in Obstacles)
            {
                var delta = ball.Position - obstacle.Position;
                if (MomentumRules.SweepCircle(delta, ball.Velocity, MomentumObstacle.Radius + MomentumRules.Radius, remaining, out var t))
                    Consider(ref best, t, 1, obstacle.Id, (delta + ball.Velocity * t).Normalized);
            }
            var v = ball.Velocity; var p = ball.Position;
            Wall(ref best, v.X < 0 ? (Layout.Left + MomentumRules.Radius - p.X) / v.X : double.PositiveInfinity, remaining, 1, new SimVector2(1, 0));
            Wall(ref best, v.X > 0 ? (Layout.Right - MomentumRules.Radius - p.X) / v.X : double.PositiveInfinity, remaining, 2, new SimVector2(-1, 0));
            Wall(ref best, v.Y < 0 ? (Layout.Top + MomentumRules.Radius - p.Y) / v.Y : double.PositiveInfinity, remaining, 3, new SimVector2(0, 1));
            Wall(ref best, v.Y > 0 ? (Layout.Bottom - MomentumRules.Radius - p.Y) / v.Y : double.PositiveInfinity, remaining, 4, new SimVector2(0, -1));
            if (!ball.Boosted)
            {
                var start = Layout.ZoneAt(tickStart);
                var zoneVelocity = (Layout.ZoneAt(tickStart + tickDuration) - start) / tickDuration;
                var relative = p - (start + zoneVelocity * elapsedTime);
                if (MomentumRules.SweepCircle(relative, v - zoneVelocity, MomentumRules.ZoneRadius + MomentumRules.Radius, remaining, out var t))
                    Consider(ref best, t, 3, 1, default);
            }
            return best;
        }
        private static void Wall(ref Contact best, double time, double remaining, int id, SimVector2 normal)
        { if (time >= -1e-9 && time <= remaining) Consider(ref best, Math.Max(time, 0), 2, id, normal); }
        private void Step(MomentumBall ball, double seconds)
        {
            SetSpeed(ball, ball.Speed - MomentumRules.Drag * seconds);
            if (!ball.Alive) return;
            var remaining = seconds + ball.PendingTime; ball.PendingTime = 0;
            var duration = remaining; var start = Time - duration; var elapsed = 0.0;
            for (var n = 0; n < 8 && ball.Alive && remaining > 1e-8; n++)
            {
                var hit = Query(ball, remaining, elapsed, start, duration);
                if (!hit.Hit) { ball.Position += ball.Velocity * remaining; remaining = 0; break; }
                ball.Position += ball.Velocity * hit.Time; remaining -= hit.Time; elapsed += hit.Time;
                if (hit.Priority == 0)
                {
                    var damage = MomentumRules.Damage(ball.Ammo, ball.Speed);
                    var appliedDamage = Math.Min(hit.Target.Hp, damage);
                    hit.Target.Hp = Math.Max(0, hit.Target.Hp - damage);
                    Stats.Record(Time, appliedDamage, damage, ball.Ammo);
                    Events.Add(new MomentumEvent("hit", hit.Target.Position, damage, hit.Target.Id));
                    if (!hit.Target.Alive)
                    {
                        Gold += hit.Target.Reward; DestroyedCount++;
                        Events.Add(new MomentumEvent("destroy", hit.Target.Position, hit.Target.Reward, hit.Target.Id));
                    }
                    ball.Exiting.Add(hit.Target.Id);
                    SetSpeed(ball, MomentumRules.ExitSpeed(ball.Ammo, ball.Speed, hit.Target.Resistance));
                }
                else if (hit.Priority == 3)
                {
                    ball.Boosted = true; BoostCount++;
                    SetSpeed(ball, Math.Min(MomentumRules.MaximumSpeed, ball.Speed * 2));
                    Events.Add(new MomentumEvent("boost", ball.Position, ball.Speed));
                }
                else
                {
                    ball.Velocity = SimVector2.Reflect(ball.Velocity, hit.Normal);
                    SetSpeed(ball, ball.Speed * .9); ball.Position += hit.Normal * .2;
                }
            }
            if (ball.Alive) ball.PendingTime = remaining;
        }
        private static void SetSpeed(MomentumBall ball, double speed)
        {
            ball.Velocity = ball.Velocity.Normalized * Math.Max(0, speed);
            if (speed <= MomentumRules.StopSpeed) ball.Alive = false;
        }
    }
}
