using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

[RegisterInIl2Cpp]
public class NegativeRealityEffect(IntPtr ptr) : MonoBehaviour(ptr)
{
    public float amount = 1f;
    public float invertStrength = 1f;
    public float contrast = 2.05f;
    public float crushBlack = 0.08f;
    public float crushWhite = 0.07f;
    public float desaturate = 0.18f;

    public float cyanBleed = 0.85f;
    public Color ghostTint = new(0.68f, 1f, 1f, 1f);
    public float whiteGlow = 1.25f;
    public float glowThreshold = 0.42f;
    public float glowRadius = 2.2f;

    public float edgeStrength = 0.95f;
    public Color edgeTint = new(0.95f, 1f, 1f, 1f);

    public float vignetteStrength = 0.58f;
    public float vignetteSize = 0.42f;
    public float vignetteSoftness = 0.55f;
    public Color vignetteColor = new(0.01f, 0f, 0.025f, 1f);

    public float pulseStrength = 0.18f;
    public float pulseSpeed = 1.65f;
    public float pulseSharpness = 7.5f;

    public float filmBurn = 0.28f;
    public float burnSpeed = 0.35f;
    public float noiseStrength = 0.11f;
    public float noiseScale = 260f;

    public float chromaticOffset = 0.0035f;
    public float scanlineStrength = 0.08f;

    public Material _mat;

    public void OnEnable()
    {
        var shader = NewModAsset.NegativeReality.LoadAsset();

        if (shader == null)
        {
            enabled = false;
            return;
        }

        _mat = new Material(shader) { hideFlags = HideFlags.DontSave };
    }

    public void OnDisable()
    {
        if (_mat != null)
        {
            Destroy(_mat);
            _mat = null;
        }
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (_mat == null)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat("_Amount", amount);
        _mat.SetFloat("_InvertStrength", invertStrength);
        _mat.SetFloat("_Contrast", contrast);
        _mat.SetFloat("_CrushBlack", crushBlack);
        _mat.SetFloat("_CrushWhite", crushWhite);
        _mat.SetFloat("_Desaturate", desaturate);

        _mat.SetFloat("_CyanBleed", cyanBleed);
        _mat.SetColor("_GhostTint", ghostTint);
        _mat.SetFloat("_WhiteGlow", whiteGlow);
        _mat.SetFloat("_GlowThreshold", glowThreshold);
        _mat.SetFloat("_GlowRadius", glowRadius);

        _mat.SetFloat("_EdgeStrength", edgeStrength);
        _mat.SetColor("_EdgeTint", edgeTint);

        _mat.SetFloat("_VignetteStrength", vignetteStrength);
        _mat.SetFloat("_VignetteSize", vignetteSize);
        _mat.SetFloat("_VignetteSoftness", vignetteSoftness);
        _mat.SetColor("_VignetteColor", vignetteColor);

        _mat.SetFloat("_PulseStrength", pulseStrength);
        _mat.SetFloat("_PulseSpeed", pulseSpeed);
        _mat.SetFloat("_PulseSharpness", pulseSharpness);

        _mat.SetFloat("_FilmBurn", filmBurn);
        _mat.SetFloat("_BurnSpeed", burnSpeed);
        _mat.SetFloat("_NoiseStrength", noiseStrength);
        _mat.SetFloat("_NoiseScale", noiseScale);

        _mat.SetFloat("_ChromaticOffset", chromaticOffset);
        _mat.SetFloat("_ScanlineStrength", scanlineStrength);

        Graphics.Blit(src, dst, _mat);
    }
}