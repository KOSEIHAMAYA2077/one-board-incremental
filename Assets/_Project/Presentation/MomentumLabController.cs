using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed class MomentumLabController : MonoBehaviour
    {
        public const string GameVersion = "0.4.0-momentum";
        public MomentumSimulation Simulation { get; private set; }
        public bool PersistenceEnabled { get; set; } = true;
        private Camera _camera; private PrototypeAudio _audio;
        private readonly Dictionary<int, TargetView> _targets = new Dictionary<int, TargetView>();
        private readonly Dictionary<int, BallView> _balls = new Dictionary<int, BallView>();
        private readonly List<Popup> _popups = new List<Popup>();
        private SpriteRenderer _zone, _gunBarrel;
        private LineRenderer _zoneRing, _aimLine;
        private GUIStyle _body, _small, _title, _gold, _button, _targetLabel;
        private Font _font;
        private readonly Dictionary<GUIStyle, GUIStyle> _scaled = new Dictionary<GUIStyle, GUIStyle>();
        private readonly List<string> _overflows = new List<string>();
        private float _scale;
        private bool _focus = true, _diagnostic;
        private int _blockedFrame, _heardShots, _lastGold;
        private string _message = "一クリックで三発。水色の動くゾーンに通してみよう。";
        private string _logPath;
        private readonly string _session = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff");
        public IReadOnlyList<string> UiOverflows => _overflows;

        private void Awake()
        {
            _diagnostic = Array.IndexOf(Environment.GetCommandLineArgs(), "-momentum-capture") >= 0;
            if (_diagnostic) { PersistenceEnabled = false; Application.runInBackground = true; }
            Simulation = new MomentumSimulation();
            _camera = GetComponentInChildren<Camera>(); _audio = GetComponent<PrototypeAudio>();
            Time.fixedDeltaTime = 1f / 60; Application.targetFrameRate = 120;
            LoadMagazine(); BuildBoard();
            _logPath = Path.Combine(Application.persistentDataPath, "sessions", _session + ".log");
            Log($"start version={GameVersion} commit={BuildMetadata.CommitHash} seed={Simulation.Seed}");
            SyncViews();
        }
        private void BuildBoard()
        {
            RouteDrawing.Shape(transform, "Background", Vector2.zero, new Vector2(16, 9), new Color(.035f, .05f, .07f), false, -20);
            RouteDrawing.Shape(transform, "Field", LogicalSpace.ToWorld(new SimVector2(940, 440)), new Vector2(12.4f, 6.4f), new Color(.05f, .08f, .105f), false, -15);
            var border = RouteDrawing.Line(transform, "Reflecting boundary", new Color(.23f, .38f, .45f), .035f, 0);
            border.loop = true; border.positionCount = 4;
            border.SetPositions(new[] { World(320, 120), World(1560, 120), World(1560, 760), World(320, 760) });
            foreach (var obstacle in Simulation.Obstacles)
            {
                RouteDrawing.Shape(transform, "Reflecting obstacle", LogicalSpace.ToWorld(obstacle.Position), Vector2.one * .52f, new Color(.26f, .34f, .42f), true, 2);
                CircleLine("Obstacle rim", obstacle.Position, 26, new Color(.65f, .74f, .84f), .025f, 3);
            }
            foreach (var target in Simulation.Targets)
            {
                var shape = RouteDrawing.Shape(transform, "Target " + target.Id, LogicalSpace.ToWorld(target.Position), Vector2.one * (float)(target.Radius * 2 / 100), TargetColor(target), true, 3);
                var ring = CircleLine("Armor rim", target.Position, target.Radius + 5, new Color(1, .65f, .35f), .035f, 4);
                _targets[target.Id] = new TargetView { Shape = shape, Ring = ring };
            }
            _zone = RouteDrawing.Shape(transform, "Moving speed zone", LogicalSpace.ToWorld(Simulation.ZonePosition), Vector2.one * 1.3f, new Color(.15f, .82f, 1, .12f), true, 1);
            _zoneRing = CircleLine("Speed zone boundary", Simulation.ZonePosition, 65, new Color(.2f, .85f, 1, .75f), .025f, 2);
            RouteDrawing.Shape(transform, "Gun", LogicalSpace.ToWorld(MomentumRules.Gun), new Vector2(.55f, .32f), new Color(1, .76f, .32f), false, 4);
            _gunBarrel = RouteDrawing.Shape(transform, "Barrel", LogicalSpace.ToWorld(MomentumRules.Gun + new SimVector2(0, -15)), new Vector2(.12f, .36f), new Color(1, .85f, .5f), false, 5);
            _aimLine = RouteDrawing.Line(transform, "Aim guide", new Color(1, .79f, .4f, .4f), .015f, 2);
        }
        private static Vector3 World(double x, double y) => LogicalSpace.ToWorld(new SimVector2(x, y));
        private static Color TargetColor(MomentumTarget t) => t.Armored ? new Color(.56f, .26f, .16f) : new Color(.16f, .48f, .33f);
        private static Color AmmoColor(MomentumAmmo ammo) => ammo == MomentumAmmo.Normal ? new Color(1, .79f, .38f) : new Color(.48f, .75f, 1);
        private LineRenderer CircleLine(string name, SimVector2 center, double radius, Color color, float width, int order)
        { var line = RouteDrawing.Line(transform, name, color, width, order); line.loop = true; SetCircle(line, center, radius); return line; }
        private static void SetCircle(LineRenderer line, SimVector2 center, double radius)
        {
            line.positionCount = 48;
            for (var i = 0; i < 48; i++) line.SetPosition(i, LogicalSpace.ToWorld(center + new SimVector2(Math.Cos(i * Math.PI / 24), Math.Sin(i * Math.PI / 24)) * radius));
        }
        public void SetEditing(bool editing)
        {
            Simulation.SetEditing(editing); _blockedFrame = Time.frameCount + 1;
            if (!editing) SaveMagazine();
            Log(editing ? "edit_started" : "edit_finished");
        }
        public bool TryFire(SimVector2 aim)
        {
            if (!_focus || Time.frameCount <= _blockedFrame || !Simulation.TryFire(aim)) return false;
            Log($"magazine id={Simulation.MagazineCount} aim={aim} slots={string.Join(",", Simulation.Magazine)}");
            SyncViews(); return true;
        }
        private bool MouseAim(out SimVector2 aim)
        {
            aim = default;
            if (_camera == null || !_camera.pixelRect.Contains(Input.mousePosition)) return false;
            aim = LogicalSpace.ToLogical(_camera.ScreenToWorldPoint(Input.mousePosition));
            return aim.X >= 320 && aim.X <= 1560 && aim.Y >= 120 && aim.Y <= 760;
        }
        private void Update()
        {
            if (!_focus) return;
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape)) SetEditing(!Simulation.Editing);
            var over = MouseAim(out var aim);
            if (!Simulation.Editing && over && Input.GetMouseButtonDown(0)) TryFire(aim);
            _aimLine.enabled = over && !Simulation.Editing;
            if (_aimLine.enabled)
            {
                var dir = (aim - MomentumRules.Gun).Normalized;
                _aimLine.positionCount = 2; _aimLine.SetPosition(0, LogicalSpace.ToWorld(MomentumRules.Gun));
                _aimLine.SetPosition(1, LogicalSpace.ToWorld(MomentumRules.Gun + dir * 150));
                _gunBarrel.transform.position = LogicalSpace.ToWorld(MomentumRules.Gun + dir * 16);
                _gunBarrel.transform.rotation = Quaternion.Euler(0, 0, (float)(-Math.Atan2(dir.Y, dir.X) * 180 / Math.PI - 90));
            }
        }
        private void FixedUpdate() { if (_focus) StepSimulation(1.0 / 60); }
        public void StepSimulation(double seconds)
        {
            Simulation.Tick(seconds);
            if (Simulation.Editing) return;
            foreach (var e in Simulation.Events)
            {
                _popups.Add(new Popup { Position = e.Position, Kind = e.Kind, Amount = e.Amount });
                if (e.Kind == "hit") _audio?.PlayHit();
                if (e.Kind == "destroy") { _lastGold = (int)e.Amount; _message = $"撃破 +{_lastGold} Gold。壊れた的は次の斉射で別の位置へ。"; }
                if (e.Kind == "boost") _message = $"加速！ 速度 {e.Amount:0}。この弾は奥まで届きやすくなります。";
                Log($"{e.Kind} target={e.Target} amount={e.Amount:0.00}");
            }
            for (var i = _popups.Count - 1; i >= 0; i--)
            { _popups[i].Time -= seconds; if (_popups[i].Time <= 0) _popups.RemoveAt(i); }
            SyncViews();
        }
        private void SyncViews()
        {
            if (Simulation.FiredCount != _heardShots) { _heardShots = Simulation.FiredCount; _audio?.PlayFire(); }
            foreach (var target in Simulation.Targets)
            {
                var v = _targets[target.Id]; v.Shape.enabled = target.Alive; v.Ring.enabled = target.Alive && target.Armored;
                v.Shape.transform.position = LogicalSpace.ToWorld(target.Position);
                v.Shape.color = Color.Lerp(new Color(.17f, .22f, .26f), TargetColor(target), (float)(target.Hp / target.MaximumHp));
                SetCircle(v.Ring, target.Position, target.Radius + 5);
            }
            _zone.transform.position = LogicalSpace.ToWorld(Simulation.ZonePosition); SetCircle(_zoneRing, Simulation.ZonePosition, 65);
            var alive = new HashSet<int>();
            foreach (var ball in Simulation.Balls)
            {
                alive.Add(ball.Id);
                if (!_balls.TryGetValue(ball.Id, out var view))
                {
                    view = new BallView { Head = RouteDrawing.Shape(transform, "Ball " + ball.Id, LogicalSpace.ToWorld(ball.Position), Vector2.one * .16f, AmmoColor(ball.Ammo), true, 7),
                        Trail = RouteDrawing.Line(transform, "Speed trail", AmmoColor(ball.Ammo), .035f, 6) };
                    _balls.Add(ball.Id, view);
                }
                view.Head.transform.position = LogicalSpace.ToWorld(ball.Position);
                var color = ball.Boosted ? Color.Lerp(AmmoColor(ball.Ammo), Color.white, .5f) : AmmoColor(ball.Ammo);
                view.Head.color = color; view.Trail.startColor = new Color(color.r, color.g, color.b, .15f); view.Trail.endColor = color;
                view.Points.Enqueue(LogicalSpace.ToWorld(ball.Position)); while (view.Points.Count > 18) view.Points.Dequeue();
                view.Trail.positionCount = view.Points.Count; view.Trail.SetPositions(view.Points.ToArray());
            }
            var remove = new List<int>();
            foreach (var item in _balls) if (!alive.Contains(item.Key)) { Destroy(item.Value.Head.gameObject); Destroy(item.Value.Trail.gameObject); remove.Add(item.Key); }
            foreach (var id in remove) _balls.Remove(id);
        }
        private static string AmmoName(MomentumAmmo ammo) => ammo == MomentumAmmo.Normal ? "通常" : "貫通";
        private void EnsureStyles()
        {
            if (_body != null) return;
            _font = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic UI", "Meiryo", "Arial" }, 16);
            _body = new GUIStyle { font = _font, fontSize = 16, wordWrap = true, normal = { textColor = new Color(.85f, .9f, .94f) } };
            _small = new GUIStyle(_body) { fontSize = 13, normal = { textColor = new Color(.64f, .75f, .82f) } };
            _title = new GUIStyle(_body) { fontSize = 21, fontStyle = FontStyle.Bold };
            _gold = new GUIStyle(_title) { fontSize = 38, normal = { textColor = new Color(1, .79f, .38f) } };
            _button = new GUIStyle(GUI.skin.button) { font = _font, fontSize = 16, wordWrap = true, padding = new RectOffset(8, 8, 4, 4) };
            _targetLabel = new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(.93f, .96f, .98f) } };
        }
        private void OnGUI()
        {
            if (_camera == null || Simulation == null) return;
            EnsureStyles(); if (Event.current.type == EventType.Repaint) _overflows.Clear();
            var prior = GUI.matrix; var r = _camera.pixelRect;
            GUI.matrix = Matrix4x4.TRS(new Vector3(r.x, Screen.height - r.yMax, 0), Quaternion.identity, Vector3.one * (r.width / 1600));
            Panel(new Rect(0, 0, 1600, 100), new Color(.04f, .06f, .085f));
            Label(28, 18, 290, 36, "MOMENTUM LAB", _title);
            Label(28, 60, 290, 28, "速度を使い切るマガジン", _small);
            for (var i = 0; i < 3; i++)
            {
                var x = 350 + i * 165;
                Panel(new Rect(x, 12, 150, 76), new Color(.1f, .15f, .2f));
                Label(x + 12, 16, 125, 25, "弾倉 " + (i + 1), _small);
                Label(x + 12, 43, 125, 34, AmmoName(Simulation.Magazine[i]), _title);
            }
            Label(865, 20, 420, 32, Simulation.Editing ? "編集：すべて停止中" : Simulation.Bursting ? $"斉射中：残り {Simulation.RemainingInBurst} 発" : Simulation.Ready ? "クリックで三発発射" : $"リロード {Simulation.ReloadRemaining:0.0} 秒", _body);
            Label(865, 58, 400, 26, "発射間隔 0.12秒 / 撃ち切り後 0.8秒", _small);
            if (Button(new Rect(1280, 25, 280, 48), Simulation.Editing ? "再開 [R]" : "弾倉を編集 [R]")) SetEditing(!Simulation.Editing);
            Panel(new Rect(20, 120, 275, 640), new Color(.06f, .09f, .12f));
            Label(40, 142, 235, 28, "獲得 GOLD", _small); Label(40, 177, 235, 65, Simulation.Gold.ToString(), _gold);
            Label(40, 263, 235, 58, $"斉射 {Simulation.MagazineCount} 回 / 発射 {Simulation.FiredCount} 発\n撃破 {Simulation.DestroyedCount} / 加速 {Simulation.BoostCount}", _body);
            Label(40, 340, 235, 34, "弾のちがい", _title);
            Label(40, 387, 235, 62, "通常：一撃が強い\n威力40 / 抵抗をそのまま受ける", _small);
            Label(40, 469, 235, 62, "貫通：勢いを失いにくい\n威力26 / 抵抗による減速は1/4", _small);
            Label(40, 555, 235, 83, "青い円：速度 ×2\n速度が高いほど威力も上昇\n同じ弾への加速は一回まで", _small);
            Label(40, 664, 235, 72, $"飛行中 {Simulation.Balls.Count} 発\n速度80以下で消滅\n最大速度1800 / 時間減速あり", _small);
            foreach (var target in Simulation.Targets)
            {
                if (!target.Alive) continue; var p = target.Position;
                Label((float)p.X - 72, (float)p.Y - 23, 144, 25, target.Armored ? "装甲" : "通常", _targetLabel);
                Label((float)p.X - 72, (float)p.Y + 1, 144, 25, $"{target.Hp:0} / {target.MaximumHp:0}", _targetLabel);
                Label((float)p.X - 72, (float)(p.Y + target.Radius + 10), 144, 24, $"抵抗 {target.Resistance:0}", _targetLabel);
            }
            var zone = Simulation.ZonePosition;
            Label((float)zone.X - 54, (float)zone.Y - 14, 120, 30, "速度 ×2", _body);
            for (var i = Math.Max(0, Simulation.Balls.Count - 3); i < Simulation.Balls.Count; i++)
            {
                var b = Simulation.Balls[i]; Label(Mathf.Clamp((float)b.Position.X + 12, 330, 1460), Mathf.Clamp((float)b.Position.Y - 25, 124, 710), 95, 25, $"{b.Speed:0}", _small);
            }
            foreach (var pop in _popups)
                Label((float)pop.Position.X - 50, (float)pop.Position.Y - 55 - (float)(1 - pop.Time) * 25, 180, 32,
                    pop.Kind == "hit" ? $"−{pop.Amount:0}" : pop.Kind == "destroy" ? $"+{pop.Amount:0} Gold" : "加速 ×2", _body);
            Panel(new Rect(0, 780, 1600, 120), new Color(.04f, .06f, .085f));
            Label(30, 802, 850, 65, _message, _body);
            Label(1020, 796, 540, 28, "クリック：三発　R：弾倉編集／停止", _small);
            Label(1020, 835, 540, 53, $"{GameVersion} / {BuildMetadata.CommitHash}\nSeed {Simulation.Seed}", _small);
            if (Simulation.Editing) DrawEditor();
            GUI.matrix = prior;
        }
        private void DrawEditor()
        {
            Panel(new Rect(310, 110, 1280, 660), new Color(.025f, .04f, .065f, .97f));
            Label(395, 188, 1050, 44, "一クリックで、この三発を撃ち切る。", _title);
            Label(395, 257, 1020, 75, "通常は一撃重視。貫通は抵抗に強く、奥まで届く。\nクリックで弾を切り替え。飛行中の弾と加速ゾーンは停止しています。", _body);
            for (var i = 0; i < 3; i++)
            {
                var x = 395 + i * 360;
                if (Button(new Rect(x, 376, 320, 100), $"{i + 1}　{AmmoName(Simulation.Magazine[i])}\nクリックで切り替え"))
                {
                    var slots = new[] { Simulation.Magazine[0], Simulation.Magazine[1], Simulation.Magazine[2] };
                    slots[i] = slots[i] == MomentumAmmo.Normal ? MomentumAmmo.Pierce : MomentumAmmo.Normal;
                    Simulation.SetMagazine(slots);
                }
            }
            Label(395, 523, 1040, 62, "速度が高いほど威力が上がり、的を抜けやすくなる。\n硬い的も通常弾で削れる。加速ゾーンを通すルートも試そう。", _body);
            if (Button(new Rect(1135, 652, 300, 54), "この弾倉で再開 [R]")) SetEditing(false);
        }
        private static void Panel(Rect rect, Color color)
        { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        private GUIStyle PixelStyle(GUIStyle style, float scale)
        {
            if (!Mathf.Approximately(scale, _scale)) { _scaled.Clear(); _scale = scale; }
            if (!_scaled.TryGetValue(style, out var pixel)) { pixel = RecipeUiText.PixelStyle(style, scale); _scaled.Add(style, pixel); }
            return pixel;
        }
        private void Check(Rect rect, string text, GUIStyle style)
        { if (Event.current.type == EventType.Repaint && style.CalcHeight(new GUIContent(text), rect.width) > rect.height + 1) _overflows.Add(text.Replace('\n', ' ')); }
        private void Label(float x, float y, float w, float h, string text, GUIStyle style)
        {
            var matrix = GUI.matrix; var rect = RecipeUiText.PixelRect(new Rect(x, y, w, h), matrix); var pixel = PixelStyle(style, matrix.m00);
            GUI.matrix = Matrix4x4.identity; Check(rect, text, pixel); GUI.Label(rect, text, pixel); GUI.matrix = matrix;
        }
        private bool Button(Rect logical, string text)
        {
            var matrix = GUI.matrix; var rect = RecipeUiText.PixelRect(logical, matrix); var style = PixelStyle(_button, matrix.m00);
            GUI.matrix = Matrix4x4.identity; Check(rect, text, style); var clicked = GUI.Button(rect, text, style); GUI.matrix = matrix; return clicked;
        }
        [Serializable] private sealed class SavedMagazine { public int schemaVersion = 1; public MomentumAmmo[] slots; }
        private string MagazinePath => Path.Combine(Application.persistentDataPath, "magazine.json");
        private void SaveMagazine()
        {
            if (!PersistenceEnabled) return;
            try
            {
                var history = Path.Combine(Application.persistentDataPath, "magazine-history"); Directory.CreateDirectory(history);
                var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff");
                if (File.Exists(MagazinePath)) File.Copy(MagazinePath, Path.Combine(history, stamp + "-previous.json"));
                var data = new SavedMagazine { slots = new[] { Simulation.Magazine[0], Simulation.Magazine[1], Simulation.Magazine[2] } };
                var json = JsonUtility.ToJson(data, true); File.WriteAllText(Path.Combine(history, stamp + ".json"), json); File.WriteAllText(MagazinePath, json);
            }
            catch (Exception e) { Debug.LogWarning("[Momentum] Save failed: " + e.Message); _message = "弾倉を保存できませんでした。"; }
        }
        private void LoadMagazine()
        {
            if (!PersistenceEnabled || !File.Exists(MagazinePath)) return;
            try
            {
                var data = JsonUtility.FromJson<SavedMagazine>(File.ReadAllText(MagazinePath));
                Simulation.SetEditing(true);
                if (data == null || data.schemaVersion != 1 || !Simulation.SetMagazine(data.slots)) Debug.LogWarning("[Momentum] Invalid magazine preserved; using initial values.");
            }
            catch (Exception e) { Debug.LogWarning("[Momentum] Load failed: " + e.Message); }
            finally { Simulation.SetEditing(false); }
        }
        private void Log(string message)
        {
            if (!PersistenceEnabled || _logPath == null) return;
            try { Directory.CreateDirectory(Path.GetDirectoryName(_logPath)); File.AppendAllText(_logPath, DateTime.UtcNow.ToString("O") + " " + message + Environment.NewLine); }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        private void OnApplicationFocus(bool focus) { _focus = focus || _diagnostic; _blockedFrame = Time.frameCount + 1; }
        private void OnApplicationQuit() { SaveMagazine(); Log($"end gold={Simulation.Gold} magazines={Simulation.MagazineCount} fired={Simulation.FiredCount}"); }
        private IEnumerator Start()
        {
            if (!_diagnostic) yield break;
            var args = Environment.GetCommandLineArgs(); var index = Array.IndexOf(args, "-momentum-capture");
            if (index + 1 >= args.Length) yield break;
            var folder = args[index + 1]; Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(1); yield return new WaitForEndOfFrame();
            var playingOverflow = string.Join("\n", _overflows);
            _focus = true; _blockedFrame = -1;
            TryFire(MomentumRules.ZoneAt(Simulation.Time + .2));
            for (var i = 0; i < 35; i++) StepSimulation(1.0 / 60);
            var three = Simulation.FiredCount == 3; var boost = Simulation.BoostCount;
            CaptureBoard(Path.Combine(folder, "01-flight-board.png"));
            SetEditing(true); var before = Simulation.Time; StepSimulation(1.0 / 60); var paused = before == Simulation.Time;
            yield return new WaitForEndOfFrame(); var editorOverflow = string.Join("\n", _overflows);
            SetEditing(false); for (var i = 0; i < 800; i++) StepSimulation(1.0 / 60);
            File.WriteAllText(Path.Combine(folder, "smoke.txt"), $"screen={Screen.width}x{Screen.height}; fired={Simulation.FiredCount}; boosts={boost}; paused={paused}; remaining={Simulation.Balls.Count}; gold={Simulation.Gold}\nPlaying overflow: {playingOverflow}\nEditor overflow: {editorOverflow}");
            yield return new WaitForSecondsRealtime(.2f);
            Application.Quit(three && boost > 0 && paused && Simulation.Balls.Count == 0 && playingOverflow.Length == 0 && editorOverflow.Length == 0 ? 0 : 1);
        }
        private void CaptureBoard(string path)
        {
            var rt = RenderTexture.GetTemporary(1920, 1080, 24); var old = RenderTexture.active; var cameraTarget = _camera.targetTexture;
            var texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try { _camera.targetTexture = rt; _camera.Render(); RenderTexture.active = rt; texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); }
            finally { _camera.targetTexture = cameraTarget; RenderTexture.active = old; RenderTexture.ReleaseTemporary(rt); Destroy(texture); }
        }
        private sealed class TargetView { public SpriteRenderer Shape; public LineRenderer Ring; }
        private sealed class BallView { public SpriteRenderer Head; public LineRenderer Trail; public readonly Queue<Vector3> Points = new Queue<Vector3>(); }
        private sealed class Popup { public SimVector2 Position; public string Kind; public double Amount; public double Time = 1; }
    }
}
