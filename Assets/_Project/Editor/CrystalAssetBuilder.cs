using System;
using System.IO;
using System.Globalization;
using System.Text;
using IncrementalGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IncrementalGame.Editor
{
    public static class CrystalAssetBuilder
    {
        public const string Root="Assets/Resources/CrystalKitV1";
        public const string Scene="Assets/_Project/Scenes/CrystalGallery.unity";
        [MenuItem("Incremental Game/Crystal Kit/Generate V1")]
        public static void Generate()
        {
            Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");
            Directory.CreateDirectory("ArtExports/CrystalKitV1"); AssetDatabase.Refresh();
            var lib=ScriptableObject.CreateInstance<CrystalKitLibrary>();lib.Shapes=new Mesh[8];lib.Wires=new Mesh[8];
            for(var i=0;i<8;i++)
            {
                lib.Shapes[i]=Save(CrystalMeshFactory.Polyhedron(i),Root+"/Meshes/Shape"+i+".asset");
                lib.Wires[i]=Save(CrystalMeshFactory.Edges(lib.Shapes[i],i>=6?.0035f:.006f),Root+"/Meshes/Edges"+i+".asset");
                ExportObj(lib.Shapes[i],"ArtExports/CrystalKitV1/Shape"+i+".obj");
            }
            lib.Ring=Save(CrystalMeshFactory.Torus(),Root+"/Meshes/Ring.asset");
            lib.Glass=Save(new Material(Shader.Find("Lucent/Crystal Glass")){name="Lucent optical shell"},Root+"/Materials/Glass.mat");
            lib.Metal=new Material(Shader.Find("Lucent/Facet Metal and Light")){name="Lucent titanium"};lib.Metal.SetColor("_Tint",new Color(.17f,.25f,.32f));lib.Metal.SetFloat("_Emission",.04f);lib.Metal=Save(lib.Metal,Root+"/Materials/Titanium.mat");
            lib.Light=new Material(Shader.Find("Lucent/Facet Metal and Light")){name="Lucent light filaments"};lib.Light.SetFloat("_Emission",.45f);lib.Light=Save(lib.Light,Root+"/Materials/Filament.mat");
            lib=Save(lib,Root+"/Library.asset");
            foreach(var name in CrystalKitFactory.Names)
            {
                var kind=Array.IndexOf(CrystalKitFactory.Names,name);var obj=CrystalKitFactory.Build(kind,lib);
                PrefabUtility.SaveAsPrefabAsset(obj,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(obj);
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("Lucent Gallery");var cameraObject=new GameObject("Gallery Camera");cameraObject.transform.SetParent(root.transform);
            cameraObject.transform.position=new Vector3(0,0,-16);var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4.5f;camera.backgroundColor=new Color(.033f,.064f,.084f);camera.clearFlags=CameraClearFlags.SolidColor;camera.nearClipPlane=.1f;camera.farClipPlane=80;
            cameraObject.AddComponent<LogicalCameraFitter>();root.AddComponent<CrystalGalleryController>();
            EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("[Lucent] 11 prefabs, 8 OBJ shapes and gallery saved.");
        }
        private static T Save<T>(T item,string path) where T:UnityEngine.Object
        {
            var existing=AssetDatabase.LoadAssetAtPath<T>(path);
            if(existing!=null) { EditorUtility.CopySerialized(item,existing);UnityEngine.Object.DestroyImmediate(item);EditorUtility.SetDirty(existing);return existing; }
            AssetDatabase.CreateAsset(item,path);return item;
        }
        private static void ExportObj(Mesh mesh,string path)
        {
            var b=new StringBuilder("# Original Lucent geometry; unit radius; flat normals\n");var c=CultureInfo.InvariantCulture;
            foreach(var p in mesh.vertices) b.AppendFormat(c,"v {0} {1} {2}\n",p.x,p.y,p.z);
            foreach(var n in mesh.normals) b.AppendFormat(c,"vn {0} {1} {2}\n",n.x,n.y,n.z);
            var t=mesh.triangles;for(var i=0;i<t.Length;i+=3) b.AppendFormat(c,"f {0}//{0} {1}//{1} {2}//{2}\n",t[i]+1,t[i+1]+1,t[i+2]+1);
            File.WriteAllText(path,b.ToString());
        }
        [MenuItem("Incremental Game/Crystal Kit/Build Gallery Windows")]
        public static void BuildWindows()
        {
            var path=Environment.GetEnvironmentVariable("CRYSTAL_BUILD_PATH");if(string.IsNullOrEmpty(path)) throw new InvalidOperationException("CRYSTAL_BUILD_PATH required");
            var product=PlayerSettings.productName;var company=PlayerSettings.companyName;var version=PlayerSettings.bundleVersion;
            var width=PlayerSettings.defaultScreenWidth;var height=PlayerSettings.defaultScreenHeight;var mode=PlayerSettings.fullScreenMode;
            try
            {
                PlayerSettings.productName="Lucent Crystal Gallery";PlayerSettings.companyName="KOSEI HAMAYA";PlayerSettings.bundleVersion="1.0.0-art";
                PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[] {Scene},target=BuildTarget.StandaloneWindows64,locationPathName=path});
                if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("Gallery build failed");
            }
            finally { PlayerSettings.productName=product;PlayerSettings.companyName=company;PlayerSettings.bundleVersion=version;PlayerSettings.defaultScreenWidth=width;PlayerSettings.defaultScreenHeight=height;PlayerSettings.fullScreenMode=mode;AssetDatabase.SaveAssets(); }
        }
        [MenuItem("Incremental Game/Crystal Kit/Export Reusable Package")]
        public static void ExportPackage()
        {
            var path=Environment.GetEnvironmentVariable("CRYSTAL_PACKAGE_PATH");if(string.IsNullOrEmpty(path)) throw new InvalidOperationException("CRYSTAL_PACKAGE_PATH required");
            AssetDatabase.ExportPackage(new[] {Root,"Assets/_Project/Presentation/CrystalAppearance.cs","Assets/_Project/Presentation/CrystalKitLibrary.cs","Assets/_Project/Presentation/CrystalKitFactory.cs","Assets/_Project/Presentation/CrystalMeshFactory.cs","Assets/Resources/LucentGlass.shader","Assets/Resources/LucentSolid.shader"},path,ExportPackageOptions.Recurse);
        }
    }
}
