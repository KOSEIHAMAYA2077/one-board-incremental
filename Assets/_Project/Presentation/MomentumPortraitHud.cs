using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumLabController
    {
        private readonly Queue<string> _combatFeed = new Queue<string>();
        private GUIStyle _hpLabel;
        private static readonly string[] StageNames = { "01 / PRISM GARDEN", "02 / ARMOR VAULT", "03 / CRYSTAL SWARM" };
        private static string ModName(MomentumMod mod) => mod == MomentumMod.Power ? "威力 +25%" : mod == MomentumMod.Speed ? "初速 +20%" : mod == MomentumMod.Pierce ? "抵抗による減速 1/4" : mod == MomentumMod.Split ? "初回命中で二分裂" : mod == MomentumMod.Golden ? "4発ごとに Golden" : mod == MomentumMod.Blast ? "3体目に命中で爆発" : "共鳴 / 威力 +20%";
        private void RecordCombatFeed(MomentumEvent e)
        {
            var description = e.Kind == "hit" ? $"的{e.Target:00}  威力 {e.Amount:0.0}" : e.Kind == "destroy" ? $"的{e.Target:00}  撃破 +{e.Amount:0} G" : e.Kind == "boost" ? "加速ゾーン ×2" : e.Kind == "split" ? "分裂 → 子弾2発" : "3体命中 → 爆発";
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
            DrawTelemetry(); DrawLoadout(); DrawStageLabels();
            GUI.matrix = prior;
        }
        private void DrawTelemetry()
        {
            var sim = Simulation; var p = sim.Progress;
            Panel(new Rect(24,32,450,836),new Color(.045f,.075f,.105f));
            Label(44,48,410,36,"MOMENTUM / CHALLENGE",_title);
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
            Label(44,805,410,48,ProgressWarning ?? $"Seed {sim.Seed} / 自動保存あり\nF2：表示比較   R：Pause   Space：残弾回収",_small);
        }
        private void DrawLoadout()
        {
            var sim=Simulation; var p=sim.Progress; var free=sim.CanConfigure;
            Panel(new Rect(1126,32,450,836),new Color(.045f,.075f,.105f));
            Label(1146,48,410,35,"ARSENAL / 銃と効果",_title);
            var state=sim.Editing?"PAUSED / Rで再開":sim.Bursting?$"射出中：あと{sim.RemainingInBurst}発":sim.Ready?$"READY / 次を発射可（残弾{sim.Balls.Count}）":sim.ChallengeState!=MomentumChallengeState.Active?"左のStageを選んで次の挑戦へ":sim.RemainingTargets==0?"全的撃破 / 残弾回収で結果を確定":sim.ChallengeMagazines>=sim.ChallengeLimit?"最終マガジン / 攻撃の決着待ち":sim.ReloadRemaining>0?$"Reload {sim.ReloadRemaining:0.0}s":!sim.HasRoomForMagazine?"弾数上限 / 待機または残弾回収":$"減速待ち / 最大速度 {sim.FastestBallSpeed:0}";
            Label(1146,91,410,28,state,_body);
            if(ActionButton(new Rect(1146,128,410,36),"残弾回収 [Space] / 残りの攻撃を放棄",!sim.Editing && (sim.Bursting || sim.Balls.Count>0))) RecallVolley();
            if(ActionButton(new Rect(1146,176,200,44),(p.gun==0?"● ":"")+"REVOLVER / 6発",free)) { p.SelectGun(0); SaveProgress(); }
            if(ActionButton(new Rect(1356,176,200,44),p.uziUnlocked?(p.gun==1?"● ":"")+"UZI / 18発":"UZI開放 / 40 G",free && (p.uziUnlocked || p.gold>=40)))
            { if(!p.uziUnlocked) p.BuyGun(); p.SelectGun(1); SaveProgress(); }
            var gunInfo=p.gun==0?"太い6発 / 基礎48・初速780 / 間隔0.12秒":"小さい18発 / 基礎17・初速1050 / 間隔0.05秒";
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
            Label(500,862,600,29,"クリック：一斉射 / Space：回収 / オレンジの的は高抵抗",_targetLabel);
        }
    }
}
