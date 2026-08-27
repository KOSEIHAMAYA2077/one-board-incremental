using System.Collections.Generic;
using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class ProjectileSimulationTests
    {
        private static readonly SimulationBounds WideBounds =
            new SimulationBounds(-1000.0, 1000.0, -1000.0, 1000.0);

        [Test]
        public void NoCollisionAdvancesByVelocityAndDeltaTime()
        {
            var projectile = CreateProjectile(new SimVector2(0.0, 100.0), 0);
            var result = ProjectileSimulation.Step(
                projectile,
                0.5,
                new SequenceCollisionQuery(ProjectileCollision.None),
                4,
                WideBounds);

            Assert.That(projectile.Position.Y, Is.EqualTo(50.0).Within(1e-9));
            Assert.That(projectile.Alive, Is.True);
            Assert.That(result.Missed, Is.False);
        }

        [Test]
        public void CollectorContactStopsFastProjectile()
        {
            var projectile = CreateProjectile(new SimVector2(0.0, 50000.0), 0);
            var collector = new ProjectileCollision(
                CollisionSurfaceKind.Collector,
                500.0,
                new SimVector2(0.0, 500.0),
                new SimVector2(0.0, -1.0));

            var result = ProjectileSimulation.Step(
                projectile,
                1.0 / 60.0,
                new SequenceCollisionQuery(collector),
                4,
                WideBounds);

            Assert.That(result.CollectorContact, Is.True);
            Assert.That(projectile.Alive, Is.False);
            Assert.That(projectile.Position.Y, Is.LessThan(500.0));
        }

        [Test]
        public void WallReflectionUsesNormalAndContinuesRemainingDistance()
        {
            var projectile = CreateProjectile(new SimVector2(0.0, 100.0), 1);
            var wall = new ProjectileCollision(
                CollisionSurfaceKind.ReflectionWall,
                40.0,
                new SimVector2(0.0, 40.0),
                new SimVector2(0.0, -1.0));

            var result = ProjectileSimulation.Step(
                projectile,
                1.0,
                new SequenceCollisionQuery(wall, ProjectileCollision.None),
                4,
                WideBounds);

            Assert.That(result.ReflectionCount, Is.EqualTo(1));
            Assert.That(projectile.Velocity.Y, Is.EqualTo(-100.0).Within(1e-9));
            Assert.That(projectile.Alive, Is.True);
        }

        [Test]
        public void ContactGuardTerminatesRepeatedZeroDistanceContacts()
        {
            var projectile = CreateProjectile(new SimVector2(100.0, 0.0), 8);
            var wall = new ProjectileCollision(
                CollisionSurfaceKind.ReflectionWall,
                0.0,
                new SimVector2(0.0, 0.0),
                new SimVector2(-1.0, 0.0));

            var result = ProjectileSimulation.Step(
                projectile,
                1.0,
                new RepeatingCollisionQuery(wall),
                2,
                WideBounds);

            Assert.That(result.ContactGuardReached, Is.True);
            Assert.That(result.Missed, Is.True);
            Assert.That(projectile.Alive, Is.False);
        }

        private static ProjectileState CreateProjectile(SimVector2 velocity, int reflections) =>
            new ProjectileState(
                1,
                new SimVector2(0.0, 0.0),
                velocity,
                8.0,
                reflections,
                1,
                1.0);

        private sealed class SequenceCollisionQuery : IProjectileCollisionQuery
        {
            private readonly Queue<ProjectileCollision> _collisions;

            public SequenceCollisionQuery(params ProjectileCollision[] collisions)
            {
                _collisions = new Queue<ProjectileCollision>(collisions);
            }

            public ProjectileCollision CircleCast(
                SimVector2 position,
                double radius,
                SimVector2 direction,
                double distance) =>
                _collisions.Count == 0 ? ProjectileCollision.None : _collisions.Dequeue();
        }

        private sealed class RepeatingCollisionQuery : IProjectileCollisionQuery
        {
            private readonly ProjectileCollision _collision;

            public RepeatingCollisionQuery(ProjectileCollision collision)
            {
                _collision = collision;
            }

            public ProjectileCollision CircleCast(
                SimVector2 position,
                double radius,
                SimVector2 direction,
                double distance) => _collision;
        }
    }
}
