using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class FreePlacementController : MonoBehaviour
    {
        public const string GameVersion = "0.2.0-placement";
        private readonly List<BoardPieceView> _views = new List<BoardPieceView>();
        private readonly List<Flight> _flights = new List<Flight>();
        private readonly List<FadingPath> _paths = new List<FadingPath>();
        private readonly List<BoardPiece> _pieces = new List<BoardPiece>();
        private readonly BoardRoutingQuery _query = new BoardRoutingQuery();
        private readonly ReloadState _reload = new ReloadState(0.65);
        private SimVector2 _lastAimDirection = new SimVector2(0, -1);
        private DeterministicRandom _random = new DeterministicRandom(20260828);
        private Camera _camera;
        private PrototypeAudio _audio;
        private LineRenderer _preview, _cone, _normal;
        private Transform _grid;
        private BoardPieceView _selected;
        private bool _dragging, _valid = true;
        private SimVector2 _originalPosition, _dragOffset;
        private double _originalAngle;
        private int _blockedUntilFrame, _shots, _hits, _lastGold, _lastCollector;
        private double _seconds, _gold;
        private string _message = "まずは撃ってみよう。紫を通ると次の報酬が2倍。";
        private string _lastRoute = "まだ発射していません";
        private GUIStyle _body, _title, _big, _small, _button;
        private Font _font;
        private bool _hasFocus = true;
        private readonly string _session = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
        private string _logPath;

        public bool Editing { get; private set; }
        public bool PersistenceEnabled { get; set; } = true;
        public double Gold => _gold;
        public int ActiveShotCount => _flights.Count + RecipeActiveCount;
        public double SimulatedSeconds => _seconds;
        public IReadOnlyList<BoardPiece> Pieces => _pieces;

        private void Awake()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-placement-capture") >= 0 || Array.IndexOf(Environment.GetCommandLineArgs(), "-recipe-capture") >= 0)
            { PersistenceEnabled = false; Application.runInBackground = true; }
            _camera = GetComponentInChildren<Camera>();
            if (_camera == null) _camera = Camera.main;
            _audio = GetComponent<PrototypeAudio>();
            Application.targetFrameRate = 120; Time.fixedDeltaTime = 1f / 60;
            _pieces.AddRange(FreePlacementBoard.CreateInitial());
            LoadLayout();
            BuildBoard();
            if (RecipeMode) { LoadRecipe(); _message = "Rで五発を編集。貫通は直列、分裂は扇状の配置を試そう。"; }
            _logPath = Path.Combine(Application.persistentDataPath, "placement-sessions", _session + ".log");
            Log($"start version={(RecipeMode ? RecipeVersion : GameVersion)} commit={BuildMetadata.CommitHash} seed=20260828");
        }

        private void BuildBoard()
        {
            RouteDrawing.Shape(transform, "Background", Vector2.zero, new Vector2(16, 9), new Color(0.035f, 0.055f, 0.08f), false, -20);
            RouteDrawing.Shape(transform, "Play area", LogicalSpace.ToWorld(new SimVector2(930, 445)), new Vector2(12.6f, 6.1f), new Color(0.055f, 0.086f, 0.115f), false, -15);
            var gridObj = new GameObject("Placement grid"); gridObj.transform.SetParent(transform); _grid = gridObj.transform;
            for (var x = 300; x <= 1560; x += 20)
                RouteDrawing.Shape(_grid, "Grid column", LogicalSpace.ToWorld(new SimVector2(x, 445)), new Vector2(0.006f, 6.1f), new Color(0.12f, 0.19f, 0.23f), false, -10);
            for (var y = 140; y <= 750; y += 20)
                RouteDrawing.Shape(_grid, "Grid row", LogicalSpace.ToWorld(new SimVector2(930, y)), new Vector2(12.6f, 0.006f), new Color(0.12f, 0.19f, 0.23f), false, -10);
            _grid.gameObject.SetActive(false);
            var edge = RouteDrawing.Line(transform, "Board boundary", new Color(0.18f, 0.29f, 0.35f), 0.016f, -5);
            edge.loop = true; edge.positionCount = 4;
            edge.SetPosition(0, LogicalSpace.ToWorld(new SimVector2(300, 140)));
            edge.SetPosition(1, LogicalSpace.ToWorld(new SimVector2(1560, 140)));
            edge.SetPosition(2, LogicalSpace.ToWorld(new SimVector2(1560, 750)));
            edge.SetPosition(3, LogicalSpace.ToWorld(new SimVector2(300, 750)));
            foreach (var piece in _pieces)
            {
                var obj = new GameObject($"{piece.Kind} {piece.Id}"); obj.transform.SetParent(transform);
                var view = obj.AddComponent<BoardPieceView>(); view.Initialize(piece); _views.Add(view);
            }
            RouteDrawing.Shape(transform, "Gun base", LogicalSpace.ToWorld(FreePlacementBoard.Gun), new Vector2(0.7f, 0.38f), new Color(1f, 0.72f, 0.3f), false, 2);
            RouteDrawing.Shape(transform, "Gun barrel", LogicalSpace.ToWorld(FreePlacementBoard.Gun + new SimVector2(0, -24)), new Vector2(0.16f, 0.36f), new Color(1f, 0.85f, 0.53f), false, 3);
            _preview = RouteDrawing.Line(transform, "Central ray preview", new Color(0.45f, 0.82f, 0.94f, 0.48f), 0.024f);
            _cone = RouteDrawing.Line(transform, "Actual spread bounds", new Color(1f, 0.72f, 0.3f, 0.42f), 0.013f);
            _normal = RouteDrawing.Line(transform, "Mirror face normal", new Color(1, 1, 1, 0.8f), 0.025f, 5);
            Physics2D.SyncTransforms();
        }

        public bool TryMovePiece(int id, SimVector2 position, double angle)
        {
            if (!Editing) return false;
            var view = _views.Find(v => v.Piece.Id == id);
            if (view == null) return false;
            var before = view.Piece.Position; var beforeAngle = view.Piece.Angle;
            view.Piece.Position = position;
            if (view.Piece.Kind == BoardPieceKind.Mirror) view.Piece.Angle = angle;
            if (!FreePlacementBoard.CanPlace(view.Piece, _pieces))
            { view.Piece.Position = before; view.Piece.Angle = beforeAngle; return false; }
            view.Apply(view == _selected); Physics2D.SyncTransforms(); return true;
        }

        public void SetEditing(bool editing)
        {
            if (RecipeEditing) SetRecipeEditing(false);
            if (editing == Editing) return;
            if (_dragging) EndDrag(false);
            Editing = editing;
            _blockedUntilFrame = Time.frameCount + 1;
            _grid.gameObject.SetActive(editing);
            if (editing)
            {
                EndRecipeRuns();
                // A layout change must not alter the targets encountered by a previously fired shot.
                foreach (var flight in _flights) { Destroy(flight.Path.gameObject); Destroy(flight.Head.gameObject); }
                _flights.Clear();
                _message = "装置をドラッグ。青いMirrorは Q / E またはホイールで回転。";
            }
            else { SaveLayout(); }
            Log(editing ? "editing_started" : "editing_finished");
        }

        private bool MouseLogical(out SimVector2 point)
        {
            point = default;
            if (_camera == null || !_camera.pixelRect.Contains(Input.mousePosition)) return false;
            point = LogicalSpace.ToLogical(_camera.ScreenToWorldPoint(Input.mousePosition)); return true;
        }
        private static bool OverUi(SimVector2 p) => p.X < 280 || p.Y < 120 || p.Y > 780;

        private void Update()
        {
            if (!_hasFocus) return;
            if (RecipeMode && Input.GetKeyDown(KeyCode.R)) SetRecipeEditing(!RecipeEditing);
            if (RecipeEditing)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SetRecipeEditing(false);
                _cone.enabled = _preview.enabled = _normal.enabled = false;
                return;
            }
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Tab)) SetEditing(!Editing);
            if (Input.GetKeyDown(KeyCode.Escape) && Editing)
            {
                if (_dragging) EndDrag(false); else SetEditing(false);
            }
            var overBoard = MouseLogical(out var mouse) && !OverUi(mouse);
            if (Editing)
            {
                if (Input.GetMouseButtonDown(0) && overBoard && Time.frameCount > _blockedUntilFrame)
                {
                    _selected = _views.Find(v => v.Collider.OverlapPoint(LogicalSpace.ToWorld(mouse)));
                    if (_selected != null)
                    {
                        _dragging = true; _originalPosition = _selected.Piece.Position; _originalAngle = _selected.Piece.Angle;
                        _dragOffset = _originalPosition - mouse;
                    }
                }
                if (_dragging && MouseLogical(out mouse))
                {
                    _selected.Piece.Position = FreePlacementBoard.Snap(mouse + _dragOffset);
                    _valid = FreePlacementBoard.CanPlace(_selected.Piece, _pieces) && !OverUi(mouse);
                }
                var turn = Input.GetKeyDown(KeyCode.E) ? 5 : Input.GetKeyDown(KeyCode.Q) ? -5 :
                    overBoard ? (int)Mathf.Sign(Input.mouseScrollDelta.y) * (Input.mouseScrollDelta.y == 0 ? 0 : 5) : 0;
                if (turn != 0) RotateSelected(turn);
                if (_dragging && Input.GetMouseButtonUp(0)) EndDrag(_valid);
            }
            else if (overBoard && Input.GetMouseButtonDown(0)) TryFire(mouse);
            foreach (var view in _views)
            {
                view.TickVisual(Time.deltaTime);
                view.Apply(Editing && view == _selected, view != _selected || !_dragging || _valid);
            }
            Physics2D.SyncTransforms();
            DrawAim(overBoard ? mouse : new SimVector2(800, 200), overBoard);
            for (var i = _paths.Count - 1; i >= 0; i--)
            {
                _paths[i].Remaining -= Time.deltaTime;
                if (_paths[i].Remaining <= 0) { Destroy(_paths[i].Line.gameObject); _paths.RemoveAt(i); }
            }
        }

        private void RotateSelected(double delta)
        {
            if (_selected == null || _selected.Piece.Kind != BoardPieceKind.Mirror) return;
            if (_dragging)
            { _selected.Piece.Angle = (_selected.Piece.Angle + delta) % 360; _valid = FreePlacementBoard.CanPlace(_selected.Piece, _pieces); }
            else if (!TryMovePiece(_selected.Piece.Id, _selected.Piece.Position, (_selected.Piece.Angle + delta) % 360))
                _message = "重なるため、この角度には回せません。";
        }

        private void EndDrag(bool accept)
        {
            if (!accept)
            { _selected.Piece.Position = _originalPosition; _selected.Piece.Angle = _originalAngle; _message = "配置できない位置です。元の場所へ戻しました。"; }
            else { _message = "配置しました。再開するとこの位置から撃てます。"; Log($"move id={_selected.Piece.Id} position={_selected.Piece.Position} angle={_selected.Piece.Angle}"); }
            _selected.Apply(true); _dragging = false; _valid = true; Physics2D.SyncTransforms();
        }

        public bool TryFire(SimVector2 aim)
        {
            if (RecipeMode) return TryRecipeFire(aim);
            if (Editing || !_hasFocus || Time.frameCount <= _blockedUntilFrame || !_reload.TryFire()) return false;
            var delta = aim - FreePlacementBoard.Gun;
            var direction = ResolveAim(delta);
            var offset = _random.NextSignedOffset(AimCalculator.GetSpreadDegrees(delta.Magnitude));
            var shot = new RoutingShot(FreePlacementBoard.Gun, AimCalculator.RotateDegrees(direction, offset) * 900);
            shot.Trace.Add(shot.Position);
            var path = RouteDrawing.Line(transform, "Shot " + ++_shots, new Color(1f, 0.78f, 0.32f), 0.055f, 6);
            var head = RouteDrawing.Shape(transform, "Projectile tip", LogicalSpace.ToWorld(shot.Position), Vector2.one * 0.16f, new Color(1, 0.93f, 0.66f), true, 7);
            _flights.Add(new Flight { Shot = shot, Path = path, Head = head });
            _audio?.PlayFire(); Log($"fire id={_shots} aim={aim} offset={offset:0.000}"); return true;
        }

        private void FixedUpdate() { if (_hasFocus) SimulateTick(Time.fixedDeltaTime); }
        public void SimulateTick(double seconds)
        {
            if (RecipeMode) { SimulateRecipeTick(seconds); return; }
            if (Editing) return;
            _seconds += seconds;
            if (_reload.Tick(seconds)) _audio?.PlayReady();
            Physics2D.SyncTransforms();
            for (var i = _flights.Count - 1; i >= 0; i--)
            {
                var flight = _flights[i]; var shot = flight.Shot;
                var count = shot.Contacts.Count;
                RoutingSimulation.Step(shot, seconds, _query);
                flight.Head.transform.position = LogicalSpace.ToWorld(shot.Position);
                for (var k = count; k < shot.Contacts.Count; k++) _views.Find(v => v.Piece.Id == shot.Contacts[k])?.Flash();
                UpdatePath(flight.Path, shot);
                if (shot.Alive) continue;
                if (shot.HitTarget != 0)
                {
                    _gold += shot.Gold; _hits++; _lastGold = shot.Gold; _lastCollector = shot.HitTarget;
                    _lastRoute = string.Join(" → ", shot.Contacts.ConvertAll(NameOf));
                    _message = $"+{shot.Gold} Gold  /  {_lastRoute}"; _audio?.PlayHit();
                }
                else { _lastRoute = string.Join(" → ", shot.Contacts.ConvertAll(NameOf)); _message = "次は少し狙いや配置を変えてみよう。"; }
                Log($"shot_finished reward={shot.Gold} collector={shot.HitTarget} mirrors={shot.Mirrors.Count} amplifiers={shot.Amplifiers.Count}");
                Destroy(flight.Head.gameObject);
                _paths.Add(new FadingPath { Line = flight.Path, Remaining = 0.65f }); _flights.RemoveAt(i);
            }
        }

        private static void UpdatePath(LineRenderer path, RoutingShot shot)
        {
            path.positionCount = shot.Trace.Count + 1;
            for (var j = 0; j < shot.Trace.Count; j++) path.SetPosition(j, LogicalSpace.ToWorld(shot.Trace[j]));
            path.SetPosition(shot.Trace.Count, LogicalSpace.ToWorld(shot.Position));
            var color = shot.Amplifiers.Count > 0 ? new Color(0.82f, 0.58f, 1f) :
                shot.Mirrors.Count > 0 ? new Color(0.3f, 0.85f, 1f) : new Color(1f, 0.78f, 0.32f);
            path.startColor = color; path.endColor = color;
        }

        private void DrawAim(SimVector2 aim, bool show)
        {
            _preview.enabled = Editing && show; _cone.enabled = !Editing && show; _normal.enabled = Editing && _selected != null && _selected.Piece.Kind == BoardPieceKind.Mirror;
            if (_normal.enabled)
            {
                var piece = _selected.Piece; var start = piece.Position; var end = start + piece.AxisX * 75;
                _normal.positionCount = 4;
                _normal.SetPosition(0, LogicalSpace.ToWorld(start)); _normal.SetPosition(1, LogicalSpace.ToWorld(end));
                _normal.SetPosition(2, LogicalSpace.ToWorld(end - piece.AxisX * 15 + piece.AxisY * 10));
                _normal.SetPosition(3, LogicalSpace.ToWorld(end));
            }
            if (!show) return;
            var delta = aim - FreePlacementBoard.Gun;
            var direction = ResolveAim(delta);
            if (Editing)
            {
                if (_selected != null && _selected.Piece.Kind == BoardPieceKind.Mirror) direction = (_selected.Piece.Position - FreePlacementBoard.Gun).Normalized;
                var ghost = new RoutingShot(FreePlacementBoard.Gun, direction * 900); ghost.Trace.Add(ghost.Position);
                for (var i = 0; i < 180 && ghost.Alive; i++) RoutingSimulation.Step(ghost, 1.0 / 30, _query);
                UpdatePath(_preview, ghost);
                _preview.startColor = _preview.endColor = new Color(0.6f, 0.85f, 1, 0.45f);
            }
            else
            {
                var spread = AimCalculator.GetSpreadDegrees(delta.Magnitude);
                _cone.positionCount = 3;
                _cone.SetPosition(0, LogicalSpace.ToWorld(FreePlacementBoard.Gun + AimCalculator.RotateDegrees(direction, -spread) * 300));
                _cone.SetPosition(1, LogicalSpace.ToWorld(FreePlacementBoard.Gun));
                _cone.SetPosition(2, LogicalSpace.ToWorld(FreePlacementBoard.Gun + AimCalculator.RotateDegrees(direction, spread) * 300));
            }
        }

        private string NameOf(int id) { var piece = _pieces.Find(p => p.Id == id); return piece == null ? "?" : piece.Kind == BoardPieceKind.Collector ? "Collector " + id : piece.Kind.ToString(); }

        private SimVector2 ResolveAim(SimVector2 delta)
        {
            if (delta.Magnitude >= 100) _lastAimDirection = delta.Normalized;
            return _lastAimDirection;
        }

        private void OnGUI()
        {
            if (_camera == null) return;
            EnsureStyles();
            if (Event.current.type == EventType.Repaint) _uiOverflows.Clear();
            var oldMatrix = GUI.matrix;
            var rect = _camera.pixelRect; var scale = rect.width / 1600;
            GUI.matrix = Matrix4x4.TRS(new Vector3(rect.x, Screen.height - rect.yMax, 0), Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(0, 0, 1600, 112), new Color(0.04f, 0.065f, 0.09f));
            Label(28, 20, 250, 35, RecipeMode ? "RECIPE LAB" : "ROUTE LAB", _title);
            Label(28, 64, 250, 30, RecipeMode ? "五発 × 自由配置 / 試作 2" : "自由配置 / 試作 1A", _small);
            if (!RecipeMode) {
            Label(310, 24, 750, 35, Editing ? "置いて、回して、経路をつくる。" : "自分の盤面を、一発が走る。", _title);
            Label(310, 66, 700, 26, Editing ? "配置中は時間停止。薄い線は散布なしの参考軌道。" : "左クリックで発射。遠くを狙うほど散布が狭まります。", _small);
            if (Button(new Rect(1270, 28, 280, 56), Editing ? "▶ 撃ってみる [B]" : "✦ 配置する [B]")) SetEditing(!Editing);
            } else DrawRecipeHeader();
            Panel(new Rect(18, 134, 254, 618), new Color(0.065f, 0.097f, 0.125f));
            Label(38, 155, 220, 28, "獲得した GOLD", _small);
            Label(38, 184, 220, 70, _gold.ToString("0"), _big);
            Label(38, 262, 210, 72, $"命中 {_hits} / {_shots} 発\n直近の報酬 +{_lastGold}", _body);
            Label(38, 349, 215, 32, "盤面の装置", _body);
            Label(38, 393, 220, 55, "● Collector ×2\n命中で 2 Gold", _small);
            Label(38, 468, 220, 55, "┃ Mirror\n反射後の報酬 ×1.5", _small);
            Label(38, 543, 220, 55, "▰ Amplifier\n通過後の次の報酬 ×2", _small);
            Label(38, 626, 215, 88, Editing ? "ドラッグ：移動\nQ / E・ホイール：回転\n赤い位置：配置できません" : RecipeMode ? RecipeStatus : (_reload.IsReady ? "● 発射できます" : $"装填中 {_reload.RemainingSeconds:0.00} 秒"), _small);
            foreach (var view in _views)
            {
                var p = view.Piece.Position;
                Label((float)p.X - 80, (float)p.Y - 18, 160, 36,
                    view.Piece.Kind == BoardPieceKind.Collector ? "2" : view.Piece.Kind == BoardPieceKind.Amplifier ? "×2" : "", _centerStyle);
                if (Editing && view == _selected)
                    Label((float)p.X - 110, (float)(p.Y + view.Piece.Size.Y / 2 + 14), 240, 32,
                        view.Piece.Kind == BoardPieceKind.Mirror ? $"Mirror  {view.Piece.Angle:0}°" : NameOf(view.Piece.Id), _small);
            }
            Panel(new Rect(0, 780, 730, 120), new Color(0.04f, 0.065f, 0.09f));
            Panel(new Rect(890, 780, 710, 120), new Color(0.04f, 0.065f, 0.09f));
            Label(28, 802, 690, 50, _message, _body);
            Label(940, 800, 620, 30, Editing ? "位置は20px刻み。重なり・盤面外は禁止。" : RecipeMode ? "R：弾の並び　B：配置　左クリック：一発" : "紫 → 緑で +4。反射も組み合わせると +6。", _small);
            Label(940, 836, 610, 52, $"{(RecipeMode ? RecipeVersion : GameVersion)}  /  {BuildMetadata.CommitHash}\nSeed 20260828", _small);
            if (Editing && Button(new Rect(38, 714, 214, 32), "配置を保存")) { SaveLayout(); _blockedUntilFrame = Time.frameCount + 1; }
            if (RecipeEditing) DrawRecipeEditor();
            GUI.matrix = oldMatrix;
        }
        private GUIStyle _centerStyle;
        private void EnsureStyles()
        {
            if (_body != null) return;
            _font = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic UI", "Meiryo", "Arial" }, 22);
            _body = new GUIStyle(GUI.skin.label) { font = _font, fontSize = 21, wordWrap = true, normal = { textColor = new Color(0.86f, 0.92f, 0.94f) } };
            _title = new GUIStyle(_body) { fontSize = 28, fontStyle = FontStyle.Bold };
            _big = new GUIStyle(_title) { fontSize = 56, normal = { textColor = new Color(1f, 0.79f, 0.4f) } };
            _small = new GUIStyle(_body) { fontSize = 18, normal = { textColor = new Color(0.62f, 0.73f, 0.79f) } };
            _button = new GUIStyle(GUI.skin.button) { font = _font, fontSize = 22, fontStyle = FontStyle.Bold };
            _centerStyle = new GUIStyle(_title) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.04f, 0.08f, 0.1f) } };
            if (RecipeMode)
            {
                _body.fontSize = RecipeUiText.BodySize; _title.fontSize = RecipeUiText.TitleSize;
                _big.fontSize = RecipeUiText.GoldSize; _small.fontSize = RecipeUiText.SmallSize;
                _button.fontSize = RecipeUiText.ButtonSize; _button.fontStyle = FontStyle.Normal;
                _button.wordWrap = true; _button.padding = new RectOffset(10, 10, 4, 4);
                _centerStyle.fontSize = RecipeUiText.TitleSize;
                foreach (var style in new[] { _body, _title, _big, _small, _centerStyle })
                { style.padding = new RectOffset(); style.margin = new RectOffset(); style.contentOffset = Vector2.zero; }
            }
        }
        private static void Panel(Rect rect, Color color) { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        private readonly Dictionary<GUIStyle, GUIStyle> _pixelStyles = new Dictionary<GUIStyle, GUIStyle>();
        private readonly List<string> _uiOverflows = new List<string>();
        public IReadOnlyList<string> UiOverflows => _uiOverflows;
        private float _textScale;
        private GUIStyle ScreenTextStyle(GUIStyle source, float scale)
        {
            if (!Mathf.Approximately(scale, _textScale)) { _pixelStyles.Clear(); _textScale = scale; }
            if (!_pixelStyles.TryGetValue(source, out var result))
            { result = RecipeUiText.PixelStyle(source, scale); _pixelStyles.Add(source, result); }
            return result;
        }
        private void CheckTextFits(Rect rect, string text, GUIStyle style)
        {
            if (Event.current.type != EventType.Repaint) return;
            var required = style.CalcHeight(new GUIContent(text), rect.width);
            if (required > rect.height + 1) _uiOverflows.Add($"{text.Replace('\n', ' ')} requires {required:0.0}px, has {rect.height:0.0}px");
        }
        private void Label(float x, float y, float w, float h, string text, GUIStyle style)
        {
            var rect = new Rect(x, y, w, h);
            if (!RecipeMode) { GUI.Label(rect, text, style); return; }
            var matrix = GUI.matrix;
            var pixelRect = RecipeUiText.PixelRect(rect, matrix);
            var pixelStyle = ScreenTextStyle(style, matrix.m00);
            GUI.matrix = Matrix4x4.identity;
            CheckTextFits(pixelRect, text, pixelStyle); GUI.Label(pixelRect, text, pixelStyle);
            GUI.matrix = matrix;
        }
        private bool Button(Rect rect, string text)
        {
            if (!RecipeMode) return GUI.Button(rect, text, _button);
            var matrix = GUI.matrix;
            var pixelRect = RecipeUiText.PixelRect(rect, matrix);
            var style = ScreenTextStyle(_button, matrix.m00);
            GUI.matrix = Matrix4x4.identity;
            CheckTextFits(pixelRect, text, style); var clicked = GUI.Button(pixelRect, text, style);
            GUI.matrix = matrix; return clicked;
        }

        [Serializable] private sealed class SavedLayout { public int version = 1; public List<SavedPiece> pieces = new List<SavedPiece>(); }
        [Serializable] private sealed class SavedPiece { public int id; public double x, y, angle; }
        private string LayoutPath => Path.Combine(Application.persistentDataPath, "placement-layout.json");
        private void SaveLayout()
        {
            if (!PersistenceEnabled) return;
            if (_dragging) { _message = "装置を置いてから保存してください。"; return; }
            try
            {
                var data = new SavedLayout();
                foreach (var piece in _pieces) data.pieces.Add(new SavedPiece { id = piece.Id, x = piece.Position.X, y = piece.Position.Y, angle = piece.Angle });
                var history = Path.Combine(Application.persistentDataPath, "placement-layout-history"); Directory.CreateDirectory(history);
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(Path.Combine(history, DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff") + ".json"), json);
                File.WriteAllText(LayoutPath, json); _message = "配置を保存しました。次回もこの配置から始まります。";
            }
            catch (Exception e) { _message = "配置を保存できませんでした。"; Debug.LogWarning("[Placement] Save failed: " + e.Message); }
        }
        private void LoadLayout()
        {
            if (!PersistenceEnabled) return;
            try
            {
                if (!File.Exists(LayoutPath)) return;
                var data = JsonUtility.FromJson<SavedLayout>(File.ReadAllText(LayoutPath));
                if (data == null || data.version != 1 || data.pieces == null || data.pieces.Count != _pieces.Count) return;
                var candidates = FreePlacementBoard.CreateInitial(); var ids = new HashSet<int>();
                foreach (var record in data.pieces)
                {
                    var piece = candidates.Find(p => p.Id == record.id);
                    if (piece == null || !ids.Add(record.id)) return;
                    piece.Position = new SimVector2(record.x, record.y);
                    piece.Angle = piece.Kind == BoardPieceKind.Mirror ? record.angle : 0;
                }
                foreach (var piece in candidates) if (!FreePlacementBoard.CanPlace(piece, candidates)) return;
                _pieces.Clear(); _pieces.AddRange(candidates);
            }
            catch (Exception e) { Debug.LogWarning("[Placement] Layout ignored: " + e.Message); }
        }
        private void Log(string message)
        {
            if (!PersistenceEnabled) return;
            if (string.IsNullOrEmpty(_logPath)) return;
            try { Directory.CreateDirectory(Path.GetDirectoryName(_logPath)); File.AppendAllText(_logPath, DateTime.UtcNow.ToString("O") + " " + message + Environment.NewLine); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        private void OnApplicationFocus(bool focus)
        {
            _hasFocus = focus || Array.IndexOf(Environment.GetCommandLineArgs(), "-placement-capture") >= 0 || Array.IndexOf(Environment.GetCommandLineArgs(), "-recipe-capture") >= 0;
            if (!focus && _dragging) EndDrag(false);
            _blockedUntilFrame = Time.frameCount + 1;
        }
        private void OnApplicationQuit() { if (_dragging) EndDrag(false); SaveLayout(); if (RecipeMode) SaveRecipe(); Log($"end shots={_shots} hits={_hits} gold={_gold}"); }

        // Opt-in standalone diagnostic. Does not load or save the user's layout or session logs.
        private IEnumerator Start()
        {
            if (RecipeMode) { yield return RecipeDiagnostic(); yield break; }
            var args = Environment.GetCommandLineArgs();
            var flag = Array.IndexOf(args, "-placement-capture");
            if (flag < 0 || flag + 1 >= args.Length) yield break;
            var folder = args[flag + 1]; Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(1);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "01-play.png"));
            yield return new WaitForSecondsRealtime(0.3f);
            _hasFocus = true;
            TryFire(new SimVector2(800, 220));
            for (var i = 0; i < 60; i++) SimulateTick(1.0 / 60);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "02-hit.png"));
            yield return new WaitForSecondsRealtime(0.3f);
            SetEditing(true); _selected = _views[2];
            var validMove = TryMovePiece(3, new SimVector2(600, 560), -35);
            var invalidMove = TryMovePiece(3, new SimVector2(800, 220), 0);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "03-edit.png"));
            CaptureBoard(Path.Combine(folder, "04-board-offscreen.png"));
            yield return new WaitForSecondsRealtime(0.3f);
            File.WriteAllText(Path.Combine(folder, "smoke.txt"), $"gold={Gold}; validMove={validMove}; invalidMoveRejected={!invalidMove}; editing={Editing}");
            Application.Quit(Gold == 4 && validMove && !invalidMove ? 0 : 1);
        }

        private void CaptureBoard(string path)
        {
            var target = RenderTexture.GetTemporary(1600, 900, 24);
            var previous = RenderTexture.active;
            var previousTarget = _camera.targetTexture;
            var pixels = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                _camera.targetTexture = target; _camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally { _camera.targetTexture = previousTarget; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); Destroy(pixels); }
        }
        private sealed class Flight { public RoutingShot Shot; public LineRenderer Path; public SpriteRenderer Head; }
        private sealed class FadingPath { public LineRenderer Line; public float Remaining; }
    }
}
