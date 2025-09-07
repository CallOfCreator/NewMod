using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects
{
    [RegisterInIl2Cpp]
    public class SlowPulseHueEffect(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public float hueSpeed = 0.35f;
        public float saturation = 1.0f;
        public float strength = 1.0f;
        private readonly Shader _shader = NewModAsset.SlowPulseHueShader.LoadAsset();
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
            _mat.SetFloat("_HueSpeed", hueSpeed);
            _mat.SetFloat("_Saturation", saturation);
            _mat.SetFloat("_Strength", strength);
            Graphics.Blit(src, dst, _mat);
        }
    }
}
