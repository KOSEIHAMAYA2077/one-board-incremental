using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class ProjectileStateTests
    {
        [Test]
        public void ReflectionUsesSurfaceNormalAndConsumesBudget()
        {
            var projectile = new ProjectileState(
                1,
                new SimVector2(0.0, 0.0),
                new SimVector2(3.0, -4.0),
                8.0,
                1,
                7,
                1.0);

            Assert.That(projectile.TryReflect(new SimVector2(0.0, 1.0)), Is.True);
            Assert.That(projectile.Velocity.X, Is.EqualTo(3.0).Within(1e-9));
            Assert.That(projectile.Velocity.Y, Is.EqualTo(4.0).Within(1e-9));
            Assert.That(projectile.HasReflected, Is.True);
            Assert.That(projectile.RemainingReflections, Is.Zero);
            Assert.That(projectile.DistinctReflectionSurfaceCount, Is.EqualTo(1));
            Assert.That(projectile.SpawnSequence, Is.EqualTo(7));
            Assert.That(projectile.GunMultiplierSnapshot, Is.EqualTo(1.0));

            Assert.That(projectile.TryReflect(new SimVector2(1.0, 0.0)), Is.False);
            Assert.That(projectile.Alive, Is.False);
        }
    }
}
