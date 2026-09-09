using System;

namespace NewMod.RoleLogic;

public readonly record struct ScreenDesynchronizationFrame(float Intensity, float Burst, bool Finished);

public static class ScreenDesynchronizationAnimation
{
    public static ScreenDesynchronizationFrame Sample(float elapsed, float duration)
    {
        var time = Math.Clamp(elapsed, 0f, duration);
        var visibility = Math.Min(Math.Clamp(time / 0.25f, 0f, 1f), Math.Clamp((duration - time) / 0.35f, 0f, 1f));
        var opening = Pulse(time, 1f);
        var recurring = time < 1f ? 0f : Pulse((time - 1f) % 4.5f, 0.6f);
        var closing = time < duration - 1f ? 0f : Pulse(time - duration + 1f, 1f);
        var burst = Math.Max(opening, Math.Max(recurring, closing)) * visibility;

        return new ScreenDesynchronizationFrame((0.42f + burst * 0.18f) * visibility, burst, elapsed >= duration);
    }

    private static float Pulse(float time, float duration)
    {
        return time <= 0f || time >= duration ? 0f : MathF.Sin(MathF.PI * time / duration);
    }
}