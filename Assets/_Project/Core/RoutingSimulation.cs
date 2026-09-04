using System;
using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public readonly struct RoutingContact
    {
        public RoutingContact(int targetId, BoardPieceKind kind, double distance, SimVector2 point, SimVector2 normal)
        { TargetId = targetId; Kind = kind; Distance = distance; Point = point; Normal = normal; }
        public int TargetId { get; }
        public BoardPieceKind Kind { get; }
        public double Distance { get; }
        public SimVector2 Point { get; }
        public SimVector2 Normal { get; }
        public bool Hit => TargetId != 0;
    }

    public interface IRoutingQuery
    {
        RoutingContact Cast(RoutingShot shot, double distance);
    }

    public sealed class RoutingShot
    {
        public const double Radius = 8;
        public RoutingShot(SimVector2 position, SimVector2 velocity) { Position = position; Velocity = velocity; }
        public SimVector2 Position;
        public SimVector2 Velocity;
        public bool Alive = true;
        public int ReflectionsLeft = 6;
        public readonly HashSet<int> Mirrors = new HashSet<int>();
        public readonly HashSet<int> Amplifiers = new HashSet<int>();
        public double RewardToken = 1;
        public double PendingDistance;
        public int Gold;
        public double Age;
        public int HitTarget;
        public readonly List<SimVector2> Trace = new List<SimVector2>();
        public readonly List<int> Contacts = new List<int>();
    }

    public static class RoutingSimulation
    {
        public static void Step(RoutingShot shot, double seconds, IRoutingQuery query)
        {
            if (!shot.Alive) return;
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            shot.Age += seconds;
            var distance = shot.Velocity.Magnitude * seconds + shot.PendingDistance;
            shot.PendingDistance = 0;
            var contacts = 0;
            while (shot.Alive && distance > 0.00001 && contacts < 8)
            {
                var direction = shot.Velocity.Normalized;
                var hit = query.Cast(shot, distance);
                if (!hit.Hit)
                {
                    shot.Position += direction * distance;
                    distance = 0;
                    break;
                }
                contacts++;
                shot.Position += direction * hit.Distance;
                distance = Math.Max(0, distance - hit.Distance);
                shot.Trace.Add(shot.Position);
                shot.Contacts.Add(hit.TargetId);
                switch (hit.Kind)
                {
                    case BoardPieceKind.Collector:
                        var reflection = shot.Mirrors.Count == 0 ? 1 : shot.Mirrors.Count == 1 ? 1.5 : 2;
                        // This sandbox starts with the first Collector Value upgrade already owned.
                        shot.Gold = (int)Math.Floor(2 * reflection * shot.RewardToken);
                        shot.RewardToken = 1;
                        shot.HitTarget = hit.TargetId;
                        shot.Alive = false;
                        break;
                    case BoardPieceKind.Amplifier:
                        if (shot.Amplifiers.Add(hit.TargetId)) shot.RewardToken = Math.Min(4, shot.RewardToken * 2);
                        // Query ignores this amplifier for the rest of this root shot's lifetime.
                        break;
                    case BoardPieceKind.Mirror:
                        if (shot.ReflectionsLeft-- <= 0) { shot.Alive = false; break; }
                        shot.Mirrors.Add(hit.TargetId);
                        shot.Velocity = SimVector2.Reflect(shot.Velocity, hit.Normal);
                        shot.Position = hit.Point + hit.Normal * (RoutingShot.Radius + 0.3);
                        distance = Math.Max(0, distance - 0.3);
                        break;
                }
            }
            if (shot.Alive) shot.PendingDistance = distance;
            if (shot.Position.X < -100 || shot.Position.X > 1700 || shot.Position.Y < -100 || shot.Position.Y > 1000)
                shot.Alive = false;
        }
    }
}
