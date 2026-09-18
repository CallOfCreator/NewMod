using HarmonyLib;
using Hazel;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using UnityEngine;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Patches;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class DoubleAgentSabotagePatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl player, MessageReader msgReader)
    {
        if (AmongUsClient.Instance.AmHost && DoubleAgent.CounterfeitActive)
            return false;
        if (!AmongUsClient.Instance.AmHost || player.Data.Role is not DoubleAgent)
            return true;

        DoubleAgent.BeginCounterfeit(player, msgReader.Buffer[msgReader.readHead]);
        return false;
    }
}

[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageDoors))]
public static class DoubleAgentDoorPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return PlayerControl.LocalPlayer.Data.Role is not DoubleAgent;
    }
}

[HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.FixedUpdate))]
public static class DoubleAgentMapCooldownPatch
{
    [HarmonyPostfix]
    public static void Postfix(InfectedOverlay __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not DoubleAgent)
            return;
        foreach (var room in __instance.rooms)
            room.SetSpecialActive(room.room == SystemTypes.Comms && !DoubleAgent.CounterfeitActive ? Mathf.Clamp01((DoubleAgent.CooldownUntil - Time.time) / OptionGroupSingleton<DoubleAgentOptions>.Instance.CounterfeitCooldown) : 1f);
    }
}

[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
public static class DoubleAgentCommsPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not DoubleAgent)
            return true;
        if (!DoubleAgent.CounterfeitActive && Time.time >= DoubleAgent.CooldownUntil)
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)SystemTypes.Comms);
        return false;
    }
}