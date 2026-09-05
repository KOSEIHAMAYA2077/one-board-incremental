using System.Collections.Generic;

namespace IncrementalGame.Core
{
    // Passive instrumentation. Effective DPS excludes overkill and uses simulation time.
    public sealed class MomentumCombatStats
    {
        private readonly Queue<Sample> _recent = new Queue<Sample>();
        private double _recentDamage;
        public double TotalDamage { get; private set; }
        public int Hits { get; private set; }
        public double LastNormalImpact { get; private set; }
        public double LastPierceImpact { get; private set; }
        public double RecentDps => _recentDamage / 5;
        public void Advance(double time)
        {
            while (_recent.Count > 0 && _recent.Peek().Time <= time - 5)
                _recentDamage -= _recent.Dequeue().Damage;
            if (_recent.Count == 0) _recentDamage = 0;
        }
        public void Record(double time, double applied, double impact, MomentumAmmo ammo)
        {
            Advance(time); TotalDamage += applied; Hits++;
            _recent.Enqueue(new Sample { Time = time, Damage = applied }); _recentDamage += applied;
            if (ammo == MomentumAmmo.Normal) LastNormalImpact = impact; else LastPierceImpact = impact;
        }
        private struct Sample { public double Time, Damage; }
    }
}
