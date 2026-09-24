using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Options;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class PlayerRoleNamePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (!ShipStatus.Instance || __instance.Data?.Role == null)
            return;

        var text = FormatName(__instance.CurrentOutfit.PlayerName, __instance.Data);
        if (__instance.cosmetics.nameText.text != text)
            __instance.cosmetics.nameText.text = text;
    }

    public static string FormatName(string name, NetworkedPlayerInfo player)
    {
        var viewer = PlayerControl.LocalPlayer;
        if (!viewer || player.Role == null)
            return name;

        var canSeeRole = viewer.PlayerId == player.PlayerId ||
                         (viewer.Data?.IsDead == true && OptionGroupSingleton<GeneralOption>.Instance.ShouldDeadPlayersSeeRoles);
        if (!canSeeRole)
            return name;

        var roleName = player.Role.NiceName;
        var color = player.Role.TeamColor;
        if (CustomRoleManager.GetCustomRoleBehaviour(player.Role.Role, out var role))
        {
            roleName = role.RoleName;
            color = role.RoleColor;
        }

        return $"{name} <color=#{ColorUtility.ToHtmlStringRGB(color)}>({roleName})</color>";
    }
}
