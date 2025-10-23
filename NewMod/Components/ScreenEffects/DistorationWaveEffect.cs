using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects
{
    [RegisterInIl2Cpp]
    public class DistorationWaveEffect(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public float amplitude = 0.05f;
        public float frequency = 5f;
        public float speed = 1f;
        public float radius = 0.25f;
        public float falloff = 1f;
        public Vector2 center = new Vector2(0.5f, 0.5f);
        public Color tint = Color.white;
        private readonly Shader _shader = NewModAsset.DistorationWaveShader.LoadAsset();
        public Material _mat;

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
            _mat.SetFloat("_Speed", speed);
            _mat.SetFloat("_Radius", radius);
            _mat.SetFloat("_Falloff", falloff);
            _mat.SetVector("_Center", center);
            _mat.SetColor("_Tint", tint);
            Graphics.Blit(src, dst, _mat);
        }
    }
}
