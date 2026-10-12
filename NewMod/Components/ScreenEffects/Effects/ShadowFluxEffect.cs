using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class ShadowFluxEffect : ScreenEffect
{
    public override void Initialize()
    {
        var shader = NewModAsset.ShadowFluxShader.LoadAsset();
        var texture = NewModAsset.NoiseTex.LoadAsset();

        if (!shader)
        {
            Error("ShadowFluxEffect - Shader null");
            Active = false;
            return;
        }

        CreateMaterial(shader);
        _mat.SetTexture(Shader.PropertyToID("_NoiseTex"), texture);
    }
}
