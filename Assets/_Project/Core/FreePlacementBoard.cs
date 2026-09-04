using System;
using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public enum BoardPieceKind { Collector, Mirror, Amplifier }

    public sealed class BoardPiece
    {
        public BoardPiece(int id, BoardPieceKind kind, SimVector2 position, SimVector2 size, double angle = 0)
        { Id = id; Kind = kind; Position = position; Size = size; Angle = angle; }
        public int Id { get; }
        public BoardPieceKind Kind { get; }
        public SimVector2 Position { get; set; }
        public SimVector2 Size { get; }
        // Clockwise degrees in logical (down-positive) coordinates.
        public double Angle { get; set; }
        public bool IsCircle => Kind == BoardPieceKind.Collector;
        public SimVector2 AxisX => AimCalculator.RotateDegrees(new SimVector2(1, 0), Angle);
        public SimVector2 AxisY => AimCalculator.RotateDegrees(new SimVector2(0, 1), Angle);
    }

    public static class FreePlacementBoard
    {
        public const double Grid = 20;
        public const double Left = 300, Right = 1560, Top = 140, Bottom = 750;
        public static readonly SimVector2 Gun = new SimVector2(800, 820);

        public static List<BoardPiece> CreateInitial() => new List<BoardPiece>
        {
            new BoardPiece(1, BoardPieceKind.Collector, new SimVector2(800, 220), new SimVector2(100, 100)),
            new BoardPiece(2, BoardPieceKind.Collector, new SimVector2(1200, 360), new SimVector2(100, 100)),
            new BoardPiece(3, BoardPieceKind.Mirror, new SimVector2(460, 580), new SimVector2(18, 220), -5),
            new BoardPiece(4, BoardPieceKind.Amplifier, new SimVector2(800, 480), new SimVector2(96, 56))
        };

        public static SimVector2 Snap(SimVector2 p) => new SimVector2(
            Math.Round(p.X / Grid, MidpointRounding.AwayFromZero) * Grid,
            Math.Round(p.Y / Grid, MidpointRounding.AwayFromZero) * Grid);

        public static bool CanPlace(BoardPiece piece, IReadOnlyList<BoardPiece> pieces)
        {
            if (double.IsNaN(piece.Position.X) || double.IsNaN(piece.Position.Y) ||
                double.IsInfinity(piece.Position.X) || double.IsInfinity(piece.Position.Y) ||
                double.IsNaN(piece.Angle) || double.IsInfinity(piece.Angle)) return false;
            var x = piece.AxisX; var y = piece.AxisY;
            var extentX = piece.IsCircle ? piece.Size.X / 2 :
                Math.Abs(x.X) * piece.Size.X / 2 + Math.Abs(y.X) * piece.Size.Y / 2;
            var extentY = piece.IsCircle ? piece.Size.X / 2 :
                Math.Abs(x.Y) * piece.Size.X / 2 + Math.Abs(y.Y) * piece.Size.Y / 2;
            if (piece.Position.X - extentX < Left || piece.Position.X + extentX > Right ||
                piece.Position.Y - extentY < Top || piece.Position.Y + extentY > Bottom) return false;
            foreach (var other in pieces)
                if (other.Id != piece.Id && Overlaps(piece, other)) return false;
            return true;
        }

        public static bool Overlaps(BoardPiece a, BoardPiece b)
        {
            const double clearance = 2;
            if (a.IsCircle && b.IsCircle)
                return (a.Position - b.Position).Magnitude < (a.Size.X + b.Size.X) / 2 + clearance;
            if (a.IsCircle || b.IsCircle)
            {
                var circle = a.IsCircle ? a : b;
                var box = a.IsCircle ? b : a;
                var delta = circle.Position - box.Position;
                var dx = Math.Max(0, Math.Abs(SimVector2.Dot(delta, box.AxisX)) - box.Size.X / 2);
                var dy = Math.Max(0, Math.Abs(SimVector2.Dot(delta, box.AxisY)) - box.Size.Y / 2);
                var radius = circle.Size.X / 2 + clearance;
                return dx * dx + dy * dy < radius * radius;
            }
            foreach (var axis in new[] { a.AxisX, a.AxisY, b.AxisX, b.AxisY })
            {
                var separation = Math.Abs(SimVector2.Dot(b.Position - a.Position, axis));
                var ra = Math.Abs(SimVector2.Dot(a.AxisX, axis)) * a.Size.X / 2 +
                         Math.Abs(SimVector2.Dot(a.AxisY, axis)) * a.Size.Y / 2;
                var rb = Math.Abs(SimVector2.Dot(b.AxisX, axis)) * b.Size.X / 2 +
                         Math.Abs(SimVector2.Dot(b.AxisY, axis)) * b.Size.Y / 2;
                if (separation >= ra + rb + clearance) return false;
            }
            return true;
        }
    }
}
