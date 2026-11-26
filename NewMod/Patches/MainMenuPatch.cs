using HarmonyLib;
using MiraAPI.LocalSettings;
using NewMod.Components;
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

    public static Texture2D _cachedCursor;
    private static bool _contentInjected;

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
        if (LocalSettingsTabSingleton<NewModLocalSettings>.Instance.EnableCustomCursor.Value)
        {
            var cursor = GetCursorTexture();
            if (cursor)
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        }

        var layout = __instance.gameObject.AddComponent<MainMenuLayout>();
        layout.Initialize(__instance);
        RightPanel = layout.RightPanel;
        LogoSprite = layout.Logo;

        SeasonManager.InitializeSeasons(__instance);
        ModCompatibility.Initialize();
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPostfix]
    public static void OpenGameModeMenuPostfix(MainMenuManager __instance)
    {
        var selection = __instance.howToPlayButton;
        if (__instance.playLocalButton.gameObject.activeSelf)
            selection = __instance.playLocalButton;
        else if (__instance.PlayOnlineButton.enabled)
            selection = __instance.PlayOnlineButton;
        __instance.GetComponent<MainMenuLayout>().OpenSubmenu(selection);
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPostfix]
    public static void OpenAccountMenuPostfix(MainMenuManager __instance)
    {
        __instance.GetComponent<MainMenuLayout>().OpenSubmenu(__instance.accountCTAButton);
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPostfix]
    public static void OpenCreditsPostfix(MainMenuManager __instance)
    {
        var layout = __instance.GetComponent<MainMenuLayout>();
        layout.OpenSubmenu(layout.BackButton);
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenEnterCodeMenu))]
    [HarmonyPostfix]
    public static void OpenEnterCodeMenuPostfix(MainMenuManager __instance)
    {
        if (__instance.enterCodeButtons.activeSelf)
            ControllerManager.Instance.SetBackButton(__instance.enterCodeButtons.transform.Find("Header/BackButton").GetComponent<PassiveButton>());
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenRedeemCodeMenu))]
    [HarmonyPostfix]
    public static void OpenRedeemCodeMenuPostfix(MainMenuManager __instance)
    {
        ControllerManager.Instance.SetBackButton(__instance.redeemCodeContainer.Find("Header/BackButton").GetComponent<PassiveButton>());
    }

    public static Texture2D GetCursorTexture()
    {
        if (_cachedCursor)
            return _cachedCursor;

        var texture = NewModAsset.CustomCursor.LoadAsset().texture;
        var target = RenderTexture.GetTemporary(texture.width, texture.height, 0);
        var previous = RenderTexture.active;
        Graphics.Blit(texture, target);
        RenderTexture.active = target;
        _cachedCursor = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        _cachedCursor.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        _cachedCursor.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);
        return _cachedCursor;
    }
}
