using HarmonyLib;
using MiraAPI.GameOptions;
using NewMod.Options;

namespace NewMod.Patches.Compatibility
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class StartGamePatch
    {
        public static bool Prefix(AmongUsClient __instance)
        {
            if (!ModCompatibility.IsLaunchpadLoaded()) return true;
            
            var settings = OptionGroupSingleton<CompatibilityOptions>.Instance;

            if (!settings.AllowRevenantHitmanCombo)
            {
                var hitman = ModCompatibility.IsRoleActive("Hitman");
                var revenant = ModCompatibility.IsRoleActive("Revenant");

                if (hitman && revenant)
                {
                    if (settings.Compatibility == CompatibilityOptions.ModPriority.PreferNewMod)
                    {
                        ModCompatibility.DisableRole("Hitman", ModCompatibility.LaunchpadReloaded_GUID);
                    }
                    else
                    {
                        ModCompatibility.DisableRole("Revenant", NewMod.Id);
                    }
                }
            }
            if (settings.Compatibility == CompatibilityOptions.ModPriority.PreferNewMod)
            {
                ModCompatibility.DisableRole("Medic", ModCompatibility.LaunchpadReloaded_GUID);
            }
            else
            {
                ModCompatibility.DisableRole("Necromancer", NewMod.Id);
            }
            return true;
        }
    }
}