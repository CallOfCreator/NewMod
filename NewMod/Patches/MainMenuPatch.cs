using System.Linq;
using AchievementsAPI.API;
using HarmonyLib;
using MiraAPI.LocalSettings;
using NewMod.Achievements;
using NewMod.LocalSettings;
using NewMod.Seasons;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(MainMenuManager))]
[HarmonyPriority(Priority.VeryHigh)]
public static class MainMenuPatch
{
    public static SpriteRenderer LogoSprite = null!;
    public static Transform RightPanel = null!;

    public static Texture2D? _cachedCursor;
    private static bool _contentInjected;

    [HarmonyPatch(nameof(MainMenuManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix(MainMenuManager __instance)
    {
        // Inspired by: https://github.com/Reach-For-Stars-Team/Stargazer/blob/master/Stargazer/Features/MainMenu/ReworkedMainMenu.cs#L7
        var background = __instance.transform.Find("MainUI/AspectScaler/BackgroundTexture").GetComponent<SpriteRenderer>();
        background.color = new Color32(69, 38, 105, 255);

        foreach (var btn in __instance.GetComponentsInChildren<PassiveButton>())
        {
            btn.inactiveSprites.GetComponent<SpriteRenderer>().material.color = new Color(1.15f, 0.1f, 0.75f, 1f);
            btn.activeSprites.GetComponent<SpriteRenderer>().material.color = new Color(1.5f, 0.25f, 1.05f, 1f);
        }

        __instance.mainButtons[0].transform.parent.parent.GetComponent<SpriteRenderer>().material.color = new Color(1.1f, 0.08f, 0.75f, 1f);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void StartPrefix()
    {
        if (_contentInjected)
            return;

        _contentInjected = true;
        SeasonManager.InjectSeasonContent();
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(MainMenuManager __instance)
    {
        if (!AchievementsManager.Tabs.Any(tab => tab is PreseasonAchievementsTab))
            AchievementsManager.Tabs.Add(new PreseasonAchievementsTab());

        if (!_cachedCursor)
        {
            var cursorSprite = NewModAsset.CustomCursor.LoadAsset();
            _cachedCursor = cursorSprite ? cursorSprite.texture : null;
        }

        if (_cachedCursor && LocalSettingsTabSingleton<NewModLocalSettings>.Instance.EnableCustomCursor.Value) Cursor.SetCursor(_cachedCursor, Vector2.zero, CursorMode.Auto);

        RightPanel = __instance.transform.Find("MainUI/AspectScaler/RightPanel");

        var logoObject = new GameObject("NewModLogo");
        logoObject.transform.SetParent(__instance.transform.Find("MainCanvas/MainPanel/RightPanel"), false);

        logoObject.transform.localPosition = new Vector3(2.14f, -0.7136f, 1f);
        logoObject.transform.localScale = new Vector3(0.7f, 0.9f, 0.9f);

        LogoSprite = logoObject.AddComponent<SpriteRenderer>();
        LogoSprite.sprite = SeasonManager.CurrentActiveSeasons.Any(season => season is Preseason) ? NewModAsset.NewModLogo.LoadAsset() : NewModAsset.NormalLogo.LoadAsset();

        SeasonManager.InitializeSeasons(__instance);
        ModCompatibility.Initialize();
    }
}