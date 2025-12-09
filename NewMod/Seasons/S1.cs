using System;
using NewMod.Modifiers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NewMod.Seasons
{
    public class S1 : ISeason
    {
        public string Name => "Season 1";
        public Il2CppSystem.DateTime SeasonStartDate => new(2025, 12, 6, 0, 0, 0, 0, Il2CppSystem.DateTimeKind.Utc);
        public Il2CppSystem.DateTime SeasonEndDate => new(2026, 2, 4, 0, 0, 0, 0, Il2CppSystem.DateTimeKind.Utc);
        public Color SeasonMainColor => Color.yellow;
        public void HandleMainMenu(MainMenuManager mainMenuManager)
        {
            var seasonText = new GameObject("NewMod_Season");
            seasonText.transform.SetParent(mainMenuManager.transform.Find("MainUI/AspectScaler/RightPanel"), false);
            seasonText.transform.localPosition = new Vector3(-6.552f, 0.1f, 0f);
            var tmp = seasonText.gameObject.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.TopRight;
            tmp.fontSize = 2.2f;

            var now = AmongUsDateTime.UtcNow;
            var daysLeft = (SeasonEndDate.Date - now.Date).Days;
            if (daysLeft < 0)
                daysLeft = 0;

            var colorHex = ColorUtility.ToHtmlStringRGB(SeasonMainColor);

            tmp.text =
               $"<color=#00FF00>Active Seasons:</color> " +
               $"<color=#{colorHex}>{Name}</color>\n" +
               $"<size=70%><color=#{colorHex}>{daysLeft} days</color> left</size>";
        }
        public IReadOnlyList<Type> GetSeasonRoleTypes()
        {
            return [];
        }
        public IReadOnlyList<Type> GetSeasonModifierTypes()
        {
            return
            [
                typeof(FatefulModifier),
                typeof(LazyModifier)
            ];
        }
        public IReadOnlyList<Type> GetSeasonGamemodeTypes()
        {
            return [];
        }
    }
}