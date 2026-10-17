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
        var now = AmongUsDateTime.UtcNow;
        if (now < SeasonStartDate.ToUniversalTime() || now >= SeasonEndDate.ToUniversalTime())
            return;

        var seasonText = new GameObject("NewMod_Season");
        seasonText.transform.SetParent(menuManager.transform.Find("MainUI/AspectScaler/RightPanel"), false);
        seasonText.transform.localPosition = new Vector3(-6.552f, 0.1f, 0f);

        var text = seasonText.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.TopRight;
        text.fontSize = 2.2f;
        Coroutines.Start(CoSeasonText(text));
    }

    public IEnumerator CoSeasonText(TextMeshPro text)
    {
        var endDate = SeasonEndDate.ToUniversalTime();
        var color = ColorUtility.ToHtmlStringRGB(SeasonMainColor);
        var labelTransform = text.transform;
        var position = labelTransform.localPosition;
        var startedAt = Time.unscaledTime;
        var nextUpdate = startedAt;
        var lastCountdown = string.Empty;

        while (text)
        {
            var now = Time.unscaledTime;
            if (now >= nextUpdate)
            {
                nextUpdate = now + 1f;
                var remaining = endDate - AmongUsDateTime.UtcNow;
                if (remaining.TotalSeconds <= 0)
                {
                    UnityEngine.Object.Destroy(text.gameObject);
                    yield break;
                }

                var seconds = (int)Math.Ceiling(remaining.TotalSeconds);
                var countdown = seconds switch
                {
                    >= 86400 => $"{seconds / 86400}d {seconds % 86400 / 3600:00}h",
                    >= 3600 => $"{seconds / 3600}h {seconds % 3600 / 60:00}m",
                    >= 60 => $"{seconds / 60}m {seconds % 60:00}s",
                    _ => $"{seconds}s",
                };

                if (countdown != lastCountdown)
                {
                    lastCountdown = countdown;
                    text.text = $"<color=#00FF00>Active season:</color> <color=#{color}>{Name}</color>\n<size=70%><color=#{color}>{countdown}</color> left</size>";
                }
            }

            var elapsed = now - startedAt;
            text.alpha = 0.96f + 0.04f * Mathf.Cos(elapsed * 1.5f);
            labelTransform.localPosition = position + Vector3.up * (0.025f * Mathf.Sin(elapsed * 1.5f));
            yield return null;
        }
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
