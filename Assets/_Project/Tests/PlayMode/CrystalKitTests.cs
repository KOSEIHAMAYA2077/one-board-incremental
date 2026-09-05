using System.Collections;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class CrystalKitTests
    {
        [UnityTest] public IEnumerator GalleryShardsAreBoundedAndExpire()
        {
            var root=new GameObject("Gallery fixture");root.SetActive(false);
            var camera=new GameObject("Camera");camera.transform.SetParent(root.transform);camera.AddComponent<Camera>().orthographic=true;
            var gallery=root.AddComponent<CrystalGalleryController>();
            try
            {
                root.SetActive(true);yield return null;
                gallery.PreviewShatter();Assert.That(gallery.ShardCount,Is.EqualTo(96));
                gallery.PreviewShatter();Assert.That(gallery.ShardCount,Is.EqualTo(96),"Repeated activation cannot grow without limit");
                yield return new WaitForSeconds(1.4f);Assert.That(gallery.ShardCount,Is.Zero);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [Test] public void PolyhedraHaveExpectedFacesFiniteGeometryAndOutwardNormals()
        {
            var faces=new[] {4,8,60,48,16,32,128,512};
            for(var kind=0;kind<8;kind++)
            {
                var mesh=CrystalMeshFactory.Polyhedron(kind);
                try
                {
                    Assert.That(mesh.triangles.Length/3,Is.EqualTo(faces[kind]));
                    var vertices=mesh.vertices;var indices=mesh.triangles;
                    foreach(var p in vertices) Assert.That(float.IsNaN(p.sqrMagnitude)||float.IsInfinity(p.sqrMagnitude),Is.False);
                    for(var i=0;i<indices.Length;i+=3)
                    {
                        var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];var normal=Vector3.Cross(b-a,c-a);
                        Assert.That(normal.sqrMagnitude,Is.GreaterThan(1e-10));Assert.That(Vector3.Dot(normal,a+b+c),Is.GreaterThan(0));
                    }
                    Assert.That(mesh.bounds.size.z,Is.GreaterThan(.8f));
                }
                finally { Object.DestroyImmediate(mesh); }
            }
        }
        [Test] public void PrefabsAreReusableAndStateDoesNotMutateSharedMaterials()
        {
            for(var kind=0;kind<CrystalKitFactory.Names.Length;kind++)
            {
                var prefab=Resources.Load<GameObject>("CrystalKitV1/Prefabs/"+CrystalKitFactory.Names[kind]);Assert.That(prefab,Is.Not.Null);
                var first=Object.Instantiate(prefab);var second=Object.Instantiate(prefab);
                try
                {
                    Assert.That(first.GetComponentsInChildren<Collider>().Length,Is.Zero);
                    var a=first.GetComponent<CrystalAppearance>();var b=second.GetComponent<CrystalAppearance>();
                    var material=a.Shells[0].sharedMaterial;var initial=material.GetColor("_Tint");
                    a.Apply(.2f,CrystalStatus.Golden,0,.4f);b.Apply(1,CrystalStatus.None);
                    Assert.That(a.Health,Is.EqualTo(.2f));Assert.That(a.ShellColor,Is.Not.EqualTo(b.ShellColor));
                    Assert.That(a.CoreColor,Is.Not.EqualTo(b.CoreColor));Assert.That(material.GetColor("_Tint"),Is.EqualTo(initial));
                    if(a.StatusRing!=null) Assert.That(a.StatusRing.activeSelf,Is.True);
                    a.Apply(1,CrystalStatus.None);if(a.StatusRing!=null) Assert.That(a.StatusRing.activeSelf,Is.False);
                    foreach(var r in first.GetComponentsInChildren<MeshRenderer>(true)) Assert.That(r.sharedMaterial.shader,Is.Not.Null);
                }
                finally { Object.DestroyImmediate(first);Object.DestroyImmediate(second); }
            }
        }
        [UnityTest] public IEnumerator CrystalLayerAnimatesWithoutChangingSimulationAndRestoresAfterPause()
        {
            var root=new GameObject("Crystal integration fixture");root.SetActive(false);
            var camera=new GameObject("Camera");camera.transform.SetParent(root.transform);camera.AddComponent<Camera>().orthographic=true;
            var controller=root.AddComponent<MomentumLabController>();controller.PersistenceEnabled=false;
            try
            {
                root.SetActive(true);yield return null;yield return null;
                var sim=controller.Simulation;var view=controller.NeonView;
                Assert.That(view.CrystalKitEnabled,Is.True);Assert.That(view.CrystalKitTargetCount,Is.EqualTo(12));
                var target=sim.Targets[0];var position=target.Position;var hp=target.Hp;var time=sim.Time;var gold=sim.Gold;
                view.SetCrystalKitEnabled(false);view.SetCrystalKitEnabled(true);
                Assert.That(sim.Time,Is.EqualTo(time));Assert.That(sim.Gold,Is.EqualTo(gold));Assert.That(target.Hp,Is.EqualTo(hp));
                var app=view.CrystalTarget(target.Id);var rotation=app.transform.localRotation;
                controller.StepSimulation(.05);
                Assert.That(app.transform.localRotation,Is.Not.EqualTo(rotation));Assert.That(target.Position.X,Is.EqualTo(position.X));Assert.That(target.Position.Y,Is.EqualTo(position.Y));
                target.Hp=target.MaximumHp*.3;target.GoldenMarked=true;view.Sync(sim);
                Assert.That(app.Health,Is.EqualTo(.3f).Within(.001));Assert.That(app.Status,Is.EqualTo(CrystalStatus.Golden));
                controller.SetEditing(true);rotation=app.transform.localRotation;controller.StepSimulation(.05);
                Assert.That(app.transform.localRotation,Is.EqualTo(rotation));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
