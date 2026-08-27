namespace IncrementalGame.Core
{
    public sealed class ProjectileState
    {
        public ProjectileState(
            long id,
            SimVector2 position,
            SimVector2 velocity,
            double radius,
            int remainingReflections,
            long spawnSequence,
            double gunMultiplierSnapshot)
        {
            Id = id;
            Position = position;
            Velocity = velocity;
            Radius = radius;
            RemainingReflections = remainingReflections;
            SpawnSequence = spawnSequence;
            GunMultiplierSnapshot = gunMultiplierSnapshot;
            Alive = true;
        }

        public long Id { get; }
        public SimVector2 Position { get; set; }
        public SimVector2 Velocity { get; set; }
        public double Radius { get; }
        public int RemainingReflections { get; set; }
        public int DistinctReflectionSurfaceCount { get; private set; }
        public long SpawnSequence { get; }
        public double GunMultiplierSnapshot { get; }
        public bool HasReflected { get; set; }
        public bool Alive { get; set; }

        public void Advance(double deltaSeconds)
        {
            Position += Velocity * deltaSeconds;
        }

        public bool TryReflect(SimVector2 normal)
        {
            if (!Alive || RemainingReflections <= 0)
            {
                Alive = false;
                return false;
            }

            Velocity = SimVector2.Reflect(Velocity, normal);
            RemainingReflections -= 1;
            DistinctReflectionSurfaceCount = 1;
            HasReflected = true;
            return true;
        }
    }
}
