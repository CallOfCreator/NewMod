using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class EarthquakeEffect : ScreenEffect
{
    public float amplitude = 2.5f;
    public float frequency = 14f;
    public float jitter = 0.6f;
    public float ghost = 0.3f;
    public float warp = 0.015f;
    public Shader _shader = NewModAsset.EarthquakeShader.LoadAsset();

    public override void Initialize()
    {
        _mat = new Material(_shader) { hideFlags = HideFlags.DontSave };
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        _mat.SetFloat("_Amplitude", amplitude);
        _mat.SetFloat("_Frequency", frequency);
        _mat.SetFloat("_Jitter", jitter);
        _mat.SetFloat("_Ghost", ghost);
        _mat.SetFloat("_Warp", warp);
        Graphics.Blit(src, dst, _mat);
    }
}