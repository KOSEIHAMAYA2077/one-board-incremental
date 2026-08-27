using System;

namespace IncrementalGame.Core
{
    public readonly struct SimVector2 : IEquatable<SimVector2>
    {
        private const double Epsilon = 1e-12;

        public SimVector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
        public double SqrMagnitude => X * X + Y * Y;
        public double Magnitude => Math.Sqrt(SqrMagnitude);

        public SimVector2 Normalized
        {
            get
            {
                var magnitude = Magnitude;
                return magnitude <= Epsilon ? new SimVector2(0.0, 0.0) : this / magnitude;
            }
        }

        public static SimVector2 operator +(SimVector2 left, SimVector2 right) =>
            new SimVector2(left.X + right.X, left.Y + right.Y);

        public static SimVector2 operator -(SimVector2 left, SimVector2 right) =>
            new SimVector2(left.X - right.X, left.Y - right.Y);

        public static SimVector2 operator *(SimVector2 value, double scalar) =>
            new SimVector2(value.X * scalar, value.Y * scalar);

        public static SimVector2 operator /(SimVector2 value, double scalar) =>
            new SimVector2(value.X / scalar, value.Y / scalar);

        public static double Dot(SimVector2 left, SimVector2 right) =>
            left.X * right.X + left.Y * right.Y;

        public static SimVector2 Reflect(SimVector2 direction, SimVector2 normal)
        {
            var unitNormal = normal.Normalized;
            return direction - unitNormal * (2.0 * Dot(direction, unitNormal));
        }

        public bool Equals(SimVector2 other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is SimVector2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X:0.###}, {Y:0.###})";
    }
}
