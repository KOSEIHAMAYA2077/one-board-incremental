using System.Collections;
using System.Collections.Generic;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class RecipePlayModeTests
    {
        private GameObject _root;
        private readonly List<BoardPieceView> _views = new List<BoardPieceView>();
        [SetUp] public void SetUp()
        {
            _root = new GameObject("Recipe fixture");
            foreach (var piece in FreePlacementBoard.CreateInitial())
            {
                var obj = new GameObject("Piece"); obj.transform.SetParent(_root.transform);
                var view = obj.AddComponent<BoardPieceView>(); view.Initialize(piece); _views.Add(view);
            }
        }
        [UnityTearDown] public IEnumerator TearDown() { Object.Destroy(_root); _views.Clear(); yield return null; }
        private RecipeLineage Run(RecipeBullet bullet, int primer)
        {
            foreach (var view in _views) view.Apply(); Physics2D.SyncTransforms();
            var line = new RecipeLineage(bullet, primer, FreePlacementBoard.Gun, new SimVector2(0, -900));
            for (var i = 0; i < 240 && !line.Complete; i++) line.Tick(1.0 / 60, new BoardRoutingQuery());
            return line;
        }
        [Test] public void PiercePassesTwoActualCollectorsAndConsumesTokenOnlyOnce()
        {
            _views[1].Piece.Position = new SimVector2(800, 350);
            var line = Run(RecipeBullet.Pierce, 2);
            Assert.That(line.RewardVisited.Count, Is.EqualTo(2));
            Assert.That(line.YieldGold, Is.EqualTo(7)); // floor(2*1.25*2) + floor(2*1.25)
            Assert.That(line.Projectiles[0].Contacts, Is.EqualTo(new[] { 4, 2, 1 }));
        }
        [Test] public void SplitFansToTwoCollectorsWithoutDuplicateSiblingRewards()
        {
            _views[0].Piece.Position = new SimVector2(730, 220);
            _views[1].Piece.Position = new SimVector2(870, 220);
            var line = Run(RecipeBullet.Split, 2);
            Assert.That(line.YieldGold, Is.EqualTo(8)); Assert.That(line.RewardVisited.Count, Is.EqualTo(2));
            Assert.That(line.EffectVisited.Count, Is.EqualTo(1)); Assert.That(line.Complete, Is.True);
        }
        [Test] public void SameSeedAndLayoutProduceSameLineageResults()
        {
            var first = Run(RecipeBullet.Split, 2); var second = Run(RecipeBullet.Split, 2);
            Assert.That(second.YieldGold, Is.EqualTo(first.YieldGold));
            Assert.That(second.Projectiles.Count, Is.EqualTo(first.Projectiles.Count));
            for (var i = 0; i < first.Projectiles.Count; i++)
                Assert.That(second.Projectiles[i].Position, Is.EqualTo(first.Projectiles[i].Position));
        }
        [Test] public void UnityJsonRoundTripRestoresActiveAndPendingRecipe()
        {
            var cycle = new RecipeCycle();
            var restored = new RecipeCycle();
            Assert.That(restored.Restore(JsonUtility.FromJson<RecipeSnapshot>(JsonUtility.ToJson(cycle.Capture()))), Is.True);
            cycle.TryFire(); cycle.Apply(new[] { RecipeBullet.Pierce, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal });
            Assert.That(restored.Restore(JsonUtility.FromJson<RecipeSnapshot>(JsonUtility.ToJson(cycle.Capture()))), Is.True);
            Assert.That(restored.Pending[0], Is.EqualTo(RecipeBullet.Pierce)); Assert.That(restored.Slot, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator RecipeEditingPreservesShotsAndBlocksClosingClick()
        {
            foreach (var view in _views) view.gameObject.SetActive(false);
            var session = new GameObject("Recipe session"); session.transform.SetParent(_root.transform); session.SetActive(false);
            var cameraObject = new GameObject("Camera"); cameraObject.transform.SetParent(session.transform); cameraObject.AddComponent<Camera>().orthographic = true;
            var controller = session.AddComponent<FreePlacementController>(); controller.PersistenceEnabled = false; controller.RecipeMode = true;
            session.SetActive(true); yield return null; yield return null;
            Assert.That(controller.TryFire(new SimVector2(800, 220)), Is.True);
            var count = controller.ActiveShotCount;
            controller.SetRecipeEditing(true); var time = controller.SimulatedSeconds;
            controller.SimulateTick(5);
            Assert.That(controller.SimulatedSeconds, Is.EqualTo(time)); Assert.That(controller.ActiveShotCount, Is.EqualTo(count));
            Assert.That(controller.TryFire(new SimVector2(800, 220)), Is.False);
            Assert.That(controller.ApplyRecipe(new[] { RecipeBullet.Pierce, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal }), Is.True);
            Assert.That(controller.Recipe.Pending, Is.Not.Null);
            Assert.That(controller.TryFire(new SimVector2(800, 220)), Is.False);
            controller.SetEditing(true); Assert.That(controller.ActiveShotCount, Is.Zero);
        }
    }
}
