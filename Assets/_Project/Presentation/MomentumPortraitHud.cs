using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumLabController
    {
        private readonly Queue<string> _combatFeed = new Queue<string>();
        private GUIStyle _hpLabel;
        private GUIStyle _clearHeading, _clearText;
        public bool ClearPanelVisible => Simulation != null && !Simulation.Editing && Simulation.ChallengeState == MomentumChallengeState.Cleared;
        public bool CanAdvanceFromClear => ClearPanelVisible && Simulation.Stage + 1 < StageNames.Length && Simulation.Stage + 1 <= Simulation.Progress.highestStage;
        public bool ContinueFromClear(bool next)
        {
            if (!ClearPanelVisible || (next && !CanAdvanceFromClear)) return false;
            return BeginChallenge(Simulation.Stage + (next ? 1 : 0));
        }
        private static readonly string[] StageNames = { "01 / PRISM GARDEN", "02 / ARMOR VAULT", "03 / CRYSTAL SWARM" };
        private static string ModName(MomentumMod mod) => mod == MomentumMod.Power ? "威力 +25%" : mod == MomentumMod.Speed ? "初速 +20%" : mod == MomentumMod.Pierce ? "抵抗による減速 1/4" : mod == MomentumMod.Split ? "初回命中で二分裂" : mod == MomentumMod.Golden ? "4発ごとに Golden" : mod == MomentumMod.Blast ? "3体目に命中で爆発" : "共鳴 / 威力 +20%";
        private void RecordCombatFeed(MomentumEvent e)
        {
            var description = e.Kind == "hit" ? $"的{e.Target:00}  威力 {e.Amount:0.0}" : e.Kind == "destroy" ? $"的{e.Target:00}  撃破 +{e.Amount:0} G" : e.Kind == "boost" ? "加速ゾーン ×2" : e.Kind == "split" ? "分裂 → 子弾2発" : e.Kind == "pickup" ? "B獲得 → バンパーへ届けよう" : e.Kind == "bounty" ? $"B換金 +{e.Amount:0} G" : "3体命中 → 爆発";
            _combatFeed.Enqueue($"{Simulation.Time,5:0.0}s  {description}");
            while (_combatFeed.Count > 5) _combatFeed.Dequeue();
        }
        private bool ActionButton(Rect rect, string label, bool enabled)
        {
            var previous = GUI.enabled; GUI.enabled = previous && enabled;
            var clicked = Button(rect, label); GUI.enabled = previous; return clicked;
        }
        private void OnGUI()
        {
            if (_camera == null || Simulation == null) return;
            EnsureStyles();
            if (_hpLabel == null) _hpLabel = new GUIStyle(_targetLabel) { fontSize = 16, fontStyle = FontStyle.Bold };
            if (Event.current.type == EventType.Repaint) _overflows.Clear();
            var prior = GUI.matrix; var r = _camera.pixelRect;
            GUI.matrix = Matrix4x4.TRS(new Vector3(r.x, Screen.height-r.yMax,0),Quaternion.identity,Vector3.one*(r.width/1600));
            var enabled=GUI.enabled;GUI.enabled=enabled && MenuPage==0;
            DrawTelemetry(); DrawLoadout(); DrawStageLabels(); DrawClearPanel();
            DrawUtilityChrome();GUI.enabled=enabled;DrawUtilityMenu();
            GUI.matrix = prior;
        }
        private void DrawTelemetry()
        {
            var sim = Simulation; var p = sim.Progress;
            Panel(new Rect(24,32,450,836),new Color(.045f,.075f,.105f));
            Label(44,48,310,36,"MOMENTUM LAB",_title);
            Label(44,91,410,25,"銃 × 効果 × 一斉射の連鎖",_small);
            Panel(new Rect(44,128,410,114),new Color(.075f,.14f,.18f));
            Label(60,140,370,25,"実効 DPS / 直近5秒",_small);
            Label(60,170,370,58,$"{sim.Stats.RecentDps:0.0}",_gold);
            Label(44,261,410,35,$"GOLD  {sim.Gold}    今回 +{sim.ChallengeGold}",_title);
            Label(44,307,410,28,$"累計Damage {sim.Stats.TotalDamage:0} / 撃破 {sim.DestroyedCount}",_body);
            Label(44,348,410,30,StageNames[sim.Stage],_title);
            Label(44,389,410,51,$"マガジン {sim.ChallengeMagazines} / {sim.ChallengeLimit} 使用\n残りの的 {sim.RemainingTargets} / 12　生存敵のHPは継続",_body);
            var state = sim.ChallengeState == MomentumChallengeState.Cleared ? (sim.ChallengeMagazines==1?"ONE MAG CLEAR! 特殊効果を開放。":"CLEAR! 次Stageへ / 既到達Stageも再訪可") : sim.ChallengeState == MomentumChallengeState.Failed ? "挑戦終了。獲得Goldを保持して再挑戦。" : "1マガジン全破壊 → 特殊効果を恒久開放";
            Label(44,451,410,45,state,_body);
            for(var i=0;i<3;i++)
            {
                var label=$"STAGE {i+1}"+((p.masteryMask&(1<<i))!=0?" ★":"");
                if(ActionButton(new Rect(44+i*138,505,132,38),label,sim.CanConfigure && i<=p.highestStage)) BeginChallenge(i);
            }
            Label(44,552,410,42,"上のStageで新しい挑戦を開始。\n途中で選び直しても、獲得済みGoldは失いません。",_small);
            Label(44,609,410,28,"RECENT EVENTS",_small);
            var row=0; foreach(var entry in _combatFeed) Label(44,644+row++*23,410,23,entry,_small);
            Label(44,773,410,27,$"{GameVersion} / {BuildMetadata.CommitHash}",_small);
            Label(44,805,410,48,ProgressWarning ?? $"Seed {sim.Seed} / 自動保存あり\nR：銃切替   ?：説明   ≡：設定   Esc：終了",_small);
        }
        private void DrawLoadout()
        {
            var sim=Simulation; var p=sim.Progress; var free=sim.CanConfigure;
            Panel(new Rect(1126,32,450,836),new Color(.045f,.075f,.105f));
            Label(1146,48,410,35,"ARSENAL / 銃と効果",_title);
            var state=sim.Editing?"PAUSED / メニューを閉じて再開":sim.Bursting?$"{MomentumSimulation.GunName(sim.FiringGun)} 射出中：あと{sim.RemainingInBurst}発":sim.Ready?$"READY / 次を発射可（残弾{sim.Balls.Count}）":sim.ChallengeState!=MomentumChallengeState.Active?"左のStageを選んで次の挑戦へ":sim.RemainingTargets==0?"全的撃破 / 残弾回収で結果を確定":sim.ChallengeMagazines>=sim.ChallengeLimit?"最終マガジン / 攻撃の決着待ち":sim.ReloadRemaining>0?$"Reload {sim.ReloadRemaining:0.0}s":!sim.HasRoomForMagazine?"弾数上限 / 待機または残弾回収":$"減速待ち / 最大速度 {sim.FastestBallSpeed:0}";
            Label(1146,91,410,28,state,_body);
            if(ActionButton(new Rect(1146,128,410,36),"残弾回収 / 残りの攻撃を放棄",!sim.Editing && (sim.Bursting || sim.Balls.Count>0))) RecallVolley();
            var names=new[]{"REV / 6","UZI / 18","SG / 8","SNI / 1"};
            for(var i=0;i<4;i++)
            {
                var owned=p.OwnsGun(i);var label=owned?(p.gun==i?"● ":"")+names[i]:"UZI 40G";
                if(ActionButton(new Rect(1146+i*104,176,98,44),label,CanControl && (owned || free && p.gold>=40)))
                { if(!owned) p.BuyGun();SelectWeapon(i); }
            }
            var cadence=p.gun==2?"8粒同時":p.gun==3?"単発":$"{MomentumSimulation.ShotInterval(p.gun):0.##}秒間隔";
            var gunInfo=$"{MomentumSimulation.GunName(p.gun)} / 基礎{MomentumSimulation.GunDamage(p.gun):0}・初速{MomentumSimulation.GunSpeed(p.gun):0} / {cadence}";
            Label(1146,230,410,42,$"{gunInfo}\n再射撃：全弾速度{MomentumSimulation.RefireSpeed:0}以下＋Reload完了",_small);
            Label(1146,270,410,30,$"効果容量 {MomentumProgress.Used(p.equippedMods)} / 20 pt",_title);
            for(var i=0;i<MomentumProgress.Mods.Length;i++)
            {
                var mod=MomentumProgress.Mods[i]; var owned=p.Owns(mod);
                var price=mod==MomentumMod.Power?10:mod==MomentumMod.Split?20:0;
                var unlock=price>0?$"開放 {price} G":$"Stage {(mod==MomentumMod.Golden?1:mod==MomentumMod.Blast?2:3)} を1マガジンClear";
                var label=owned?$"{(p.Has(mod)?"●":"○")} {ModName(mod)}  [{MomentumProgress.Cost(mod)}pt]":$"{ModName(mod)} / {unlock}";
                var fits=p.Has(mod) || MomentumProgress.Used(p.equippedMods)+MomentumProgress.Cost(mod)<=20;
                if(ActionButton(new Rect(1146,310+i*43,410,38),label,free && (owned && fits || !owned && price>0 && p.gold>=price)))
                { if(owned) p.Toggle(mod); else p.BuyMod(mod); SaveProgress(); }
            }
            Label(1146,610,410,40,$"連鎖10秒 / 同時256弾・生成1024弾まで\n分裂抑制 {sim.SuppressedSplits} 回 / 親弾は継続",_small);
            if(ActionButton(new Rect(1146,653,410,38),p.magazineLevel==2?"携行マガジン 5 / 最大":$"携行 {p.MagazineLimit} → {p.MagazineLimit+1} / {p.MagazinePrice} G",free && p.magazineLevel<2 && p.gold>=p.MagazinePrice)) { p.BuyMagazine(); SaveProgress(); }
            if(ActionButton(new Rect(1146,697,410,38),p.powerLevel==5?"基礎火力 +50% / 最大":$"基礎火力 +{p.powerLevel*10}% → +{(p.powerLevel+1)*10}% / {p.PowerPrice} G",free && p.powerLevel<5 && p.gold>=p.PowerPrice)) { p.BuyPower(); SaveProgress(); }
            Label(1146,743,410,25,"携行数の強化は次のChallengeから適用",_small);
            for(var i=0;i<3;i++)
            {
                if(ActionButton(new Rect(1146+i*138,777,132,32),$"構成{i+1} 保存",free)) { p.StorePreset(i); SaveProgress(); }
                if(ActionButton(new Rect(1146+i*138,815,132,32),$"構成{i+1} 読込",free)) { p.LoadPreset(i); SaveProgress(); }
            }
        }
        private void DrawStageLabels()
        {
            Label(500,10,600,29,Simulation.Editing?"PAUSED":"CRYSTAL CHAMBER / 3 : 4",_targetLabel);
            foreach(var t in Simulation.Targets)
            {
                if(!t.Alive) continue;
                Label((float)t.Position.X-26,(float)t.Position.Y-14,52,28,$"{t.Hp:0}",_hpLabel);
            }
            var z=Simulation.ZonePosition; Label((float)z.X-60,(float)z.Y-14,120,28,"速度 ×2",_targetLabel);
            foreach(var can in Simulation.Pickups)
            {
                if(!can.Active) continue;
                var x=(float)can.Position.X;var y=(float)can.Position.Y;
                if(!NeonEnabled) Panel(new Rect(x-14,y-18,28,36),new Color(.3f,.55f,.08f));
                Label(x-16,y-16,32,32,"B",_hpLabel);
            }
            foreach(var ball in Simulation.Balls) if(ball.BountyCharged) Label((float)ball.Position.X-12,(float)ball.Position.Y-30,24,22,"B",_hpLabel);
            Label(500,862,600,29,(KeyboardControl?"KEY：Q/E 照準・Space 発射":"MOUSE：照準・クリック 発射")+" / R・ホイール：銃",_targetLabel);
        }
        private void DrawClearPanel()
        {
            if (!ClearPanelVisible) return;
            if (_clearHeading == null)
            {
                _clearHeading = new GUIStyle(_gold) { fontSize = 48, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(.4f,1f,.82f) } };
                _clearText = new GUIStyle(_body) { alignment = TextAnchor.MiddleCenter };
            }
            var sim = Simulation;
            Panel(new Rect(500,50,600,800),new Color(.015f,.03f,.05f,.68f));
            Panel(new Rect(544,270,512,342),new Color(.2f,.75f,.66f));
            Panel(new Rect(547,273,506,336),new Color(.045f,.09f,.12f));
            Label(568,289,464,68,"CLEAR!",_clearHeading);
            Label(568,364,464,32,StageNames[sim.Stage],_clearText);
            Label(568,407,464,50,$"{sim.ChallengeMagazines} マガジンで全破壊 / 獲得 {sim.ChallengeGold} G\n"+(sim.ChallengeMagazines==1?"1マガジン達成！ 特殊効果を開放済み":"獲得Goldと開放済みの強化は保持されます"),_clearText);
            Label(568,468,464,28,CanAdvanceFromClear?"次のステージへ進もう":"全3ステージ達成！ 再挑戦や構成変更を楽しもう",_targetLabel);
            var enabled = _focus && Time.frameCount > _blockedFrame;
            if(ActionButton(new Rect(570,515,220,56),CanAdvanceFromClear?"次へ →":"全ステージ達成",enabled && CanAdvanceFromClear)) ContinueFromClear(true);
            if(ActionButton(new Rect(810,515,220,56),"もう一度",enabled && ClearPanelVisible)) ContinueFromClear(false);
            Label(568,578,464,25,"左右のパネルで強化・ステージ選択もできます",_targetLabel);
        }
    }
}
