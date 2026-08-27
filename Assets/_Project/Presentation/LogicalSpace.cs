using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public static class LogicalSpace
    {
        public const float Width = 1600f;
        public const float Height = 900f;
        public const float UnitsPerWorldUnit = 100f;

        public static Vector2 ToWorld(SimVector2 logical) => new Vector2(
            ((float)logical.X - Width * 0.5f) / UnitsPerWorldUnit,
            (Height * 0.5f - (float)logical.Y) / UnitsPerWorldUnit);

        public static SimVector2 ToLogical(Vector2 world) => new SimVector2(
            world.x * UnitsPerWorldUnit + Width * 0.5f,
            Height * 0.5f - world.y * UnitsPerWorldUnit);

        public static Vector2 DirectionToWorld(SimVector2 logicalDirection) =>
            new Vector2((float)logicalDirection.X, -(float)logicalDirection.Y).normalized;

        public static SimVector2 DirectionToLogical(Vector2 worldDirection) =>
            new SimVector2(worldDirection.x, -worldDirection.y).Normalized;
    }
}
