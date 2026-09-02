using HarmonyLib;
using Hazel;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Patches;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class DoubleAgentSabotagePatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl player, MessageReader msgReader)
    {
        if (!AmongUsClient.Instance.AmHost || player.Data.Role is not DoubleAgent || DoubleAgent.CounterfeitActive)
            return true;

        DoubleAgent.BeginCounterfeit(player, msgReader.Buffer[msgReader.readHead]);
        return false;
    }
}
