using System.Collections.Generic;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public static class CrystalKitFactory
    {
        public static readonly string[] Names={"Tetra","Octa","Urchin","Bumper","Launcher","Cabinet","Projectile","Hexadeca","Triaconta","FacetedSphere","SmoothSphere"};
        public static GameObject Spawn(int kind,Transform parent)
        {
            var prefab=Resources.Load<GameObject>("CrystalKitV1/Prefabs/"+Names[kind]);
            return prefab==null?null:Object.Instantiate(prefab,parent,false);
        }
        public static GameObject Build(int kind,CrystalKitLibrary lib)
        {
            var root=new GameObject("Lucent "+Names[kind]);
            var shells=new List<Renderer>();var wires=new List<Renderer>();var cores=new List<Renderer>();
            var app=root.AddComponent<CrystalAppearance>();
            app.BaseColor=kind==2?new Color(1,.64f,.27f):kind==3?new Color(.65f,.47f,1):new Color(.19f,.86f,.9f);
            if(kind<3 || kind>=6)
            {
                var shape=kind==6?1:kind>=7?kind-3:kind;
                Part(root.transform,"Glass shell",lib.Shapes[shape],lib.Glass,Vector3.zero,Vector3.one,shells);
                if(kind!=10) Part(root.transform,"Luminous ridges",lib.Wires[shape],lib.Light,Vector3.zero,Vector3.one,wires);
                Part(root.transform,"Suspended core",lib.Shapes[1],lib.Light,Vector3.zero,Vector3.one*.2f,cores);
                var orbit=Part(root.transform,"Status orbit",lib.Ring,lib.Light,Vector3.zero,Vector3.one*.62f,cores);
                orbit.transform.localRotation=Quaternion.Euler(40,20,0); app.StatusRing=orbit.gameObject;
            }
            else if(kind==3)
            {
                Part(root.transform,"Rebound crown",lib.Ring,lib.Glass,Vector3.zero,Vector3.one,shells);
                Part(root.transform,"Crown outer rail",lib.Ring,lib.Metal,new Vector3(0,0,.12f),new Vector3(1.08f,1.08f,.8f));
                Part(root.transform,"Rebound energy ring",lib.Ring,lib.Light,new Vector3(0,0,-.08f),new Vector3(.83f,.83f,.3f),wires);
                Part(root.transform,"Rebound heart",lib.Shapes[1],lib.Glass,new Vector3(0,0,-.12f),Vector3.one*.46f,shells);
                Part(root.transform,"Rebound core",lib.Shapes[1],lib.Light,new Vector3(0,0,-.1f),Vector3.one*.18f,cores);
                for(var i=0;i<6;i++) { var a=i*Mathf.PI/3; Part(root.transform,"Crown tooth",lib.Shapes[0],lib.Glass,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*.78f,Vector3.one*.18f,shells); }
            }
            else if(kind==4)
            {
                Part(root.transform,"Pivot housing",lib.Shapes[3],lib.Metal,new Vector3(0,-.27f,.2f),new Vector3(.58f,.5f,.45f));
                Part(root.transform,"Crystal magazine",lib.Shapes[3],lib.Glass,new Vector3(0,-.17f,0),new Vector3(.4f,.4f,.48f),shells);
                Part(root.transform,"Magazine rim",lib.Wires[3],lib.Light,new Vector3(0,-.17f,0),new Vector3(.4f,.4f,.48f),wires);
                for(var i=-1;i<=1;i+=2)
                {
                    Part(root.transform,"Barrel housing",lib.Shapes[3],lib.Metal,new Vector3(i*.2f,.45f,.07f),new Vector3(.12f,.73f,.27f));
                    Part(root.transform,"Crystal accelerator rail",lib.Shapes[3],lib.Glass,new Vector3(i*.14f,.48f,-.08f),new Vector3(.075f,.61f,.2f),shells);
                    Part(root.transform,"Rail filament",lib.Shapes[1],lib.Light,new Vector3(i*.14f,.48f,-.12f),new Vector3(.027f,.55f,.035f),cores);
                }
                Part(root.transform,"Core chamber",lib.Shapes[1],lib.Light,new Vector3(0,-.15f,-.2f),Vector3.one*.15f,cores);
                Part(root.transform,"Muzzle collar",lib.Shapes[3],lib.Metal,new Vector3(0,1.03f,0),new Vector3(.3f,.12f,.3f));
                Part(root.transform,"Muzzle aperture",lib.Shapes[1],lib.Light,new Vector3(0,1.06f,-.15f),new Vector3(.12f,.05f,.07f),cores);
            }
            else if(kind==5)
            {
                for(var side=-1;side<=1;side+=2)
                {
                    Part(root.transform,"Cabinet side spine",lib.Shapes[3],lib.Metal,new Vector3(side*3.07f,0,.18f),new Vector3(.18f,4.16f,.42f));
                    Part(root.transform,"Translucent rail",lib.Shapes[3],lib.Glass,new Vector3(side*3.03f,0,-.03f),new Vector3(.08f,3.92f,.24f),shells);
                    Part(root.transform,"Boundary filament",lib.Shapes[3],lib.Light,new Vector3(side*3,0,-.16f),new Vector3(.014f,4,.025f),wires);
                    Part(root.transform,"Cabinet end spine",lib.Shapes[3],lib.Metal,new Vector3(0,side*4.07f,.18f),new Vector3(3.1f,.18f,.42f));
                    Part(root.transform,"End glass",lib.Shapes[3],lib.Glass,new Vector3(0,side*4.03f,-.03f),new Vector3(2.86f,.08f,.24f),shells);
                    Part(root.transform,"End filament",lib.Shapes[3],lib.Light,new Vector3(0,side*4,-.16f),new Vector3(3,.014f,.025f),wires);
                    for(var end=-1;end<=1;end+=2)
                    {
                        Part(root.transform,"Corner crystal",lib.Shapes[1],lib.Glass,new Vector3(side*3.03f,end*4.03f,-.15f),new Vector3(.22f,.22f,.32f),shells);
                        Part(root.transform,"Corner diode",lib.Shapes[1],lib.Light,new Vector3(side*3.03f,end*4.03f,-.15f),Vector3.one*.07f,cores);
                    }
                    for(var i=-3;i<=3;i++) Part(root.transform,"Segment tick",lib.Shapes[1],lib.Light,new Vector3(side*3.08f,i,-.2f),new Vector3(.05f,.035f,.03f),cores);
                }
            }
            app.Shells=shells.ToArray();app.Edges=wires.ToArray();app.Cores=cores.ToArray(); app.Apply(1,CrystalStatus.None);
            return root;
        }
        private static MeshRenderer Part(Transform parent,string name,Mesh mesh,Material material,Vector3 position,Vector3 scale,List<Renderer> group=null)
        {
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;
            obj.AddComponent<MeshFilter>().sharedMesh=mesh; var r=obj.AddComponent<MeshRenderer>();r.sharedMaterial=material;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;group?.Add(r);return r;
        }
    }
}
