using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class AimCalculatorTests
    {
        [TestCase(0.0, 14.0)]
        [TestCase(100.0, 14.0)]
        [TestCase(350.0, 8.0)]
        [TestCase(600.0, 2.0)]
        [TestCase(900.0, 2.0)]
        public void SpreadIsClampedAndInterpolated(double distance, double expected)
        {
            Assert.That(AimCalculator.GetSpreadDegrees(distance), Is.EqualTo(expected).Within(1e-9));
        }

        [Test]
        public void RotateDegreesKeepsUnitLength()
        {
            var result = AimCalculator.RotateDegrees(new SimVector2(0.0, 2.0), 30.0);
            Assert.That(result.Magnitude, Is.EqualTo(1.0).Within(1e-9));
            Assert.That(result.X, Is.EqualTo(-0.5).Within(1e-9));
        }
    }
}
