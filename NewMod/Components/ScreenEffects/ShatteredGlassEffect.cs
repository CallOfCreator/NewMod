using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class ShatteredGlassEffect : ScreenEffect
{
    public float damageAmount = 1f;

    public Vector2 impactCenter = new(0.5f, 0.5f);
    public float crackScale = 1f;

    public bool crackInvert;
    public float crackThreshold = 0.48f;
    public float crackSharpness = 1.5f;
    public float crackStrength = 1.4f;

    public float refraction = 0.011f;
    public float depth = 0.014f;
    public float prismShift = 0.003f;

    public float bevelStrength = 1.25f;
    public float shadowStrength = 0.72f;
    public float crackDarkness = 0.72f;

    public float frostStrength = 0.55f;
    public float frostDesaturate = 0.48f;

    public Color glassTint = new(0.72f, 0.9f, 1f, 1f);
    public Color edgeTint = new(0.9f, 0.97f, 1f, 1f);
    public Color shadowTint = new(0.06f, 0.09f, 0.13f, 1f);

    public float impactGlow = 1.25f;
    public float impactDarkness = 0.5f;

    public float sparkleStrength = 0.85f;
    public float sparkleSpeed = 2.1f;

    public float revealSoftness = 0.14f;
    public float shockwaveStrength = 0.75f;

    public float globalDesaturate = 0.1f;
    public float alpha = 1f;

    public override void Initialize()
    {
        var shader = NewModAsset.ShatteredGlassShader.LoadAsset();
        var texture = NewModAsset.ShatteredGlassTexture.LoadAsset();

        if (!shader || !texture)
        {
            Active = false;
            return;
        }

        _mat = new Material(shader) { hideFlags = HideFlags.DontSave };
        _mat.SetTexture("_CrackTex", texture);
    }

    public override void Render(RenderTexture src, RenderTexture dst)
    {
        if (!_mat)
        {
            Graphics.Blit(src, dst);
            return;
        }

        _mat.SetFloat("_DamageAmount", damageAmount);

        _mat.SetVector("_ImpactCenter", new Vector4(impactCenter.x, impactCenter.y, 0f, 0f));
        _mat.SetFloat("_CrackScale", crackScale);

        _mat.SetFloat("_CrackInvert", crackInvert ? 1f : 0f);
        _mat.SetFloat("_CrackThreshold", crackThreshold);
        _mat.SetFloat("_CrackSharpness", crackSharpness);
        _mat.SetFloat("_CrackStrength", crackStrength);

        _mat.SetFloat("_Refraction", refraction);
        _mat.SetFloat("_Depth", depth);
        _mat.SetFloat("_PrismShift", prismShift);

        _mat.SetFloat("_BevelStrength", bevelStrength);
        _mat.SetFloat("_ShadowStrength", shadowStrength);
        _mat.SetFloat("_CrackDarkness", crackDarkness);

        _mat.SetFloat("_FrostStrength", frostStrength);
        _mat.SetFloat("_FrostDesaturate", frostDesaturate);

        _mat.SetColor("_GlassTint", glassTint);
        _mat.SetColor("_EdgeTint", edgeTint);
        _mat.SetColor("_ShadowTint", shadowTint);

        _mat.SetFloat("_ImpactGlow", impactGlow);
        _mat.SetFloat("_ImpactDarkness", impactDarkness);

        _mat.SetFloat("_SparkleStrength", sparkleStrength);
        _mat.SetFloat("_SparkleSpeed", sparkleSpeed);

        _mat.SetFloat("_RevealSoftness", revealSoftness);
        _mat.SetFloat("_ShockwaveStrength", shockwaveStrength);

        _mat.SetFloat("_GlobalDesaturate", globalDesaturate);
        _mat.SetFloat("_Alpha", alpha);

        Graphics.Blit(src, dst, _mat);
    }

    public void SetImpact(Vector2 viewportPosition)
    {
        impactCenter = new Vector2(Mathf.Clamp01(viewportPosition.x), Mathf.Clamp01(viewportPosition.y));
    }
}