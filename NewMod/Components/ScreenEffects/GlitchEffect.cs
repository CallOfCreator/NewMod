using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class GlitchEffect : ScreenEffect
{
    public float intensity = 0.45f;
    public float blockSize = 64f;
    public float colorSplit = 1.2f;
    public float scanline = 0.15f;
    public float speed = 4f;
    public Shader _shader = NewModAsset.GlitchShader.LoadAsset();

    public override void Initialize()
    {
        _mat = new Material(_shader) { hideFlags = HideFlags.DontSave };
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        _mat.SetFloat("_Intensity", intensity);
        _mat.SetFloat("_BlockSize", blockSize);
        _mat.SetFloat("_ColorSplit", colorSplit);
        _mat.SetFloat("_Scanline", scanline);
        _mat.SetFloat("_Speed", speed);
        Graphics.Blit(src, dst, _mat);
    }
}