using System;
using System.IO;
using IncrementalGame.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IncrementalGame.Editor
{
    public static class RecipeLabSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/RecipeLab.unity";

        [MenuItem("Incremental Game/Generate Recipe Lab Scene")]
        public static void Generate()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Recipe Lab Prototype");
            var cameraObject = new GameObject("Main Camera"); cameraObject.transform.SetParent(root.transform);
            cameraObject.tag = "MainCamera"; cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.025f, 0.04f, 0.06f);
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100;
            cameraObject.AddComponent<LogicalCameraFitter>(); cameraObject.AddComponent<AudioListener>();
            root.AddComponent<AudioSource>(); root.AddComponent<PrototypeAudio>(); root.AddComponent<FreePlacementController>().RecipeMode = true;
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Incremental Game/Build/Recipe Lab Windows")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath)) Generate();
            var output = Environment.GetEnvironmentVariable("RECIPE_BUILD_PATH");
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("RECIPE_BUILD_PATH is required.");
            var oldName = PlayerSettings.productName;
            var oldVersion = PlayerSettings.bundleVersion;
            var oldCompany = PlayerSettings.companyName;
            try
            {
                PlayerSettings.productName = "One Board Recipe Lab";
                PlayerSettings.companyName = "KOSEI HAMAYA";
                PlayerSettings.bundleVersion = FreePlacementController.RecipeVersion;
                PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
                Prototype0Build.WriteBuildMetadata();
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath }, target = BuildTarget.StandaloneWindows64,
                    locationPathName = output, options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException($"Placement build failed: {report.summary.result}");
                Debug.Log($"[Placement] Windows build succeeded: {output}");
            }
            finally
            {
                PlayerSettings.productName = oldName; PlayerSettings.bundleVersion = oldVersion;
                PlayerSettings.companyName = oldCompany; AssetDatabase.SaveAssets();
            }
        }
    }
}
