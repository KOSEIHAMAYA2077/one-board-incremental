using System.Collections.Generic;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    // Original, closed low-poly geometry. Unit radius, flat normals, no external models.
    public static class CrystalMeshFactory
    {
        public static Mesh Polyhedron(int kind)
        {
            var v=new List<Vector3>(); var t=new List<int>();
            if(kind==0)
            {
                var p=new[] { new Vector3(1,1,1).normalized,new Vector3(-1,-1,1).normalized,new Vector3(-1,1,-1).normalized,new Vector3(1,-1,-1).normalized };
                Face(v,t,p[0],p[1],p[2]); Face(v,t,p[0],p[3],p[1]); Face(v,t,p[0],p[2],p[3]); Face(v,t,p[1],p[3],p[2]);
            }
            else if(kind==1)
            {
                var ring=new[] { Vector3.right,Vector3.up,Vector3.left,Vector3.down };
                for(var i=0;i<4;i++) { Face(v,t,Vector3.forward,ring[i],ring[(i+1)%4]); Face(v,t,Vector3.back,ring[(i+1)%4],ring[i]); }
            }
            else if(kind==2)
            {
                var g=(1+Mathf.Sqrt(5))/2;
                var p=new[] { new Vector3(-1,g,0),new Vector3(1,g,0),new Vector3(-1,-g,0),new Vector3(1,-g,0),new Vector3(0,-1,g),new Vector3(0,1,g),new Vector3(0,-1,-g),new Vector3(0,1,-g),new Vector3(g,0,-1),new Vector3(g,0,1),new Vector3(-g,0,-1),new Vector3(-g,0,1) };
                var f=new[] {0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8, 3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1};
                for(var i=0;i<p.Length;i++) p[i]=p[i].normalized*.59f;
                for(var i=0;i<f.Length;i+=3)
                {
                    var a=p[f[i]]; var b=p[f[i+1]]; var c=p[f[i+2]]; var tip=(a+b+c).normalized;
                    Face(v,t,a,b,tip); Face(v,t,b,c,tip); Face(v,t,c,a,tip);
                }
            }
            else if(kind==4 || kind==5)
            {
                var n=kind==4?8:16;
                for(var i=0;i<n;i++)
                {
                    Face(v,t,Vector3.back,RingPoint(i,n,.8f,0),RingPoint(i+1,n,.8f,0));
                    Face(v,t,Vector3.forward,RingPoint(i+1,n,.8f,0),RingPoint(i,n,.8f,0));
                }
            }
            else if(kind==6 || kind==7)
            {
                var octa=Polyhedron(1);var points=octa.vertices;var indices=octa.triangles;
                for(var i=0;i<indices.Length;i+=3) Subdivide(v,t,points[indices[i]],points[indices[i+1]],points[indices[i+2]],kind==6?2:3);
                if(Application.isPlaying) Object.Destroy(octa); else Object.DestroyImmediate(octa);
            }
            else
            {
                const int n=8;
                for(var i=0;i<n;i++)
                {
                    var a=RingPoint(i,n,.8f,-.45f); var b=RingPoint(i+1,n,.8f,-.45f);
                    var c=RingPoint(i,n,1,0); var d=RingPoint(i+1,n,1,0);
                    var e=RingPoint(i,n,.8f,.45f); var f=RingPoint(i+1,n,.8f,.45f);
                    Face(v,t,Vector3.back*.45f,b,a); Face(v,t,a,b,d); Face(v,t,a,d,c);
                    Face(v,t,c,d,f); Face(v,t,c,f,e); Face(v,t,Vector3.forward*.45f,e,f);
                }
            }
            var result=Finish(v,t,"Lucent "+new[] {"Tetrahedron","Octahedron","Stellated Icosahedron","Bevel Prism","16-face Bipyramid","32-face Bipyramid","Faceted Sphere","Smooth Sphere"}[kind]);
            if(kind==7) { var normals=result.vertices;for(var i=0;i<normals.Length;i++) normals[i]=normals[i].normalized;result.normals=normals; }
            return result;
        }
        private static void Subdivide(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,int level)
        {
            if(level==0) { Face(v,t,a,b,c);return; }
            var ab=(a+b).normalized;var bc=(b+c).normalized;var ca=(c+a).normalized;
            Subdivide(v,t,a,ab,ca,level-1);Subdivide(v,t,ab,b,bc,level-1);Subdivide(v,t,ca,bc,c,level-1);Subdivide(v,t,ab,bc,ca,level-1);
        }
        public static Mesh Torus(float radius=.8f,float tube=.12f,int segments=32)
        {
            var v=new List<Vector3>(); var t=new List<int>();
            for(var i=0;i<segments;i++) for(var j=0;j<6;j++)
            {
                var a=TorusPoint(i,j,radius,tube,segments); var b=TorusPoint(i+1,j,radius,tube,segments);
                var c=TorusPoint(i+1,j+1,radius,tube,segments); var d=TorusPoint(i,j+1,radius,tube,segments);
                RawFace(v,t,a,b,c); RawFace(v,t,a,c,d);
            }
            return Finish(v,t,"Lucent Hex-section Ring");
        }
        private static Vector3 TorusPoint(int i,int j,float r,float tube,int n)
        { var a=i*Mathf.PI*2/n; var b=j*Mathf.PI/3; return new Vector3(Mathf.Cos(a)*(r+tube*Mathf.Cos(b)),Mathf.Sin(a)*(r+tube*Mathf.Cos(b)),tube*Mathf.Sin(b)); }
        private static Vector3 RingPoint(int i,int n,float r,float z) => new Vector3(Mathf.Cos(i*Mathf.PI*2/n)*r,Mathf.Sin(i*Mathf.PI*2/n)*r,z);
        public static Mesh Edges(Mesh source,float width=.008f)
        {
            var v=new List<Vector3>(); var t=new List<int>(); var p=source.vertices; var f=source.triangles;
            var edges=new Dictionary<string,Vector3[]>();
            for(var i=0;i<f.Length;i+=3) for(var j=0;j<3;j++)
            {
                var a=p[f[i+j]]; var b=p[f[i+(j+1)%3]];
                var sa=Key(a); var sb=Key(b); var key=string.CompareOrdinal(sa,sb)<0?sa+":"+sb:sb+":"+sa;
                if(!edges.ContainsKey(key)) edges.Add(key,new[] {a,b});
            }
            foreach(var edge in edges.Values) Tube(v,t,edge[0],edge[1],width);
            return Finish(v,t,source.name+" Edges");
        }
        private static string Key(Vector3 v) => Mathf.RoundToInt(v.x*100000)+","+Mathf.RoundToInt(v.y*100000)+","+Mathf.RoundToInt(v.z*100000);
        private static void Tube(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,float r)
        {
            var axis=(b-a).normalized; var u=Vector3.Cross(axis,Mathf.Abs(axis.y)<.9f?Vector3.up:Vector3.right).normalized*r; var w=Vector3.Cross(axis,u);
            for(var i=0;i<4;i++)
            {
                var n=u*Mathf.Cos(i*Mathf.PI/2)+w*Mathf.Sin(i*Mathf.PI/2); var m=u*Mathf.Cos((i+1)*Mathf.PI/2)+w*Mathf.Sin((i+1)*Mathf.PI/2);
                RawFace(v,t,a+n,a+m,b+m); RawFace(v,t,a+n,b+m,b+n);
            }
        }
        private static void Face(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c)
        { if(Vector3.Dot(Vector3.Cross(b-a,c-a),a+b+c)<0) { var swap=b; b=c; c=swap; } RawFace(v,t,a,b,c); }
        private static void RawFace(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c)
        { var n=v.Count; v.Add(a);v.Add(b);v.Add(c); t.Add(n);t.Add(n+1);t.Add(n+2); }
        private static Mesh Finish(List<Vector3> v,List<int> t,string name)
        { var mesh=new Mesh {name=name}; mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds(); return mesh; }
    }
}
