using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using NewMod.RoleLogic;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

[RegisterInIl2Cpp]
public class ScrDesyncEffect(nint ptr) : MonoBehaviour(ptr)
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

    private Material _material;
    private float _startedAt;

    public void OnEnable()
    {
        _material = new Material(NewModAsset.GlitchScreenV2.LoadAsset()) { hideFlags = HideFlags.DontSave };
        _startedAt = Time.time;
    }

    public void OnDisable()
    {
        Destroy(_material);
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
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

        _material.SetFloat("_Intensity", frame.Intensity);
        _material.SetFloat("_BlockSize", blockSize);
        _material.SetFloat("_ColorSplit", colorSplit);
        _material.SetFloat("_Scanline", scanline);
        _material.SetFloat("_Speed", speed);
        _material.SetFloat("_Burst", frame.Burst);
        _material.SetFloat("_TearStrength", tearStrength);
        _material.SetFloat("_Datamosh", datamosh);
        _material.SetFloat("_EdgeGlitch", edgeGlitch);
        _material.SetFloat("_SyncFailure", syncFailure);
        _material.SetFloat("_Blackout", blackout);
        _material.SetFloat("_FlashStrength", flashStrength);
        _material.SetFloat("_VerticalTear", verticalTear);
        _material.SetFloat("_MicroJitter", microJitter);
        _material.SetFloat("_FreezeStrength", freezeStrength);
        _material.SetColor("_CorruptTint", corruptTint);
        _material.SetColor("_ColdTint", coldTint);
        _material.SetColor("_BlackTint", blackTint);
        Graphics.Blit(source, destination, _material);
    }
}