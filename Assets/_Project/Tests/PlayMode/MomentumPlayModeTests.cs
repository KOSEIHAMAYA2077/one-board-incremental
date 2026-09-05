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
                Assert.That(controller.NeonView.TargetCount, Is.EqualTo(7));
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
                Assert.That(sim.FiredCount, Is.EqualTo(3));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest] public IEnumerator ControllerFiresThreeAndEditingBlocksCloseClick()
        {
            var root = new GameObject("Momentum fixture"); root.SetActive(false);
            var cameraObj = new GameObject("Camera"); cameraObj.transform.SetParent(root.transform);
            var camera = cameraObj.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
            var controller = root.AddComponent<MomentumLabController>(); controller.PersistenceEnabled = false;
            try
            {
                root.SetActive(true); yield return null; yield return null;
                Assert.That(controller.Simulation.Layout.Width / controller.Simulation.Layout.Height, Is.EqualTo(9.0 / 16));
                Assert.That(controller.TryFire(new SimVector2(200, 400)), Is.False, "Left telemetry must not shoot");
                Assert.That(controller.TryFire(new SimVector2(1250, 400)), Is.False, "Right loadout must not shoot");
                Assert.That(controller.TryFire(new SimVector2(940, 200)), Is.True);
                for (var i = 0; i < 20; i++) controller.StepSimulation(1.0 / 60);
                Assert.That(controller.Simulation.FiredCount, Is.EqualTo(3));
                controller.SetEditing(true); var time = controller.Simulation.Time;
                controller.StepSimulation(1.0 / 60); Assert.That(controller.Simulation.Time, Is.EqualTo(time));
                controller.SetEditing(false); Assert.That(controller.TryFire(new SimVector2(940, 200)), Is.False);
                Assert.That(root.GetComponentsInChildren<LineRenderer>().Length, Is.GreaterThan(8));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
