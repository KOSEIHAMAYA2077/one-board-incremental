using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed class UnityProjectileCollisionQuery : IProjectileCollisionQuery
    {
        public ProjectileCollision CircleCast(
            SimVector2 position,
            double radius,
            SimVector2 direction,
            double distance)
        {
            var worldPosition = LogicalSpace.ToWorld(position);
            var worldDirection = LogicalSpace.DirectionToWorld(direction);
            var radiusWorld = (float)(radius / LogicalSpace.UnitsPerWorldUnit);
            var distanceWorld = (float)(distance / LogicalSpace.UnitsPerWorldUnit);
            var hit = Physics2D.CircleCast(
                worldPosition,
                radiusWorld,
                worldDirection,
                distanceWorld);

            if (hit.collider == null)
            {
                return ProjectileCollision.None;
            }

            var surfaceKind = CollisionSurfaceKind.Solid;
            if (hit.collider.GetComponent<CollectorTargetView>() != null)
            {
                surfaceKind = CollisionSurfaceKind.Collector;
            }
            else if (hit.collider.GetComponent<ReflectionWallView>() != null)
            {
                surfaceKind = CollisionSurfaceKind.ReflectionWall;
            }

            return new ProjectileCollision(
                surfaceKind,
                hit.distance * LogicalSpace.UnitsPerWorldUnit,
                LogicalSpace.ToLogical(hit.point),
                LogicalSpace.DirectionToLogical(hit.normal));
        }
    }
}
