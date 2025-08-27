using UnityEngine;
using HarmonyLib;
using NewMod.Roles.NeutralRoles;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using System.Collections.Generic;
using System.Reflection;
using System;

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
                _cachedCursor = NewModAsset.CustomCursor.LoadAsset().texture;
            }
            if (_cachedCursor != null)
            {
                Cursor.SetCursor(_cachedCursor, CursorMode.Auto);
            }
            RightPanel = __instance.transform.Find("MainUI/AspectScaler/RightPanel");

            if ((NewModDateTime.IsNewModBirthdayWeek || NewModDateTime.IsWraithCallerUnlocked) && !_wraithRegistered)
            {
                RegisterWraithCaller();
                _wraithRegistered = true;
            }

            if (NewModDateTime.IsNewModBirthdayWeek)
            {
                RightPanel.gameObject.SetActive(false);
                __instance.screenTint.enabled = false;

                var auLogo = GameObject.Find("LOGO-AU");
                auLogo.transform.localPosition = new Vector3(-3.50f, 1.85f, 0f);
                auLogo.transform.localScale = new Vector3(0.32f, 0.32f, 1f);

                var newmodLogo = new GameObject("NewModLogo");
                var parent = __instance.transform.Find("MainUI/AspectScaler/LeftPanel");
                newmodLogo.transform.SetParent(parent, false);
                newmodLogo.transform.localPosition = new Vector3(-0.1427f, 2.8094f, 0.7182f);
                newmodLogo.transform.localScale = new Vector3(0.3711f, 0.4214f, 1.16f);
                LogoSprite = newmodLogo.AddComponent<SpriteRenderer>();
                LogoSprite.sprite = NewModAsset.ModLogo.LoadAsset();

                var auBG = __instance.transform.Find("MainUI/AspectScaler/BackgroundTexture").GetComponent<SpriteRenderer>();
                if (auBG != null)
                {
                    auBG.sprite = NewModAsset.MainMenuBG.LoadAsset();
                }
            }
            else
            {
                // Preserve the old layout when it's not the birthday update
                var Logo = new GameObject("NewModLogo");
                Logo.transform.SetParent(__instance.transform.Find("MainCanvas/MainPanel/RightPanel"), false);
                Logo.transform.localPosition = new Vector3(2.34f, -0.7136f, 1f);
                LogoSprite = Logo.AddComponent<SpriteRenderer>();
                LogoSprite.sprite = NewModAsset.ModLogo.LoadAsset();
            }
            ModCompatibility.Initialize();
        }
        public static void RegisterWraithCaller()
        {
            var roleType = typeof(WraithCaller);
            var customRoleManager = typeof(CustomRoleManager);
            var registerTypes = customRoleManager.GetMethod("RegisterRoleTypes", BindingFlags.NonPublic | BindingFlags.Static);
            var registerInManager = customRoleManager.GetMethod("RegisterInRoleManager", BindingFlags.NonPublic | BindingFlags.Static);
            var plugin = MiraPluginManager.GetPluginByGuid(NewMod.Id);
            registerTypes.Invoke(null, [new List<Type> { roleType }, plugin]);
            registerInManager.Invoke(null, null);
        }
        [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
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
    }
}
