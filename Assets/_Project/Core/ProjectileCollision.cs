namespace IncrementalGame.Core
{
    public enum CollisionSurfaceKind
    {
        None = 0,
        Collector = 1,
        ReflectionWall = 2,
        Solid = 3
    }

    public readonly struct ProjectileCollision
    {
        public ProjectileCollision(
            CollisionSurfaceKind surfaceKind,
            double distance,
            SimVector2 point,
            SimVector2 normal)
        {
            SurfaceKind = surfaceKind;
            Distance = distance;
            Point = point;
            Normal = normal;
        }

        public CollisionSurfaceKind SurfaceKind { get; }
        public double Distance { get; }
        public SimVector2 Point { get; }
        public SimVector2 Normal { get; }
        public bool HasCollision => SurfaceKind != CollisionSurfaceKind.None;

        public static ProjectileCollision None => new ProjectileCollision(
            CollisionSurfaceKind.None,
            0.0,
            new SimVector2(0.0, 0.0),
            new SimVector2(0.0, 0.0));
    }

    public interface IProjectileCollisionQuery
    {
        ProjectileCollision CircleCast(
            SimVector2 position,
            double radius,
            SimVector2 direction,
            double distance);
    }

    public readonly struct SimulationBounds
    {
        public SimulationBounds(double minimumX, double maximumX, double minimumY, double maximumY)
        {
            MinimumX = minimumX;
            MaximumX = maximumX;
            MinimumY = minimumY;
            MaximumY = maximumY;
        }

        public double MinimumX { get; }
        public double MaximumX { get; }
        public double MinimumY { get; }
        public double MaximumY { get; }

        public bool Contains(SimVector2 point) =>
            point.X >= MinimumX && point.X <= MaximumX &&
            point.Y >= MinimumY && point.Y <= MaximumY;
    }
}
