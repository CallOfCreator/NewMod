using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class ShadowFluxEffect : ScreenEffect
{
    public float noiseScale = 2f;
    public float speed = 0.3f;
    public float edgeWidth = 0.25f;
    public float threshold = 0.55f;
    public float opacity = 0.75f;
    public float darkness = 0.8f;
    public Color tint = Color.white;

    public override void Initialize()
    {
        var shader = NewModAsset.ShadowFluxShader.LoadAsset();
        var texture = NewModAsset.NoiseTex.LoadAsset();

        if (shader == null) Error("ShadowFluxEffect - Shader null");

        _mat = new Material(shader) { hideFlags = HideFlags.DontSave };
        _mat.SetTexture("_NoiseTex", texture);
    }

    public override void Render(RenderTexture src, RenderTexture dst)
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