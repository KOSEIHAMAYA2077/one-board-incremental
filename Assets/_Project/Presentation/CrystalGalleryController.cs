using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    // Standalone art playground: deliberately no Simulation or save system.
    public sealed class CrystalGalleryController : MonoBehaviour
    {
        private readonly List<CrystalAppearance> _pieces=new List<CrystalAppearance>();
        private readonly List<Vector3> _homes=new List<Vector3>();
        private readonly List<float> _sizes=new List<float>();
        private readonly List<int> _kinds=new List<int>();
        private readonly List<GameObject> _decor=new List<GameObject>();
        private int _selected=-1,_page,_status;
        private float _health=1,_opacity=.3f,_tilt=35,_spin,_clock,_impact;
        private bool _animate=true;
        private float _brokenRemaining;
        private readonly List<Shard> _shards=new List<Shard>();
        private sealed class Shard { public Transform Transform;public Vector3 Velocity,Spin;public float Life; }
        private Camera _camera;
        private GUIStyle _title,_body,_small,_button;
        public int OverflowCount {get;private set;}
        public int ShardCount => _shards.Count;
        private static readonly string[] Labels={"TETRA / 正四面体","OCTA / 正八面体","URCHIN / いがぐり","REBOUND / バンパー","LANCE / 結晶銃","CHAMBER / 筐体","SEED / 弾頭","16 FACES / 十六面体","32 FACES / 三十二面体","FACET / 多面球","ORB / 滑らかな球"};
        private readonly int[][] _pages={new[] {0,1,2,3,4,5},new[] {7,8,9,10,6,1}};
        private void Awake()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;
            _camera=GetComponentInChildren<Camera>();_camera.allowHDR=true;_camera.gameObject.AddComponent<LucentBloom>(); BuildCollection();
        }
        private void ClearObjects()
        {
            foreach(var shard in _shards) if(shard.Transform!=null) Destroy(shard.Transform.gameObject);
            _shards.Clear();_brokenRemaining=0;
            foreach(var p in _pieces) if(p!=null) Destroy(p.gameObject);
            foreach(var d in _decor) if(d!=null) Destroy(d);
            _pieces.Clear();_homes.Clear();_sizes.Clear();_kinds.Clear();_decor.Clear();
        }
        private void BuildCollection()
        {
            ClearObjects();
            var kinds=_selected<0?_pages[_page]:new[] {_selected};
            for(var i=0;i<kinds.Length;i++)
            {
                var k=kinds[i];var pos=_selected<0?new Vector3(-5.4f+(i%3)*3.15f,1.65f-(i/3)*3.25f,0):new Vector3(-2.25f,.25f,0);
                var size=k==5?.26f:k==4?.84f:k==6?.7f:.95f;
                if(_selected>=0) size=k==5?.7f:k==4?1.65f:2.1f;
                var model=CrystalKitFactory.Spawn(k,transform); if(model==null) continue;
                model.transform.localPosition=pos;model.transform.localScale=Vector3.one*size;
                _pieces.Add(model.GetComponent<CrystalAppearance>());_homes.Add(pos);_sizes.Add(size);_kinds.Add(k);
                var shadow=RouteDrawing.Shape(transform,"Soft grounding",new Vector2(pos.x+.07f,pos.y-.28f),new Vector2(size*1.55f,size*.7f),new Color(.003f,.01f,.02f,.42f),true,-2);
                _decor.Add(shadow.gameObject);
                if(k==5)
                {
                    var floor=RouteDrawing.Shape(model.transform,"Cabinet removable floor",Vector2.zero,new Vector2(5.95f,7.95f),new Color(.025f,.06f,.09f),false,-3);
                    for(var j=0;j<5;j++)
                    {
                        var enemy=CrystalKitFactory.Spawn(j%3,model.transform);
                        enemy.transform.localPosition=new Vector3(-1.6f+(j%3)*1.6f,1.8f-(j/3)*1.8f,-.1f);enemy.transform.localScale=Vector3.one*.42f;
                    }
                    var gun=CrystalKitFactory.Spawn(4,model.transform);gun.transform.localPosition=new Vector3(0,-3.3f,0);gun.transform.localScale=Vector3.one*.5f;
                }
            }
            Animate(0);
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space)) _animate=!_animate;
            if(Input.GetKeyDown(KeyCode.H)) _impact=1;
            if(Input.GetKeyDown(KeyCode.B)) PreviewShatter();
            if(Input.GetKeyDown(KeyCode.Tab)) { _selected=-1;_page=1-_page;BuildCollection(); }
            if(Input.GetKeyDown(KeyCode.Escape)) { _selected=-1;BuildCollection(); }
            if(_animate) { _clock+=Time.deltaTime;_spin+=Time.deltaTime*14; }
            _impact=Mathf.Max(0,_impact-Time.deltaTime*3);
            _brokenRemaining=Mathf.Max(0,_brokenRemaining-Time.deltaTime);
            foreach(var p in _pieces) p.gameObject.SetActive(_brokenRemaining<=0);
            for(var i=_shards.Count-1;i>=0;i--)
            {
                var shard=_shards[i];shard.Life-=Time.deltaTime;
                if(shard.Life<=0) { Destroy(shard.Transform.gameObject);_shards.RemoveAt(i);continue; }
                shard.Transform.position+=shard.Velocity*Time.deltaTime;shard.Transform.Rotate(shard.Spin*Time.deltaTime);
                shard.Transform.localScale=Vector3.one*Mathf.Min(.13f,shard.Life*.25f);
            }
            Animate(_clock);
        }
        public void PreviewShatter()
        {
            if(_brokenRemaining>0) return;
            var lib=Resources.Load<CrystalKitLibrary>("CrystalKitV1/Library");var random=new System.Random(3791);_brokenRemaining=1.2f;
            foreach(var p in _pieces) for(var i=0;i<16 && _shards.Count<128;i++)
            {
                var obj=new GameObject("Gallery crystal fragment");obj.transform.SetParent(transform,false);obj.transform.position=p.transform.position;
                obj.AddComponent<MeshFilter>().sharedMesh=lib.Shapes[0];var r=obj.AddComponent<MeshRenderer>();r.sharedMaterial=lib.Glass;
                var block=new MaterialPropertyBlock();block.SetColor("_Tint",p.ShellColor);block.SetFloat("_Opacity",.65f);r.SetPropertyBlock(block);
                var direction=new Vector3((float)random.NextDouble()*2-1,(float)random.NextDouble()*2-1,(float)random.NextDouble()*2-1).normalized;
                _shards.Add(new Shard {Transform=obj.transform,Velocity=direction*(1+(float)random.NextDouble()*2),Spin=direction*180,Life=1.2f});
            }
        }
        private void Animate(float time)
        {
            for(var i=0;i<_pieces.Count;i++)
            {
                var k=_kinds[i];var moving=k!=5;
                _pieces[i].transform.localPosition=_homes[i]+Vector3.up*(moving?Mathf.Sin(time*1.4f+i)*.07f:0);
                _pieces[i].transform.localRotation=Quaternion.Euler(_tilt,k==5?-12:18,k==5?0:k==4?-24:_spin+i*17);
                _pieces[i].Apply(_health,(CrystalStatus)_status,_impact,_opacity);
            }
        }
        private void Styles()
        {
            if(_title!=null) return;
            var font=Font.CreateDynamicFontFromOSFont(new[] {"Yu Gothic UI","Meiryo","Arial"},18);
            _body=new GUIStyle {font=font,fontSize=16,wordWrap=true,normal={textColor=new Color(.83f,.9f,.94f)}};
            _title=new GUIStyle(_body){fontSize=32,fontStyle=FontStyle.Bold};
            _small=new GUIStyle(_body){fontSize=13,normal={textColor=new Color(.52f,.69f,.76f)}};
            _button=new GUIStyle(GUI.skin.button){font=font,fontSize=15,wordWrap=true};
        }
        private void Text(Rect rect,string text,GUIStyle style)
        { if(Event.current.type==EventType.Repaint && style.CalcHeight(new GUIContent(text),rect.width)>rect.height+1) OverflowCount++; GUI.Label(rect,text,style); }
        private static void Panel(Rect rect,Color color) { var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old; }
        private void OnGUI()
        {
            Styles();if(Event.current.type==EventType.Repaint) OverflowCount=0;
            var old=GUI.matrix;var s=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*s)/2,(Screen.height-900*s)/2,0),Quaternion.identity,Vector3.one*s);
            Panel(new Rect(0,0,1600,112),new Color(.016f,.03f,.045f));
            Text(new Rect(40,22,900,43),"LUCENT / CRYSTAL STUDIES",_title);
            Text(new Rect(42,73,1020,26),"ASSET KIT 01     /     透ける外殻・浮かぶコア・光る稜線     /     ORIGINAL PROCEDURAL 3D",_small);
            Panel(new Rect(1150,130,420,680),new Color(.025f,.05f,.071f));
            Text(new Rect(1176,151,366,34),"MATERIAL PLAYGROUND",_body);
            Text(new Rect(1176,192,366,44),"ダメージは外殻へ。\nスキル状態はコアと輪へ。",_body);
            Text(new Rect(1176,257,366,25),$"HP / 残り {_health*100:0}%",_body);
            _health=GUI.HorizontalSlider(new Rect(1176,291,360,22),_health,0,1);
            Text(new Rect(1176,333,366,25),$"GLASS / 不透明度 {_opacity:0.00}",_body);
            _opacity=GUI.HorizontalSlider(new Rect(1176,367,360,22),_opacity,.08f,.75f);
            Text(new Rect(1176,409,366,25),$"TILT / 真上から {_tilt:0}°",_body);
            _tilt=GUI.HorizontalSlider(new Rect(1176,443,360,22),_tilt,0,55);
            var states=new[] {"通常","Golden","過充電（見本）","凍結（見本）"};
            for(var i=0;i<4;i++) if(GUI.Button(new Rect(1176+(i%2)*184,492+(i/2)*42,174,36),(_status==i?"● ":"")+states[i],_button)) _status=i;
            if(GUI.Button(new Rect(1176,591,174,40),_animate?"回転・浮遊を停止":"回転・浮遊を再開",_button)) _animate=!_animate;
            if(GUI.Button(new Rect(1360,591,174,40),"命中Flash [H]",_button)) _impact=1;
            if(GUI.Button(new Rect(1176,646,174,40),"砕ける [B]",_button)) PreviewShatter();
            if(GUI.Button(new Rect(1360,646,174,40),"一覧へ [Esc]",_button)) { _selected=-1;BuildCollection(); }
            if(GUI.Button(new Rect(1176,698,358,40),_page==0?"球・16面体・32面体を見る [Tab]":"基本セットに戻る [Tab]",_button)) { _selected=-1;_page=1-_page;BuildCollection(); }
            Text(new Rect(1176,754,366,42),"表示だけの実験室です。\nゲームのHP・Saveには影響しません。",_small);
            for(var i=0;i<_pieces.Count;i++)
            {
                var x=_selected<0?62+(i%3)*315:270;var y=_selected<0?382+(i/3)*325:730;
                if(GUI.Button(new Rect(x,y,_selected<0?260:600,36),Labels[_kinds[i]]+(_selected<0?"  ↗":""),_button)) { _selected=_kinds[i];BuildCollection();break; }
            }
            Text(new Rect(42,839,1500,30),"LUCENT V1  /  11 PREFABS     ·     Space：動きの停止     ·     H：命中     ·     半透明は軽量表現（物理屈折なし）",_small);
            GUI.matrix=old;
        }
        private IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();var index=Array.IndexOf(args,"-crystal-capture");
            if(index<0 || index+1>=args.Length) yield break;
            var folder=args[index+1];Directory.CreateDirectory(folder);Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(1);var errors=0;
            for(var shot=0;shot<5;shot++)
            {
                _animate=false;_clock=1.2f;_spin=18;
                _selected=shot==2?1:shot==3?4:shot==4?5:-1;_page=shot==1?1:0;
                _health=shot==2?.25f:1;_status=shot==2?1:0;BuildCollection();
                yield return null;yield return new WaitForEndOfFrame();errors+=OverflowCount;
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"gallery-{shot}.png"));yield return new WaitForSecondsRealtime(.2f);
            }
            File.WriteAllText(Path.Combine(folder,"gallery-smoke.txt"),$"Assets={CrystalKitFactory.Names.Length}; Overflows={errors}; NoSaveSystem=True");
            Application.Quit(errors==0?0:1);
        }
    }
}
