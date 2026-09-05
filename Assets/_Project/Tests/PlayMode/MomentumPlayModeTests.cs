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
        [UnityTest] public IEnumerator ControllerFiresThreeAndEditingBlocksCloseClick()
        {
            var root = new GameObject("Momentum fixture"); root.SetActive(false);
            var cameraObj = new GameObject("Camera"); cameraObj.transform.SetParent(root.transform);
            var camera = cameraObj.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.5f;
            var controller = root.AddComponent<MomentumLabController>(); controller.PersistenceEnabled = false;
            try
            {
                root.SetActive(true); yield return null; yield return null;
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
