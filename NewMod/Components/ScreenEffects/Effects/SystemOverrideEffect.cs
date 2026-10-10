using MiraAPI.Modifiers;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class SystemOverrideEffect : ScreenEffect
{
    public float _startedAt;

    public override void Initialize()
    {
        _startedAt = Time.time;
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (!SystemOverrideGE.Active || MeetingHud.Instance || ExileController.Instance || PlayerControl.LocalPlayer.HasModifier<InVoid>())
        {
            Graphics.Blit(source, destination);
            return;
        }

        var time = Time.time - _startedAt;

        if (time < 0.09f || time is >= 0.16f and < 0.27f || Mathf.Repeat(time, 2.7f) < 0.05f)
        {
            Graphics.Blit(Texture2D.blackTexture, destination);
            return;
        }

        var phase = Mathf.FloorToInt(time / 1.7f) % 5;

        switch (phase)
        {
            case 0:
                Graphics.Blit(source, destination);
                break;
            case 1:
                Graphics.Blit(source, destination, new Vector2(-1f, 1f), new Vector2(1f, 0f));
                break;
            case 2:
                Graphics.Blit(source, destination, new Vector2(1f, -1f), new Vector2(0f, 1f));
                break;
            case 3:
                Graphics.Blit(source, destination, new Vector2(-1f, -1f), Vector2.one);
                break;
            default:
                var offset = Mathf.Sin(time * 43f) * 0.018f;
                Graphics.Blit(source, destination, Vector2.one, new Vector2(offset, 0f));
                break;
        }
    }
}
