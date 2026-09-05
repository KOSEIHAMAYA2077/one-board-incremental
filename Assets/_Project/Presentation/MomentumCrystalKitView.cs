using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumNeonView
    {
        private Transform _kitLayer,_kitGun;
        private readonly Dictionary<int,CrystalAppearance> _kitTargets=new Dictionary<int,CrystalAppearance>();
        private readonly List<Renderer> _oldKitRenderers=new List<Renderer>();
        private CrystalAppearance _kitGunAppearance;
        public bool CrystalKitEnabled {get;private set;}
        public int CrystalKitTargetCount => _kitTargets.Count;
        public CrystalAppearance CrystalTarget(int id) => _kitTargets[id];
        private void InitializeCrystalKit(MomentumSimulation sim)
        {
            if(Resources.Load<GameObject>("CrystalKitV1/Prefabs/Octa")==null) return;
            foreach(var view in _targets.Values) foreach(var r in view.Root.GetComponentsInChildren<Renderer>())
                if(r.name!="Collision footprint" && r.name!="Crystal floor shadow") _oldKitRenderers.Add(r);
            foreach(Transform child in transform)
                if(child.name=="Reflector bumper" || child.name=="Crystal launcher" || child.name.StartsWith("Neon wall")) _oldKitRenderers.AddRange(child.GetComponentsInChildren<Renderer>());
            _kitLayer=new GameObject("Lucent V1 presentation layer").transform;_kitLayer.SetParent(transform,false);
            var choices=new[] {0,1,7,8,9,10};
            foreach(var t in sim.Targets)
            {
                var obj=CrystalKitFactory.Spawn(t.Armored?2:choices[(t.Id-1)%choices.Length],_kitLayer);
                var appearance=obj.GetComponent<CrystalAppearance>();appearance.EdgeInflation=.014f;
                // A dense sphere wire becomes subpixel noise at the gameplay size.
                if(!t.Armored && choices[(t.Id-1)%choices.Length]==9) foreach(var r in appearance.Edges) r.enabled=false;
                _kitTargets.Add(t.Id,appearance);
            }
            foreach(var bumper in sim.Obstacles)
            {
                var obj=CrystalKitFactory.Spawn(3,_kitLayer);obj.transform.position=LogicalSpace.ToWorld(bumper.Position);
                obj.transform.localScale=Vector3.one*.26f;obj.transform.localRotation=Quaternion.Euler(28,0,0);
            }
            var cabinet=CrystalKitFactory.Spawn(5,_kitLayer);
            cabinet.transform.position=LogicalSpace.ToWorld(new SimVector2((sim.Layout.Left+sim.Layout.Right)/2,(sim.Layout.Top+sim.Layout.Bottom)/2));
            cabinet.transform.localScale=new Vector3((float)sim.Layout.Width/600,(float)sim.Layout.Height/800,1);
            _kitGun=CrystalKitFactory.Spawn(4,_kitLayer).transform;_kitGun.position=LogicalSpace.ToWorld(sim.Layout.Gun);_kitGun.localScale=Vector3.one*.43f;
            _kitGunAppearance=_kitGun.GetComponent<CrystalAppearance>();SetCrystalKitEnabled(true);
        }
        public void SetCrystalKitEnabled(bool enabled)
        {
            CrystalKitEnabled=enabled && _kitLayer!=null;
            if(_kitLayer!=null) _kitLayer.gameObject.SetActive(CrystalKitEnabled);
            foreach(var r in _oldKitRenderers) r.enabled=!CrystalKitEnabled;
        }
        private void SyncCrystalKit(MomentumSimulation sim)
        {
            if(_kitLayer==null) return;
            foreach(var t in sim.Targets)
            {
                var app=_kitTargets[t.Id];app.gameObject.SetActive(t.Alive);
                var phase=(float)sim.Time*1.4f+t.Id;
                app.transform.position=(Vector3)LogicalSpace.ToWorld(t.Position)+new Vector3(0,Mathf.Sin(phase)*.016f,-.06f-Mathf.Sin(phase)*.025f);
                app.transform.localRotation=Quaternion.Euler(35,18,(float)sim.Time*10+t.Id*37);
                app.transform.localScale=Vector3.one*(float)(t.Radius/100)*.94f;
                app.BaseColor=t.Armored?new Color(1,.64f,.27f):new Color(.19f,.86f,.9f);
                app.Apply((float)(t.Hp/t.MaximumHp),t.GoldenMarked?CrystalStatus.Golden:CrystalStatus.None,_targets[t.Id].Flash/.25f,.5f);
            }
            _kitGunAppearance.Apply(1,CrystalStatus.None,sim.Bursting?.25f:0,.38f);
        }
    }
}
