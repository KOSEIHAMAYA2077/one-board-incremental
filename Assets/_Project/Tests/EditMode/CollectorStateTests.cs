using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class CollectorStateTests
    {
        [Test]
        public void VisualReturnDoesNotCreateAnotherReward()
        {
            var economy = new GameEconomy();
            var collector = new CollectorState(0.35);

            Assert.That(collector.TryAcceptHit(), Is.True);
            economy.ResolveCollectorHit(false);
            Assert.That(collector.TryAcceptHit(), Is.False);
            Assert.That(collector.Tick(0.35), Is.True);

            Assert.That(economy.Gold, Is.EqualTo(1.0));
            Assert.That(economy.LifetimeGold, Is.EqualTo(1.0));
            Assert.That(economy.HitCount, Is.EqualTo(1));
        }
    }
}
