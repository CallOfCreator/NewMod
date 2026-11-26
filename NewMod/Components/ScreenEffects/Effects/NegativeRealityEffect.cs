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

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (_mat == null || PlayerControl.LocalPlayer.HasModifier<InVoid>())
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, destination, _mat);
    }
}
