using System;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumLabController
    {
        public bool KeyboardControl { get; private set; }
        public int MenuPage { get; private set; } // 0 play, 1 help, 2 settings, 3 quit
        public bool QuitRequested { get; private set; }
        public float SeVolume { get; private set; } = 1;
        public int Theme { get; private set; }
        private bool _editingBeforeMenu;
        private SimVector2 _controlDirection=new SimVector2(0,-1);
        private Vector3 _lastMousePosition;
        private double _keyboardAngle=-Math.PI/2;
        private bool CanControl => _focus && MenuPage==0 && !Simulation.Editing && Time.frameCount>_blockedFrame;

        public bool SelectWeapon(int gun)
        {
            if(!CanControl || !Simulation.Progress.SelectGun(gun)) return false;
            SaveProgress();return true; // The active magazine keeps its existing snapshot.
        }
        public bool CycleWeapon(int direction=1)
        {
            if(!CanControl || direction==0) return false;
            var current=Simulation.Progress.gun;
            for(var n=1;n<=4;n++)
            {
                var next=(current+Math.Sign(direction)*n+8)%4;
                if(Simulation.Progress.OwnsGun(next)) return SelectWeapon(next);
            }
            return false;
        }
        public bool MouseControl(SimVector2 aim,bool fire=false)
        {
            if(!CanControl || !MomentumRules.Finite(aim.X) || !MomentumRules.Finite(aim.Y) || !Simulation.Layout.Contains(aim) || (aim-Simulation.Layout.Gun).Magnitude<30) return false;
            KeyboardControl=false;_controlDirection=(aim-Simulation.Layout.Gun).Normalized;
            _keyboardAngle=Math.Atan2(_controlDirection.Y,_controlDirection.X);
            SteerBurst(aim);DrawAim(_controlDirection);
            return !fire || TryFire(aim);
        }
        private SimVector2 KeyboardTarget()
        {
            var l=Simulation.Layout;var g=l.Gun;var d=_controlDirection;var distance=200.0;
            if(d.X>1e-8) distance=Math.Min(distance,(l.Right-1-g.X)/d.X);
            if(d.X< -1e-8) distance=Math.Min(distance,(l.Left+1-g.X)/d.X);
            if(d.Y>1e-8) distance=Math.Min(distance,(l.Bottom-1-g.Y)/d.Y);
            if(d.Y< -1e-8) distance=Math.Min(distance,(l.Top+1-g.Y)/d.Y);
            return g+d*distance;
        }
        public bool KeyboardInput(float turn,double seconds,bool fire)
        {
            if(!CanControl || !MomentumRules.Finite(turn) || !MomentumRules.Finite(seconds) || seconds<0) return false;
            KeyboardControl=true;
            _keyboardAngle+=Mathf.Clamp(turn,-1,1)*Math.Min(seconds,.1)*Math.PI*2/3; // 120 degrees/s
            _controlDirection=new SimVector2(Math.Cos(_keyboardAngle),Math.Sin(_keyboardAngle));
            var target=KeyboardTarget();SteerBurst(target);DrawAim(_controlDirection);
            return !fire || TryFire(target);
        }
        private void UpdateControls()
        {
            if(!_focus || _diagnostic) return;
            var currentMouse=Input.mousePosition;var moved=(currentMouse-_lastMousePosition).sqrMagnitude>4;
            _lastMousePosition=currentMouse;
            if(Input.GetKeyDown(KeyCode.Escape)) { if(MenuPage==3) CloseMenu();else OpenMenu(3);return; }
            if(MenuPage!=0)
            {
                if(MenuPage==3 && Input.GetKeyDown(KeyCode.Return)) ConfirmQuit();
                return;
            }
            if(Input.GetKeyDown(KeyCode.F1)) { OpenMenu(1);return; }
            if(Input.GetKeyDown(KeyCode.F2)) SetTheme(Theme==2?0:2);
            if(Input.GetKeyDown(KeyCode.F3)) SetTheme(Theme==0?1:0);
            if(Input.GetKeyDown(KeyCode.R)) CycleWeapon();
            if(Math.Abs(Input.mouseScrollDelta.y)>.01f) CycleWeapon(Input.mouseScrollDelta.y>0?1:-1);
            var over=MouseAim(out var aim);var clicked=Input.GetMouseButtonDown(0);
            if(over && (moved || clicked || !KeyboardControl)) MouseControl(aim,clicked);
            var turn=(Input.GetKey(KeyCode.E)?1:0)-(Input.GetKey(KeyCode.Q)?1:0);
            if(turn!=0 || Input.GetKeyDown(KeyCode.Space)) KeyboardInput(turn,Time.unscaledDeltaTime,Input.GetKeyDown(KeyCode.Space));
            _aimLine.enabled=CanControl && !ClearPanelVisible && (KeyboardControl || over);
            if(_aimLine.enabled) DrawAim(_controlDirection);
        }
        public void OpenMenu(int page)
        {
            if(page<1 || page>3) return;
            if(MenuPage==0) _editingBeforeMenu=Simulation.Editing;
            MenuPage=page;SetEditing(true);
        }
        public void CloseMenu()
        {
            if(MenuPage==0) return;
            MenuPage=0;SetEditing(_editingBeforeMenu);SaveUtilitySettings();
        }
        public void ConfirmQuit()
        {
            if(MenuPage!=3) return;
            SaveProgress();SaveUtilitySettings();QuitRequested=true;
            if(!Application.isEditor && !_diagnostic) Application.Quit();
        }
        public void SetSeVolume(float volume)
        {
            SeVolume=float.IsNaN(volume)||float.IsInfinity(volume)?1:Mathf.Clamp01(volume);
            var source=_audio!=null?_audio.GetComponent<AudioSource>():null;
            if(source!=null) source.volume=SeVolume;
        }
        public void SetTheme(int theme)
        {
            Theme=Mathf.Clamp(theme,0,2);SetNeonEnabled(Theme!=2);
            NeonView.SetCrystalKitEnabled(Theme==0);
        }
        private void LoadUtilitySettings()
        {
            _lastMousePosition=Input.mousePosition;
            SetSeVolume(PersistenceEnabled?PlayerPrefs.GetFloat("Momentum.UtilityV1.SE",1):1);
            SetTheme(PersistenceEnabled?PlayerPrefs.GetInt("Momentum.UtilityV1.Theme",0):0);
        }
        private void SaveUtilitySettings()
        {
            if(!PersistenceEnabled) return;
            PlayerPrefs.SetFloat("Momentum.UtilityV1.SE",SeVolume);PlayerPrefs.SetInt("Momentum.UtilityV1.Theme",Theme);PlayerPrefs.Save();
        }
        private void DrawUtilityChrome()
        {
            if(Button(new Rect(361,48,40,34),"?")) OpenMenu(1);
            if(Button(new Rect(409,48,40,34),"≡")) OpenMenu(2);
        }
        private void DrawUtilityMenu()
        {
            if(MenuPage==0) return;
            Panel(new Rect(0,0,1600,900),new Color(0,0,0,.82f));
            Panel(new Rect(390,102,820,692),new Color(.045f,.09f,.13f));
            Label(424,126,694,42,MenuPage==1?"HOW TO PLAY / 操作と遊び方":MenuPage==2?"MENU / 設定":"EXIT / ゲーム終了",_title);
            if(Button(new Rect(1122,122,52,38),"×")) { CloseMenu();return; }
            if(MenuPage==1)
            {
                Label(424,187,746,165,"マウス移動：照準 / 左クリック：1マガジン発射\nQ・E：左右へ照準 / Space：1マガジン発射\nQ・E・Spaceでキー操作へ，盤面内のマウス移動で戻る\nR・ホイール：銃切替（射撃中は次のマガジンから）\nEsc：終了確認 / F1・?：ヘルプ / ≡：設定・Pause",_body);
                Label(424,365,746,190,"的を壊してGoldを獲得し，強化・効果を組み合わせる。\n限られたマガジンで全破壊すると次のStageへ。\n水色ゾーンは速度×2。低速になった弾は急減速して消える。\nB缶を拾った弾を紫のバンパーへ届けると +12 Gold。\n壁では換金しない。分裂した子弾にはB効果を引き継がない。\n缶は次のChallengeで復活。拾うだけでは報酬なし。",_body);
                Label(424,575,746,100,"SHOTGUN：8粒を同時に扇状発射 / SNIPER：太く速い1発\n新しい2銃は試作用に最初から使用可能。UZIは40Gで開放。\n停止中の残弾を終えたい場合は右の『残弾回収』へ。",_body);
            }
            else if(MenuPage==2)
            {
                Label(424,195,746,28,$"SE音量  {SeVolume*100:0}%",_body);
                if(Button(new Rect(424,235,235,38),"− 10%")) SetSeVolume(SeVolume-.1f);
                if(Button(new Rect(671,235,235,38),SeVolume>0?"消音":"音量 100%")) SetSeVolume(SeVolume>0?0:1);
                if(Button(new Rect(918,235,235,38),"＋ 10%")) SetSeVolume(SeVolume+.1f);
                Label(424,284,746,30,"BGM：未実装（音楽は再生されません）",_body);
                Label(424,346,746,30,"表示テーマ",_title);
                var names=new[]{"CRYSTAL","NEON","DIAGRAM"};
                for(var i=0;i<3;i++) if(Button(new Rect(424+i*247,397,235,48),(Theme==i?"● ":"")+names[i])) SetTheme(i);
                Label(424,470,746,70,"設定・ヘルプを開いている間はゲームが停止します。\nSE音量とテーマはこの端末に保存します。",_body);
                if(Button(new Rect(424,571,350,50),"残弾回収して再開"))
                { _editingBeforeMenu=false;CloseMenu();RecallVolley();return; }
                if(Button(new Rect(797,571,350,50),"ゲーム終了…")) { OpenMenu(3);return; }
            }
            else
            {
                Label(424,218,746,142,"ゲームを終了しますか？\n獲得済みGoldと強化は自動保存されます。\n飛行中の弾と途中のChallengeは保存されません。\n次回は新しいChallengeから再開します。",_body);
                if(ProgressWarning!=null) Label(424,395,746,70,ProgressWarning+"\n未保存の進行が失われる可能性があります。",_body);
                if(Button(new Rect(424,542,350,58),"終了する [Enter]")) ConfirmQuit();
                if(Button(new Rect(797,542,350,58),"キャンセル [Esc]")) { CloseMenu();return; }
            }
            if(MenuPage!=3 && Button(new Rect(424,715,730,48),"閉じて戻る")) { SaveUtilitySettings();CloseMenu(); }
        }
    }
}
