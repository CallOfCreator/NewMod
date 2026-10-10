using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class EarthquakeEffect : ScreenEffect
{
    public override void Initialize()
    {
        CreateMaterial(NewModAsset.EarthquakeShader.LoadAsset());
    }

}