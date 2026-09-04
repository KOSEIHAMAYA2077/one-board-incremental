using System.Collections.Generic;
using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class FreePlacementTests
    {
        [Test]
        public void InitialLayoutIsValidAndCollectorOverlapIsRejected()
        {
            var pieces = FreePlacementBoard.CreateInitial();
            foreach (var piece in pieces) Assert.That(FreePlacementBoard.CanPlace(piece, pieces), Is.True);
            pieces[1].Position = pieces[0].Position;
            Assert.That(FreePlacementBoard.CanPlace(pieces[1], pieces), Is.False);
        }

        [Test]
        public void RotationUsesActualCornersAndRejectsBoundaryCrossing()
        {
            var pieces = FreePlacementBoard.CreateInitial(); var mirror = pieces[2];
            mirror.Position = new SimVector2(340, 440); mirror.Angle = 0;
            Assert.That(FreePlacementBoard.CanPlace(mirror, pieces), Is.True);
            mirror.Angle = 90;
            Assert.That(FreePlacementBoard.CanPlace(mirror, pieces), Is.False);
        }

        [Test]
        public void CircleBoxChecksDoNotTreatEmptyCornersAsSolid()
        {
            var circle = new BoardPiece(1, BoardPieceKind.Collector, new SimVector2(700, 500), new SimVector2(100, 100));
            var box = new BoardPiece(2, BoardPieceKind.Amplifier, new SimVector2(790, 590), new SimVector2(100, 100));
            Assert.That(FreePlacementBoard.Overlaps(circle, box), Is.False);
            box.Position = new SimVector2(760, 500);
            Assert.That(FreePlacementBoard.Overlaps(circle, box), Is.True);
        }

        [TestCase(0, 0, 2)]
        [TestCase(1, 0, 3)]
        [TestCase(0, 1, 4)]
        [TestCase(1, 1, 6)]
        public void RouteRewardsMatchReflectionAndAmplifier(int mirrors, int amplifiers, int gold)
        {
            var shot = new RoutingShot(new SimVector2(800, 600), new SimVector2(0, -900));
            if (mirrors > 0) shot.Mirrors.Add(3);
            if (amplifiers > 0) shot.RewardToken = 2;
            RoutingSimulation.Step(shot, 1.0 / 60, new CollectorQuery());
            Assert.That(shot.Gold, Is.EqualTo(gold)); Assert.That(shot.Alive, Is.False);
        }

        [Test]
        public void BoardKeepsGunAndHudClear()
        {
            var pieces = FreePlacementBoard.CreateInitial();
            foreach (var point in new[] { FreePlacementBoard.Gun, new SimVector2(200, 300), new SimVector2(800, 80) })
            { pieces[0].Position = point; Assert.That(FreePlacementBoard.CanPlace(pieces[0], pieces), Is.False); }
        }
        private sealed class CollectorQuery : IRoutingQuery
        {
            public RoutingContact Cast(RoutingShot shot, double distance) =>
                new RoutingContact(1, BoardPieceKind.Collector, 1, shot.Position, new SimVector2(0, 1));
        }
    }
}
