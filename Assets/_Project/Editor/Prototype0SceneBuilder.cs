using System.IO;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IncrementalGame.Editor
{
    public static class Prototype0SceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Prototype0.unity";

        [MenuItem("Incremental Game/Generate Prototype 0 Scene")]
        public static void GenerateScene()
        {
            Directory.CreateDirectory("Assets/_Project/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<LogicalCameraFitter>();

            CreateShape(
                "Board",
                Vector2.zero,
                new Vector2(16f, 9f),
                new Color(0.055f, 0.075f, 0.105f),
                -20);

            var gun = CreateShape(
                "Gun",
                LogicalSpace.ToWorld(new SimVector2(800.0, 820.0)),
                new Vector2(0.7f, 0.36f),
                new Color(0.95f, 0.55f, 0.18f),
                2);

            var collectorObject = CreateShape(
                "Collector",
                LogicalSpace.ToWorld(new SimVector2(800.0, 225.0)),
                new Vector2(1.35f, 0.72f),
                new Color(0.32f, 0.92f, 0.48f),
                1);
            var collectorCollider = collectorObject.AddComponent<BoxCollider2D>();
            collectorCollider.size = Vector2.one;
            var collector = collectorObject.AddComponent<CollectorTargetView>();

            var wallObject = CreateShape(
                "Reflection Wall",
                LogicalSpace.ToWorld(new SimVector2(460.0, 580.0)),
                new Vector2(0.18f, 2.65f),
                new Color(0.15f, 0.82f, 0.95f),
                1);
            wallObject.transform.rotation = Quaternion.Euler(0f, 0f, 5f);
            var wallCollider = wallObject.AddComponent<BoxCollider2D>();
            wallCollider.size = Vector2.one;
            wallObject.AddComponent<ReflectionWallView>();

            var aimObject = new GameObject("Aim Cone");
            var aimCone = aimObject.AddComponent<LineRenderer>();
            aimCone.positionCount = 3;
            aimCone.loop = false;
            aimCone.useWorldSpace = true;
            aimCone.startWidth = 0.018f;
            aimCone.endWidth = 0.018f;
            aimCone.startColor = new Color(1f, 0.74f, 0.22f, 0.52f);
            aimCone.endColor = new Color(1f, 0.74f, 0.22f, 0.08f);
            aimCone.material = new Material(Shader.Find("Sprites/Default"));
            aimCone.sortingOrder = 3;

            var controllerObject = new GameObject("Prototype 0 Controller");
            var audioSource = controllerObject.AddComponent<AudioSource>();
            audioSource.volume = 0.7f;
            var audio = controllerObject.AddComponent<PrototypeAudio>();
            var controller = controllerObject.AddComponent<Prototype0Controller>();
            controller.Configure(camera, collector, gun.transform, aimCone, audio, 20260828);

            Physics2D.gravity = Vector2.zero;
            Time.fixedDeltaTime = 1f / 60f;
            PlayerSettings.productName = "One Board Incremental Prototype 0";
            PlayerSettings.companyName = "KOSEI HAMAYA";
            PlayerSettings.bundleVersion = "0.1.0-prototype0";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Prototype0] Generated scene at {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            GenerateScene();
        }

        private static GameObject CreateShape(
            string objectName,
            Vector2 position,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            var gameObject = new GameObject(objectName);
            gameObject.transform.position = new Vector3(position.x, position.y, 0f);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            var shape = gameObject.AddComponent<PrototypeShapeView>();
            shape.Configure(color, size);
            return gameObject;
        }
    }
}
