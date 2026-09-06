using System;
using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class MomentumChallengeTests
    {
        [TestCase(0,6,.4)] [TestCase(1,18,.12)]
        public void SlowCadenceKeepsShotCountAndReloadStartsAfterLastShot(int gun,int count,double interval)
        {
            var p=Full();p.gun=gun;var sim=new MomentumSimulation(progress:p);
            Assert.That(sim.TryFire(new SimVector2(800,200)),Is.True);sim.Targets.Clear();sim.Obstacles.Clear();
            Assert.That(sim.FiredCount,Is.EqualTo(1));
            while(sim.Bursting)
            {
                sim.Tick(1.0/60);
                Assert.That(sim.FiredCount,Is.EqualTo(System.Math.Min(count,1+(int)System.Math.Floor((sim.Time+1e-9)/interval))));
                foreach(var b in sim.Balls) Assert.That(b.ExpiresAt,Is.EqualTo(10),"Later shots do not reset the magazine lifetime");
                Assert.That(sim.Time,Is.LessThan(3));
            }
            Assert.That(sim.Time,Is.InRange((count-1)*interval-1e-8,(count-1)*interval+1.0/60));
            Assert.That(sim.ReloadRemaining,Is.EqualTo(.8).Within(1e-8));
            Run(sim,47);Assert.That(sim.ReloadRemaining,Is.GreaterThan(0));
            Run(sim,1);Assert.That(sim.ReloadRemaining,Is.EqualTo(0).Within(1e-8));
            Assert.That(sim.FiredCount,Is.EqualTo(count));
        }
        [Test] public void SteeringOnlyChangesFutureShotsAndRejectsInvalidAim()
        {
            var sim=new MomentumSimulation(progress:Full());
            Assert.That(sim.UpdateBurstAim(new SimVector2(1000,400)),Is.False);
            sim.TryFire(new SimVector2(800,200));sim.Targets.Clear();sim.Obstacles.Clear();
            var first=sim.Balls[0];first.Boosted=true;var initialVelocity=first.Velocity;
            Assert.That(sim.UpdateBurstAim(new SimVector2(1050,815)),Is.True);
            Assert.That(first.Velocity,Is.EqualTo(initialVelocity),"Already flying balls are not steered");
            var aimed=sim.BurstAim;
            foreach(var invalid in new[]{new SimVector2(double.NaN,0),new SimVector2(0,double.PositiveInfinity),new SimVector2(200,400),sim.Layout.Gun})
            { Assert.That(sim.UpdateBurstAim(invalid),Is.False);Assert.That(sim.BurstAim,Is.EqualTo(aimed)); }
            sim.SetEditing(true);Assert.That(sim.UpdateBurstAim(new SimVector2(550,815)),Is.False);Run(sim,40);
            Assert.That(sim.FiredCount,Is.EqualTo(1));sim.SetEditing(false);
            Run(sim,23);Assert.That(sim.FiredCount,Is.EqualTo(1));Run(sim,1);
            var second=sim.Balls.Find(b=>b.Id==2);Assert.That(second.Velocity.Normalized.X,Is.GreaterThan(.99));
            Assert.That(first.Velocity.Normalized.Y,Is.LessThan(-.99));
            Assert.That(sim.UpdateBurstAim(new SimVector2(550,815)),Is.True);Run(sim,24);
            Assert.That(sim.Balls.Find(b=>b.Id==3).Velocity.Normalized.X,Is.LessThan(-.99));
            sim.Recall();Assert.That(sim.UpdateBurstAim(new SimVector2(800,200)),Is.False);
            var legacy=new MomentumSimulation();legacy.TryFire(new SimVector2(800,200));
            Assert.That(legacy.UpdateBurstAim(new SimVector2(1000,400)),Is.False,"Old lab keeps locked aim");
        }
        [Test] public void IdenticalTimedSteeringInputsRemainDeterministic()
        {
            var a=new MomentumSimulation(123,progress:Full());var b=new MomentumSimulation(123,progress:Full());
            a.TryFire(new SimVector2(800,200));b.TryFire(new SimVector2(800,200));
            for(var i=0;i<660;i++)
            {
                var aim=new SimVector2(i%48<24?550:1050,220);
                Assert.That(a.UpdateBurstAim(aim),Is.EqualTo(b.UpdateBurstAim(aim)));
                a.Tick(1.0/60);b.Tick(1.0/60);
                Assert.That(a.FiredCount,Is.EqualTo(b.FiredCount));Assert.That(a.Balls.Count,Is.EqualTo(b.Balls.Count));
                for(var n=0;n<a.Balls.Count;n++) { Assert.That(a.Balls[n].Position,Is.EqualTo(b.Balls[n].Position));Assert.That(a.Balls[n].Velocity,Is.EqualTo(b.Balls[n].Velocity)); }
            }
            Assert.That(a.Gold,Is.EqualTo(b.Gold));Assert.That(a.Balls,Is.Empty);
        }
        [TestCase(.05)] [TestCase(1.0/60)] [TestCase(.01)]
        public void QuickTailStopsWithinQuarterSecondWithoutLongSlide(double tick)
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress()); sim.Targets.Clear(); sim.Obstacles.Clear();
            var b=new MomentumBall { Id=100, Position=new SimVector2(800,400), Velocity=new SimVector2(300,0), Boosted=true, Generation=5 };
            sim.Balls.Add(b); var steps=(int)System.Math.Round(.25/tick);
            for(var i=0;i<steps-1;i++) sim.Tick(tick);
            Assert.That(b.Alive,Is.True);
            sim.Tick(tick);
            Assert.That(b.Alive,Is.False); Assert.That(sim.Balls,Is.Empty);
            Assert.That(b.Position.X-800,Is.InRange(30,50),"The old low-speed tail travelled about 348 logical pixels");
        }
        [Test] public void StrongerChallengeDragSplitsThresholdTimeAndPreservesLegacy()
        {
            Assert.That(MomentumRules.TimeSpeed(900,1.0/60,true),Is.EqualTo(897).Within(1e-8));
            Assert.That(MomentumRules.TimeSpeed(309,.05,true),Is.EqualTo(300).Within(1e-8));
            Assert.That(MomentumRules.TimeSpeed(301,1.0/60,true),Is.EqualTo(300-880.0/90).Within(1e-8));
            Assert.That(MomentumRules.TimeSpeed(900,1.0/60,false),Is.EqualTo(898).Within(1e-8));
            Assert.That(MomentumRules.TimeSpeed(300,.05,false),Is.EqualTo(294),"Legacy lab keeps its decay");
            var once=MomentumRules.TimeSpeed(301,.05,true);
            var split=301.0; for(var i=0;i<3;i++) split=MomentumRules.TimeSpeed(split,1.0/60,true);
            Assert.That(split,Is.EqualTo(once).Within(1e-8));
        }
        [Test] public void QuickTailFreezesDuringPauseAndStillDeliversLastImpact()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress()); sim.Targets.Clear(); sim.Obstacles.Clear();
            var target=new MomentumTarget { Id=100, Position=new SimVector2(800,300), Hp=90, Armored=true };sim.Targets.Add(target);
            var b=new MomentumBall { Id=100, Position=new SimVector2(800,350), Velocity=new SimVector2(0,-300), Boosted=true };
            sim.Balls.Add(b);sim.SetEditing(true);Run(sim,60);
            Assert.That(b.Speed,Is.EqualTo(300));Assert.That(b.Alive,Is.True);
            sim.SetEditing(false);sim.Tick(1.0/60);
            Assert.That(target.Hp,Is.LessThan(90));Assert.That(b.Alive,Is.False);
        }
        private static void Run(MomentumSimulation sim, int ticks = 660) { for(var i=0;i<ticks;i++) sim.Tick(1.0/60); }
        private static MomentumProgress Full(int mods=14) => new MomentumProgress { gold=1000, highestStage=2, masteryMask=7, unlockedMods=127, equippedMods=mods, uziUnlocked=true };
        [Test] public void ArenaHasTwelveSeparatedTargetsAcrossStagesAndSeeds()
        {
            for(uint seed=1;seed<=100;seed++)
            {
                var sim=new MomentumSimulation(seed, progress:Full());
                Assert.That(sim.Layout.Width/sim.Layout.Height,Is.EqualTo(.75));
                for(var stage=0;stage<3;stage++)
                {
                    Assert.That(sim.StartChallenge(stage),Is.True);
                    Assert.That(sim.RemainingTargets,Is.EqualTo(12));
                    foreach(var t in sim.Targets) Assert.That(sim.PositionAvailable(t.Position,t.Radius,t.Id),Is.True,$"seed {seed} stage {stage}");
                    foreach(var b in sim.Obstacles) Assert.That(b.Position.Y,Is.LessThan(150));
                }
            }
        }
        [Test] public void ReloadOverlapsFastFlightAndAllDescendantsExpireTogether()
        {
            var sim=new MomentumSimulation(progress:Full());
            foreach(var t in sim.Targets) t.Hp=1000000;
            Assert.That(sim.TryFire(sim.ZonePosition),Is.True);
            Run(sim,180); // New six-shot burst takes 2s, followed by 0.8s reload.
            sim.Balls.Add(new MomentumBall { Id=9999, Position=new SimVector2(800,780), Velocity=new SimVector2(0,-301), ExpiresAt=10, Generation=5 });
            Assert.That(sim.ReloadRemaining,Is.Zero);
            Assert.That(sim.Ready,Is.False);
            Assert.That(sim.TryFire(sim.ZonePosition),Is.False);
            Run(sim,520);
            Assert.That(sim.Balls,Is.Empty); Assert.That(sim.FiredCount,Is.EqualTo(6));
            Assert.That(sim.Ready,Is.True);
        }
        [Test] public void RefireUsesFastestLivingDescendantAndIncludesThreshold()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            sim.Balls.Add(new MomentumBall { Velocity=new SimVector2(300,0), Generation=5 });
            Assert.That(sim.Ready,Is.True);
            var fast=new MomentumBall { Velocity=new SimVector2(0,300.01), Generation=9 };
            sim.Balls.Add(fast);
            Assert.That(sim.Ready,Is.False,"One fast descendant blocks, even if the average is below 300");
            fast.Alive=false;
            Assert.That(sim.Ready,Is.True);
            sim.SetEditing(true); Assert.That(sim.Ready,Is.False);
        }
        [Test] public void SlowTailDoesNotSkipReloadOrQueueRejectedClicks()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            sim.TryFire(sim.ZonePosition); Run(sim,130);
            sim.Balls.Clear();
            sim.Balls.Add(new MomentumBall { Position=new SimVector2(800,780), Velocity=new SimVector2(0,-100), Boosted=true });
            Assert.That(sim.ReloadRemaining,Is.GreaterThan(0));
            Assert.That(sim.TryFire(sim.ZonePosition),Is.False);
            Run(sim,50);
            Assert.That(sim.Ready,Is.True); Assert.That(sim.FiredCount,Is.EqualTo(6));
        }
        [Test] public void NewMagazineKeepsOldTailAndIndependentExpiry()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            foreach(var t in sim.Targets) t.Hp=1000000;
            sim.TryFire(sim.ZonePosition); Run(sim,180); sim.Balls.Clear();
            var old=new MomentumBall { Id=999, MagazineId=1, Position=new SimVector2(800,780), Velocity=new SimVector2(0,-300), Boosted=true, ExpiresAt=10 };
            sim.Balls.Add(old);
            var newExpiry=sim.Time+10;
            Assert.That(sim.TryFire(sim.ZonePosition),Is.True);
            Assert.That(sim.Balls.Contains(old),Is.True); Assert.That(old.ExpiresAt,Is.EqualTo(10));
            Assert.That(sim.Balls.Find(b=>b.MagazineId==2).ExpiresAt,Is.EqualTo(newExpiry));
            Assert.That(sim.ChallengeMagazines,Is.EqualTo(2));
            Run(sim,660); Assert.That(sim.Balls,Is.Empty); Assert.That(sim.FiredCount,Is.EqualTo(12));
        }
        [TestCase(1022, 0, 1024)]
        [TestCase(1023, 1, 1023)]
        public void OverlappingMagazinesKeepIndependentSplitBudgets(int oldCount,int suppressed,int finalCount)
        {
            var sim=new MomentumSimulation(progress:Full());
            sim.TryFire(sim.ZonePosition); Run(sim,180); sim.Balls.Clear(); sim.Targets.Clear(); sim.Obstacles.Clear();
            sim.Targets.Add(new MomentumTarget { Id=1, Position=new SimVector2(800,300), Hp=1000000 });
            var old=new MomentumBall { Id=999, MagazineId=1, Position=new SimVector2(800,335), Velocity=new SimVector2(0,-300), Mods=MomentumMod.Split, ResistanceScale=.25, Boosted=true, ExpiresAt=10 };
            sim.Balls.Add(old);
            var counts=(System.Collections.Generic.Dictionary<int,int>)typeof(MomentumSimulation)
                .GetField("_spawnedByMagazine",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(sim);
            counts[1]=oldCount;
            Assert.That(sim.TryFire(sim.ZonePosition),Is.True);
            var before=sim.SuppressedSplits; sim.Tick(1.0/60);
            Assert.That(sim.SuppressedSplits-before,Is.EqualTo(suppressed));
            Assert.That(counts[1],Is.EqualTo(finalCount)); Assert.That(counts[2],Is.EqualTo(1));
            foreach(var b in sim.Balls) if(b.MagazineId==1) Assert.That(b.ExpiresAt,Is.EqualTo(10));
        }
        [TestCase(0, 250)] [TestCase(1, 238)]
        public void NextMagazineReservesRoomUnderGlobalCap(int gun,int tailCount)
        {
            var p=Full(); p.gun=gun;
            var sim=new MomentumSimulation(progress:p);
            for(var i=0;i<tailCount+1;i++) sim.Balls.Add(new MomentumBall { Id=1000+i, Position=new SimVector2(800,780), Velocity=new SimVector2(0,-100), Boosted=true });
            Assert.That(sim.Ready,Is.False);
            sim.Balls.RemoveAt(0); Assert.That(sim.Ready,Is.True);
            Assert.That(sim.TryFire(sim.ZonePosition),Is.True);
            for(var i=0;i<180;i++) { sim.Tick(1.0/60); Assert.That(sim.Balls.Count+sim.RemainingInBurst,Is.LessThanOrEqualTo(256)); }
        }
        [Test] public void ReacceleratedTailBlocksAgainAndAllDeadTargetsCannotConsumeMagazine()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            var ball=new MomentumBall { Position=sim.ZonePosition, Velocity=new SimVector2(0,-300) };
            sim.Balls.Add(ball); Assert.That(sim.Ready,Is.True);
            sim.Tick(1.0/60);
            Assert.That(ball.Speed,Is.GreaterThan(300)); Assert.That(sim.Ready,Is.False);
            ball.Velocity=new SimVector2(0,-100);
            foreach(var t in sim.Targets) t.Hp=0;
            Assert.That(sim.TryFire(sim.ZonePosition),Is.False); Assert.That(sim.ChallengeMagazines,Is.Zero);
        }
        [Test] public void RecallCancelsUnemittedShotsWithoutRefundOrFakeReward()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            sim.TryFire(sim.ZonePosition); Assert.That(sim.FiredCount,Is.EqualTo(1));
            Assert.That(sim.Recall(),Is.True); Run(sim,120);
            Assert.That(sim.FiredCount,Is.EqualTo(1)); Assert.That(sim.ChallengeMagazines,Is.EqualTo(1));
            Assert.That(sim.Gold,Is.Zero); Assert.That(sim.Balls,Is.Empty);
        }
        [Test] public void LastMagazineResolvesBeforeFailureAndRestartRestoresHp()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            sim.Targets[0].Hp=1;
            for(var i=0;i<3;i++)
            {
                Assert.That(sim.TryFire(new SimVector2(800,840)),Is.False,"Too close to gun");
                Assert.That(sim.TryFire(sim.ZonePosition),Is.True);
                Assert.That(sim.ChallengeState,Is.EqualTo(MomentumChallengeState.Active));
                sim.Recall(); Run(sim,60);
            }
            Assert.That(sim.ChallengeState,Is.EqualTo(MomentumChallengeState.Failed));
            Assert.That(sim.Ready,Is.False); Assert.That(sim.Targets[0].Hp,Is.EqualTo(1));
            Assert.That(sim.StartChallenge(0),Is.True);
            foreach(var t in sim.Targets) Assert.That(t.Hp,Is.EqualTo(t.MaximumHp));
        }
        [Test] public void MasteryIsOnceAndNotRequiredForNextStage()
        {
            var p=new MomentumProgress(); p.Complete(0,2);
            Assert.That(p.highestStage,Is.EqualTo(1)); Assert.That(p.masteryMask,Is.Zero);
            p.Complete(0,1); p.Complete(0,1);
            Assert.That(p.masteryMask,Is.EqualTo(1)); Assert.That(p.Owns(MomentumMod.Golden),Is.True); Assert.That(p.gold,Is.Zero);
            Assert.That(p.Valid(),Is.True);
        }
        [Test] public void CapacityPurchasesPresetsAndInvalidSaveAreChecked()
        {
            var p=Full(14); Assert.That(p.Valid(),Is.True);
            Assert.That(p.Toggle(MomentumMod.Blast),Is.True); Assert.That(MomentumProgress.Used(p.equippedMods),Is.EqualTo(20));
            Assert.That(p.Toggle(MomentumMod.Power),Is.False);
            p.SelectGun(1); p.StorePreset(1); p.SelectGun(0); p.Toggle(MomentumMod.Split);
            p.LoadPreset(1); Assert.That(p.gun,Is.EqualTo(1)); Assert.That(p.Has(MomentumMod.Split),Is.True);
            p.schemaVersion=99; Assert.That(p.Valid(),Is.False);
            Assert.Throws<ArgumentException>(()=>new MomentumSimulation(progress:p));
            p=new MomentumProgress(); Assert.That(p.BuyGun(),Is.False); Assert.That(p.BuyMagazine(),Is.False);
            p.gold=100; Assert.That(p.BuyMagazine(),Is.True); Assert.That(p.gold,Is.EqualTo(70)); Assert.That(p.MagazineLimit,Is.EqualTo(4));
        }
        [Test] public void ChallengeLimitIsSnapshotAndSurvivorsDoNotRefill()
        {
            var p=Full(); var sim=new MomentumSimulation(progress:p);
            p.BuyMagazine(); Assert.That(sim.ChallengeLimit,Is.EqualTo(3));
            sim.Targets[0].Hp=0; sim.Targets[1].Hp=1;
            sim.TryFire(sim.ZonePosition);
            Assert.That(sim.Targets[0].Alive,Is.False); Assert.That(sim.Targets[1].Hp,Is.EqualTo(1));
            sim.Recall(); sim.StartChallenge(0); Assert.That(sim.ChallengeLimit,Is.EqualTo(4));
        }
        [Test] public void SplitChildrenCanSplitAgainAndKeepExpiry()
        {
            var sim=new MomentumSimulation(progress:Full()); sim.Targets.Clear(); sim.Obstacles.Clear();
            sim.Targets.Add(new MomentumTarget { Id=1, Position=new SimVector2(800,300), Hp=1000 });
            var b=new MomentumBall { Id=100, Generation=3, Mods=MomentumMod.Split, Position=new SimVector2(800,345), Velocity=new SimVector2(0,-900), ExpiresAt=10 };
            sim.Balls.Add(b); sim.Tick(1.0/60);
            Assert.That(sim.Balls.Count,Is.EqualTo(2));
            foreach(var child in sim.Balls) { Assert.That(child.Generation,Is.EqualTo(4)); Assert.That(child.ExpiresAt,Is.EqualTo(10)); Assert.That(child.DamageScale,Is.EqualTo(.55)); Assert.That(child.Speed,Is.EqualTo(b.Speed).Within(.001)); }
            sim.Tick(1.0/60); Assert.That(sim.Balls.Count,Is.EqualTo(2),"Children must survive their next tick");
        }
        [Test] public void GoldenMarkPaysOnceAndBlastDoesNotRecursivelyTrigger()
        {
            var sim=new MomentumSimulation(progress:Full()); sim.Targets.Clear(); sim.Obstacles.Clear();
            var target=new MomentumTarget { Id=3, Position=new SimVector2(800,300), Hp=1 };
            sim.Targets.Add(target); sim.Targets.Add(new MomentumTarget { Id=4, Position=new SimVector2(880,300), Hp=1 });
            var b=new MomentumBall { Id=100, Golden=true, Mods=MomentumMod.Blast, Position=new SimVector2(800,345), Velocity=new SimVector2(0,-900) };
            b.HitTargets.Add(1); b.HitTargets.Add(2); sim.Balls.Add(b); sim.Tick(1.0/60);
            Assert.That(sim.DestroyedCount,Is.EqualTo(2)); Assert.That(sim.ChallengeGold,Is.EqualTo(12));
            Assert.That(sim.Events.FindAll(e=>e.Kind=="blast").Count,Is.EqualTo(1));
        }
        [Test] public void FullChainIsDeterministicBoundedAndFinishes()
        {
            var p=Full(14); p.gun=1;
            var sim=new MomentumSimulation(progress:p); sim.TryFire(sim.ZonePosition);
            for(var i=0;i<660;i++) { sim.Tick(1.0/60); Assert.That(sim.Balls.Count,Is.LessThanOrEqualTo(256)); }
            Assert.That(sim.Balls,Is.Empty); Assert.That(sim.FiredCount,Is.EqualTo(18));
            var other=new MomentumSimulation(progress:new MomentumProgress { unlockedMods=127, masteryMask=7, highestStage=2, equippedMods=14, gun=1, uziUnlocked=true, gold=1000 });
            other.TryFire(other.ZonePosition); Run(other);
            Assert.That(sim.Gold,Is.EqualTo(other.Gold)); Assert.That(sim.DestroyedCount,Is.EqualTo(other.DestroyedCount));
        }
        [Test] public void SafetyCapPreservesParentAndSharedExpiryStopsEvenFastChildren()
        {
            var sim=new MomentumSimulation(progress:Full()); sim.Targets.Clear(); sim.Obstacles.Clear();
            sim.Targets.Add(new MomentumTarget { Id=1, Position=new SimVector2(800,300), Hp=1000000 });
            for(var i=0;i<256;i++) sim.Balls.Add(new MomentumBall { Id=1000+i, Mods=MomentumMod.Split, Position=new SimVector2(800,345), Velocity=new SimVector2(0,-900), ExpiresAt=.03 });
            sim.Tick(1.0/60);
            Assert.That(sim.SuppressedSplits,Is.GreaterThan(0)); Assert.That(sim.Balls.Count,Is.EqualTo(256));
            foreach(var b in sim.Balls) Assert.That(b.Alive,Is.True);
            sim.Tick(1.0/60); Assert.That(sim.Balls,Is.Empty);
        }
        [Test] public void LastImpactCanClearAndFailureDoesNotTakeEarnedGold()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress());
            sim.TryFire(sim.ZonePosition); sim.Recall(); Run(sim,60);
            sim.TryFire(sim.ZonePosition); sim.Recall(); Run(sim,60);
            sim.TryFire(sim.ZonePosition); sim.Recall();
            Assert.That(sim.ChallengeState,Is.EqualTo(MomentumChallengeState.Failed));
            sim.StartChallenge(0);
            sim.TryFire(sim.ZonePosition); Run(sim,130);
            foreach(var t in sim.Targets) t.Hp=0;
            var last=sim.Targets[0]; last.Hp=1; last.Position=new SimVector2(800,300);
            sim.Balls.Clear(); sim.Balls.Add(new MomentumBall { Id=1000, Position=new SimVector2(800,335), Velocity=new SimVector2(0,-300), ExpiresAt=sim.Time+.05 });
            Run(sim,10);
            Assert.That(sim.ChallengeState,Is.EqualTo(MomentumChallengeState.Cleared));
            Assert.That(sim.Progress.masteryMask,Is.EqualTo(1)); Assert.That(sim.Progress.highestStage,Is.EqualTo(1));
            var gold=sim.Gold;
            sim.StartChallenge(1);
            for(var i=0;i<3;i++) { sim.TryFire(sim.ZonePosition); sim.Recall(); Run(sim,60); }
            Assert.That(sim.Gold,Is.EqualTo(gold));
        }
    }
}
