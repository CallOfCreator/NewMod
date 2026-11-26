using System;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class EnergyThiefBreakEffect : ScreenEffect
{
    public float _startedAt;

    public override void Initialize()
    {
        CreateMaterial(NewModAsset.EnergyThiefBreak.LoadAsset(), "_Amount", "_Explosion");
        _mat.SetVector(Shader.PropertyToID("_BreakCenter"), new Vector4(0.5f, 0.5f, 0f, 0f));
        _startedAt = Time.unscaledTime;
    }

    public override void OnHudUpdate()
    {
        if (Sample(Time.unscaledTime - _startedAt).Finished)
            Remove();
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        var frame = Sample(Time.unscaledTime - _startedAt);
        _mat.SetFloat(Shader.PropertyToID("_Amount"), frame.Amount);
        _mat.SetFloat(Shader.PropertyToID("_Explosion"), frame.Explosion);
        Graphics.Blit(source, destination, _mat);
    }

    public void Restart()
    {
        _startedAt = Time.unscaledTime;
    }

    public const float Duration = 1.35f;

    public static (float Amount, float Explosion, bool Finished) Sample(float elapsed)
    {
        var time = Math.Clamp(elapsed, 0f, Duration);
        var amount = Math.Clamp(time / 0.22f, 0f, 1f);

        if (time > 1.05f)
            amount *= (Duration - time) / 0.3f;

        var explosion = Math.Clamp((time - 0.15f) / 1.2f, 0f, 1f);
        return (amount, explosion, elapsed >= Duration);
    }
}
