using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class SlowPulseHueEffect : ScreenEffect
{
    public float hueSpeed = 0.35f;
    public float saturation = 1.0f;
    public float strength = 1.0f;
    public Shader _shader = NewModAsset.SlowPulseHueShader.LoadAsset();

    public override void Initialize()
    {
        _mat = new Material(_shader) { hideFlags = HideFlags.DontSave };
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        _mat.SetFloat("_HueSpeed", hueSpeed);
        _mat.SetFloat("_Saturation", saturation);
        _mat.SetFloat("_Strength", strength);
        Graphics.Blit(src, dst, _mat);
    }
}