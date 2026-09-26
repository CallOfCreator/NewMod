using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TMPro;

namespace NewMod.Patches;

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
public static class RoleTabIconPatch
{
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (__instance.name != "RolePanel")
            return;

        var player = PlayerControl.LocalPlayer;
        if (!player || player.Data?.Role == null || !CustomRoleManager.GetCustomRoleBehaviour(player.Data.Role.Role, out var role))
            return;

        var sprite = role.Configuration.Icon?.LoadAsset();
        if (!sprite)
            return;

        var icon = TmpSpriteUtils.CreateSpriteAsset(sprite, $"NewMod.Roles.{player.Data.Role.Role}");
        var tabText = __instance.tab.GetComponentInChildren<TextMeshPro>();
        tabText.spriteAsset = icon;
        tabText.text = $"{role.RoleName}<space=0.5em><sprite name=\"{icon.name}\" tint=0>";
    }
}