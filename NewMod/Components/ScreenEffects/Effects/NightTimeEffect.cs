using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using NewMod.Patches;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class NightTimeEffect : ScreenEffect
{
    public enum CyclePhase
    {
        Day,
        Sunset,
        Night,
        Sunrise
    }

    public bool autoCycle = true;
    public bool useExternalClock;
    public bool midnightMode;
    public bool skipDuringMeetings = true;
    public bool skipDuringExile = true;
    public bool skipInVoid = true;

    public CyclePhase startPhase = CyclePhase.Sunset;
    public float dayDuration = 25f;
    public float sunsetDuration = 8f;
    public float nightDuration = 35f;
    public float sunriseDuration = 8f;
    public float cycleSpeed = 1f;
    public float cycleOffset;
    public float externalElapsedSeconds;
    public float amount = 1f;

    public float midnightFadeDuration = 1.5f;
    public float midnightPlayerBrightness = 1.65f;
    public Color sunsetTint = new(1f, 0.62f, 0.38f, 1f);
    public Color sunriseTint = new(1f, 0.74f, 0.59f, 1f);

    public CyclePhase currentPhase = CyclePhase.Sunset;
    public float currentNightAmount;
    public float currentCycleProgress;
    public float currentMidnightAmount;
    public float cycleStartedAt;
    public float lastRenderTime;

    public override void Initialize()
    {
        var shader = NewModAsset.NightTimeShader.LoadAsset();

        if (!shader)
        {
            Active = false;
            return;
        }

        CreateMaterial(shader, "_Amount", "_Twilight", "_EffectTime", "_Midnight", "_TwilightTint");
        RestartCycle();
        lastRenderTime = Time.unscaledTime;
        currentMidnightAmount = midnightMode ? 1f : 0f;
    }

    public override void Dispose()
    {
        NightTimePlayerPatch.Restore();
        base.Dispose();
    }

    public void RestartCycle()
    {
        cycleStartedAt = Time.unscaledTime;
    }

    public void SetPhase(CyclePhase phase)
    {
        startPhase = phase;
        RestartCycle();
    }

    public void UpdateCycle(out float night, out float twilight)
    {
        night = 1f;
        twilight = 0f;
        if (!autoCycle)
        {
            currentPhase = CyclePhase.Night;
            currentCycleProgress = 0f;
            return;
        }

        var day = Mathf.Max(0.01f, dayDuration);
        var sunset = Mathf.Max(0.01f, sunsetDuration);
        var darkness = Mathf.Max(0.01f, nightDuration);
        var sunrise = Mathf.Max(0.01f, sunriseDuration);
        var total = day + sunset + darkness + sunrise;

        var start = startPhase switch
        {
            CyclePhase.Sunset => day,
            CyclePhase.Night => day + sunset,
            CyclePhase.Sunrise => day + sunset + darkness,
            _ => 0f
        };

        var elapsed = useExternalClock ? externalElapsedSeconds : Time.unscaledTime - cycleStartedAt;

        var position = Mathf.Repeat(elapsed * Mathf.Max(0f, cycleSpeed) + start + cycleOffset, total);
        currentCycleProgress = position / total;

        if (position < day)
        {
            currentPhase = CyclePhase.Day;
            night = 0f;
            return;
        }

        position -= day;

        if (position < sunset)
        {
            currentPhase = CyclePhase.Sunset;
            var progress = position / sunset;
            night = Mathf.SmoothStep(0f, 1f, progress);
            twilight = Mathf.Sin(progress * Mathf.PI);
            return;
        }

        position -= sunset;

        if (position < darkness)
        {
            currentPhase = CyclePhase.Night;
            return;
        }

        currentPhase = CyclePhase.Sunrise;
        var sunriseProgress = Mathf.Clamp01((position - darkness) / sunrise);
        night = 1f - Mathf.SmoothStep(0f, 1f, sunriseProgress);
        twilight = Mathf.Sin(sunriseProgress * Mathf.PI);
    }

    public bool ShouldRender()
    {
        return Active && _mat && !(skipDuringMeetings && MeetingHud.Instance) &&
            !(skipDuringExile && ExileController.Instance) &&
            !(skipInVoid && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.HasModifier<InVoid>());
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        if (!ShouldRender())
        {
            Graphics.Blit(src, dst);
            return;
        }

        var time = Time.unscaledTime;
        var delta = Mathf.Clamp(time - lastRenderTime, 0f, 0.1f);
        lastRenderTime = time;
        currentMidnightAmount = Mathf.MoveTowards(currentMidnightAmount, midnightMode ? 1f : 0f,
            delta / Mathf.Max(0.01f, midnightFadeDuration));

        UpdateCycle(out var night, out var twilight);
        currentNightAmount = Mathf.Clamp01(night * amount);
        var twilightAmount = Mathf.Clamp01(twilight * amount);

        if (currentNightAmount <= 0.0001f && twilightAmount <= 0.0001f)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat(Shader.PropertyToID("_Amount"), currentNightAmount);
        _mat.SetFloat(Shader.PropertyToID("_Twilight"), twilightAmount);
        _mat.SetFloat(Shader.PropertyToID("_EffectTime"), useExternalClock ? externalElapsedSeconds : time);
        _mat.SetFloat(Shader.PropertyToID("_Midnight"), currentMidnightAmount);
        _mat.SetColor(Shader.PropertyToID("_TwilightTint"), currentPhase == CyclePhase.Sunrise ? sunriseTint : sunsetTint);

        Graphics.Blit(src, dst, _mat);
    }
}