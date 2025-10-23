using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects
{
    [RegisterInIl2Cpp]
    public class ShadowFluxEffect(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public Texture2D texture = NewModAsset.NoiseTex.LoadAsset();
        public float noiseScale = 2f;
        public float speed = 0.3f;
        public float edgeWidth = 0.25f;
        public float threshold = 0.55f;
        public float opacity = 0.75f;
        public float darkness = 0.8f;
        public Color tint = Color.white;
        public static Shader _shader = NewModAsset.ShadowFluxShader.LoadAsset();
        public Material _mat;

        public void OnEnable()
        {
            _mat = new Material(_shader) { hideFlags = HideFlags.DontSave };
            _mat.SetTexture("_NoiseTex", texture);
        }
        public void OnDisable()
        {
            Destroy(_mat);
        }
        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            _mat.SetFloat("_NoiseScale", noiseScale);
            _mat.SetFloat("_Speed", speed);
            _mat.SetFloat("_EdgeWidth", edgeWidth);
            _mat.SetFloat("_Threshold", threshold);
            _mat.SetFloat("_Opacity", opacity);
            _mat.SetFloat("_Darkness", darkness);
            _mat.SetColor("_Tint", tint);
            Graphics.Blit(src, dst, _mat);
        }
    }
}
