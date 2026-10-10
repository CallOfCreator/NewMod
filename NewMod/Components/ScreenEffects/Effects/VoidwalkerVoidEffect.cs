using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class VoidwalkerVoidEffect : ScreenEffect
{
    public float amount = 1f;

    public override void Initialize()
    {
        var shader = NewModAsset.VoidwalkerVoidShader.LoadAsset();

        if (!shader)
        {
            Active = false;
            return;
        }

        CreateMaterial(shader, "_Amount");
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        if (!_mat)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat(Shader.PropertyToID("_Amount"), amount);

        Graphics.Blit(src, dst, _mat);
    }
}