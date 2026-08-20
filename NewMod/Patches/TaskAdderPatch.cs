using HarmonyLib;
using UnityEngine;

namespace NewMod.Patches;

public static class FreeplayLaptopPatches
{
    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void SystemConsoleCanUsePostfix(SystemConsole __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay || __instance.MinigamePrefab is not TaskAdderGame)
        {
            return;
        }

        var player = pc.Object;
        var position = player.GetTruePosition();

        couldUse = player.CanMove;
        canUse = couldUse && (!__instance.onlyFromBelow || position.y < __instance.transform.position.y);

        __result = float.MaxValue;

        if (!canUse)
        {
            return;
        }

        __result = Vector2.Distance(position, __instance.transform.position);

        canUse = __result <= __instance.UsableDistance;
    }

    [HarmonyPatch(typeof(TaskAddButton), nameof(TaskAddButton.AddTask))]
    [HarmonyPrefix]
    public static bool TaskAddButtonPrefix(TaskAddButton __instance)
    {
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay || !__instance.Role)
        {
            return true;
        }

        var player = PlayerControl.LocalPlayer;

        if (player.Data.Role.Role == __instance.Role.Role)
        {
            return false;
        }

        if (PlayerTask.DestroyTasksOfType<ImportantTextTask>(player) && !PlayerTask.PlayerHasTaskOfType<NormalPlayerTask>(player))
        {
            ShipStatus.Instance.Begin();
        }

        player.RpcSetRole(__instance.Role.Role, true);
        player.transform.position = __instance.SafePositionWorld;

        if (player.Data.Role is ImpostorGhostRole impostorGhostRole)
        {
            impostorGhostRole.WasManuallyPicked = true;
        }

        return false;
    }
}