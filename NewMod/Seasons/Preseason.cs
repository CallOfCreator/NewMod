using System.Collections.Generic;
using Il2CppSystem;
using NewMod.Achievements;
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
using TMPro;
using UnityEngine;
using Type = System.Type;

namespace NewMod.Seasons;

public class Preseason : ISeason
{
    public string Name => "Preseason ALPHA";

    public DateTime SeasonStartDate => new(2026, 08, 28, 18, 0, 0, 0, DateTimeKind.Utc);

    public DateTime SeasonEndDate => new(2026, 09, 8, 18, 30, 0, 0, DateTimeKind.Utc);

    public Color SeasonMainColor => Color.blue;

    public bool ContentExpires => true;

    public void HandleMainMenu(MainMenuManager menuManager)
    {
        var seasonText = new GameObject("NewMod_Season");

        seasonText.transform.SetParent(menuManager.transform.Find("MainUI/AspectScaler/RightPanel"), false);

        seasonText.transform.localPosition = new Vector3(-6.552f, 0.1f, 0f);

        var text = seasonText.AddComponent<TextMeshPro>();

        text.alignment = TextAlignmentOptions.TopRight;

        text.fontSize = 2.2f;

        var now = AmongUsDateTime.UtcNow;

        var daysLeft = Math.Max(0, (SeasonEndDate.Date - now.Date).Days);

        var color = ColorUtility.ToHtmlStringRGB(SeasonMainColor);

        text.text = $"<color=#00FF00>Active Seasons:</color> " + $"<color=#{color}>{Name}</color>\n" + $"<size=70%><color=#{color}>{daysLeft} days</color> left</size>";
    }

    public IReadOnlyList<Type> GetSeasonRoleTypes()
    {
        return
        [
            typeof(TerminatorRole),
            typeof(MirrorBladeRole),
            typeof(VerifierRole),
            typeof(Voidwalker)
        ];
    }

    public IReadOnlyList<Type> GetSeasonModifierTypes()
    {
        return
        [
            typeof(FatefulModifier),
            typeof(LazyModifier),
            typeof(MomentumModifier),
            typeof(InVoid),
            typeof(JustLeftVoid)
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
            typeof(EnterVoid),
            typeof(TopWraithLaneButton),
            typeof(MidWraithLaneButton),
            typeof(BottomWraithLaneButton),
            typeof(BanishWraithButton),
            typeof(WraithSiegeReviveButton)
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
            typeof(CrismonVortexGE),
            typeof(IdentityCrisisGE),
            typeof(NoMansLandGE)
        ];
    }

    public IReadOnlyList<Type> GetSeasonAchievementTabTypes()
    {
        return
        [
            typeof(PreseasonAchievementsTab)
        ];
    }
}