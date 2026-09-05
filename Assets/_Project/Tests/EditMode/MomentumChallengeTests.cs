using System;
using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class MomentumChallengeTests
    {
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
        [Test] public void ReloadOverlapsFlightAndAllDescendantsExpireTogether()
        {
            var sim=new MomentumSimulation(progress:Full());
            Assert.That(sim.TryFire(sim.ZonePosition),Is.True);
            Run(sim,90);
            sim.Balls.Add(new MomentumBall { Id=9999, Position=new SimVector2(800,780), Velocity=new SimVector2(0,-100), ExpiresAt=10, Generation=5 });
            Assert.That(sim.ReloadRemaining,Is.Zero);
            Assert.That(sim.Ready,Is.False);
            Assert.That(sim.TryFire(sim.ZonePosition),Is.False);
            Run(sim,520);
            Assert.That(sim.Balls,Is.Empty); Assert.That(sim.FiredCount,Is.EqualTo(6));
            Assert.That(sim.Ready,Is.True);
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
            foreach(var child in sim.Balls) { Assert.That(child.Generation,Is.EqualTo(4)); Assert.That(child.ExpiresAt,Is.EqualTo(10)); Assert.That(child.DamageScale,Is.EqualTo(.55)); }
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
            sim.TryFire(sim.ZonePosition); Run(sim,40);
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
