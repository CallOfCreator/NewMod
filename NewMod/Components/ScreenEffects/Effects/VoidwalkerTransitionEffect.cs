using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class VoidwalkerTransitionEffect : ScreenEffect
{
    public float progress;
    public bool exitMode;

    public override void Initialize()
    {
        var shader = NewModAsset.VoidwalkerTransitionVoid.LoadAsset();

        if (!shader)
        {
            Active = false;
            return;
        }

        CreateMaterial(shader, "_Progress", "_ExitMode");
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        if (!_mat)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat(Shader.PropertyToID("_Progress"), progress);
        _mat.SetFloat(Shader.PropertyToID("_ExitMode"), exitMode ? 1f : 0f);

        Graphics.Blit(src, dst, _mat);
    }

    public void SetEnter(float value)
    {
        exitMode = false;
        progress = Mathf.Clamp01(value);
    }

    public void SetExit(float value)
    {
        exitMode = true;
        progress = Mathf.Clamp01(value);
    }
}