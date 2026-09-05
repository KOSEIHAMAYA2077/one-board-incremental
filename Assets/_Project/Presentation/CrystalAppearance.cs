using UnityEngine;

namespace IncrementalGame.Presentation
{
    public enum CrystalStatus { None, Golden, OverchargePreview, FrostPreview }
    // View data only. The caller owns health/status; this component never changes gameplay.
    public sealed class CrystalAppearance : MonoBehaviour
    {
        public Renderer[] Shells, Edges, Cores;
        public GameObject StatusRing;
        public Color BaseColor=new Color(.2f,.88f,.92f);
        public float Health=1, Opacity=.32f;
        public float EdgeInflation;
        public CrystalStatus Status;
        private MaterialPropertyBlock _properties;
        public Color ShellColor { get; private set; }
        public Color CoreColor { get; private set; }
        public void Apply(float health,CrystalStatus status,float flash=0,float opacity=.32f)
        {
            Health=Mathf.Clamp01(health);Status=status;Opacity=Mathf.Clamp01(opacity);
            ShellColor=Color.Lerp(new Color(1,.19f,.18f),BaseColor,Mathf.Pow(Health,.65f));
            CoreColor=status==CrystalStatus.Golden?new Color(1,.74f,.08f):status==CrystalStatus.OverchargePreview?new Color(.85f,.22f,1):status==CrystalStatus.FrostPreview?new Color(.42f,.73f,1):BaseColor;
            if(_properties==null) _properties=new MaterialPropertyBlock();
            Paint(Shells,ShellColor,flash,0);
            Paint(Edges,Color.Lerp(ShellColor,Color.white,.28f+flash*.5f),flash,.35f,EdgeInflation);
            Paint(Cores,Color.Lerp(CoreColor,Color.white,flash*.7f),flash,.6f);
            if(StatusRing!=null) StatusRing.SetActive(status!=CrystalStatus.None);
        }
        private void Paint(Renderer[] renderers,Color color,float flash,float emission,float inflate=0)
        {
            if(renderers==null) return;
            _properties.Clear();_properties.SetColor("_Tint",color);_properties.SetFloat("_Opacity",Opacity);
            _properties.SetFloat("_Damage",1-Health);_properties.SetFloat("_Flash",flash);_properties.SetFloat("_Emission",emission+flash);
            _properties.SetFloat("_Inflate",inflate);
            foreach(var renderer in renderers) if(renderer!=null) renderer.SetPropertyBlock(_properties);
        }
        private void OnEnable() { Apply(Health,Status,0,Opacity); }
    }
}
