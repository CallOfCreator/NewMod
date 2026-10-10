using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class NegativeRealityEffect : ScreenEffect
{
    public override void Initialize()
    {
        var shader = NewModAsset.NegativeRealityShader.LoadAsset();

        if (shader == null)
        {
            Active = false;
            return;
        }

        CreateMaterial(shader);
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        if (_mat == null || PlayerControl.LocalPlayer.HasModifier<InVoid>())
        {
            Graphics.Blit(src, dst);
            return;
        }

        Graphics.Blit(src, dst, _mat);
    }
}