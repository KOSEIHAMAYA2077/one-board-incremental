using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class FreePlacementController
    {
        public const string RecipeVersion = "0.3.0-recipe";
        [SerializeField] public bool RecipeMode;
        public bool RecipeEditing { get; private set; }
        public RecipeCycle Recipe { get; private set; } = new RecipeCycle();
        private RecipeBullet[] _draft, _undo;
        private readonly List<RecipeRunView> _recipeRuns = new List<RecipeRunView>();
        private readonly List<RewardPopup> _rewardPopups = new List<RewardPopup>();
        private int _lastLineageYield;
        private int RecipeActiveCount { get { var count = 0; foreach (var run in _recipeRuns) count += run.Lineage.ActiveCount; return count; } }
        private string RecipeStatus => RecipeEditing ? "Recipe編集中：時間停止" : Recipe.Ready ? "● " + BulletName(Recipe.Current) + " を発射\nPrimer " + Recipe.Primer : $"装填中 {Recipe.Remaining:0.00} 秒";
        private static string BulletName(RecipeBullet type) => type == RecipeBullet.Normal ? "通常" : type == RecipeBullet.Pierce ? "貫通" : "分裂";
        private static Color BulletColor(RecipeBullet type) => type == RecipeBullet.Normal ? new Color(1, .78f, .32f) : type == RecipeBullet.Pierce ? new Color(.3f, .85f, 1) : new Color(1, .46f, .76f);

        public void SetRecipeEditing(bool editing)
        {
            if (!RecipeMode || RecipeEditing == editing) return;
            if (Editing) SetEditing(false);
            RecipeEditing = editing; _blockedUntilFrame = Time.frameCount + 1;
            if (editing) { _draft = RecipeCycle.Copy(Recipe.Pending ?? Recipe.Active); _undo = null; }
            Log(editing ? "recipe_edit_started" : "recipe_edit_closed");
        }
        public bool ApplyRecipe(IReadOnlyList<RecipeBullet> slots)
        {
            if (!RecipeMode || !RecipeEditing || !Recipe.Apply(slots)) return false;
            SaveRecipe(); Log("recipe_applied slots=" + string.Join(",", RecipeCycle.Copy(slots)));
            SetRecipeEditing(false);
            _message = Recipe.Pending == null ? "Recipeを適用しました。" : "予約しました。現在の五発を終えると切り替わります。";
            return true;
        }
        private bool TryRecipeFire(SimVector2 aim)
        {
            if (Editing || RecipeEditing || !_hasFocus || Time.frameCount <= _blockedUntilFrame || !Recipe.Ready) return false;
            var type = Recipe.Current; var primer = Recipe.Primer;
            if (!Recipe.TryFire()) return false;
            var delta = aim - FreePlacementBoard.Gun;
            var offset = _random.NextSignedOffset(AimCalculator.GetSpreadDegrees(delta.Magnitude));
            var lineage = new RecipeLineage(type, primer, FreePlacementBoard.Gun, AimCalculator.RotateDegrees(ResolveAim(delta), offset) * 900);
            var run = new RecipeRunView { Lineage = lineage, Id = ++_shots };
            _recipeRuns.Add(run); RefreshRecipeView(run); _audio?.PlayFire();
            Log($"fire id={run.Id} cycle={Recipe.CycleId} slot={Recipe.Slot} bullet={type} primer={primer} offset={offset:0.000}");
            return true;
        }
        private void SimulateRecipeTick(double seconds)
        {
            if (Editing || RecipeEditing) return;
            _seconds += seconds;
            if (Recipe.Tick(seconds)) _audio?.PlayReady();
            Physics2D.SyncTransforms();
            for (var i = 0; i < _recipeRuns.Count;)
            {
                var run = _recipeRuns[i]; run.Lineage.Tick(seconds, _query);
                RefreshRecipeView(run);
                if (run.Lineage.Complete) { FinishRecipeRun(run); _recipeRuns.RemoveAt(i); } else i++;
            }
            for (var i = _rewardPopups.Count - 1; i >= 0; i--)
            { _rewardPopups[i].Remaining -= seconds; if (_rewardPopups[i].Remaining <= 0) _rewardPopups.RemoveAt(i); }
        }
        private void RefreshRecipeView(RecipeRunView run)
        {
            foreach (var shot in run.Lineage.Projectiles)
            {
                if (!run.Views.TryGetValue(shot, out var view))
                {
                    view = new RecipeProjectileView
                    {
                        Path = RouteDrawing.Line(transform, $"Lineage {run.Id} / {shot.SpawnSequence}", BulletColor(shot.Bullet), .045f, 6),
                        Head = RouteDrawing.Shape(transform, "Recipe projectile", LogicalSpace.ToWorld(shot.Position), Vector2.one * .16f, BulletColor(shot.Bullet), true, 7)
                    };
                    run.Views.Add(shot, view);
                }
                UpdatePath(view.Path, shot); view.Path.startColor = view.Path.endColor = BulletColor(shot.Bullet);
                view.Head.transform.position = LogicalSpace.ToWorld(shot.Position); view.Head.enabled = shot.Alive;
                for (var j = view.Contacts; j < shot.Contacts.Count; j++) _views.Find(v => v.Piece.Id == shot.Contacts[j])?.Flash();
                view.Contacts = shot.Contacts.Count;
            }
            while (run.RewardIndex < run.Lineage.Rewards.Count)
            {
                var reward = run.Lineage.Rewards[run.RewardIndex++];
                _gold += reward.Gold; _hits++; _lastGold = reward.Gold; _audio?.PlayHit();
                _rewardPopups.Add(new RewardPopup { Position = _pieces.Find(p => p.Id == reward.Target).Position, Gold = reward.Gold });
                Log($"hit lineage={run.Id} projectile={reward.Projectile} target={reward.Target} gold={reward.Gold}");
            }
        }
        private void FinishRecipeRun(RecipeRunView run)
        {
            _lastLineageYield = run.Lineage.YieldGold;
            _message = $"一発の合計 +{_lastLineageYield} Gold  /  生成 {run.Lineage.Projectiles.Count} 弾";
            if (run.Lineage.LimitedCount > 0) _message += $"  LIMIT {run.Lineage.LimitedCount}";
            Log($"lineage_finished id={run.Id} yield={_lastLineageYield} generated={run.Lineage.Projectiles.Count} limited={run.Lineage.LimitedCount}");
            foreach (var view in run.Views.Values)
            { Destroy(view.Head.gameObject); _paths.Add(new FadingPath { Line = view.Path, Remaining = .8f }); }
        }
        private void EndRecipeRuns()
        {
            foreach (var run in _recipeRuns) { run.Lineage.End(); FinishRecipeRun(run); }
            _recipeRuns.Clear();
        }
        private void DrawRecipeHeader()
        {
            for (var n = 0; n < 5; n++)
            {
                var index = (Recipe.Slot + n) % 5;
                var source = index < Recipe.Slot && Recipe.Pending != null ? Recipe.Pending : Recipe.Active;
                var type = source[index]; var x = 310 + n * 148;
                Panel(new Rect(x, 12, 138, 85), n == 0 ? new Color(.17f, .23f, .29f) : new Color(.08f, .12f, .16f));
                Label(x + 10, 15, 126, 22, n == 0 ? (Recipe.Ready ? "NOW" : "発射済／装填中") : n == 1 ? "NEXT" : "あと " + (n + 1), _small);
                var old = GUI.color; GUI.color = BulletColor(type);
                Label(x + 10, 37, 120, 29, BulletName(type), _title); GUI.color = old;
                Label(x + 10, 71, 125, 22, $"{index + 1} / Primer {RecipeCycle.PrimerAt(source, index)}", _small);
            }
            if (Button(new Rect(1070, 18, 235, 44), "弾の並び [R]")) SetRecipeEditing(!RecipeEditing);
            if (Button(new Rect(1320, 18, 250, 44), Editing ? "再開 [B]" : "配置 [B]")) SetEditing(!Editing);
            Label(1070, 72, 480, 26, Recipe.Pending != null ? "次周期から新Recipeを適用" : "Capacity 4 / 通常 0・貫通 2・分裂 3", _small);
            var progress = Recipe.Ready ? 1f : 1f - (float)(Recipe.Remaining / .65);
            Panel(new Rect(310, 102, 730 * progress, 4), BulletColor(Recipe.Current));
            foreach (var popup in _rewardPopups)
                Label((float)popup.Position.X - 45, (float)popup.Position.Y - 90 - (float)(1 - popup.Remaining) * 22, 120, 40, "+" + popup.Gold, _title);
        }
        private void DrawRecipeEditor()
        {
            Panel(new Rect(0, 112, 1600, 788), new Color(.015f, .025f, .045f, .93f));
            Panel(new Rect(300, 170, 1260, 570), new Color(.065f, .09f, .13f));
            Label(340, 195, 1140, 45, "五発の並びをつくる", _title);
            Label(340, 250, 1140, 60, "クリックで 通常 → 貫通 → 分裂。直前の通常弾が、特殊弾を最大2段階強化します。\n編集中は弾も時間も停止。発射済みなら変更は次の周期から。", _body);
            for (var i = 0; i < 5; i++)
            {
                var x = 340 + i * 235;
                if (Button(new Rect(x, 345, 215, 83), $"{i + 1}   {BulletName(_draft[i])}\nCost {RecipeCycle.Cost(_draft[i])}"))
                { _undo = RecipeCycle.Copy(_draft); _draft[i] = (RecipeBullet)(((int)_draft[i] + 1) % 3); }
                var primer = RecipeCycle.PrimerAt(_draft, i);
                Label(x, 441, 223, 65, _draft[i] == RecipeBullet.Normal ? "安定した通常弾" : $"Primer {primer}\n" + (_draft[i] == RecipeBullet.Pierce ? $"通過 {2 + primer} 回 / ×1.25" : $"子弾 {2 + primer} 発 / 開き30°"), _body);
            }
            for (var n = 0; n < 3; n++)
                if (Button(new Rect(340 + n * 235, 530, 215, 40), "全て" + BulletName((RecipeBullet)n)))
                { _undo = RecipeCycle.Copy(_draft); for (var i = 0; i < 5; i++) _draft[i] = (RecipeBullet)n; }
            GUI.enabled = _undo != null;
            if (Button(new Rect(1045, 530, 215, 40), "一回戻す")) { _draft = _undo; _undo = null; }
            GUI.enabled = true;
            var valid = RecipeCycle.IsValid(_draft);
            var old = GUI.color; if (!valid) GUI.color = new Color(1, .4f, .4f);
            Label(340, 607, 650, 45, $"Capacity {RecipeCycle.TotalCost(_draft)} / 4" + (valid ? "" : "  超過：通常弾に戻してください"), _title); GUI.color = old;
            if (Button(new Rect(1030, 655, 210, 52), "変更せず閉じる")) SetRecipeEditing(false);
            GUI.enabled = valid;
            if (Button(new Rect(1260, 655, 260, 52), Recipe.HasFired ? "次周期へ予約" : "この並びで開始")) ApplyRecipe(_draft);
            GUI.enabled = true;
        }
        private string RecipePath => Path.Combine(Application.persistentDataPath, "recipe.json");
        private void SaveRecipe()
        {
            if (!PersistenceEnabled) return;
            try
            {
                var data = Recipe.Capture();
                var json = JsonUtility.ToJson(data, true);
                var history = Path.Combine(Application.persistentDataPath, "recipe-history"); Directory.CreateDirectory(history);
                if (File.Exists(RecipePath)) File.Copy(RecipePath, Path.Combine(history, DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff") + "-previous.json"));
                File.WriteAllText(Path.Combine(history, DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff") + ".json"), json);
                File.WriteAllText(RecipePath, json);
            }
            catch (Exception e) { Debug.LogWarning("[Recipe] Save failed: " + e.Message); _message = "Recipeの保存に失敗しました。"; }
        }
        private void LoadRecipe()
        {
            if (!PersistenceEnabled || !File.Exists(RecipePath)) return;
            try
            {
                var data = JsonUtility.FromJson<RecipeSnapshot>(File.ReadAllText(RecipePath));
                if (!Recipe.Restore(data))
                    Debug.LogWarning("[Recipe] Unsupported save kept unchanged; using initial recipe.");
            }
            catch (Exception e) { Debug.LogWarning("[Recipe] Existing save kept unchanged: " + e.Message); }
        }
        private IEnumerator RecipeDiagnostic()
        {
            var args = Environment.GetCommandLineArgs(); var flag = Array.IndexOf(args, "-recipe-capture");
            if (flag < 0 || flag + 1 >= args.Length) yield break;
            var folder = args[flag + 1]; Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(1);
            _hasFocus = true; _blockedUntilFrame = -1;
            SetRecipeEditing(true);
            var applied = ApplyRecipe(new[] { RecipeBullet.Split, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal });
            _blockedUntilFrame = -1; TryFire(new SimVector2(800, 220));
            for (var i = 0; i < 26; i++) SimulateTick(1.0 / 60);
            var branches = ActiveShotCount;
            CaptureBoard(Path.Combine(folder, "01-split-board.png"));
            SetRecipeEditing(true); var before = SimulatedSeconds; SimulateTick(10);
            var paused = before == SimulatedSeconds;
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, "02-editor.png"));
            SetRecipeEditing(false);
            for (var i = 0; i < 180; i++) SimulateTick(1.0 / 60);
            File.WriteAllText(Path.Combine(folder, "smoke.txt"), $"applied={applied}; branches={branches}; paused={paused}; gold={Gold}; active={ActiveShotCount}");
            yield return new WaitForSecondsRealtime(.5f);
            Application.Quit(applied && branches == 4 && paused && Gold == 4 && ActiveShotCount == 0 ? 0 : 1);
        }
        private sealed class RecipeRunView { public RecipeLineage Lineage; public int Id, RewardIndex; public readonly Dictionary<RoutingShot, RecipeProjectileView> Views = new Dictionary<RoutingShot, RecipeProjectileView>(); }
        private sealed class RecipeProjectileView { public LineRenderer Path; public SpriteRenderer Head; public int Contacts; }
        private sealed class RewardPopup { public SimVector2 Position; public int Gold; public double Remaining = 1; }
    }
}
