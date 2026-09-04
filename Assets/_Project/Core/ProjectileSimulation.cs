using System;

namespace IncrementalGame.Core
{
    public readonly struct ProjectileStepResult
    {
        public ProjectileStepResult(
            bool collectorContact,
            bool missed,
            bool contactGuardReached,
            int reflectionCount)
        {
            CollectorContact = collectorContact;
            Missed = missed;
            ContactGuardReached = contactGuardReached;
            ReflectionCount = reflectionCount;
        }

        public bool CollectorContact { get; }
        public bool Missed { get; }
        public bool ContactGuardReached { get; }
        public int ReflectionCount { get; }
    }

    public static class ProjectileSimulation
    {
        private const double ContactEpsilon = 0.1;
        private const double SeparationEpsilon = 0.3;

        public static ProjectileStepResult Step(
            ProjectileState state,
            double deltaSeconds,
            IProjectileCollisionQuery collisionQuery,
            int maximumContacts,
            SimulationBounds bounds)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (collisionQuery == null)
            {
                throw new ArgumentNullException(nameof(collisionQuery));
            }

            if (deltaSeconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            if (maximumContacts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumContacts));
            }

            var remainingDistance = state.Velocity.Magnitude * deltaSeconds;
            var contacts = 0;
            var reflectionCount = 0;
            var collectorContact = false;
            var missed = false;

            while (state.Alive && remainingDistance > 1e-5 && contacts < maximumContacts)
            {
                contacts += 1;
                var direction = state.Velocity.Normalized;
                var hit = collisionQuery.CircleCast(
                    state.Position,
                    state.Radius,
                    direction,
                    remainingDistance);

                if (!hit.HasCollision)
                {
                    state.Position += direction * remainingDistance;
                    remainingDistance = 0.0;
                    break;
                }

                var safeTravel = Math.Max(0.0, hit.Distance - ContactEpsilon);
                state.Position += direction * safeTravel;
                remainingDistance -= safeTravel;

                if (hit.SurfaceKind == CollisionSurfaceKind.Collector)
                {
                    collectorContact = true;
                    state.Alive = false;
                    break;
                }

                if (hit.SurfaceKind == CollisionSurfaceKind.ReflectionWall)
                {
                    if (!state.TryReflect(hit.Normal))
                    {
                        missed = true;
                        break;
                    }

                    reflectionCount += 1;
                    state.Position = hit.Point + hit.Normal * (state.Radius + SeparationEpsilon);
                    remainingDistance = Math.Max(
                        0.0,
                        remainingDistance - ContactEpsilon - SeparationEpsilon);
                    continue;
                }

                state.Alive = false;
                missed = true;
            }

            var guardReached = state.Alive && contacts >= maximumContacts && remainingDistance > 1e-5;
            if (guardReached)
            {
                state.Alive = false;
                missed = true;
            }

            if (state.Alive && !bounds.Contains(state.Position))
            {
                state.Alive = false;
                missed = true;
            }

            return new ProjectileStepResult(
                collectorContact,
                missed,
                guardReached,
                reflectionCount);
        }
    }
}
