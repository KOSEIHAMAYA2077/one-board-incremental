using System.Collections;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class MomentumArsenalPlayTests
    {
        [UnityTest] public IEnumerator ControlsMenusThemesAndCanViewsAreIsolatedFromSaves()
        {
            var root=new GameObject("Arsenal control fixture");root.SetActive(false);
            var camera=new GameObject("Camera");camera.transform.SetParent(root.transform);camera.AddComponent<Camera>().orthographic=true;
            root.AddComponent<PrototypeAudio>();
            var c=root.AddComponent<MomentumLabController>();c.PersistenceEnabled=false;
            try
            {
                root.SetActive(true);yield return null;yield return null;c.enabled=false;
                var sim=c.Simulation;foreach(var t in sim.Targets) t.Hp=1000000;
                Assert.That(c.NeonView.PickupViewCount,Is.EqualTo(2));
                Assert.That(c.NeonView.GetComponentsInChildren<Collider>().Length,Is.Zero);
                Assert.That(c.CycleWeapon(),Is.True);Assert.That(sim.Progress.gun,Is.EqualTo(2),"Locked UZI is skipped");
                c.CycleWeapon();Assert.That(sim.Progress.gun,Is.EqualTo(3));c.CycleWeapon();Assert.That(sim.Progress.gun,Is.Zero);
                Assert.That(c.MouseControl(new SimVector2(800,200)),Is.True);Assert.That(c.KeyboardControl,Is.False);
                Assert.That(c.KeyboardInput(1,.1,true),Is.True);Assert.That(c.KeyboardControl,Is.True);
                Assert.That(sim.BurstAim.X,Is.GreaterThan(.2));
                var first=sim.Balls[0];var velocity=first.Velocity;
                c.KeyboardInput(-1,.1,false);Assert.That(sim.BurstAim.X,Is.EqualTo(0).Within(.0001));Assert.That(first.Velocity,Is.EqualTo(velocity));
                Assert.That(c.SelectWeapon(3),Is.True);Assert.That(sim.FiringGun,Is.Zero);
                c.OpenMenu(1);var frozen=sim.Time;var count=sim.FiredCount;
                c.StepSimulation(.05);Assert.That(sim.Time,Is.EqualTo(frozen));
                Assert.That(c.KeyboardInput(1,.1,true),Is.False);Assert.That(c.CycleWeapon(),Is.False);
                Assert.That(c.MouseControl(new SimVector2(940,200),true),Is.False);Assert.That(sim.FiredCount,Is.EqualTo(count));
                c.ConfirmQuit();Assert.That(c.QuitRequested,Is.False);
                c.OpenMenu(2);c.SetSeVolume(.3f);Assert.That(root.GetComponent<AudioSource>().volume,Is.EqualTo(.3f));
                c.SetSeVolume(-1);Assert.That(c.SeVolume,Is.Zero);c.SetSeVolume(2);Assert.That(c.SeVolume,Is.EqualTo(1));
                for(var i=0;i<3;i++) { c.SetTheme(i);Assert.That(c.NeonEnabled,Is.EqualTo(i!=2));Assert.That(sim.Time,Is.EqualTo(frozen)); }
                c.CloseMenu();Assert.That(sim.Editing,Is.False);Assert.That(c.KeyboardInput(0,0,true),Is.False,"Close guard");
                yield return null;yield return null;
                c.MouseControl(new SimVector2(1000,300));Assert.That(c.KeyboardControl,Is.False);
                Assert.That(c.MouseControl(new SimVector2(200,300),true),Is.False);
                c.SetEditing(true);c.OpenMenu(1);c.CloseMenu();Assert.That(sim.Editing,Is.True,"Preserve an existing pause");
                c.SetEditing(false);c.OpenMenu(3);c.ConfirmQuit();Assert.That(c.QuitRequested,Is.True,"Editor diagnostic never exits the test host");
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [TestCase(2)] [TestCase(3)] public void NewGunAndPresetsRoundTripJson(int gun)
        {
            var p=new MomentumProgress();p.SelectGun(gun);p.StorePreset(1);
            var copy=JsonUtility.FromJson<MomentumProgress>(JsonUtility.ToJson(p));
            Assert.That(copy.Valid(),Is.True);Assert.That(copy.gun,Is.EqualTo(gun));
            copy.SelectGun(0);copy.LoadPreset(1);Assert.That(copy.gun,Is.EqualTo(gun));
        }
    }
}
