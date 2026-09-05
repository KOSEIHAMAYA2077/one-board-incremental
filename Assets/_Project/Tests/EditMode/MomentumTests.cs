using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class MomentumTests
    {
        [Test] public void PortraitIsNineBySixteenAndKeepsCircularGeometry()
        {
            var layout=MomentumBoardLayout.Portrait;
            Assert.That(layout.Width/layout.Height,Is.EqualTo(9.0/16));
            Assert.That(layout.Contains(layout.Gun),Is.True);
            Assert.That(MomentumRules.Radius,Is.EqualTo(8));
            for(var i=0;i<200;i++)
            {
                var zone=layout.ZoneAt(i*.1);
                Assert.That(zone.X-MomentumRules.ZoneRadius,Is.GreaterThan(layout.Left));
                Assert.That(zone.X+MomentumRules.ZoneRadius,Is.LessThan(layout.Right));
            }
            Assert.That(new MomentumSimulation().Layout,Is.SameAs(MomentumBoardLayout.Landscape));
        }
        [Test] public void PortraitSeedsPlaceAllSevenTargetsWithoutOverlap()
        {
            for(uint seed=1;seed<=100;seed++)
            {
                var sim=new MomentumSimulation(seed,MomentumBoardLayout.Portrait);
                var other=new MomentumSimulation(seed,MomentumBoardLayout.Portrait);
                for(var i=0;i<7;i++)
                {
                    var t=sim.Targets[i];
                    Assert.That(t.Alive,Is.True,$"seed={seed} target={i}");
                    Assert.That(sim.PositionAvailable(t.Position,t.Radius,t.Id),Is.True);
                    Assert.That(t.Position,Is.EqualTo(other.Targets[i].Position));
                }
            }
        }
        [Test] public void PortraitWallsReflectWithinTheNarrowStage()
        {
            var sim=new MomentumSimulation(layout:MomentumBoardLayout.Portrait); sim.Targets.Clear(); sim.Obstacles.Clear();
            var b=Ball(1,new SimVector2(sim.Layout.Right-9,400),new SimVector2(900,0)); sim.Balls.Add(b);
            sim.Tick(1.0/60); Assert.That(b.Velocity.X,Is.LessThan(0));
            for(var i=0;i<600;i++)
            {
                sim.Tick(1.0/60);
                foreach(var ball in sim.Balls) Assert.That(sim.Layout.Contains(ball.Position),Is.True);
            }
            Assert.That(sim.Balls.Count,Is.Zero);
        }
        [Test] public void DpsUsesFiveSecondEffectiveDamageAndExpires()
        {
            var stats=new MomentumCombatStats();
            stats.Record(1,10,40,MomentumAmmo.Normal); stats.Record(2,25,26,MomentumAmmo.Pierce);
            Assert.That(stats.RecentDps,Is.EqualTo(7)); Assert.That(stats.TotalDamage,Is.EqualTo(35));
            Assert.That(stats.LastNormalImpact,Is.EqualTo(40)); Assert.That(stats.LastPierceImpact,Is.EqualTo(26));
            stats.Advance(6); Assert.That(stats.RecentDps,Is.EqualTo(5));
            stats.Advance(7); Assert.That(stats.RecentDps,Is.Zero); Assert.That(stats.TotalDamage,Is.EqualTo(35));
        }
        [Test] public void CombatStatsExcludeOverkillAndFreezeWhileEditing()
        {
            var sim=Empty(); var t=Target(1,new SimVector2(800,300),false); t.Hp=1; sim.Targets.Add(t);
            sim.Balls.Add(Ball(1,new SimVector2(800,345),new SimVector2(0,-900)));
            sim.Tick(1.0/60); Assert.That(sim.Stats.TotalDamage,Is.EqualTo(1)); Assert.That(sim.Stats.Hits,Is.EqualTo(1));
            var dps=sim.Stats.RecentDps; sim.SetEditing(true);
            for(var i=0;i<400;i++) sim.Tick(1.0/60);
            Assert.That(sim.Stats.RecentDps,Is.EqualTo(dps));
            sim.SetEditing(false); for(var i=0;i<400;i++) sim.Tick(1.0/60);
            Assert.That(sim.Stats.RecentDps,Is.Zero);
        }
        [Test] public void NormalHasMoreDamageButPierceRetainsMoreSpeed()
        {
            Assert.That(MomentumRules.Damage(MomentumAmmo.Normal, 900), Is.EqualTo(40));
            Assert.That(MomentumRules.Damage(MomentumAmmo.Pierce, 900), Is.EqualTo(26));
            Assert.That(MomentumRules.ExitSpeed(MomentumAmmo.Normal, 600, 400), Is.EqualTo(200));
            Assert.That(MomentumRules.ExitSpeed(MomentumAmmo.Pierce, 600, 400), Is.EqualTo(500));
            Assert.That(MomentumRules.Damage(MomentumAmmo.Normal, 9000), Is.EqualTo(80));
        }
        [Test] public void StoppedBallStillDealsItsImpactDamage()
        {
            var sim = Empty(); var t = Target(1, new SimVector2(800, 300), true); sim.Targets.Add(t);
            var ball = Ball(1, new SimVector2(800, 350), new SimVector2(0, -300)); sim.Balls.Add(ball);
            sim.Tick(1.0 / 60);
            Assert.That(t.Hp, Is.LessThan(90)); Assert.That(ball.Alive, Is.False); Assert.That(sim.Balls.Count, Is.Zero);
        }
        [Test] public void TimeDragAndWallLossEventuallyEndTheBall()
        {
            var sim = Empty(); var b = Ball(1, new SimVector2(600, 220), new SimVector2(900, 0)); sim.Balls.Add(b);
            sim.Tick(1.0 / 60); Assert.That(b.Speed, Is.EqualTo(898).Within(.001));
            for (var i = 0; i < 900; i++) sim.Tick(1.0 / 60);
            Assert.That(sim.Balls.Count, Is.Zero);
        }
        [Test] public void BoostAppliesOnceAndRespectsMaximumSpeed()
        {
            var sim = Empty(); var b = Ball(1, sim.ZonePosition, new SimVector2(0, -1600)); sim.Balls.Add(b);
            sim.Tick(1.0 / 60); Assert.That(b.Speed, Is.EqualTo(1800).Within(.001)); Assert.That(sim.BoostCount, Is.EqualTo(1));
            b.Position = sim.ZonePosition; sim.Tick(1.0 / 60);
            Assert.That(sim.BoostCount, Is.EqualTo(1)); Assert.That(b.Speed, Is.LessThan(1800));
        }
        [Test] public void RelativeSweepDetectsZoneMovingIntoBall()
        {
            Assert.That(MomentumRules.SweepCircle(new SimVector2(100, 0), new SimVector2(-200, 0), 70, .2, out var time), Is.True);
            Assert.That(time, Is.EqualTo(.15).Within(.000001));
        }
        [Test] public void MovingZoneBoostsBallItSweepsAcrossDuringTick()
        {
            var sim = Empty(); var b = Ball(1, sim.ZonePosition + new SimVector2(75, 0), new SimVector2(0, 100)); sim.Balls.Add(b);
            sim.Tick(1.0 / 60);
            Assert.That(b.Boosted, Is.True); Assert.That(sim.BoostCount, Is.EqualTo(1));
        }
        [Test] public void ThreeShotCadenceDoesNotAccumulateTickRounding()
        {
            var sim = Empty(); sim.TryFire(new SimVector2(940, 200));
            for (var i = 0; i < 15; i++) sim.Tick(1.0 / 60);
            Assert.That(sim.FiredCount, Is.EqualTo(3));
        }
        [Test] public void FastBallCannotSkipTargetAndContactDoesNotRepeatWhileInside()
        {
            var sim = Empty(); var t = Target(1, new SimVector2(800, 300), true); sim.Targets.Add(t);
            var b = Ball(1, new SimVector2(800, 370), new SimVector2(0, -1800)); b.Ammo = MomentumAmmo.Pierce; sim.Balls.Add(b);
            sim.Tick(1.0 / 60); var hp = t.Hp;
            Assert.That(hp, Is.LessThan(90)); sim.Tick(1.0 / 60); Assert.That(t.Hp, Is.EqualTo(hp));
        }
        [Test] public void ReturningToTargetDealsDamageAgain()
        {
            var sim = Empty(); var t = Target(1, new SimVector2(800, 300), true); sim.Targets.Add(t);
            var b = Ball(1, new SimVector2(800, 350), new SimVector2(0, -900)); b.Ammo = MomentumAmmo.Pierce; sim.Balls.Add(b);
            for (var i = 0; i < 40; i++) sim.Tick(1.0 / 60); // Pass, reflect at top wall, return.
            Assert.That(t.Hp, Is.LessThan(55));
        }
        [Test] public void DestructionPaysOnceAndOnlyDeadTargetsRefillAtNextVolley()
        {
            var sim = Empty(); var t = Target(1, new SimVector2(800, 300), false); t.Hp = 1; sim.Targets.Add(t);
            sim.Balls.Add(Ball(1, new SimVector2(800, 345), new SimVector2(0, -900)));
            sim.Balls.Add(Ball(2, new SimVector2(800, 345), new SimVector2(0, -900)));
            sim.Tick(1.0 / 60); Assert.That(sim.Gold, Is.EqualTo(3)); Assert.That(sim.DestroyedCount, Is.EqualTo(1));
            sim.TryFire(new SimVector2(940, 200)); Assert.That(t.Hp, Is.EqualTo(50));
            Assert.That(t.Position, Is.Not.EqualTo(new SimVector2(800, 300)));
        }
        [Test] public void MagazineFiresThreeAndDoesNotQueueReloadClicks()
        {
            var sim = new MomentumSimulation(); Assert.That(sim.TryFire(new SimVector2(940, 200)), Is.True);
            Assert.That(sim.TryFire(new SimVector2(940, 200)), Is.False);
            for (var i = 0; i < 20; i++) sim.Tick(1.0 / 60);
            Assert.That(sim.FiredCount, Is.EqualTo(3)); Assert.That(sim.Ready, Is.False);
            for (var i = 0; i < 100; i++) sim.Tick(1.0 / 60);
            Assert.That(sim.FiredCount, Is.EqualTo(3)); Assert.That(sim.Ready, Is.True);
        }
        [Test] public void EditingFreezesBurstZoneAndBallsAndDoesNotChangeBurstSnapshot()
        {
            var sim = Empty(); sim.TryFire(new SimVector2(940, 200)); sim.SetEditing(true);
            var pos = sim.Balls[0].Position; var zone = sim.ZonePosition;
            sim.SetMagazine(new[] { MomentumAmmo.Pierce, MomentumAmmo.Normal, MomentumAmmo.Pierce });
            sim.Tick(1.0 / 60);
            Assert.That(sim.Time, Is.Zero); Assert.That(sim.Balls[0].Position, Is.EqualTo(pos)); Assert.That(sim.ZonePosition, Is.EqualTo(zone));
            sim.SetEditing(false); for (var i = 0; i < 17; i++) sim.Tick(1.0 / 60);
            Assert.That(sim.Balls[1].Ammo, Is.EqualTo(MomentumAmmo.Pierce)); Assert.That(sim.Balls[2].Ammo, Is.EqualTo(MomentumAmmo.Normal));
        }
        [Test] public void SeededLayoutsAreReproducibleAndDoNotOverlap()
        {
            for (uint seed = 1; seed <= 50; seed++)
            {
                var a = new MomentumSimulation(seed); var b = new MomentumSimulation(seed);
                for (var i = 0; i < a.Targets.Count; i++)
                {
                    var t = a.Targets[i]; Assert.That(t.Alive, Is.True); Assert.That(t.Position, Is.EqualTo(b.Targets[i].Position));
                    Assert.That(a.PositionAvailable(t.Position, t.Radius, t.Id), Is.True);
                }
            }
        }
        [Test] public void SameSeedAndInputGiveSameOutcome()
        {
            var a = new MomentumSimulation(); var b = new MomentumSimulation();
            a.TryFire(new SimVector2(1000, 200)); b.TryFire(new SimVector2(1000, 200));
            for (var i = 0; i < 800; i++) { a.Tick(1.0 / 60); b.Tick(1.0 / 60); }
            Assert.That(a.Gold, Is.EqualTo(b.Gold)); Assert.That(a.BoostCount, Is.EqualTo(b.BoostCount));
            for (var i = 0; i < 7; i++) Assert.That(a.Targets[i].Hp, Is.EqualTo(b.Targets[i].Hp));
        }
        private static MomentumSimulation Empty() { var sim = new MomentumSimulation(); sim.Targets.Clear(); sim.Obstacles.Clear(); return sim; }
        private static MomentumTarget Target(int id, SimVector2 p, bool armor) => new MomentumTarget { Id = id, Position = p, Armored = armor, Hp = armor ? 90 : 50 };
        private static MomentumBall Ball(int id, SimVector2 p, SimVector2 v) => new MomentumBall { Id = id, Position = p, Velocity = v, Ammo = MomentumAmmo.Normal };
    }
}
