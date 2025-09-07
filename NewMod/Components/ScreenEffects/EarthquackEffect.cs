using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects
{
    [RegisterInIl2Cpp]
    public class EarthquakeEffect(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public float amplitude = 2.5f;
        public float frequency = 14f;
        public float jitter = 0.6f;
        public float ghost = 0.3f;
        public float warp = 0.015f;
        private readonly Shader _shader = NewModAsset.EarthquakeShader.LoadAsset();
        private Material _mat;
        public void OnEnable()
        {
            _mat = new Material(_shader) { hideFlags = HideFlags.DontSave };
        }
        public void OnDisable()
        {
            Destroy(_mat);
        }
        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            _mat.SetFloat("_Amplitude", amplitude);
            _mat.SetFloat("_Frequency", frequency);
            _mat.SetFloat("_Jitter", jitter);
            _mat.SetFloat("_Ghost", ghost);
            _mat.SetFloat("_Warp", warp);
            Graphics.Blit(src, dst, _mat);
        }
    }
}
