using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public static class RouteDrawing
    {
        private static Sprite _square, _circle;
        private static Material _lineMaterial;
        public static Material LineMaterial => _lineMaterial != null ? _lineMaterial :
            (_lineMaterial = new Material(Shader.Find("Sprites/Default")));

        public static Sprite Sprite(bool circle)
        {
            if (circle && _circle != null) return _circle;
            if (!circle && _square != null) return _square;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var colors = new Color[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var distance = new Vector2(x + 0.5f - size / 2f, y + 0.5f - size / 2f).magnitude;
                    colors[y * size + x] = new Color(1, 1, 1, circle ? Mathf.Clamp01(size / 2f - distance) : 1);
                }
            texture.SetPixels(colors); texture.Apply(false, true);
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one / 2, size);
            if (circle) _circle = sprite; else _square = sprite;
            return sprite;
        }

        public static SpriteRenderer Shape(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool circle, int order = 1)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = size;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite(circle); renderer.color = color; renderer.sortingOrder = order;
            return renderer;
        }

        public static LineRenderer Line(Transform parent, string name, Color color, float width, int order = 2)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false);
            var line = obj.AddComponent<LineRenderer>(); line.useWorldSpace = true;
            line.sharedMaterial = LineMaterial; line.startColor = color; line.endColor = color;
            line.startWidth = width; line.endWidth = width; line.sortingOrder = order;
            line.positionCount = 0; line.numCornerVertices = 3;
            return line;
        }
    }

    public sealed class BoardPieceView : MonoBehaviour
    {
        public BoardPiece Piece { get; private set; }
        public Collider2D Collider { get; private set; }
        private SpriteRenderer _renderer;
        private LineRenderer _outline;
        private float _flash;
        public Color BaseColor => Piece.Kind == BoardPieceKind.Collector ? new Color(0.34f, 0.9f, 0.62f) :
            Piece.Kind == BoardPieceKind.Mirror ? new Color(0.25f, 0.8f, 1f) : new Color(0.77f, 0.52f, 1f);

        public void Initialize(BoardPiece piece)
        {
            Piece = piece;
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = RouteDrawing.Sprite(piece.IsCircle); _renderer.sortingOrder = 3;
            if (piece.IsCircle) Collider = gameObject.AddComponent<CircleCollider2D>();
            else { var box = gameObject.AddComponent<BoxCollider2D>(); box.size = Vector2.one; Collider = box; }
            _outline = RouteDrawing.Line(transform, "Selection outline", Color.white, 0.025f, 4);
            _outline.loop = true;
            Apply();
        }

        public void Flash() { _flash = 0.2f; }
        public void Apply(bool selected = false, bool valid = true)
        {
            transform.position = LogicalSpace.ToWorld(Piece.Position);
            transform.localScale = new Vector3((float)Piece.Size.X / 100, (float)Piece.Size.Y / 100, 1);
            transform.rotation = Quaternion.Euler(0, 0, (float)-Piece.Angle);
            _renderer.color = valid ? Color.Lerp(BaseColor, Color.white, _flash / 0.2f) : new Color(1, 0.28f, 0.3f);
            _outline.enabled = selected;
            if (!selected) return;
            var count = Piece.IsCircle ? 48 : 4;
            _outline.positionCount = count;
            for (var i = 0; i < count; i++)
            {
                Vector2 local;
                if (Piece.IsCircle) local = new Vector2(Mathf.Cos(i * Mathf.PI * 2 / count), Mathf.Sin(i * Mathf.PI * 2 / count)) * 0.58f;
                else local = new Vector2(i < 2 ? -0.65f : 0.65f, i == 0 || i == 3 ? -0.55f : 0.55f);
                _outline.SetPosition(i, transform.TransformPoint(local));
            }
        }
        public void TickVisual(float delta) { _flash = Mathf.Max(0, _flash - delta); }
    }

    public sealed class BoardRoutingQuery : IRoutingQuery
    {
        public RoutingContact Cast(RoutingShot shot, double distance)
        {
            if (shot.Lineage != null && shot.ExitGuards.Count > 0)
            {
                var touching = new HashSet<int>();
                foreach (var collider in Physics2D.OverlapCircleAll(LogicalSpace.ToWorld(shot.Position), (float)(RoutingShot.Radius + 0.05) / 100))
                { var view = collider.GetComponent<BoardPieceView>(); if (view != null) touching.Add(view.Piece.Id); }
                shot.ExitGuards.IntersectWith(touching);
            }
            var hits = Physics2D.CircleCastAll(LogicalSpace.ToWorld(shot.Position), (float)RoutingShot.Radius / 100,
                LogicalSpace.DirectionToWorld(shot.Velocity), (float)distance / 100);
            RaycastHit2D best = default;
            BoardPieceView chosen = null;
            foreach (var hit in hits)
            {
                var view = hit.collider.GetComponent<BoardPieceView>();
                if (view == null) continue;
                if (shot.Lineage != null ? shot.ExitGuards.Contains(view.Piece.Id) :
                    view.Piece.Kind == BoardPieceKind.Amplifier && shot.Amplifiers.Contains(view.Piece.Id)) continue;
                if (chosen == null || hit.distance < best.distance - 0.00001f ||
                    (Mathf.Abs(hit.distance - best.distance) <= 0.00001f && view.Piece.Id < chosen.Piece.Id))
                { best = hit; chosen = view; }
            }
            return chosen == null ? default : new RoutingContact(chosen.Piece.Id, chosen.Piece.Kind,
                best.distance * 100, LogicalSpace.ToLogical(best.point), LogicalSpace.DirectionToLogical(best.normal));
        }
    }
}
