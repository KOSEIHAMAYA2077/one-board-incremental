namespace IncrementalGame.Core
{
    public sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0 ? 0x6D2B79F5u : seed;
        }

        public uint NextUInt()
        {
            var value = _state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            _state = value;
            return value;
        }

        public double NextUnitDouble() => NextUInt() / ((double)uint.MaxValue + 1.0);

        public double NextSignedOffset(double maximumAbsoluteDegrees) =>
            (NextUnitDouble() * 2.0 - 1.0) * maximumAbsoluteDegrees;
    }
}
