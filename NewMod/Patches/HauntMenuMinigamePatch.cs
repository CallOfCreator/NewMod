using HarmonyLib;
using MiraAPI.Roles;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
    public static class HauntMenuMinigamePatch
    {
        public static bool Prefix(HauntMenuMinigame __instance)
        {
            var targetData = __instance.HauntTarget.Data;
            
            if (targetData.Role is ICustomRole customRole)
            {
                __instance.FilterText.text = customRole.RoleName;
                return false;
            }
            else
            {
                __instance.FilterText.text = targetData.Role.NiceName;
            }
            return false;
        }
    }
}
