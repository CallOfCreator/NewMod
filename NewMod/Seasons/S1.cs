using System;
using System.Collections.Generic;
using NewMod.Buttons.Roles.S1;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles.S1;
using TMPro;
using UnityEngine;

namespace NewMod.Seasons
{
    public class S1 : ISeason
    {
        public string Name => "Season 1";
        public Il2CppSystem.DateTime SeasonStartDate => new(2026, 05, 10, 0, 0, 0, 0, Il2CppSystem.DateTimeKind.Utc);
        public Il2CppSystem.DateTime SeasonEndDate => new(2026, 08, 10, 0, 0, 0, 0, Il2CppSystem.DateTimeKind.Utc);
        public Color SeasonMainColor => Color.yellow;

        public void HandleMainMenu(MainMenuManager menuManager)
        {
            var seasonText = new GameObject("NewMod_Season");
            seasonText.transform.SetParent(menuManager.transform.Find("MainUI/AspectScaler/RightPanel"), false);
            seasonText.transform.localPosition = new Vector3(-6.552f, 0.1f, 0f);

            var tmp = seasonText.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.TopRight;
            tmp.fontSize = 2.2f;

            var now = AmongUsDateTime.UtcNow;
            int daysLeft = Math.Max(0, (SeasonEndDate.Date - now.Date).Days);
            string hex = ColorUtility.ToHtmlStringRGB(SeasonMainColor);

            tmp.text =
                $"<color=#00FF00>Active Seasons:</color> <color=#{hex}>{Name}</color>\n" +
                $"<size=70%><color=#{hex}>{daysLeft} days</color> left</size>";
        }

        public IReadOnlyList<Type> GetSeasonRoleTypes() =>
        [
            typeof(TerminatorRole),
            typeof(MirrorBladeRole),
            typeof(VerifierRole)
        ];

        public IReadOnlyList<Type> GetSeasonModifierTypes() =>
        [
            typeof(FatefulModifier),
            typeof(LazyModifier)
        ];
        public IReadOnlyList<Type> GetSeasonOptionTypes() =>
        [
            typeof(TerminatorOptions),
            typeof(MirrorBladeOptions),
            typeof(VerifierOptions)
        ];
        public IReadOnlyList<Type> GetSeasonButtonTypes() =>
        [
            typeof(MirrorReflectButton),
            typeof(ObjectiveButton),
        ];

        public IReadOnlyList<Type> GetSeasonGamemodeTypes() => [];

        public IReadOnlyList<Type> GetSeasonGETypes() =>
        [
            typeof(NegativeRealityGE),
            typeof(CrismonVortexGE)
        ];
    }
}