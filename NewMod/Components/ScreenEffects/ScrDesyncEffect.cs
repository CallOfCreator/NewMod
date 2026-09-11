using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using NewMod.RoleLogic;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

public class ScrDesyncEffect : ScreenEffect
{
    public float duration = 24f;
    public float blockSize = 48f;
    public float colorSplit = 1.15f;
    public float scanline = 0.13f;
    public float speed = 4f;
    public float tearStrength = 0.62f;
    public float datamosh = 0.48f;
    public float edgeGlitch = 0.85f;
    public float syncFailure = 0.68f;
    public float blackout = 0.36f;
    public float flashStrength = 1.15f;
    public float verticalTear = 0.38f;
    public float microJitter = 0.42f;
    public float freezeStrength = 0.38f;
    public Color corruptTint = new(1f, 0.08f, 0.72f, 1f);
    public Color coldTint = new(0.08f, 0.78f, 1f, 1f);
    public Color blackTint = new(0.008f, 0.002f, 0.012f, 1f);

    public float _startedAt;

    public override void Initialize()
    {
        _mat = new Material(NewModAsset.GlitchScreenV2.LoadAsset()) { hideFlags = HideFlags.DontSave };
        _startedAt = Time.time;
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (MeetingHud.Instance || ExileController.Instance || PlayerControl.LocalPlayer.HasModifier<InVoid>())
        {
            Graphics.Blit(source, destination);
            return;
        }

        var frame = ScreenDesynchronizationAnimation.Sample(Time.time - _startedAt, duration);

        if (frame.Finished)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _mat.SetFloat("_Intensity", frame.Intensity);
        _mat.SetFloat("_BlockSize", blockSize);
        _mat.SetFloat("_ColorSplit", colorSplit);
        _mat.SetFloat("_Scanline", scanline);
        _mat.SetFloat("_Speed", speed);
        _mat.SetFloat("_Burst", frame.Burst);
        _mat.SetFloat("_TearStrength", tearStrength);
        _mat.SetFloat("_Datamosh", datamosh);
        _mat.SetFloat("_EdgeGlitch", edgeGlitch);
        _mat.SetFloat("_SyncFailure", syncFailure);
        _mat.SetFloat("_Blackout", blackout);
        _mat.SetFloat("_FlashStrength", flashStrength);
        _mat.SetFloat("_VerticalTear", verticalTear);
        _mat.SetFloat("_MicroJitter", microJitter);
        _mat.SetFloat("_FreezeStrength", freezeStrength);
        _mat.SetColor("_CorruptTint", corruptTint);
        _mat.SetColor("_ColdTint", coldTint);
        _mat.SetColor("_BlackTint", blackTint);
        Graphics.Blit(source, destination, _mat);
    }
}