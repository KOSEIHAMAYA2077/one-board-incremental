using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class ReloadStateTests
    {
        [Test]
        public void SecondShotIsRejectedDuringReload()
        {
            var reload = new ReloadState(0.65);

            Assert.That(reload.TryFire(), Is.True);
            Assert.That(reload.TryFire(), Is.False);
            Assert.That(reload.Tick(0.64), Is.False);
            Assert.That(reload.TryFire(), Is.False);
            Assert.That(reload.Tick(0.01), Is.True);
            Assert.That(reload.TryFire(), Is.True);
        }
    }
}
