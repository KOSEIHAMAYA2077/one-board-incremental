using System.Collections;
using System.IO;
using System.Text;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumLabController
    {
        private IEnumerator CaptureArsenal(string folder)
        {
            _stressDiagnostic=true;
            var report=new StringBuilder();var ok=true;
            foreach(var gun in new[]{2,3})
            {
                Simulation.Recall();Simulation.StartChallenge(0);Simulation.Progress.equippedMods=6;
                _blockedFrame=-1;ok &= SelectWeapon(gun);ok &= KeyboardInput(0,0,true);
                var expected=gun==2?8:1;ok &= Simulation.Balls.Count==expected;
                report.AppendLine($"gun={gun}; immediateBalls={Simulation.Balls.Count}; expected={expected}; keyboard={KeyboardControl}");
                for(var i=0;i<8;i++) { StepSimulation(1.0/60);yield return null; }
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"arsenal-{gun}.png"));
                report.AppendLine("overflow="+string.Join(" | ",_overflows));ok &= _overflows.Count==0;
                yield return new WaitForSecondsRealtime(.1f);
            }
            Simulation.Recall();Simulation.StartChallenge(0);SyncViews();
            foreach(var page in new[]{1,2,3})
            {
                OpenMenu(page);var time=Simulation.Time;StepSimulation(1.0/60);ok &= time==Simulation.Time;
                yield return null;yield return null;
                yield return new WaitForEndOfFrame();
                var texture=ScreenCapture.CaptureScreenshotAsTexture();
                try
                {
                    var panelVisible=true;
                    foreach(var point in new[]{new Vector2Int(480,140),new Vector2Int(1440,140),new Vector2Int(480,940),new Vector2Int(1440,940)})
                    {
                        var pixel=texture.GetPixel(point.x,texture.height-point.y);
                        panelVisible &= pixel.b>.06f && pixel.b>pixel.r*1.5f;
                    }
                    ok &= panelVisible;report.AppendLine($"menu={page}; panelPixelsVisible={panelVisible}");
                    File.WriteAllBytes(Path.Combine(folder,$"menu-{page}.png"),texture.EncodeToPNG());
                }
                finally { Destroy(texture); }
                report.AppendLine($"menu={page}; paused={time==Simulation.Time}; overflow="+string.Join(" | ",_overflows));ok &= _overflows.Count==0;
                yield return new WaitForSecondsRealtime(.1f);CloseMenu();
            }
            report.AppendLine($"saveDisabled={!PersistenceEnabled}; result={ok}");
            File.WriteAllText(Path.Combine(folder,"arsenal-smoke.txt"),report.ToString());
            yield return new WaitForSecondsRealtime(.3f);Application.Quit(ok?0:1);
        }
    }
}
