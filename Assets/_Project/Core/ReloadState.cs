using System;

namespace IncrementalGame.Core
{
    public sealed class ReloadState
    {
        private const double Epsilon = 1e-9;

        public ReloadState(double durationSeconds)
        {
            if (durationSeconds <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            DurationSeconds = durationSeconds;
        }

        public double DurationSeconds { get; }
        public double RemainingSeconds { get; private set; }
        public bool IsReady => RemainingSeconds <= Epsilon;

        public bool TryFire()
        {
            if (!IsReady)
            {
                return false;
            }

            RemainingSeconds = DurationSeconds;
            return true;
        }

        public bool Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            var wasReady = IsReady;
            RemainingSeconds = Math.Max(0.0, RemainingSeconds - deltaSeconds);
            return !wasReady && IsReady;
        }
    }
}
