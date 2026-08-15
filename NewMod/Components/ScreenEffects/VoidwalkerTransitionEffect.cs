using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

[RegisterInIl2Cpp]
public class VoidwalkerTransitionEffect(IntPtr ptr) : MonoBehaviour(ptr)
{
    public float progress;
    public bool exitMode;

    public Color voidColor = new(0.48f, 0.1f, 1f, 1f);
    public Color glowColor = new(0.82f, 0.35f, 1f, 1f);
    public Color coreColor = new(0.025f, 0f, 0.07f, 1f);

    public float distortion = 0.055f;
    public float twist = 1.15f;
    public float ringWidth = 0.085f;
    public float glowStrength = 1.8f;
    public float flashStrength = 0.55f;

    private Material _mat;

    public void OnEnable()
    {
        var shader = NewModAsset.VoidwalkerTransitionVoid.LoadAsset();

        if (!shader)
        {
            enabled = false;
            return;
        }

        _mat = new Material(shader) { hideFlags = HideFlags.DontSave };
    }

    public void OnDisable()
    {
        if (_mat)
            Destroy(_mat);

        _mat = null;
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

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!_mat)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat("_Progress", progress);
        _mat.SetFloat("_ExitMode", exitMode ? 1f : 0f);

        _mat.SetColor("_VoidColor", voidColor);
        _mat.SetColor("_GlowColor", glowColor);
        _mat.SetColor("_CoreColor", coreColor);

        _mat.SetFloat("_Distortion", distortion);
        _mat.SetFloat("_Twist", twist);
        _mat.SetFloat("_RingWidth", ringWidth);
        _mat.SetFloat("_GlowStrength", glowStrength);
        _mat.SetFloat("_FlashStrength", flashStrength);

        Graphics.Blit(src, dst, _mat);
    }
}