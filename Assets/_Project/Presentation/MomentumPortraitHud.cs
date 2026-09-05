using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumLabController
    {
        private readonly Queue<string> _combatFeed = new Queue<string>();
        private GUIStyle _hpLabel;

        private void RecordCombatFeed(MomentumEvent e)
        {
            var description = e.Kind == "hit" ? $"的 {e.Target:00}  命中威力 {e.Amount:0.0}" :
                e.Kind == "destroy" ? $"的 {e.Target:00}  撃破  +{e.Amount:0} Gold" : $"加速 ×2  速度 {e.Amount:0}";
            _combatFeed.Enqueue($"{Simulation.Time,6:0.0}s   {description}");
            while (_combatFeed.Count > 7) _combatFeed.Dequeue();
        }

        private void OnGUI()
        {
            if (_camera == null || Simulation == null) return;
            EnsureStyles();
            if (_hpLabel == null) _hpLabel = new GUIStyle(_title) { alignment = TextAnchor.MiddleCenter, fontSize = 19 };
            if (Event.current.type == EventType.Repaint) _overflows.Clear();
            var prior = GUI.matrix; var r = _camera.pixelRect;
            GUI.matrix = Matrix4x4.TRS(new Vector3(r.x, Screen.height - r.yMax, 0), Quaternion.identity, Vector3.one * (r.width / 1600));
            DrawTelemetry(); DrawLoadout(); DrawStageLabels();
            GUI.matrix = prior;
        }

        private void DrawTelemetry()
        {
            Panel(new Rect(28,32,515,836), new Color(.035f,.052f,.073f));
            Label(48,50,470,38,"MOMENTUM LAB",_title);
            Label(48,91,470,28,"COMBAT TELEMETRY / 戦闘ログ",_small);
            Panel(new Rect(48,134,475,142),new Color(.055f,.091f,.115f));
            Label(66,147,435,28,"DPS / 直近5秒",_body);
            Label(66,176,435,61,$"{Simulation.Stats.RecentDps:0.0}",_gold);
            Label(66,240,435,27,"実際に削ったHP ÷ 5秒。過剰ダメージは除外。",_small);
            Label(48,299,215,27,"獲得 GOLD",_small);
            Label(292,299,215,27,"累計ダメージ",_small);
            Label(48,330,215,39,Simulation.Gold.ToString(),_title);
            Label(292,330,215,39,$"{Simulation.Stats.TotalDamage:0.0}",_title);
            Label(48,378,475,28,$"命中 {Simulation.Stats.Hits} 回   撃破 {Simulation.DestroyedCount}   加速 {Simulation.BoostCount}",_body);
            Panel(new Rect(48,424,475,1),new Color(.14f,.22f,.27f));
            Label(48,440,475,32,"弾ごとの威力",_title);
            Label(48,482,475,29,$"通常   基礎40 / 上限80   直近 {Simulation.Stats.LastNormalImpact:0.0}",_body);
            Label(48,516,475,29,$"貫通   基礎26 / 上限52   直近 {Simulation.Stats.LastPierceImpact:0.0}",_body);
            Label(48,553,475,48,"通常的：HP50 / 抵抗150　装甲的：HP90 / 抵抗420\n威力は命中直前の速度に比例。貫通の抵抗係数は1/4。",_small);
            Panel(new Rect(48,615,475,1),new Color(.14f,.22f,.27f));
            Label(48,630,475,30,"RECENT EVENTS / 直近の記録",_small);
            if (_combatFeed.Count == 0) Label(48,676,475,62,"中央の盤面をクリックすると三発発射。\n命中・加速・撃破の記録がここに並びます。",_body);
            var row=0;
            foreach(var entry in _combatFeed) Label(48,674+row++*24,475,24,entry,_small);
        }

        private void DrawLoadout()
        {
            Panel(new Rect(1057,32,515,836),new Color(.035f,.052f,.073f));
            Label(1080,50,468,38,"LOADOUT / 弾倉・強化",_title);
            Label(1080,91,468,28,"弾の構成を変えて、中央の盤面で試す。",_small);
            var status=Simulation.Editing?"編集中：時間停止":Simulation.Bursting?$"斉射中 / 残り {Simulation.RemainingInBurst} 発":Simulation.Ready?"READY / クリックで三発":$"RELOAD / {Simulation.ReloadRemaining:0.0} 秒";
            Label(1080,137,468,30,status,_body);
            Panel(new Rect(1080,172,468,4),new Color(.1f,.16f,.2f));
            var progress=Simulation.Bursting?1-Simulation.RemainingInBurst/3f:Simulation.Ready?1:1-(float)Simulation.ReloadRemaining/.8f;
            Panel(new Rect(1080,172,468*Mathf.Clamp01(progress),4),new Color(.1f,.8f,1));
            if(Button(new Rect(1080,193,468,42),Simulation.Editing?"この弾倉で再開 [R]":"弾倉を編集 [R]")) SetEditing(!Simulation.Editing);
            for(var i=0;i<3;i++)
            {
                var y=254+i*95; var ammo=Simulation.Magazine[i];
                var details=ammo==MomentumAmmo.Normal?"基礎威力 40 / 通常の抵抗":"基礎威力 26 / 抵抗による減速 1/4";
                if(Simulation.Editing)
                {
                    if(Button(new Rect(1080,y,468,80),$"{i+1:00}   {AmmoName(ammo)}　[切替]\n{details}"))
                    {
                        var slots=new[] {Simulation.Magazine[0],Simulation.Magazine[1],Simulation.Magazine[2]};
                        slots[i]=ammo==MomentumAmmo.Normal?MomentumAmmo.Pierce:MomentumAmmo.Normal;
                        Simulation.SetMagazine(slots);
                    }
                }
                else
                {
                    Panel(new Rect(1080,y,468,80),new Color(.065f,.1f,.135f));
                    Panel(new Rect(1080,y,4,80),AmmoColor(ammo));
                    Label(1100,y+9,424,30,$"{i+1:00}   {AmmoName(ammo)}",_title);
                    Label(1100,y+45,424,27,details,_small);
                }
            }
            Label(1080,551,468,55,$"発射間隔 0.12秒 / 撃ち切り後 0.8秒リロード\n斉射 {Simulation.MagazineCount} 回 / 発射 {Simulation.FiredCount} 発 / 飛行中 {Simulation.Balls.Count} 発",_small);
            Panel(new Rect(1080,631,468,1),new Color(.14f,.22f,.27f));
            Label(1080,649,468,34,"UPGRADES / 強化スペース",_title);
            Label(1080,698,468,63,"購入できる強化は、まだありません。\n今回は縦長ステージと情報配置の試作です。",_body);
            Label(1080,791,468,60,$"{GameVersion} / {BuildMetadata.CommitHash}\nSeed {Simulation.Seed}  |  F2：ネオン／旧表示",_small);
        }

        private void DrawStageLabels()
        {
            Label(575,10,450,29,Simulation.Editing?"PAUSED / 弾倉を編集中":"NEON CHAMBER / 9 : 16",_targetLabel);
            foreach(var target in Simulation.Targets)
            {
                if(!target.Alive) continue;
                var x=(float)target.Position.X; var y=(float)target.Position.Y;
                Label(x-45,y-26,90,24,target.Armored?"装甲":"通常",_targetLabel);
                Label(x-45,y-4,90,31,$"{target.Hp:0}",_hpLabel);
            }
            var zone=Simulation.ZonePosition;
            Label((float)zone.X-60,(float)zone.Y-14,120,28,"速度 ×2",_targetLabel);
            // Impact values live in the left feed; only brief rewards/boosts float on the narrow stage.
            var shown=0;
            for(var i=_popups.Count-1;i>=0 && shown<3;i--)
            {
                var p=_popups[i]; if(p.Kind=="hit") continue;
                var x=Mathf.Clamp((float)p.Position.X-55,585,905);
                var y=Mathf.Clamp((float)p.Position.Y-58-(float)(1-p.Time)*20-shown*19,57,814);
                Label(x,y,110,28,p.Kind=="destroy"?$"+{p.Amount:0} Gold":"加速 ×2",_targetLabel); shown++;
            }
            Label(575,863,450,28,"クリック：三発発射   /   R：弾倉編集",_targetLabel);
        }
    }
}
