using System.Collections;
using System.Diagnostics;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class Prototype0PlayModeTests
    {
        private GameObject _root;
        private GameObject _collectorObject;
        private CollectorTargetView _collector;
        private Transform _gun;
        private Prototype0Controller _controller;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _root = new GameObject("Prototype 0 Test Root");

            var cameraObject = new GameObject("Test Camera");
            cameraObject.transform.SetParent(_root.transform);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;

            var gunObject = new GameObject("Test Gun");
            gunObject.transform.SetParent(_root.transform);
            gunObject.transform.position = LogicalSpace.ToWorld(new SimVector2(800.0, 820.0));
            _gun = gunObject.transform;

            _collectorObject = new GameObject("Test Collector");
            _collectorObject.transform.SetParent(_root.transform);
            _collectorObject.transform.position = LogicalSpace.ToWorld(new SimVector2(800.0, 300.0));
            var collectorCollider = _collectorObject.AddComponent<BoxCollider2D>();
            collectorCollider.size = new Vector2(1.2f, 0.7f);
            _collector = _collectorObject.AddComponent<CollectorTargetView>();

            var controllerObject = new GameObject("Test Controller");
            controllerObject.transform.SetParent(_root.transform);
            _controller = controllerObject.AddComponent<Prototype0Controller>();
            _controller.Configure(camera, _collector, _gun, null, null, 12345);
            Physics2D.SyncTransforms();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UiClickDoesNotFire()
        {
            var fired = _controller.TryFireAtWorld(_collectorObject.transform.position, true);
            Assert.That(fired, Is.False);
            Assert.That(_controller.ActiveProjectileCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LogButtonAreaIsTreatedAsUi()
        {
            var screenPoint = new Vector2(100f, Screen.height - 250f);
            Assert.That(_controller.IsScreenPointOverPrototypeUi(screenPoint), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CursorAtGunUsesDefaultUpDirection()
        {
            var fired = _controller.TryFireAtWorld(_gun.position, false);
            var state = _controller.GetProjectileStateForTest(1);

            Assert.That(fired, Is.True);
            Assert.That(state, Is.Not.Null);
            Assert.That(state.Velocity.Y, Is.LessThan(0.0));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CircleCastPreventsFastProjectileTunnelingThroughCollector()
        {
            _controller.SpawnProjectileForTest(
                new SimVector2(800.0, 820.0),
                new SimVector2(0.0, -50000.0),
                0);

            _controller.SimulateTick(1.0 / 60.0);

            Assert.That(_controller.Economy.HitCount, Is.EqualTo(1));
            Assert.That(_controller.Economy.Gold, Is.EqualTo(1.0));
            Assert.That(_controller.ActiveProjectileCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WallNormalReflectsVelocity()
        {
            _collectorObject.transform.position = new Vector3(5f, 4f, 0f);
            var wall = CreateWall(Vector2.zero, new Vector2(4f, 0.2f), 0f);
            var projectileId = _controller.SpawnProjectileForTest(
                new SimVector2(800.0, 700.0),
                new SimVector2(0.0, -900.0),
                1);
            Physics2D.SyncTransforms();

            _controller.SimulateTick(0.3);

            var state = _controller.GetProjectileStateForTest(projectileId);
            Assert.That(state, Is.Not.Null);
            Assert.That(state.HasReflected, Is.True);
            Assert.That(state.Velocity.Y, Is.GreaterThan(0.0));
            Object.Destroy(wall);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Prototype0LayoutSupportsReflectedCollectorHit()
        {
            var collectorPosition = LogicalSpace.ToWorld(new SimVector2(800.0, 225.0));
            _collectorObject.transform.position = collectorPosition;
            _collectorObject.transform.localScale = new Vector3(1.35f, 0.72f, 1f);
            _collectorObject.GetComponent<BoxCollider2D>().size = Vector2.one;

            var wallPosition = LogicalSpace.ToWorld(new SimVector2(460.0, 580.0));
            var wall = CreateWall(wallPosition, Vector2.one, 5f);
            wall.transform.localScale = new Vector3(0.18f, 2.65f, 1f);

            var wallNormal = (Vector2)(Quaternion.Euler(0f, 0f, 5f) * Vector2.right);
            var virtualCollector = collectorPosition -
                2f * Vector2.Dot(collectorPosition - wallPosition, wallNormal) * wallNormal;
            var aimDirection = (virtualCollector - (Vector2)_gun.position).normalized;
            var projectileId = _controller.SpawnProjectileForTest(
                LogicalSpace.ToLogical(_gun.position),
                LogicalSpace.DirectionToLogical(aimDirection) * 900.0,
                1);
            Physics2D.SyncTransforms();

            var sawReflection = false;
            for (var tick = 0; tick < 120 && _controller.ActiveProjectileCount > 0; tick += 1)
            {
                _controller.SimulateTick(1.0 / 60.0);
                var state = _controller.GetProjectileStateForTest(projectileId);
                sawReflection |= state != null && state.HasReflected;
            }

            Assert.That(sawReflection, Is.True, "The route must visibly bend at the scene wall.");
            Assert.That(_controller.Economy.HitCount, Is.EqualTo(1));
            Assert.That(_controller.Economy.Gold, Is.EqualTo(1.0));
            Object.Destroy(wall);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CornerContactReturnsWithoutInfiniteRecollision()
        {
            _collectorObject.transform.position = new Vector3(5f, 4f, 0f);
            var horizontal = CreateWall(Vector2.zero, new Vector2(4f, 0.2f), 0f);
            var vertical = CreateWall(Vector2.zero, new Vector2(4f, 0.2f), 90f);
            _controller.SpawnProjectileForTest(
                LogicalSpace.ToLogical(new Vector2(-0.35f, -0.35f)),
                LogicalSpace.DirectionToLogical(new Vector2(1f, 1f)) * 900.0,
                8);
            Physics2D.SyncTransforms();
            var stopwatch = Stopwatch.StartNew();

            _controller.SimulateTick(0.5);

            stopwatch.Stop();
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
            Assert.That(_controller.ActiveProjectileCount, Is.LessThanOrEqualTo(1));
            Object.Destroy(horizontal);
            Object.Destroy(vertical);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LogicalPositionIsIndependentOfViewportShape()
        {
            var logical = new SimVector2(1234.0, 321.0);
            var before = LogicalSpace.ToWorld(logical);
            var wideViewport = LogicalCameraFitter.CalculateViewport(2560, 1080);
            var tallViewport = LogicalCameraFitter.CalculateViewport(900, 1600);
            var after = LogicalSpace.ToWorld(logical);

            Assert.That(wideViewport.width, Is.LessThan(1f));
            Assert.That(tallViewport.height, Is.LessThan(1f));
            Assert.That(after, Is.EqualTo(before));
            yield return null;
        }

        [UnityTest]
        public IEnumerator UpgradePanelPausesSimulation()
        {
            var projectileId = _controller.SpawnProjectileForTest(
                new SimVector2(400.0, 700.0),
                new SimVector2(900.0, 0.0),
                0);
            var state = _controller.GetProjectileStateForTest(projectileId);
            var initialPosition = state.Position;
            var initialTime = _controller.SimulatedTimeSeconds;
            _controller.SetUpgradePanelOpen(true);

            _controller.SimulateTick(1.0);

            Assert.That(state.Position, Is.EqualTo(initialPosition));
            Assert.That(_controller.SimulatedTimeSeconds, Is.EqualTo(initialTime));
            yield return null;
        }

        private GameObject CreateWall(Vector2 position, Vector2 size, float rotationDegrees)
        {
            var wall = new GameObject("Test Reflection Wall");
            wall.transform.SetParent(_root.transform);
            wall.transform.position = position;
            wall.transform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
            wall.AddComponent<ReflectionWallView>();
            return wall;
        }
    }
}
