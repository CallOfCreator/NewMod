using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class DistorationWaveEffect : ScreenEffect
{
    public float expiresAt = float.PositiveInfinity;

    public override void Initialize()
    {
        CreateMaterial(NewModAsset.DistorationWaveShader.LoadAsset());
    }

    public override void Tick()
    {
        if (Time.time >= expiresAt)
            Remove();
    }

}