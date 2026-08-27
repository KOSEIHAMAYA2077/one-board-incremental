using System;

namespace IncrementalGame.Core
{
    public sealed class CollectorState
    {
        public CollectorState(double hiddenDurationSeconds)
        {
            if (hiddenDurationSeconds <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(hiddenDurationSeconds));
            }

            HiddenDurationSeconds = hiddenDurationSeconds;
        }

        public double HiddenDurationSeconds { get; }
        public double RemainingHiddenSeconds { get; private set; }
        public bool CanBeHit => RemainingHiddenSeconds <= 0.0;

        public bool TryAcceptHit()
        {
            if (!CanBeHit)
            {
                return false;
            }

            RemainingHiddenSeconds = HiddenDurationSeconds;
            return true;
        }

        public bool Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            var wasHidden = !CanBeHit;
            RemainingHiddenSeconds = Math.Max(0.0, RemainingHiddenSeconds - deltaSeconds);
            return wasHidden && CanBeHit;
        }
    }
}
