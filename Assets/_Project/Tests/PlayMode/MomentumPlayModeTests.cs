using System.Collections;
using IncrementalGame.Core;
using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class MomentumPlayModeTests
    {
        [UnityTest] public IEnumerator ControllerSteersFutureShotsButUiAndPauseCannotRedirect()
        {
            var root=new GameObject("Steering fixture");root.SetActive(false);
            var camera=new GameObject("Camera");camera.transform.SetParent(root.transform);camera.AddComponent<Camera>().orthographic=true;
            var controller=root.AddComponent<MomentumLabController>();controller.PersistenceEnabled=false;
            try
            {
                root.SetActive(true);yield return null;yield return null;
                controller.enabled=false; // Test explicit inputs, not the user's real cursor.
                var sim=controller.Simulation;
                Assert.That(controller.TryFire(new SimVector2(800,200)),Is.True);
                sim.Targets.Clear();sim.Obstacles.Clear();var first=sim.Balls[0];first.Boosted=true;
                var oldVelocity=first.Velocity;
                Assert.That(controller.SteerBurst(new SimVector2(1050,815)),Is.True);
                Assert.That(first.Velocity,Is.EqualTo(oldVelocity));var direction=sim.BurstAim;
                Assert.That(controller.SteerBurst(new SimVector2(1250,400)),Is.False);
                Assert.That(controller.SteerBurst(new SimVector2(200,400)),Is.False);
                Assert.That(sim.BurstAim,Is.EqualTo(direction));
                controller.SendMessage("OnApplicationFocus",false);
                Assert.That(controller.SteerBurst(new SimVector2(550,815)),Is.False);
                controller.SendMessage("OnApplicationFocus",true);
                yield return null;yield return null;
                controller.SetEditing(true);Assert.That(controller.SteerBurst(new SimVector2(550,815)),Is.False);
                controller.SetEditing(false);Assert.That(controller.SteerBurst(new SimVector2(550,815)),Is.False,"Pause-close guard");
                for(var i=0;i<24;i++) controller.StepSimulation(1.0/60);
                Assert.That(sim.FiredCount,Is.EqualTo(2));
                Assert.That(sim.Balls.Find(b=>b.Id==2).Velocity.Normalized.X,Is.GreaterThan(.99));
                yield return null;yield return null;
                Assert.That(controller.SteerBurst(new SimVector2(550,815)),Is.True);
                for(var i=0;i<24;i++) controller.StepSimulation(1.0/60);
                Assert.That(sim.Balls.Find(b=>b.Id==3).Velocity.Normalized.X,Is.LessThan(-.99));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest] public IEnumerator BrakingShrinksVisualsAndAfterglowNeverExtendsAttack()
        {
            var root=new GameObject("Brake view fixture");
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            var view=root.AddComponent<MomentumNeonView>();view.Initialize(sim);
            try
            {
                var b=new MomentumBall { Id=999, Position=new SimVector2(800,780), Velocity=new SimVector2(300,0), Boosted=true };
                sim.Balls.Add(b);view.Sync(sim);
                var body=root.transform.Find("Neon projectile 999/Crystal projectile");
                var before=body.localScale.x;
                for(var i=0;i<10;i++) { sim.Tick(1.0/60);view.Sync(sim); }
                Assert.That(body.localScale.x,Is.LessThan(before*.7f));
                var radius=b.Radius;Assert.That(radius,Is.EqualTo(MomentumRules.Radius),"Visual shrink does not change hit radius");
                sim.SetEditing(true);var frozen=body.localScale;
                sim.Tick(1.0/60);view.Sync(sim);Assert.That(body.localScale,Is.EqualTo(frozen));
                sim.SetEditing(false);
                for(var i=0;i<5;i++) { sim.Tick(1.0/60);view.Sync(sim); }
                Assert.That(sim.Balls,Is.Empty);Assert.That(view.FlightCount,Is.Zero);
                Assert.That(view.TailGlowCount,Is.EqualTo(1));Assert.That(sim.Ready,Is.True,"Afterglow is not an attack");
                sim.SetEditing(true);sim.Tick(.05);view.Sync(sim);Assert.That(view.TailGlowCount,Is.EqualTo(1));
                sim.SetEditing(false);
                for(var i=0;i<8;i++) { sim.Tick(1.0/60);view.Sync(sim); }
                Assert.That(view.TailGlowCount,Is.Zero);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest] public IEnumerator ClearPanelAdvancesRetriesAndProtectsFinalStageAndClickThrough()
        {
            var root=new GameObject("Clear panel fixture"); root.SetActive(false);
            var cameraObj=new GameObject("Camera"); cameraObj.transform.SetParent(root.transform);
            cameraObj.AddComponent<Camera>().orthographic=true;
            var controller=root.AddComponent<MomentumLabController>(); controller.PersistenceEnabled=false;
            try
            {
                root.SetActive(true); yield return null; yield return null;
                var sim=controller.Simulation;
                Assert.That(controller.ClearPanelVisible,Is.False);
                Assert.That(controller.ContinueFromClear(true),Is.False);
                for(var stage=0;stage<3;stage++)
                {
                    Assert.That(sim.Stage,Is.EqualTo(stage));
                    sim.TryFire(sim.ZonePosition);
                    foreach(var target in sim.Targets) target.Hp=0;
                    controller.RecallVolley();
                    Assert.That(controller.ClearPanelVisible,Is.True);
                    Assert.That(controller.CanAdvanceFromClear,Is.EqualTo(stage<2));
                    var gold=sim.Gold; var mastery=sim.Progress.masteryMask;
                    controller.SetEditing(true); Assert.That(controller.ClearPanelVisible,Is.False);
                    Assert.That(controller.ContinueFromClear(false),Is.False);
                    controller.SetEditing(false);
                    Assert.That(controller.ContinueFromClear(false),Is.True);
                    Assert.That(sim.Stage,Is.EqualTo(stage)); Assert.That(sim.RemainingTargets,Is.EqualTo(12));
                    Assert.That(sim.ChallengeMagazines,Is.Zero); Assert.That(sim.Gold,Is.EqualTo(gold));
                    Assert.That(sim.Progress.masteryMask,Is.EqualTo(mastery));
                    Assert.That(controller.ClearPanelVisible,Is.False);
                    Assert.That(controller.TryFire(new SimVector2(920,450)),Is.False,"Result click must not shoot into restarted stage");
                    sim.TryFire(sim.ZonePosition);
                    foreach(var target in sim.Targets) target.Hp=0;
                    controller.RecallVolley();
                    Assert.That(controller.ContinueFromClear(true),Is.EqualTo(stage<2));
                    if(stage<2)
                    {
                        Assert.That(sim.RemainingTargets,Is.EqualTo(12));
                        Assert.That(controller.ContinueFromClear(true),Is.False,"Double activation cannot skip a stage");
                        Assert.That(controller.TryFire(new SimVector2(700,450)),Is.False);
                    }
                    else { Assert.That(sim.Stage,Is.EqualTo(2)); Assert.That(controller.ClearPanelVisible,Is.True); }
                }
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [Test] public void ProgressJsonRoundTripsOwnershipAndPresets()
        {
            var p=new MomentumProgress { gold=150 };
            p.BuyGun(); p.SelectGun(1); p.BuyMod(MomentumMod.Split); p.Toggle(MomentumMod.Split);
            p.BuyMagazine(); p.Complete(0,1); p.StorePreset(2);
            var copy=JsonUtility.FromJson<MomentumProgress>(JsonUtility.ToJson(p));
            Assert.That(copy.Valid(),Is.True); Assert.That(copy.gold,Is.EqualTo(p.gold));
            Assert.That(copy.masteryMask,Is.EqualTo(1)); Assert.That(copy.MagazineLimit,Is.EqualTo(4));
            copy.SelectGun(0); copy.LoadPreset(2); Assert.That(copy.gun,Is.EqualTo(1)); Assert.That(copy.Has(MomentumMod.Split),Is.True);
        }
        [UnityTest] public IEnumerator NeonViewIsThreeDimensionalAndDoesNotChangeSimulation()
        {
            var root = new GameObject("Neon fixture"); root.SetActive(false);
            var cameraObj = new GameObject("Camera"); cameraObj.transform.SetParent(root.transform);
            var camera = cameraObj.AddComponent<Camera>(); camera.orthographic = true;
            var controller = root.AddComponent<MomentumLabController>(); controller.PersistenceEnabled = false;
            try
            {
                root.SetActive(true); yield return null; yield return null;
                controller.SetEditing(true);
                var sim = controller.Simulation; var time = sim.Time; var gold = sim.Gold;
                Assert.That(controller.NeonView.TargetCount, Is.EqualTo(12));
                Assert.That(controller.NeonView.CrystalDepth, Is.GreaterThan(.2f));
                Assert.That(controller.NeonView.GetComponentsInChildren<Collider>().Length, Is.Zero);
                Assert.That(controller.NeonView.GetComponentsInChildren<Collider2D>().Length, Is.Zero);
                foreach (var renderer in controller.NeonView.GetComponentsInChildren<MeshRenderer>())
                {
                    Assert.That(renderer.sharedMaterial.shader, Is.Not.Null);
                    if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                        Assert.That(renderer.sharedMaterial.shader.isSupported, Is.True);
                }
                controller.SetNeonEnabled(false); Assert.That(controller.NeonView.gameObject.activeSelf, Is.False);
                controller.SetNeonEnabled(true); Assert.That(controller.NeonView.gameObject.activeSelf, Is.True);
                Assert.That(sim.Time, Is.EqualTo(time)); Assert.That(sim.Gold, Is.EqualTo(gold));
                controller.SetEditing(false);
                for(var i=0;i<2;i++) yield return null;
                Assert.That(controller.TryFire(sim.Layout.ZoneAt(sim.Time + .2)), Is.True);
                for(var i=0;i<35;i++) controller.StepSimulation(1.0/60);
                Assert.That(controller.NeonView.FlightCount, Is.EqualTo(sim.Balls.Count));
                Assert.That(sim.BoostCount, Is.GreaterThan(0));
                for(var i=0;i<800;i++) controller.StepSimulation(1.0/60);
                Assert.That(controller.NeonView.FlightCount, Is.Zero);
                Assert.That(controller.NeonView.SparkCount, Is.Zero);
                Assert.That(sim.FiredCount, Is.EqualTo(6));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest] public IEnumerator ControllerFiresSixAndEditingBlocksCloseClick()
        {
            var root = new GameObject("Momentum fixture"); root.SetActive(false);
            var cameraObj = new GameObject("Camera"); cameraObj.transform.SetParent(root.transform);
            var camera = cameraObj.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
            var controller = root.AddComponent<MomentumLabController>(); controller.PersistenceEnabled = false;
            try
            {
                root.SetActive(true); yield return null; yield return null;
                // This fixture tests refire, not combat balance: keep an encounter
                // alive during the longer burst and use only explicit test input.
                controller.enabled=false;
                foreach(var target in controller.Simulation.Targets) target.Hp=1000000;
                Assert.That(controller.Simulation.Layout.Width / controller.Simulation.Layout.Height, Is.EqualTo(3.0 / 4));
                Assert.That(controller.TryFire(new SimVector2(200, 400)), Is.False, "Left telemetry must not shoot");
                Assert.That(controller.TryFire(new SimVector2(1250, 400)), Is.False, "Right loadout must not shoot");
                Assert.That(controller.TryFire(new SimVector2(940, 200)), Is.True);
                for (var i = 0; i < 130; i++) controller.StepSimulation(1.0 / 60);
                Assert.That(controller.Simulation.FiredCount, Is.EqualTo(6));
                controller.SetEditing(true); var time = controller.Simulation.Time;
                controller.StepSimulation(1.0 / 60); Assert.That(controller.Simulation.Time, Is.EqualTo(time));
                controller.SetEditing(false); Assert.That(controller.TryFire(new SimVector2(940, 200)), Is.False);
                Assert.That(root.GetComponentsInChildren<LineRenderer>().Length, Is.GreaterThan(8));
                for(var i=0;i<50;i++) controller.StepSimulation(1.0/60);
                // Let the existing pause-close click guard elapse in actual presentation frames.
                yield return null; yield return null;
                var sim=controller.Simulation;
                sim.Balls.Clear();
                var tail=new MomentumBall { Id=999, MagazineId=1, Position=new SimVector2(800,780), Velocity=new SimVector2(0,-300), ExpiresAt=10, Boosted=true };
                sim.Balls.Add(tail);
                Assert.That(controller.TryFire(new SimVector2(200,400)),Is.False,"UI still must not fire when READY with a tail");
                Assert.That(controller.TryFire(new SimVector2(940,200)),Is.True,$"ready={sim.Ready} state={sim.ChallengeState} targets={sim.RemainingTargets} time={sim.Time} reload={sim.ReloadRemaining} bursting={sim.Bursting}");
                Assert.That(sim.Balls.Contains(tail),Is.True); Assert.That(sim.ChallengeMagazines,Is.EqualTo(2));
                controller.StepSimulation(1.0/60);
                Assert.That(controller.NeonView.FlightCount,Is.EqualTo(sim.Balls.Count));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
