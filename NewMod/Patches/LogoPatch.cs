using UnityEngine;
using HarmonyLib;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPriority(Priority.High)]
    public static class NewModLogoPatch
    {
        public static SpriteRenderer LogoSprite;

        [HarmonyPostfix]
        public static void StartPostfix(MainMenuManager __instance)
        {
           var newparent = __instance.gameModeButtons.transform.parent;
           var Logo = new GameObject("NewmodLogo");
           Logo.transform.parent = newparent;
           Logo.transform.localPosition = new(0f, -0.07f, 1f);
           LogoSprite = Logo.AddComponent<SpriteRenderer>();
           LogoSprite.sprite = NewModAsset.ModLogo.LoadAsset();
        }
    }
}
