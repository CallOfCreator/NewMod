using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
using NewMod.Options;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

public class CrimsonVortexEffect : ScreenEffect
{
    public Camera _camera;
    public Material RenderMaterial;

    public override void Initialize()
    {
        _camera = Owner.GetComponent<Camera>();

        var shader = NewModAsset.CrismonVortexShader.LoadAsset();

        var texture = NewModAsset.CrismonTexture.LoadAsset();

        if (!shader || !texture)
        {
            Active = false;
            return;
        }

        CreateMaterial(shader, "_Center", "_Radius", "_CoreRadius", "_UseCircular");

        _mat.SetColor(Shader.PropertyToID("_Tint"), new Color(0.85f, 0.015f, 0.025f, 1f));
        _mat.SetColor(Shader.PropertyToID("_HotTint"), new Color(1f, 0.18f, 0.055f, 1f));
        _mat.SetColor(Shader.PropertyToID("_DeepTint"), new Color(0.16f, 0f, 0.015f, 1f));
        _mat.SetColor(Shader.PropertyToID("_CoreTint"), new Color(0.015f, 0f, 0.008f, 1f));
        _mat.SetFloat(Shader.PropertyToID("_Amount"), 1f);
        _mat.SetFloat(Shader.PropertyToID("_Opacity"), 0.9f);
        _mat.SetFloat(Shader.PropertyToID("_Tiling"), 1.35f);
        _mat.SetFloat(Shader.PropertyToID("_DetailTiling"), 3.15f);
        _mat.SetFloat(Shader.PropertyToID("_FlowSpeed"), 0.22f);
        _mat.SetFloat(Shader.PropertyToID("_VortexStrength"), 2.35f);
        _mat.SetFloat(Shader.PropertyToID("_VortexSpeed"), 0.5f);
        _mat.SetFloat(Shader.PropertyToID("_DistortStrength"), 0.025f);
        _mat.SetFloat(Shader.PropertyToID("_SuctionStrength"), 0.018f);
        _mat.SetFloat(Shader.PropertyToID("_ChromaticShift"), 0.0022f);
        _mat.SetFloat(Shader.PropertyToID("_Density"), 1.25f);
        _mat.SetFloat(Shader.PropertyToID("_Contrast"), 1.7f);
        _mat.SetFloat(Shader.PropertyToID("_FilamentStrength"), 1.35f);
        _mat.SetFloat(Shader.PropertyToID("_FilamentSharpness"), 5.5f);
        _mat.SetFloat(Shader.PropertyToID("_EdgeSoftness"), 0.18f);
        _mat.SetFloat(Shader.PropertyToID("_CoreSoftness"), 0.3f);
        _mat.SetFloat(Shader.PropertyToID("_CoreDarkness"), 0.94f);
        _mat.SetFloat(Shader.PropertyToID("_CoreGlow"), 1.9f);
        _mat.SetFloat(Shader.PropertyToID("_PulseStrength"), 0.12f);
        _mat.SetFloat(Shader.PropertyToID("_PulseSpeed"), 1.5f);
        _mat.SetFloat(Shader.PropertyToID("_Vignette"), 0.08f);
        _mat.SetVector(Shader.PropertyToID("_ScrollDir"), new Vector4(1f, 0f, 0f, 0f));
        _mat.SetFloat(Shader.PropertyToID("_FrontWidth"), 0.75f);
        _mat.SetFloat(Shader.PropertyToID("_FrontSoftness"), 0.18f);
        _mat.SetFloat(Shader.PropertyToID("_FrontTravel"), 0.38f);
        _mat.SetFloat(Shader.PropertyToID("_LeadingEdgeWidth"), 0.045f);
        _mat.SetFloat(Shader.PropertyToID("_LeadingEdgeStrength"), 2.2f);
        _mat.SetTexture(Shader.PropertyToID("_CrimsonTex"), texture);
        RenderMaterial = new Material(_mat) { hideFlags = HideFlags.DontSave };
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (!_mat || !CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance || PlayerControl.LocalPlayer.HasModifier<InVoid>())
        {
            Graphics.Blit(source, destination);
            return;
        }

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var warning = CrismonVortexGE.IsWarning();

        var worldCenter = new Vector3(CrismonVortexGE.VortexPosition.x, CrismonVortexGE.VortexPosition.y, 0f);

        var center = _camera.WorldToViewportPoint(worldCenter);

        var radius = options.CrimsonRadius.Value / (_camera.orthographicSize * 2f);

        var killRadius = Mathf.Clamp(options.CrimsonRadius.Value * 0.12f, 0.45f, 0.9f);
        var coreRadius = killRadius / (_camera.orthographicSize * 2f);

        RenderMaterial.CopyPropertiesFromMaterial(_mat);
        RenderMaterial.SetVector(Shader.PropertyToID("_Center"), new Vector4(center.x, center.y, 0f, 0f));
        RenderMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        RenderMaterial.SetFloat(Shader.PropertyToID("_EdgeSoftness"), radius * _mat.GetFloat(Shader.PropertyToID("_EdgeSoftness")));
        RenderMaterial.SetFloat(Shader.PropertyToID("_CoreRadius"), coreRadius);
        RenderMaterial.SetFloat(Shader.PropertyToID("_CoreSoftness"), coreRadius * _mat.GetFloat(Shader.PropertyToID("_CoreSoftness")));
        RenderMaterial.SetFloat(Shader.PropertyToID("_UseCircular"), 1f);

        if (warning)
        {
            RenderMaterial.SetColor(Shader.PropertyToID("_Tint"), Color.gray);
            RenderMaterial.SetColor(Shader.PropertyToID("_HotTint"), Color.gray);
            RenderMaterial.SetColor(Shader.PropertyToID("_DeepTint"), new Color(0.16f, 0.16f, 0.16f));
            RenderMaterial.SetColor(Shader.PropertyToID("_CoreTint"), Color.black);
            RenderMaterial.SetFloat(Shader.PropertyToID("_Opacity"), _mat.GetFloat(Shader.PropertyToID("_Opacity")) * 0.4f);
            RenderMaterial.SetFloat(Shader.PropertyToID("_DistortStrength"), 0f);
            RenderMaterial.SetFloat(Shader.PropertyToID("_SuctionStrength"), 0f);
            RenderMaterial.SetFloat(Shader.PropertyToID("_ChromaticShift"), 0f);
            RenderMaterial.SetFloat(Shader.PropertyToID("_CoreGlow"), 0f);
            RenderMaterial.SetFloat(Shader.PropertyToID("_LeadingEdgeStrength"), 0f);
        }

        Graphics.Blit(source, destination, RenderMaterial);
    }

    public override void Dispose()
    {
        if (RenderMaterial)
            Object.Destroy(RenderMaterial);
        RenderMaterial = null;
        base.Dispose();
    }
}
