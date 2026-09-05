using System;

namespace IncrementalGame.Core
{
    // Historical landscape geometry remains the default for old simulations/tests.
    public sealed class MomentumBoardLayout
    {
        public static readonly MomentumBoardLayout Landscape = new MomentumBoardLayout(false);
        public static readonly MomentumBoardLayout Portrait = new MomentumBoardLayout(true);
        public static readonly MomentumBoardLayout Arena = new MomentumBoardLayout(true, true);
        public bool IsArena { get; }
        public bool IsPortrait { get; }
        private MomentumBoardLayout(bool portrait, bool arena = false) { IsPortrait = portrait; IsArena = arena; }
        public double Left => IsArena ? 500 : IsPortrait ? 575 : 320;
        public double Right => IsArena ? 1100 : IsPortrait ? 1025 : 1560;
        public double Top => IsPortrait ? 50 : 120;
        public double Bottom => IsPortrait ? 850 : 760;
        public double Width => Right - Left;
        public double Height => Bottom - Top;
        public SimVector2 Gun => IsPortrait ? new SimVector2(800, 815) : MomentumRules.Gun;
        public double TargetBottom => IsArena ? 580 : IsPortrait ? 540 : 470;
        public SimVector2 ZoneAt(double time) => IsArena ? new SimVector2(800 + 170 * Math.Sin(.8 * time), 660) : IsPortrait
            ? new SimVector2(800 + 115 * Math.Sin(.8 * time), 650)
            : MomentumRules.ZoneAt(time);
        public bool Contains(SimVector2 point) => point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        public SimVector2 ObstaclePosition(int index, double random) => IsArena ? new SimVector2(index == 0 ? 645 : 955, 120 + random * 25) : IsPortrait
            ? new SimVector2(index == 0 ? 685 : 915, 560 + random * 50)
            : new SimVector2(index == 0 ? 560 : 1280, 480 + random * 50);
        public SimVector2 TargetCandidate(double x, double y) => IsArena ? new SimVector2(550 + x * 500, 210 + y * 310) : IsPortrait
            ? new SimVector2(645 + x * 310, 115 + y * 365)
            : new SimVector2(390 + x * 1100, 185 + y * 225);
    }
}
