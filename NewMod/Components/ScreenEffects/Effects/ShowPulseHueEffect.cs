using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class SlowPulseHueEffect : ScreenEffect
{
    public override void Initialize()
    {
        CreateMaterial(NewModAsset.SlowPulseHueShader.LoadAsset());
    }

}