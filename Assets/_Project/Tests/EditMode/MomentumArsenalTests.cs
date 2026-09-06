using System;
using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class MomentumArsenalTests
    {
        private static MomentumSimulation Fixture(int gun=0)
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress { gun=gun,equippedMods=0 });
            sim.Targets.Clear();sim.Obstacles.Clear();sim.Pickups.Clear();return sim;
        }
        private static MomentumBall Ball(double x,double y,double vx,double vy) => new MomentumBall { Id=100,Position=new SimVector2(x,y),Velocity=new SimVector2(vx,vy),Boosted=true };
        [Test] public void ShotgunIsOneSimultaneousEightPelletMagazine()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress { gun=2,equippedMods=0 });
            Assert.That(sim.TryFire(new SimVector2(800,200)),Is.True);
            Assert.That(sim.Balls.Count,Is.EqualTo(8));Assert.That(sim.FiredCount,Is.EqualTo(8));
            Assert.That(sim.ChallengeMagazines,Is.EqualTo(1));Assert.That(sim.Bursting,Is.False);
            Assert.That(sim.ReloadRemaining,Is.EqualTo(.8));
            var previous=-100.0;
            foreach(var b in sim.Balls)
            {
                var angle=Math.Atan2(b.Velocity.X,-b.Velocity.Y)*180/Math.PI;
                Assert.That(angle,Is.GreaterThan(previous));previous=angle;
                Assert.That(b.Position,Is.EqualTo(sim.Layout.Gun));Assert.That(b.Speed,Is.EqualTo(760).Within(1e-8));
                Assert.That(b.Radius,Is.EqualTo(6));Assert.That(b.DamageScale,Is.EqualTo(18.0/40));Assert.That(b.ExpiresAt,Is.EqualTo(10));
            }
            Assert.That(Math.Abs(sim.Balls[0].Velocity.Normalized.X),Is.GreaterThan(.36));
            Assert.That(Math.Abs(sim.Balls[7].Velocity.Normalized.X),Is.GreaterThan(.36));
        }
        [TestCase(0,1650)] [TestCase(2,1800)]
        public void SniperIsLargeSingleFastShotWithExistingSpeedCap(int mods,double speed)
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress { gun=3,equippedMods=mods });
            sim.TryFire(new SimVector2(800,200));Assert.That(sim.Balls.Count,Is.EqualTo(1));
            var b=sim.Balls[0];Assert.That(b.Radius,Is.EqualTo(14));Assert.That(b.Speed,Is.EqualTo(speed).Within(1e-8));
            Assert.That(b.DamageScale,Is.EqualTo(90.0/40));Assert.That(sim.Bursting,Is.False);
            sim.Targets.Clear();sim.Obstacles.Clear();sim.Pickups.Clear();b.Boosted=true;
            var t=new MomentumTarget { Id=99,Position=b.Position+b.Velocity.Normalized*70,RadiusScale=.1,Hp=1000 };
            sim.Targets.Add(t);sim.Tick(.05);
            Assert.That(t.Hp,Is.LessThan(1000),"Swept collision must hit a target smaller than the travel distance");
        }
        [Test] public void MidBurstGunSwitchOnlyChangesNextMagazine()
        {
            var sim=new MomentumSimulation(progress:new MomentumProgress { equippedMods=0 });
            sim.TryFire(new SimVector2(800,200));sim.Targets.Clear();sim.Obstacles.Clear();sim.Pickups.Clear();
            Assert.That(sim.Progress.SelectGun(3),Is.True);
            for(var i=0;i<120;i++) sim.Tick(1.0/60);
            Assert.That(sim.FiredCount,Is.EqualTo(6));Assert.That(sim.FiringGun,Is.Zero);
            foreach(var b in sim.Balls) { Assert.That(b.Radius,Is.EqualTo(9));Assert.That(b.DamageScale,Is.EqualTo(48.0/40)); }
            sim.Recall();sim.Targets.Add(new MomentumTarget { Id=1,Hp=1000 });sim.StartChallenge(0);
            Assert.That(sim.TryFire(new SimVector2(800,200)),Is.True);Assert.That(sim.Balls.Count,Is.EqualTo(1));
            Assert.That(sim.FiringGun,Is.EqualTo(3));
        }
        [Test] public void NewGunOwnershipPresetsAndInvalidIdsAreValidated()
        {
            var p=new MomentumProgress();Assert.That(p.SelectGun(1),Is.False);
            foreach(var id in new[]{2,3}) { Assert.That(p.SelectGun(id),Is.True);p.StorePreset(0);p.SelectGun(0);Assert.That(p.LoadPreset(0),Is.True);Assert.That(p.gun,Is.EqualTo(id));Assert.That(p.Valid(),Is.True); }
            Assert.That(p.SelectGun(-1),Is.False);Assert.That(p.SelectGun(4),Is.False);
        }
        [Test] public void BCanPaysOnlyOnBumperAndOnlyOnce()
        {
            var sim=Fixture();var can=new MomentumPickup { Id=1,Position=new SimVector2(800,400) };sim.Pickups.Add(can);
            var b=Ball(800,445,0,-1800);sim.Balls.Add(b);sim.Tick(.05);
            Assert.That(can.Active,Is.False);Assert.That(b.BountyCharged,Is.True);Assert.That(sim.Gold,Is.Zero);
            sim.Obstacles.Add(new MomentumObstacle { Id=101,Position=new SimVector2(800,200) });
            b.Position=new SimVector2(800,240);b.Velocity=new SimVector2(0,-900);sim.Tick(1.0/60);
            Assert.That(b.BountyCharged,Is.False);Assert.That(sim.Gold,Is.EqualTo(12));Assert.That(sim.ChallengeGold,Is.EqualTo(12));
            b.Position=new SimVector2(800,240);b.Velocity=new SimVector2(0,-900);sim.Tick(1.0/60);
            Assert.That(sim.Gold,Is.EqualTo(12));
        }
        [Test] public void WallAndRecallDoNotCashBounty()
        {
            var sim=Fixture();var b=Ball(1080,400,900,0);b.BountyCharged=true;sim.Balls.Add(b);sim.Tick(.05);
            Assert.That(b.Velocity.X,Is.LessThan(0));Assert.That(b.BountyCharged,Is.True);Assert.That(sim.Gold,Is.Zero);
            sim.Recall();Assert.That(sim.Gold,Is.Zero);Assert.That(sim.Balls,Is.Empty);
        }
        [Test] public void SplitChildrenDoNotInheritBButCanPickUpTheirOwnCan()
        {
            var sim=Fixture();sim.Targets.Add(new MomentumTarget { Id=1,Position=new SimVector2(800,300),Hp=1000 });
            var b=Ball(800,345,0,-900);b.BountyCharged=true;b.Mods=MomentumMod.Split;sim.Balls.Add(b);sim.Tick(1.0/60);
            Assert.That(b.Alive,Is.False);Assert.That(sim.Balls.Count,Is.EqualTo(2));
            foreach(var child in sim.Balls) Assert.That(child.BountyCharged,Is.False);
            var chosen=sim.Balls[0];sim.Pickups.Add(new MomentumPickup { Id=1,Position=chosen.Position });
            sim.Tick(1.0/60);Assert.That(chosen.BountyCharged,Is.True);Assert.That(sim.Gold,Is.Zero);
        }
        [Test] public void ChargedBallDoesNotConsumeAnotherCanAndExpiryDoesNotPay()
        {
            var sim=Fixture();var can=new MomentumPickup { Id=1,Position=new SimVector2(800,400) };sim.Pickups.Add(can);
            var b=Ball(800,445,0,-1800);b.BountyCharged=true;b.ExpiresAt=.08;sim.Balls.Add(b);sim.Tick(.05);
            Assert.That(can.Active,Is.True);sim.Tick(.05);Assert.That(sim.Balls,Is.Empty);Assert.That(sim.Gold,Is.Zero);
        }
        [Test] public void CansResetOnlyForNewChallengeAndHaveClearPlacement()
        {
            for(uint seed=1;seed<=100;seed++)
            {
                var sim=new MomentumSimulation(seed,progress:new MomentumProgress());Assert.That(sim.Pickups.Count,Is.EqualTo(2));
                foreach(var can in sim.Pickups) foreach(var target in sim.Targets) Assert.That((can.Position-target.Position).Magnitude,Is.GreaterThan(MomentumPickup.Radius+target.Radius));
                sim.Pickups[0].Active=false;sim.TryFire(new SimVector2(800,200));sim.Recall();
                Assert.That(sim.Pickups[0].Active,Is.False);sim.StartChallenge(0);Assert.That(sim.Pickups[0].Active,Is.True);
            }
        }
    }
}
