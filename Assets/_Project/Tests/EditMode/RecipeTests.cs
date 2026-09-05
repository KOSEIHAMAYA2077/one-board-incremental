using IncrementalGame.Core;
using NUnit.Framework;

namespace IncrementalGame.Tests.EditMode
{
    public sealed class RecipeTests
    {
        private static readonly RecipeBullet N = RecipeBullet.Normal, P = RecipeBullet.Pierce, S = RecipeBullet.Split;
        [Test] public void PrimerWrapsAndStopsAtSpecialOrTwoNormals()
        {
            var slots = new[] { P, S, N, N, N };
            Assert.That(RecipeCycle.PrimerAt(slots, 0), Is.EqualTo(2));
            Assert.That(RecipeCycle.PrimerAt(slots, 1), Is.Zero);
            Assert.That(RecipeCycle.PrimerAt(new[] { N, P, N, N, P }, 1), Is.EqualTo(1));
        }
        [Test] public void CapacityRejectsInvalidOrOverBudgetRecipes()
        {
            Assert.That(RecipeCycle.IsValid(new[] { P, P, N, N, N }), Is.True);
            Assert.That(RecipeCycle.IsValid(new[] { S, P, N, N, N }), Is.False);
            Assert.That(RecipeCycle.IsValid(new[] { (RecipeBullet)99, N, N, N, N }), Is.False);
        }
        [Test] public void ReloadDoesNotQueueClicksAndPendingOnlyAppliesAtBoundary()
        {
            var cycle = new RecipeCycle(); cycle.Apply(new[] { N, N, N, N, N });
            Assert.That(cycle.TryFire(), Is.True); Assert.That(cycle.TryFire(), Is.False);
            cycle.Apply(new[] { P, N, N, N, N });
            Assert.That(cycle.Slot, Is.Zero); Assert.That(cycle.Current, Is.EqualTo(N));
            cycle.Tick(.65); Assert.That(cycle.Slot, Is.EqualTo(1));
            for (var i = 1; i < 5; i++) { cycle.TryFire(); cycle.Tick(.65); }
            Assert.That(cycle.Slot, Is.Zero); Assert.That(cycle.CycleId, Is.EqualTo(1));
            Assert.That(cycle.Current, Is.EqualTo(P)); Assert.That(cycle.Pending, Is.Null);
        }
        [Test] public void ApplyCopiesAndNeverMutatesActiveRecipeMidCycle()
        {
            var slots = new[] { P, N, N, N, N }; var cycle = new RecipeCycle(); cycle.Apply(slots); slots[0] = S;
            Assert.That(cycle.Current, Is.EqualTo(P)); cycle.TryFire();
            Assert.That(cycle.Apply(new[] { S, S, S, S, S }), Is.False);
            Assert.That(cycle.Pending, Is.Null);
        }
        [Test] public void SaveDuringFinalReloadProjectsBoundaryWithoutChangingLiveState()
        {
            var cycle = new RecipeCycle(); cycle.Apply(new[] { N, N, N, N, N });
            for (var i = 0; i < 4; i++) { cycle.TryFire(); cycle.Tick(.65); }
            cycle.TryFire(); cycle.Apply(new[] { P, N, N, N, N });
            var snapshot = cycle.Capture(); var restored = new RecipeCycle();
            Assert.That(restored.Restore(snapshot), Is.True);
            Assert.That(restored.Ready, Is.True); Assert.That(restored.Slot, Is.Zero);
            Assert.That(restored.Current, Is.EqualTo(P)); Assert.That(restored.CycleId, Is.EqualTo(1));
            Assert.That(cycle.Slot, Is.EqualTo(4)); Assert.That(cycle.Ready, Is.False); Assert.That(cycle.Pending, Is.Not.Null);
        }
        [Test] public void SplitSharesVisitsAndInheritsAmplifierBeforeGenerating()
        {
            var lineage = New(S, 2); lineage.Tick(.1, new OncePerProjectileQuery(BoardPieceKind.Amplifier));
            Assert.That(lineage.Projectiles.Count, Is.EqualTo(5));
            foreach (var child in lineage.Projectiles.GetRange(1, 4))
            { Assert.That(child.Lineage, Is.SameAs(lineage)); Assert.That(child.RewardToken, Is.EqualTo(2)); Assert.That(child.Depth, Is.EqualTo(1)); Assert.That(child.Velocity.Magnitude, Is.EqualTo(900).Within(.001)); }
            Assert.That(lineage.EffectVisited.Count, Is.EqualTo(1));
        }
        [Test] public void GenerationBudgetAndDepthNeverSilentlyOverflow()
        {
            var lineage = New(S, 2); lineage.Tick(1, new RepeatedCollectorQuery());
            Assert.That(lineage.Projectiles.Count, Is.EqualTo(64));
            Assert.That(lineage.LimitedCount, Is.GreaterThan(0));
            Assert.That(lineage.YieldGold, Is.EqualTo(2));
            Assert.That(lineage.Complete, Is.True);
            foreach (var shot in lineage.Projectiles) Assert.That(shot.Depth, Is.LessThanOrEqualTo(3));
        }
        [Test] public void PierceCountsPassagesButDuplicateRewardIsSuppressed()
        {
            var lineage = New(P, 0); lineage.Tick(.1, new RepeatedCollectorQuery());
            Assert.That(lineage.Projectiles[0].Contacts.Count, Is.EqualTo(3));
            Assert.That(lineage.YieldGold, Is.EqualTo(2));
            Assert.That(lineage.Complete, Is.True);
        }
        [Test] public void SplitMovesChildrenOnlyThroughRemainingTickDistance()
        {
            var lineage = New(S, 0); lineage.Tick(.1, new OncePerProjectileQuery(BoardPieceKind.Amplifier));
            foreach (var child in lineage.Projectiles.GetRange(1, 2))
                Assert.That((child.Position - new SimVector2(800, 819)).Magnitude, Is.EqualTo(89).Within(.001));
        }
        private static RecipeLineage New(RecipeBullet bullet, int primer) => new RecipeLineage(bullet, primer, FreePlacementBoard.Gun, new SimVector2(0, -900));
        private sealed class RepeatedCollectorQuery : IRoutingQuery
        { public RoutingContact Cast(RoutingShot shot, double distance) => new RoutingContact(1, BoardPieceKind.Collector, 1, shot.Position, new SimVector2(0, 1)); }
        private sealed class OncePerProjectileQuery : IRoutingQuery
        {
            private readonly BoardPieceKind _kind; public OncePerProjectileQuery(BoardPieceKind kind) { _kind = kind; }
            public RoutingContact Cast(RoutingShot shot, double distance) => shot.ExitGuards.Contains(1) ? default : new RoutingContact(1, _kind, 1, shot.Position, new SimVector2(0, 1));
        }
    }
}
