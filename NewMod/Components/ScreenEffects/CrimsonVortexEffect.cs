using MiraAPI.GameOptions;
using NewMod.GeneralEvents.Season1;
using NewMod.Options;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

[RegisterInIl2Cpp]
public class CrimsonVortexEffect(nint ptr) : MonoBehaviour(ptr)
{
    public Color tint = new(0.85f, 0.015f, 0.025f, 1f);
    public Color hotTint = new(1f, 0.18f, 0.055f, 1f);
    public Color deepTint = new(0.16f, 0f, 0.015f, 1f);
    public Color coreTint = new(0.015f, 0f, 0.008f, 1f);

    public float amount = 1f;
    public float opacity = 0.9f;
    public float tiling = 1.35f;
    public float detailTiling = 3.15f;
    public float flowSpeed = 0.22f;
    public float vortexStrength = 2.35f;
    public float vortexSpeed = 0.5f;
    public float distortStrength = 0.025f;
    public float suctionStrength = 0.018f;
    public float chromaticShift = 0.0022f;
    public float density = 1.25f;
    public float contrast = 1.7f;
    public float filamentStrength = 1.35f;
    public float filamentSharpness = 5.5f;
    public float edgeSoftness = 0.18f;
    public float coreSoftness = 0.3f;
    public float coreDarkness = 0.94f;
    public float coreGlow = 1.9f;
    public float pulseStrength = 0.12f;
    public float pulseSpeed = 1.5f;
    public float vignette = 0.08f;
    public Vector2 scrollDir = new(1f, 0f);
    public float frontWidth = 0.75f;
    public float frontSoftness = 0.18f;
    public float frontTravel = 0.38f;
    public float leadingEdgeWidth = 0.045f;
    public float leadingEdgeStrength = 2.2f;

    public Camera _camera;
    public Material _mat;

    public void OnEnable()
    {
        _camera = GetComponent<Camera>();

        var shader = NewModAsset.CrismonVortexShader.LoadAsset();

        var texture = NewModAsset.CrismonTexture.LoadAsset();

        if (!shader || !texture)
        {
            enabled = false;
            return;
        }

        _mat = new Material(shader) { hideFlags = HideFlags.DontSave };

        _mat.SetTexture("_CrimsonTex", texture.texture);
    }

    public void OnDisable()
    {
        if (_mat)
            Destroy(_mat);

        _mat = null;
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!_mat || !CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance)
        {
            Graphics.Blit(src, dst);
            return;
        }

        var options = OptionGroupSingleton<GEOptions>.Instance;

        var worldCenter = new Vector3(CrismonVortexGE.VortexPosition.x, CrismonVortexGE.VortexPosition.y, 0f);

        var center = _camera.WorldToViewportPoint(worldCenter);

        var radius = options.CrimsonRadius / (_camera.orthographicSize * 2f);

        var killRadius = Mathf.Clamp(options.CrimsonRadius * 0.12f, 0.45f, 0.9f);
        var coreRadius = killRadius / (_camera.orthographicSize * 2f);

        _mat.SetFloat("_Amount", amount);

        _mat.SetColor("_Tint", tint);
        _mat.SetColor("_HotTint", hotTint);
        _mat.SetColor("_DeepTint", deepTint);
        _mat.SetColor("_CoreTint", coreTint);

        _mat.SetFloat("_Opacity", opacity);
        _mat.SetFloat("_Tiling", tiling);
        _mat.SetFloat("_DetailTiling", detailTiling);

        _mat.SetFloat("_FlowSpeed", flowSpeed);
        _mat.SetFloat("_VortexStrength", vortexStrength);
        _mat.SetFloat("_VortexSpeed", vortexSpeed);

        _mat.SetFloat("_DistortStrength", distortStrength);

        _mat.SetFloat("_SuctionStrength", suctionStrength);

        _mat.SetFloat("_ChromaticShift", chromaticShift);

        _mat.SetFloat("_Density", density);
        _mat.SetFloat("_Contrast", contrast);

        _mat.SetFloat("_FilamentStrength", filamentStrength);

        _mat.SetFloat("_FilamentSharpness", filamentSharpness);

        _mat.SetVector("_Center", new Vector4(center.x, center.y, 0f, 0f));

        _mat.SetFloat("_Radius", radius);

        _mat.SetFloat("_EdgeSoftness", radius * edgeSoftness);

        _mat.SetFloat("_CoreRadius", coreRadius);

        _mat.SetFloat("_CoreSoftness", coreRadius * coreSoftness);

        _mat.SetFloat("_CoreDarkness", coreDarkness);

        _mat.SetFloat("_CoreGlow", coreGlow);

        _mat.SetFloat("_PulseStrength", pulseStrength);

        _mat.SetFloat("_PulseSpeed", pulseSpeed);

        _mat.SetFloat("_Vignette", vignette);

        _mat.SetVector("_ScrollDir", new Vector4(scrollDir.x, scrollDir.y, 0f, 0f));

        _mat.SetFloat("_FrontWidth", frontWidth);

        _mat.SetFloat("_FrontSoftness", frontSoftness);

        _mat.SetFloat("_FrontTravel", frontTravel);

        _mat.SetFloat("_LeadingEdgeWidth", leadingEdgeWidth);

        _mat.SetFloat("_LeadingEdgeStrength", leadingEdgeStrength);

        _mat.SetFloat("_UseCircular", 1f);

        Graphics.Blit(src, dst, _mat);
    }
}
