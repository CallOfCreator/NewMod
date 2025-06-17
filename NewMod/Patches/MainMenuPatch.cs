using UnityEngine;
using HarmonyLib;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPriority(Priority.VeryHigh)]
    public static class MainMenuPatch
    {
        public static SpriteRenderer LogoSprite;

        [HarmonyPostfix]
        public static void StartPostfix(MainMenuManager __instance)
        {
            var newparent = __instance.transform.FindChild("MainCanvas/MainPanel/RightPanel");
            var Logo = new GameObject("NewModLogo");
            Logo.transform.SetParent(newparent, false);
            Logo.transform.localPosition = new(2.34f, -0.7136f, 1f);
            LogoSprite = Logo.AddComponent<SpriteRenderer>();
            LogoSprite.sprite = NewModAsset.ModLogo.LoadAsset();
            
            ModCompatibility.Initialize();
        }
    }
}
