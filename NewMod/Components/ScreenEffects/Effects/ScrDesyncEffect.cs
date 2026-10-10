using System;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class ScrDesyncEffect : ScreenEffect
{
    public float duration = 24f;

    public float _startedAt;

    public override void Initialize()
    {
        CreateMaterial(NewModAsset.GlitchScreenV2.LoadAsset(), "_Intensity", "_Burst");
        _startedAt = Time.time;
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (MeetingHud.Instance || ExileController.Instance || PlayerControl.LocalPlayer.HasModifier<InVoid>())
        {
            Graphics.Blit(source, destination);
            return;
        }

        var frame = Sample(Time.time - _startedAt, duration);

        if (frame.Finished)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _mat.SetFloat(Shader.PropertyToID("_Intensity"), frame.Intensity);
        _mat.SetFloat(Shader.PropertyToID("_Burst"), frame.Burst);
        Graphics.Blit(source, destination, _mat);
    }

    public static (float Intensity, float Burst, bool Finished) Sample(float elapsed, float duration)
    {
        var time = Math.Clamp(elapsed, 0f, duration);
        var visibility = Math.Min(Math.Clamp(time / 0.25f, 0f, 1f), Math.Clamp((duration - time) / 0.35f, 0f, 1f));
        var opening = Pulse(time, 1f);
        var recurring = time < 1f ? 0f : Pulse((time - 1f) % 4.5f, 0.6f);
        var closing = time < duration - 1f ? 0f : Pulse(time - duration + 1f, 1f);
        var burst = Math.Max(opening, Math.Max(recurring, closing)) * visibility;

        return ((0.42f + burst * 0.18f) * visibility, burst, elapsed >= duration);
    }

    public static float Pulse(float time, float duration)
    {
        return time <= 0f || time >= duration ? 0f : MathF.Sin(MathF.PI * time / duration);
    }
}
