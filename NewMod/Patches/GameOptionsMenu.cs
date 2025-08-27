using HarmonyLib;
using MiraAPI.GameOptions;
using NewMod.Options;

namespace NewMod.Patches;

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CloseMenu))]
public static class GameOptionsMenu_CloseMenu_Patch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        var opts = OptionGroupSingleton<CompatibilityOptions>.Instance;

        if (opts.AllowRevenantHitmanCombo.Value)
        {
           HudManager.Instance.ShowPopUp(
                "You enabled the Revenant & Hitman combo. This may break game balance!"
            );
        }
        else
        {
             HudManager.Instance.ShowPopUp(
                "Revenant & Hitman combo disabled. Only one will be allowed per match."
            );
        }
    }
}
