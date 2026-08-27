using System;

namespace IncrementalGame.Core
{
    public sealed class GameEconomy
    {
        public const double FirstCollectorUpgradeCost = 10.0;

        public double Gold { get; private set; }
        public double LifetimeGold { get; private set; }
        public int CollectorValueLevel { get; private set; }
        public int HitCount { get; private set; }

        public int ResolveCollectorHit(bool hasReflected)
        {
            var baseGold = 1 + CollectorValueLevel;
            var multiplier = hasReflected ? 1.5 : 1.0;
            var reward = (int)Math.Floor(baseGold * multiplier);

            Gold += reward;
            LifetimeGold += reward;
            HitCount += 1;
            return reward;
        }

        public bool TryPurchaseFirstCollectorValueUpgrade()
        {
            if (CollectorValueLevel != 0 || Gold < FirstCollectorUpgradeCost)
            {
                return false;
            }

            Gold -= FirstCollectorUpgradeCost;
            CollectorValueLevel = 1;
            return true;
        }
    }
}
