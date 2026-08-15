using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

[RegisterInIl2Cpp]
public class VoidwalkerVoidEffect(IntPtr ptr) : MonoBehaviour(ptr)
{
    public float amount = 1f;

    public Color voidColor = new(0.48f, 0.12f, 1f, 1f);
    public Color deepColor = new(0.025f, 0.005f, 0.07f, 1f);
    public Color riftColor = new(0.76f, 0.35f, 1f, 1f);

    public float warpStrength = 0.012f;
    public float warpSpeed = 0.8f;

    public float voidStrength = 0.62f;
    public float riftStrength = 0.85f;
    public float riftScale = 8f;

    public float edgeGlow = 0.55f;
    public float chromaticShift = 0.0025f;

    public float vignette = 0.72f;

    public float pulseStrength = 0.16f;
    public float pulseSpeed = 1.15f;

    public float grainStrength = 0.045f;

    private Material _mat;

    public void OnEnable()
    {
        var shader = NewModAsset.VoidwalkerVoidShader.LoadAsset();

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

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!_mat)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat("_Amount", amount);

        _mat.SetColor("_VoidColor", voidColor);
        _mat.SetColor("_DeepColor", deepColor);
        _mat.SetColor("_RiftColor", riftColor);

        _mat.SetFloat("_WarpStrength", warpStrength);
        _mat.SetFloat("_WarpSpeed", warpSpeed);

        _mat.SetFloat("_VoidStrength", voidStrength);
        _mat.SetFloat("_RiftStrength", riftStrength);
        _mat.SetFloat("_RiftScale", riftScale);

        _mat.SetFloat("_EdgeGlow", edgeGlow);
        _mat.SetFloat("_ChromaticShift", chromaticShift);

        _mat.SetFloat("_Vignette", vignette);

        _mat.SetFloat("_PulseStrength", pulseStrength);
        _mat.SetFloat("_PulseSpeed", pulseSpeed);

        _mat.SetFloat("_GrainStrength", grainStrength);

        Graphics.Blit(src, dst, _mat);
    }
}