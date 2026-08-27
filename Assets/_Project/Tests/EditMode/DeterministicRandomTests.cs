using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class DeterministicRandomTests
    {
        [Test]
        public void SameSeedAndShotSequenceProducesSameOffsets()
        {
            var left = new DeterministicRandom(20260828u);
            var right = new DeterministicRandom(20260828u);

            for (var shot = 0; shot < 100; shot += 1)
            {
                Assert.That(left.NextSignedOffset(7.0), Is.EqualTo(right.NextSignedOffset(7.0)));
            }
        }

        [Test]
        public void OffsetStaysInsideConfiguredMaximumSpread()
        {
            var random = new DeterministicRandom(1u);

            for (var shot = 0; shot < 1000; shot += 1)
            {
                Assert.That(random.NextSignedOffset(7.0), Is.InRange(-7.0, 7.0));
            }
        }
    }
}
