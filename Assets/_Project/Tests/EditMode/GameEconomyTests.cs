using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class GameEconomyTests
    {
        [Test]
        public void TenDirectHitsBuyFirstUpgradeAndNextHitPaysTwo()
        {
            var economy = new GameEconomy();

            for (var hit = 0; hit < 10; hit += 1)
            {
                Assert.That(economy.ResolveCollectorHit(false), Is.EqualTo(1));
            }

            Assert.That(economy.Gold, Is.EqualTo(10.0));
            Assert.That(economy.LifetimeGold, Is.EqualTo(10.0));
            Assert.That(economy.HitCount, Is.EqualTo(10));
            Assert.That(economy.TryPurchaseFirstCollectorValueUpgrade(), Is.True);
            Assert.That(economy.Gold, Is.EqualTo(0.0));
            Assert.That(economy.LifetimeGold, Is.EqualTo(10.0));
            Assert.That(economy.CollectorValueLevel, Is.EqualTo(1));

            Assert.That(economy.ResolveCollectorHit(false), Is.EqualTo(2));
            Assert.That(economy.Gold, Is.EqualTo(2.0));
            Assert.That(economy.LifetimeGold, Is.EqualTo(12.0));
        }

        [Test]
        public void LevelOneReflectedHitPaysThree()
        {
            var economy = CreateUpgradedEconomy();
            Assert.That(economy.ResolveCollectorHit(true), Is.EqualTo(3));
        }

        [Test]
        public void InsufficientGoldAndDoublePurchaseAreRejected()
        {
            var economy = new GameEconomy();
            Assert.That(economy.TryPurchaseFirstCollectorValueUpgrade(), Is.False);

            for (var hit = 0; hit < 10; hit += 1)
            {
                economy.ResolveCollectorHit(false);
            }

            Assert.That(economy.TryPurchaseFirstCollectorValueUpgrade(), Is.True);
            economy.ResolveCollectorHit(false);
            Assert.That(economy.TryPurchaseFirstCollectorValueUpgrade(), Is.False);
            Assert.That(economy.Gold, Is.EqualTo(2.0));
        }

        private static GameEconomy CreateUpgradedEconomy()
        {
            var economy = new GameEconomy();
            for (var hit = 0; hit < 10; hit += 1)
            {
                economy.ResolveCollectorHit(false);
            }

            economy.TryPurchaseFirstCollectorValueUpgrade();
            return economy;
        }
    }
}
