using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    // Presentation only: no colliders, simulation writes, or gameplay RNG calls.
    public sealed partial class MomentumNeonView : MonoBehaviour
    {
        private readonly Dictionary<int, Crystal> _targets = new Dictionary<int, Crystal>();
        private readonly Dictionary<int, Flight> _flights = new Dictionary<int, Flight>();
        private readonly List<Spark> _sparks = new List<Spark>();
        private readonly List<TailGlow> _tailGlows = new List<TailGlow>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private Material _crystalMaterial, _glowMaterial;
        private Mesh _crystal, _diamond, _quad;
        private Transform _zone, _gun;
        private MeshRenderer _zoneAura;
        private double _lastTime = -1;
        private MaterialPropertyBlock _block;
        private readonly System.Random _visualRandom = new System.Random(85137);
        private static readonly Color Cyan = new Color(.08f, .8f, 1f);
        private static readonly Color Green = new Color(.3f, 1f, .23f);
        private static readonly Color Orange = new Color(1f, .27f, .055f);
        public int TargetCount => _targets.Count;
        public int FlightCount => _flights.Count;
        public int SparkCount => _sparks.Count;
        public int TailGlowCount => _tailGlows.Count;
        public float CrystalDepth => _crystal.bounds.size.z;

        public void Initialize(MomentumSimulation sim)
        {
            var layout = sim.Layout;
            var left=(float)layout.Left; var right=(float)layout.Right;
            var top=(float)layout.Top; var bottom=(float)layout.Bottom;
            _block = new MaterialPropertyBlock();
            _crystalMaterial = new Material(Resources.Load<Shader>("MomentumCrystal"));
            _glowMaterial = new Material(Resources.Load<Shader>("MomentumGlow"));
            _crystal = MakeCrystal(8); _diamond = MakeCrystal(4);
            _quad = new Mesh { name = "Neon glow quad" };
            _quad.vertices = new[] { new Vector3(-1,-1), new Vector3(1,-1), new Vector3(1,1), new Vector3(-1,1) };
            _quad.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            _quad.triangles = new[] { 0,1,2,0,2,3 }; _quad.RecalculateBounds(); _meshes.Add(_quad);
            RouteDrawing.Shape(transform, "Neon black floor", Vector2.zero, new Vector2(16,9), new Color(.009f,.014f,.026f), false, -20);
            RouteDrawing.Shape(transform, "Inset arena", World((left+right)/2,(top+bottom)/2), new Vector2((right-left)/100,(bottom-top)/100), new Color(.075f,.14f,.19f), false, -15);
            // The sparse grid is deliberately dim so the moving shots remain the focus.
            for (var x = left+40; x < right; x += 80) Line(transform, "Floor grid", new[] { World(x,top), World(x,bottom) }, new Color(.06f,.16f,.22f,.22f), .006f, -10);
            for (var y = top+40; y < bottom; y += 80) Line(transform, "Floor grid", new[] { World(left,y), World(right,y) }, new Color(.06f,.16f,.22f,.22f), .006f, -10);
            GlowLine(transform, "Neon wall", new[] { World(left,top), World(right,top), World(right,bottom), World(left,bottom) }, Orange, .025f, 0, true);
            foreach (var t in sim.Targets)
            {
                var root = Node("Crystal target " + t.Id, transform);
                var c = t.Armored ? new Color(1,.61f,.28f) : new Color(.4f,1,.83f);
                var radius = (float)t.Radius / 100;
                var aura = Glow(root, "Soft halo", Vector3.zero, radius * 2.1f, WithAlpha(c,.45f), 1);
                var shadow = RouteDrawing.Shape(root,"Crystal floor shadow",new Vector2(.055f,-.09f),new Vector2(radius*1.8f,radius*1.7f),new Color(.005f,.015f,.03f,.6f),true,0);
                var body = MeshObject(root, "Bevelled 3D crystal", _crystal, _crystalMaterial, 4);
                body.transform.localScale = Vector3.one * radius; Tint(body,c);
                var rim = Polygon(radius,8);
                GlowLine(root,"Crystal edges",rim,c,.016f,5,true);
                // This faint ring corresponds exactly to the unchanged circular hit area.
                Line(root,"Collision footprint",Polygon(radius,64),WithAlpha(c,.38f),.008f,3,true);
                if (t.Armored) GlowLine(root,"Armor inner band",Polygon(radius*.77f,8),new Color(1,.67f,.23f),.012f,6,true);
                _targets.Add(t.Id,new Crystal { Root=root, Body=body, Aura=aura, Color=c, Radius=radius });
            }
            foreach(var obstacle in sim.Obstacles)
            {
                var root=Node("Reflector bumper",transform); root.position=LogicalSpace.ToWorld(obstacle.Position);
                var c=new Color(.66f,.26f,1f);
                Glow(root,"Bumper halo",Vector3.zero,.62f,WithAlpha(c,.55f),1);
                var body=MeshObject(root,"Bumper crystal",_crystal,_crystalMaterial,3);
                body.transform.localScale=Vector3.one*.26f; Tint(body,c);
                GlowLine(root,"Bumper collision edge",Polygon(.26f,64),c,.024f,4,true);
            }
            _zone=Node("Moving neon accelerator",transform);
            _zoneAura=Glow(_zone,"Zone aura",Vector3.zero,.94f,WithAlpha(Cyan,.36f),1);
            GlowLine(_zone,"Exact zone boundary",Polygon(.65f,96),Cyan,.013f,2,true);
            GlowLine(_zone,"Inner orbit",Polygon(.51f,64),WithAlpha(Cyan,.55f),.009f,2,true);
            for(var i=0;i<8;i++)
            {
                var a=i*Mathf.PI/4; var p=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*.58f;
                var jewel=MeshObject(_zone,"Orbit crystal",_diamond,_crystalMaterial,3);
                jewel.transform.localPosition=p; jewel.transform.localScale=Vector3.one*.045f; Tint(jewel,Cyan);
            }
            var gunBase=Node("Crystal launcher",transform); gunBase.position=LogicalSpace.ToWorld(layout.Gun);
            Glow(gunBase,"Launcher glow",Vector3.zero,.55f,new Color(.8f,.2f,1f,.5f),3);
            var baseMesh=MeshObject(gunBase,"Launcher pedestal",_crystal,_crystalMaterial,4);
            baseMesh.transform.localScale=new Vector3(.3f,.21f,.4f); Tint(baseMesh,new Color(.65f,.25f,1));
            GlowLine(gunBase,"Launcher rim",Polygon(.27f,8),new Color(.8f,.38f,1),.017f,5,true);
            _gun=Node("Crystal barrel aim",gunBase);
            var barrel=MeshObject(_gun,"Crystal barrel",_diamond,_crystalMaterial,6);
            barrel.transform.localPosition=new Vector3(0,.17f,0); barrel.transform.localScale=new Vector3(.08f,.28f,.15f); Tint(barrel,new Color(.76f,.9f,1));
            InitializeCrystalKit(sim);
            Sync(sim);
        }

        public void SetAim(SimVector2 direction)
        { _gun.localRotation=Quaternion.Euler(0,0,(float)(-System.Math.Atan2(direction.Y,direction.X)*180/System.Math.PI-90)); if(_kitGun!=null) _kitGun.localRotation=_gun.localRotation; }

        public void Advance(MomentumSimulation sim, double delta)
        {
            foreach(var e in sim.Events)
            {
                if(e.Kind=="hit" && _targets.TryGetValue(e.Target,out var t)) { t.Flash=.25f; t.Kick=(float)(_visualRandom.NextDouble()*2-1); }
                var count=e.Kind=="destroy"?24:e.Kind=="boost"?6:e.Kind=="blast"?28:5;
                var c=e.Kind=="boost"?Cyan:e.Kind=="destroy"?Orange:Green;
                for(var i=0;i<count && _sparks.Count<192;i++)
                {
                    // Local deterministic geometry, independent of the game's RNG state.
                    var angle=(float)_visualRandom.NextDouble()*Mathf.PI*2;
                    var mesh=MeshObject(transform,"Transient crystal shard",_diamond,_crystalMaterial,8);
                    mesh.transform.position=LogicalSpace.ToWorld(e.Position);
                    var size=(e.Kind=="destroy"?.095f:.045f)*(.5f+(float)_visualRandom.NextDouble()); mesh.transform.localScale=Vector3.one*size;
                    _sparks.Add(new Spark { Mesh=mesh, Velocity=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),-.8f)*(e.Kind=="destroy"?1.8f:.8f), Life=.8f, Color=Color.Lerp(c,Color.white,.5f), Size=size });
                }
            }
            foreach(var t in _targets.Values) t.Flash=Mathf.Max(0,t.Flash-(float)delta);
            for(var i=_sparks.Count-1;i>=0;i--)
            {
                var s=_sparks[i]; s.Life-=(float)delta;
                if(s.Life<=0) { Destroy(s.Mesh.gameObject); _sparks.RemoveAt(i); continue; }
                s.Mesh.transform.position+=s.Velocity*(float)delta;
                s.Mesh.transform.Rotate(180*(float)delta,130*(float)delta,220*(float)delta);
                s.Mesh.transform.localScale=Vector3.one*s.Size*(s.Life/.8f);
                Tint(s.Mesh,WithAlpha(s.Color,s.Life/.8f));
            }
        }

        public void Sync(MomentumSimulation sim)
        {
            SyncPickupViews(sim);
            SyncCrystalKit(sim);
            foreach(var t in sim.Targets)
            {
                var view=_targets[t.Id]; view.Root.gameObject.SetActive(t.Alive);
                view.Root.position=LogicalSpace.ToWorld(t.Position);
                view.Root.localScale=Vector3.one*(float)(t.Radius/100)/view.Radius;
                view.Color=t.GoldenMarked?new Color(1,.9f,.2f):t.Armored?new Color(1,.61f,.28f):new Color(.4f,1,.83f);
                var flash=view.Flash/.25f;
                view.Body.transform.localRotation=Quaternion.Euler(flash*15,flash*view.Kick*20,flash*view.Kick*10);
                view.Body.transform.localPosition=new Vector3(view.Kick*flash*.025f,flash*.03f,-flash*.03f);
                Tint(view.Body,Color.Lerp(view.Color,Color.white,flash));
                Tint(view.Aura,WithAlpha(view.Color,.35f+flash*.5f));
            }
            _zone.position=LogicalSpace.ToWorld(sim.ZonePosition);
            Tint(_zoneAura,WithAlpha(Cyan,.34f+.06f*Mathf.Sin((float)sim.Time*3)));
            var alive=new HashSet<int>();
            foreach(var b in sim.Balls)
            {
                alive.Add(b.Id);
                var c=b.BountyCharged?new Color(.65f,1,.18f):b.Golden?new Color(1,1,.12f):b.Ammo==MomentumAmmo.Normal?new Color(1,.69f,.13f):new Color(.18f,.73f,1);
                if(!_flights.TryGetValue(b.Id,out var f))
                {
                    var root=Node("Neon projectile "+b.Id,transform);
                    var body=MeshObject(root,"Crystal projectile",_diamond,_crystalMaterial,9); body.transform.localScale=Vector3.one*.08f;
                    var aura=Glow(root,"Projectile aura",Vector3.zero,.31f,WithAlpha(c,.65f),8);
                    var trail=Line(transform,"Luminous wake",new Vector3[0],c,.035f,7);
                    var soft=Line(transform,"Outer wake",new Vector3[0],WithAlpha(c,.16f),.12f,6);
                    f=new Flight { Root=root,Body=body,Aura=aura,Trail=trail,Soft=soft }; _flights.Add(b.Id,f);
                }
                f.Root.position=LogicalSpace.ToWorld(b.Position);
                var tail = sim.Progress != null ? Mathf.Clamp01((float)((b.Speed-MomentumRules.StopSpeed)/(MomentumRules.TailSpeed-MomentumRules.StopSpeed))) : 1f;
                var strength = Mathf.SmoothStep(0,1,tail);
                var size = Mathf.Lerp(.35f,1f,strength);
                f.Tailing=tail<1; f.Color=c;
                f.Body.transform.localScale=Vector3.one*(float)(b.Radius/100)*size;
                f.Aura.transform.localScale=Vector3.one*.31f*size;
                f.Body.transform.localRotation=Quaternion.Euler(0,0,(float)(-System.Math.Atan2(b.Velocity.Y,b.Velocity.X)*180/System.Math.PI));
                var light=Mathf.Lerp(.15f,1f,strength);
                Tint(f.Body,Color.Lerp(c,Color.white,.65f)*Mathf.Lerp(.4f,1f,strength)); Tint(f.Aura,WithAlpha(c,(b.Boosted?.9f:.6f)*light));
                if(sim.Time!=_lastTime || f.Points.Count==0) f.Points.Enqueue(f.Root.position);
                while(f.Points.Count>Mathf.RoundToInt(Mathf.Lerp(2,18,strength))) f.Points.Dequeue();
                var points=f.Points.ToArray();
                UpdateTrail(f.Trail,points,WithAlpha(c,light),.045f*size); UpdateTrail(f.Soft,points,WithAlpha(c,(b.Boosted?.3f:.16f)*light),(b.Boosted?.19f:.13f)*size);
            }
            var remove=new List<int>();
            foreach(var pair in _flights) if(!alive.Contains(pair.Key))
            {
                var f=pair.Value;
                if(f.Tailing && sim.Time>_lastTime && _tailGlows.Count<64)
                    _tailGlows.Add(new TailGlow { Mesh=Glow(transform,"Tail afterglow",transform.InverseTransformPoint(f.Root.position),.12f,WithAlpha(f.Color,.4f),8), Color=f.Color, End=sim.Time+.12 });
                Destroy(f.Root.gameObject); Destroy(f.Trail.gameObject); Destroy(f.Soft.gameObject); remove.Add(pair.Key);
            }
            foreach(var id in remove) _flights.Remove(id);
            for(var i=_tailGlows.Count-1;i>=0;i--)
            {
                var g=_tailGlows[i]; var life=Mathf.Clamp01((float)((g.End-sim.Time)/.12));
                if(life<=0) { Destroy(g.Mesh.gameObject); _tailGlows.RemoveAt(i); continue; }
                g.Mesh.transform.localScale=Vector3.one*.12f*life; Tint(g.Mesh,WithAlpha(g.Color,.4f*life));
            }
            _lastTime=sim.Time;
        }

        private void UpdateTrail(LineRenderer line,Vector3[] points,Color color,float width)
        {
            line.positionCount=points.Length; line.SetPositions(points);
            line.startColor=WithAlpha(color,0); line.endColor=color; line.startWidth=.002f; line.endWidth=width;
        }
        private static Vector3 World(float x,float y) => LogicalSpace.ToWorld(new SimVector2(x,y));
        private static Color WithAlpha(Color c,float alpha) => new Color(c.r,c.g,c.b,alpha);
        private static Transform Node(string name,Transform parent) { var t=new GameObject(name).transform; t.SetParent(parent,false); return t; }
        private void Tint(Renderer renderer,Color c) { _block.Clear(); _block.SetColor("_Tint",c); renderer.SetPropertyBlock(_block); }
        private MeshRenderer MeshObject(Transform parent,string name,Mesh mesh,Material material,int order)
        {
            var t=Node(name,parent); t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=t.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial=material; renderer.sortingOrder=order;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows=false; return renderer;
        }
        private MeshRenderer Glow(Transform parent,string name,Vector3 position,float radius,Color c,int order)
        {
            var r=MeshObject(parent,name,_quad,_glowMaterial,order); r.transform.localPosition=position;
            r.transform.localScale=Vector3.one*radius; Tint(r,c); return r;
        }
        private static Vector3[] Polygon(float radius,int count)
        {
            var points=new Vector3[count];
            for(var i=0;i<count;i++) { var a=(i+.5f)*Mathf.PI*2/count; points[i]=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius; }
            return points;
        }
        private static LineRenderer Line(Transform parent,string name,Vector3[] points,Color c,float width,int order,bool loop=false)
        {
            var line=RouteDrawing.Line(parent,name,c,width,order); line.useWorldSpace=false;
            line.positionCount=points.Length; line.SetPositions(points); line.loop=loop; return line;
        }
        private static void GlowLine(Transform parent,string name,Vector3[] points,Color c,float width,int order,bool loop=false)
        {
            Line(parent,name+" diffuse",points,WithAlpha(c,c.a*.07f),width*9,order,loop);
            Line(parent,name+" halo",points,WithAlpha(c,c.a*.2f),width*3,order+1,loop);
            Line(parent,name,points,c,width,order+2,loop);
        }
        private Mesh MakeCrystal(int sides)
        {
            var v=new List<Vector3>(); var colors=new List<Color>(); var tri=new List<int>();
            var rim=Polygon(1,sides); var top=Polygon(.72f,sides);
            for(var i=0;i<sides;i++)
            {
                var j=(i+1)%sides; var shade=.38f+.25f*(.5f+.5f*Mathf.Cos(i*Mathf.PI*2/sides-2));
                Triangle(v,colors,tri, new Vector3(0,0,-.75f),top[i]+Vector3.back*.52f,top[j]+Vector3.back*.52f,new Color(.62f+shade*.4f,.62f+shade*.4f,.62f+shade*.4f));
                var light=.48f+.48f*(.5f+.5f*Mathf.Cos(i*Mathf.PI*2/sides-2));
                var c=new Color(light,light,light);
                Triangle(v,colors,tri,top[i]+Vector3.back*.52f,rim[i],rim[j],c);
                Triangle(v,colors,tri,top[i]+Vector3.back*.52f,rim[j],top[j]+Vector3.back*.52f,c);
            }
            // Tilt geometry, not the logical plane: pointer and circular footprint stay aligned.
            var tilt=Quaternion.Euler(22,0,0); for(var i=0;i<v.Count;i++) v[i]=tilt*v[i];
            var mesh=new Mesh { name="Near-top-down bevelled crystal" }; mesh.SetVertices(v); mesh.SetColors(colors); mesh.SetTriangles(tri,0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); _meshes.Add(mesh); return mesh;
        }
        private static void Triangle(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Vector3 d,Color color)
        { var n=v.Count; v.Add(a);v.Add(b);v.Add(d);c.Add(color);c.Add(color);c.Add(color);t.Add(n);t.Add(n+1);t.Add(n+2); }
        private void OnDestroy() { foreach(var mesh in _meshes) Destroy(mesh); if(_crystalMaterial!=null) Destroy(_crystalMaterial); if(_glowMaterial!=null) Destroy(_glowMaterial); }
        private sealed class Crystal { public Transform Root; public MeshRenderer Body,Aura; public Color Color; public float Flash,Kick,Radius; }
        private sealed class TailGlow { public MeshRenderer Mesh; public Color Color; public double End; }
        private sealed class Flight { public Transform Root; public MeshRenderer Body,Aura; public LineRenderer Trail,Soft; public bool Tailing; public Color Color; public readonly Queue<Vector3> Points=new Queue<Vector3>(); }
        private sealed class Spark { public MeshRenderer Mesh; public Vector3 Velocity; public float Life,Size; public Color Color; }
    }
}
