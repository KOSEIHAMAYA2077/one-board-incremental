using System;
using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class MomentumAimBalanceTests
    {
        [TestCase(false,false,1)] [TestCase(false,true,1)]
        [TestCase(false,false,.25)] [TestCase(false,true,.25)]
        [TestCase(true,false,1)] [TestCase(true,true,1)]
        [TestCase(true,false,.25)] [TestCase(true,true,.25)]
        public void ImpactLossIsDoubledOnlyInChallengeAfterDamage(bool challenge,bool armored,double resistanceScale)
        {
            var sim=new MomentumSimulation(progress:challenge?new MomentumProgress():null);
            sim.Targets.Clear();sim.Obstacles.Clear();sim.Pickups.Clear();
            var target=new MomentumTarget { Id=99,Position=new SimVector2(800,300),Hp=1000,Armored=armored };sim.Targets.Add(target);
            var ball=new MomentumBall { Id=99,Position=new SimVector2(800,355),Velocity=new SimVector2(0,-900),Boosted=true,ResistanceScale=resistanceScale };
            sim.Balls.Add(ball);sim.Tick(1.0/60);
            var impact=challenge?897:898;
            var expected=Math.Max(0,impact-(armored?420:150)*resistanceScale*(challenge?2:1));
            Assert.That(ball.Speed,Is.EqualTo(expected).Within(1e-8));
            Assert.That(target.Hp,Is.EqualTo(1000-40*impact/900.0).Within(1e-8),"Damage uses pre-contact-loss velocity");
            Assert.That(ball.Alive,Is.EqualTo(expected>80));
        }
        // Reproducible observation, not a claim that this proves human aiming is fun.
        [Test] public void FixedSeedAimSweepReportsOneMagazineOutcomes()
        {
            foreach(var mods in new[]{6,14}) for(var gun=0;gun<4;gun++)
            {
                var cases=0;var killed=0;var clears=0;var peakSum=0;var seconds=0.0;var aimRange=0;
                for(uint seed=1;seed<=8;seed++)
                {
                    var low=12;var high=0;
                    foreach(var aim in new[]{new SimVector2(660,620),new SimVector2(800,200),new SimVector2(940,620)})
                    {
                        var p=new MomentumProgress { gun=gun,uziUnlocked=true,unlockedMods=15,equippedMods=mods };
                        var sim=new MomentumSimulation(seed,progress:p);Assert.That(sim.TryFire(aim),Is.True);
                        var peak=sim.Balls.Count;
                        for(var tick=0;tick<660 && (sim.Bursting || sim.Balls.Count>0);tick++)
                        { sim.Tick(1.0/60);peak=Math.Max(peak,sim.Balls.Count); }
                        Assert.That(sim.Balls,Is.Empty);Assert.That(peak,Is.LessThanOrEqualTo(256));
                        cases++;killed+=sim.DestroyedCount;peakSum+=peak;seconds+=sim.Time;
                        if(sim.RemainingTargets==0) clears++;
                        low=Math.Min(low,sim.DestroyedCount);high=Math.Max(high,sim.DestroyedCount);
                    }
                    aimRange+=high-low;
                }
                TestContext.WriteLine(FormattableString.Invariant($"BALANCE gun={gun} mods={mods} cases={cases} meanKills={(double)killed/cases:0.000} clears={clears} meanPeak={(double)peakSum/cases:0.000} meanSeconds={seconds/cases:0.000} meanAimKillRange={aimRange/8.0:0.000}"));
            }
        }
    }
}
