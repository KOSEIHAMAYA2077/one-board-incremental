using System;
using System.IO;
using IncrementalGame.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IncrementalGame.Editor
{
    public static class MomentumLabSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/MomentumLab.unity";

        [MenuItem("Incremental Game/Generate Momentum Lab Scene")]
        public static void Generate()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Momentum Lab Prototype");
            var cameraObject = new GameObject("Main Camera"); cameraObject.transform.SetParent(root.transform);
            cameraObject.tag = "MainCamera"; cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.025f, 0.04f, 0.06f);
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100;
            cameraObject.AddComponent<LogicalCameraFitter>(); cameraObject.AddComponent<AudioListener>();
            root.AddComponent<AudioSource>(); root.AddComponent<PrototypeAudio>(); root.AddComponent<MomentumLabController>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Incremental Game/Build/Momentum Lab Windows")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath)) Generate();
            var output = Environment.GetEnvironmentVariable("MOMENTUM_BUILD_PATH");
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("MOMENTUM_BUILD_PATH is required.");
            var oldName = PlayerSettings.productName;
            var oldVersion = PlayerSettings.bundleVersion;
            var oldCompany = PlayerSettings.companyName;
            var oldWidth = PlayerSettings.defaultScreenWidth; var oldHeight = PlayerSettings.defaultScreenHeight;
            var oldMode = PlayerSettings.fullScreenMode; var oldResizable = PlayerSettings.resizableWindow;
            var oldAutoApi=PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var oldApis=PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var oldGraphicsJobs=PlayerSettings.graphicsJobs;
            try
            {
                PlayerSettings.productName = "One Board Momentum Lab";
                PlayerSettings.companyName = "KOSEI HAMAYA";
                PlayerSettings.bundleVersion = MomentumLabController.GameVersion;
                PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
                // Jobified rendering intermittently loses IMGUI batches on the test PC.
                // Scope D3D11 without graphics jobs to this player, then restore the project.
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
                PlayerSettings.graphicsJobs=false;
                Prototype0Build.WriteBuildMetadata();
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath }, target = BuildTarget.StandaloneWindows64,
                    locationPathName = output, options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException($"Momentum build failed: {report.summary.result}");
                Debug.Log($"[Momentum] Windows build succeeded: {output}");
            }
            finally
            {
                PlayerSettings.productName = oldName; PlayerSettings.bundleVersion = oldVersion;
                PlayerSettings.companyName = oldCompany; AssetDatabase.SaveAssets();
                PlayerSettings.defaultScreenWidth = oldWidth; PlayerSettings.defaultScreenHeight = oldHeight;
                PlayerSettings.fullScreenMode = oldMode; PlayerSettings.resizableWindow = oldResizable;
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,oldApis);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,oldAutoApi);
                PlayerSettings.graphicsJobs=oldGraphicsJobs;
                AssetDatabase.SaveAssets();
            }
        }
    }
}
