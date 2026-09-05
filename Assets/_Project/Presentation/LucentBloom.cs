using UnityEngine;
namespace IncrementalGame.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class LucentBloom : MonoBehaviour
    {
        private Material _material;
        [Range(0,1)] public float Strength=.38f;
        private void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(_material==null) { var shader=Resources.Load<Shader>("LucentBloom"); if(shader==null || !shader.isSupported) { Graphics.Blit(source,destination);return; } _material=new Material(shader); }
            var a=RenderTexture.GetTemporary(Mathf.Max(1,source.width/4),Mathf.Max(1,source.height/4),0,source.format);
            var b=RenderTexture.GetTemporary(a.width,a.height,0,source.format);
            try
            {
                Graphics.Blit(source,a,_material,0);
                for(var i=0;i<2;i++) { _material.SetVector("_Direction",new Vector4(1f/a.width,0,0,0));Graphics.Blit(a,b,_material,1);_material.SetVector("_Direction",new Vector4(0,1f/a.height,0,0));Graphics.Blit(b,a,_material,1); }
                _material.SetTexture("_Bloom",a);_material.SetFloat("_Strength",Strength);Graphics.Blit(source,destination,_material,2);
            }
            finally { RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b); }
        }
        private void OnDestroy() { if(_material!=null) Destroy(_material); }
    }
}
