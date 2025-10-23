using UnityEngine;
using HarmonyLib;
using Reactor.Utilities;
using System.Collections;
using NewMod.LocalSettings;
using MiraAPI.LocalSettings;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(MainMenuManager))]
    [HarmonyPriority(Priority.VeryHigh)]

    public static class MainMenuPatch
    {
        public static SpriteRenderer LogoSprite;
        public static Texture2D _cachedCursor;
        public static Transform RightPanel;
        public static bool _wraithRegistered = false;

        [HarmonyPatch(nameof(MainMenuManager.Start))]
        [HarmonyPostfix]
        public static void StartPostfix(MainMenuManager __instance)
        {
            if (_cachedCursor == null)
            {
                var cur = NewModAsset.CustomCursor.LoadAsset();
                _cachedCursor = cur != null ? cur.texture : null;
            }
            if (_cachedCursor != null && LocalSettingsTabSingleton<NewModLocalSettings>.Instance.EnableCustomCursor.Value)
            {
                Cursor.SetCursor(_cachedCursor, Vector2.zero, CursorMode.Auto);
            }

            RightPanel = __instance.transform.Find("MainUI/AspectScaler/RightPanel");

            if (NewModDateTime.IsWraithCallerUnlocked && !_wraithRegistered)
            {
                _wraithRegistered = true;
            }

            if (NewModDateTime.IsNewModBirthdayWeek)
            {
                Coroutines.Start(ApplyBirthdayUI(__instance));
            }
            else
            {
                var Logo = new GameObject("NewModLogo");
                Logo.transform.SetParent(__instance.transform.Find("MainCanvas/MainPanel/RightPanel"), false);
                Logo.transform.localPosition = new Vector3(2.34f, -0.7136f, 1f);
                LogoSprite = Logo.AddComponent<SpriteRenderer>();
                LogoSprite.sprite = NewModAsset.ModLogo.LoadAsset();
            }
            ModCompatibility.Initialize();
        }

        private static IEnumerator ApplyBirthdayUI(MainMenuManager __instance)
        {
            yield return null;

            if (RightPanel != null) RightPanel.gameObject.SetActive(false);
            if (__instance.screenTint != null) __instance.screenTint.enabled = false;

            var auLogo = GameObject.Find("LOGO-AU");
            if (auLogo != null)
            {
                auLogo.transform.localPosition = new Vector3(-3.50f, 1.85f, 0f);
                auLogo.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            }

            var parent = __instance.transform.Find("MainUI/AspectScaler/LeftPanel");
            if (parent != null)
            {
                var newmodLogo = new GameObject("NewModLogo");
                newmodLogo.transform.SetParent(parent, false);
                newmodLogo.transform.localPosition = new Vector3(-0.1427f, 2.8094f, 0.7182f);
                newmodLogo.transform.localScale = new Vector3(0.3711f, 0.4214f, 1.16f);
                LogoSprite = newmodLogo.AddComponent<SpriteRenderer>();
                var modLogo = NewModAsset.ModLogo.LoadAsset();
                if (modLogo != null) LogoSprite.sprite = modLogo;
            }

            var bgTr = __instance.transform.Find("MainUI/AspectScaler/BackgroundTexture");
            if (bgTr != null)
            {
                var auBG = bgTr.GetComponent<SpriteRenderer>();
                var bg = NewModAsset.MainMenuBG.LoadAsset();
                if (auBG != null && bg != null) auBG.sprite = bg;
            }
        }
        /*[HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
        [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
        [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
        [HarmonyPatch(nameof(MainMenuManager.OpenCreateGame))]
        [HarmonyPatch(nameof(MainMenuManager.OpenEnterCodeMenu))]
        [HarmonyPatch(nameof(MainMenuManager.OpenOnlineMenu))]
        [HarmonyPatch(nameof(MainMenuManager.OpenFindGame))]
        public static void Postfix(MainMenuManager __instance)
        {
            if (!NewModDateTime.IsNewModBirthdayWeek) return;
            RightPanel.gameObject.SetActive(true);
        }

        [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
        [HarmonyPostfix]
        public static void ResetScreenPostfix(MainMenuManager __instance)
        {
            if (!NewModDateTime.IsNewModBirthdayWeek) return;
            RightPanel.gameObject.SetActive(false);
        }
    }*/
    }
}