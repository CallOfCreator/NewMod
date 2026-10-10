using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class ShadowFluxEffect : ScreenEffect
{
    public override void Initialize()
    {
        var shader = NewModAsset.ShadowFluxShader.LoadAsset();
        var texture = NewModAsset.NoiseTex.LoadAsset();

        if (shader == null) Error("ShadowFluxEffect - Shader null");

        CreateMaterial(shader);
        _mat.SetTexture(Shader.PropertyToID("_NoiseTex"), texture);
    }

}