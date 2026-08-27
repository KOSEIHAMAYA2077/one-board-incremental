using System;

namespace IncrementalGame.Core
{
    public static class AimCalculator
    {
        public const double NearDistance = 100.0;
        public const double FarDistance = 600.0;
        public const double NearSpreadDegrees = 14.0;
        public const double FarSpreadDegrees = 2.0;

        public static double GetSpreadDegrees(double cursorDistance)
        {
            if (cursorDistance <= NearDistance)
            {
                return NearSpreadDegrees;
            }

            if (cursorDistance >= FarDistance)
            {
                return FarSpreadDegrees;
            }

            var t = (cursorDistance - NearDistance) / (FarDistance - NearDistance);
            return NearSpreadDegrees + (FarSpreadDegrees - NearSpreadDegrees) * t;
        }

        public static SimVector2 RotateDegrees(SimVector2 direction, double degrees)
        {
            var radians = degrees * Math.PI / 180.0;
            var cosine = Math.Cos(radians);
            var sine = Math.Sin(radians);
            var unit = direction.Normalized;

            return new SimVector2(
                unit.X * cosine - unit.Y * sine,
                unit.X * sine + unit.Y * cosine);
        }
    }
}
