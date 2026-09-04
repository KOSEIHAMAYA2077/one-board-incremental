using System.Collections;
using System.Collections.Generic;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class FreePlacementPlayModeTests
    {
        private GameObject _root;
        private readonly List<BoardPieceView> _views = new List<BoardPieceView>();
        [SetUp] public void SetUp()
        {
            _root = new GameObject("Routing fixture");
            foreach (var piece in FreePlacementBoard.CreateInitial())
            {
                var obj = new GameObject("Piece " + piece.Id); obj.transform.SetParent(_root.transform);
                var view = obj.AddComponent<BoardPieceView>(); view.Initialize(piece); _views.Add(view);
            }
            Physics2D.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator TearDown() { Object.Destroy(_root); _views.Clear(); yield return null; }

        private RoutingShot Run(SimVector2 aim)
        {
            Physics2D.SyncTransforms();
            var shot = new RoutingShot(FreePlacementBoard.Gun, (aim - FreePlacementBoard.Gun).Normalized * 900);
            for (var i = 0; i < 240 && shot.Alive; i++) RoutingSimulation.Step(shot, 1.0 / 60, new BoardRoutingQuery());
            return shot;
        }

        [Test]
        public void InitialRoutePassesAmplifierThenCollectorForFourGold()
        {
            var shot = Run(_views[0].Piece.Position);
            Assert.That(shot.Contacts, Is.EqualTo(new[] { 4, 1 }));
            Assert.That(shot.Gold, Is.EqualTo(4));
        }

        [Test]
        public void TwoRapidShotsBothReceiveCollectorReward()
        {
            Assert.That(Run(_views[0].Piece.Position).Gold, Is.EqualTo(4));
            _views[0].Flash();
            Assert.That(_views[0].Collider.enabled, Is.True);
            Assert.That(Run(_views[0].Piece.Position).Gold, Is.EqualTo(4));
        }

        [Test]
        public void ObliqueMirrorAndAmplifierReachCollectorForSixGold()
        {
            _views[0].Piece.Position = new SimVector2(800, 225); _views[0].Apply();
            var mirror = _views[2].Piece;
            var collectorWorld = (Vector2)_views[0].transform.position;
            var mirrorWorld = (Vector2)_views[2].transform.position;
            var normal = (Vector2)_views[2].transform.right;
            var virtualTarget = collectorWorld - 2 * Vector2.Dot(collectorWorld - mirrorWorld, normal) * normal;
            var direction = (virtualTarget - LogicalSpace.ToWorld(FreePlacementBoard.Gun)).normalized;
            // Amplifier sits along the incoming route, clear of the mirror.
            _views[3].Piece.Position = FreePlacementBoard.Gun + LogicalSpace.DirectionToLogical(direction) * 220;
            _views[3].Apply();
            var shot = Run(LogicalSpace.ToLogical(virtualTarget));
            Assert.That(shot.Mirrors.Count, Is.EqualTo(1));
            Assert.That(shot.Gold, Is.EqualTo(6));
        }

        [Test]
        public void RotatedMirrorColliderMatchesRenderedShape()
        {
            var view = _views[2]; view.Piece.Angle = 45; view.Apply(); Physics2D.SyncTransforms();
            var along = view.Piece.Position + view.Piece.AxisY * 90;
            var away = view.Piece.Position + view.Piece.AxisX * 30;
            Assert.That(view.Collider.OverlapPoint(LogicalSpace.ToWorld(along)), Is.True);
            Assert.That(view.Collider.OverlapPoint(LogicalSpace.ToWorld(away)), Is.False);
        }

        [UnityTest]
        public IEnumerator EditingPausesRejectsOverlapAndDoesNotFireOnClose()
        {
            // Disable the query-only fixture while exercising the actual controller.
            foreach (var view in _views) view.gameObject.SetActive(false);
            var session = new GameObject("Placement session"); session.transform.SetParent(_root.transform); session.SetActive(false);
            var cameraObject = new GameObject("Camera"); cameraObject.transform.SetParent(session.transform);
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true;
            var controller = session.AddComponent<FreePlacementController>(); controller.PersistenceEnabled = false;
            session.SetActive(true);
            yield return null; yield return null;
            controller.SetEditing(true);
            var time = controller.SimulatedSeconds;
            Assert.That(controller.TryMovePiece(3, new SimVector2(600, 540), -30), Is.True);
            Assert.That(controller.TryMovePiece(3, new SimVector2(800, 220), 0), Is.False);
            Assert.That(controller.Pieces[2].Position, Is.EqualTo(new SimVector2(600, 540)));
            controller.SimulateTick(1);
            Assert.That(controller.SimulatedSeconds, Is.EqualTo(time));
            Assert.That(controller.TryFire(new SimVector2(800, 220)), Is.False);
            controller.SetEditing(false);
            Assert.That(controller.TryFire(new SimVector2(800, 220)), Is.False);
            Assert.That(controller.ActiveShotCount, Is.Zero);
        }
    }
}
