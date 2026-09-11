using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class DistorationWaveEffect : ScreenEffect
{
    public float amplitude = 0.05f;
    public float frequency = 5f;
    public float speed = 1f;
    public float radius = 0.25f;
    public float falloff = 1f;
    public Vector2 center = new(0.5f, 0.5f);
    public Color tint = Color.white;
    public float expiresAt = float.PositiveInfinity;
    public Shader _shader = NewModAsset.DistorationWaveShader.LoadAsset();

    public override void Initialize()
    {
        _mat = new Material(_shader) { hideFlags = HideFlags.DontSave };
    }

    public override void Tick()
    {
        if (Time.time >= expiresAt)
            Remove();
    }

    public override void Render(RenderTexture src, RenderTexture dst)
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