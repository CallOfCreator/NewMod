using System.Collections;
using System.Collections.Generic;
using Il2CppSystem;
using NewMod.Buttons.Roles.S1;
using NewMod.GameModes.WraithSiegeGamemode;
using NewMod.GameModes.WraithSiegeGamemode.Buttons;
using NewMod.GameModes.WraithSiegeGamemode.Options;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
using NewMod.Options;
using NewMod.Options.Modifiers;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles.S1;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using DeadwireRole = NewMod.Roles.ImpostorRoles.S1.Deadwire;
using Type = System.Type;

namespace NewMod.Seasons;

public class S1 : ISeason
{
    public string Name => "Season 1";

    public DateTime SeasonStartDate => new(2026, 09, 18, 0, 0, 0, 0, DateTimeKind.Utc);

    public DateTime SeasonEndDate => new(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc);
    public Color SeasonMainColor => Color.blue;
    public bool ContentExpires => false;

    public void HandleMainMenu(MainMenuManager menuManager)
    {
        var seasonText = new GameObject("NewMod_Season");
        seasonText.transform.SetParent(menuManager.transform.Find("MainUI/NewModMenu/Home"), false);
        seasonText.transform.localPosition = new Vector3(0f, 1.8f, -1f);
        seasonText.transform.localScale = new Vector3(1f, 1.15f, 1f);

        var text = seasonText.AddComponent<TextMeshPro>();
        text.font = NewModAsset.BrookFont.LoadAsset();
        text.fontSharedMaterial = text.font.material;
        text.rectTransform.pivot = new Vector2(0.5f, 1f);
        text.rectTransform.sizeDelta = new Vector2(10f, 2.2f);
        text.alignment = TextAlignmentOptions.Top;
        text.fontSize = 15f;
        text.characterSpacing = 4f;
        text.color = Color.white;
        text.enableVertexGradient = true;
        var topColor = new Color32(255, 246, 200, 255);
        var bottomColor = new Color32(255, 205, 80, 255);
        text.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
        text.outlineColor = new Color32(255, 170, 40, 255);
        text.outlineWidth = 0.01f;
        var material = text.fontMaterial;
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.05f);
        material.EnableKeyword(ShaderUtilities.Keyword_Glow);
        material.SetColor(ShaderUtilities.ID_GlowColor, new Color32(255, 170, 40, 255));
        material.SetFloat(ShaderUtilities.ID_GlowOffset, 0f);
        material.SetFloat(ShaderUtilities.ID_GlowInner, 0.2f);
        material.SetFloat(ShaderUtilities.ID_GlowOuter, 0.5f);
        material.SetFloat(ShaderUtilities.ID_GlowPower, 0.7f);
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color32(10, 3, 40, 153));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.15f);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.05f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
        text.UpdateMeshPadding();
        Coroutines.Start(CoSeasonText(text, material));
    }

    public IEnumerator CoSeasonText(TextMeshPro text, Material material)
    {
        var startDate = SeasonStartDate.ToUniversalTime();
        var endDate = SeasonEndDate.ToUniversalTime();
        var nextUpdate = Time.unscaledTime;
        var lastCaption = string.Empty;

        while (text)
        {
            var now = Time.unscaledTime;
            if (now >= nextUpdate)
            {
                nextUpdate = now + 1f;
                var date = AmongUsDateTime.UtcNow;
                var caption = "CONTENT AVAILABLE";
                if (date >= startDate && date < endDate)
                {
                    var seconds = (int)Math.Ceiling((endDate - date).TotalSeconds);
                    caption = seconds switch
                    {
                        >= 86400 => $"{seconds / 86400}d {seconds % 86400 / 3600:00}h left",
                        >= 3600 => $"{seconds / 3600}h {seconds % 3600 / 60:00}m left",
                        >= 60 => $"{seconds / 60}m {seconds % 60:00}s left",
                        _ => $"{seconds}s left",
                    };
                }

                if (caption != lastCaption)
                {
                    lastCaption = caption;
                    text.text = $"{Name.ToUpperInvariant()}\n<size=30%>{caption}</size>";
                }
            }

            yield return null;
        }

        UnityEngine.Object.Destroy(material);
    }

    public IReadOnlyList<Type> GetSeasonRoleTypes()
    {
        return
        [
            typeof(TerminatorRole),
            typeof(MirrorBladeRole),
            typeof(VerifierRole),
            typeof(Voidwalker),
            typeof(WardenRole),
            typeof(ArbitratorRole),
            typeof(DeadwireRole),
            typeof(Nomad),
            typeof(Collector),
            typeof(Bounty),
            typeof(Usurper)
        ];
    }

    public IReadOnlyList<Type> GetSeasonModifierTypes()
    {
        return
        [
            typeof(FatefulModifier),
            typeof(LazyModifier),
            typeof(InVoid),
            typeof(MarkedModifier),
            typeof(MomentumModifier),
            typeof(OverclockedModifier)
        ];
    }

    public IReadOnlyList<Type> GetSeasonOptionTypes()
    {
        return
        [
            typeof(TerminatorOptions),
            typeof(MirrorBladeOptions),
            typeof(VerifierOptions),
            typeof(WraithSiegeOptions),
            typeof(VoidwalkerOptions),
            typeof(MomentumModifierOptions),
            typeof(OverclockedModifierOptions),
            typeof(MarkedModifierOptions),
            typeof(WardenOptions),
            typeof(ArbitratorOptions),
            typeof(DeadwireOptions),
            typeof(NomadOptions),
            typeof(CollectorOptions),
            typeof(BountyOptions),
            typeof(UsurperOptions),
            typeof(GEOptions)
        ];
    }

    public IReadOnlyList<Type> GetSeasonButtonTypes()
    {
        return
        [
            typeof(MirrorReflectButton),
            typeof(ObjectiveButton),
            typeof(TerminateButton),
            typeof(TopWraithLaneButton),
            typeof(MidWraithLaneButton),
            typeof(BottomWraithLaneButton),
            typeof(BanishWraithButton),
            typeof(WraithSiegeReviveButton),
            typeof(InspectResidualButton),
            typeof(WardenSealButton),
            typeof(ConvertFragmentsButton),
            typeof(EnterVoid),
            typeof(ArbitratorLeverageButton),
            typeof(DeadlockButton),
            typeof(OverrideButton),
            typeof(AnchorButton),
            typeof(WanderButton),
            typeof(HarvestButton),
            typeof(ManifestButton),
            typeof(CashOutButton),
            typeof(ClaimButton)
        ];
    }

    public IReadOnlyList<Type> GetSeasonGamemodeTypes()
    {
        return
        [
            typeof(WraithSiege)
        ];
    }

    public IReadOnlyList<Type> GetSeasonGETypes()
    {
        return
        [
            typeof(NegativeRealityGE),
            typeof(CrismonVortexGE),
            typeof(AbilityExchangeGE),
            typeof(IdentityCrisisGE),
            typeof(NoMansLandGE),
            typeof(RoleScrambleGE),
            typeof(ScrDesyncEffectGE),
            typeof(SystemOverrideGE)
        ];
    }
}
