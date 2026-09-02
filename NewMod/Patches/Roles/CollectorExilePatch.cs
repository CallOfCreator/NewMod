using HarmonyLib;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Patches;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
public static class CollectorExilePatch
{
    [HarmonyPrefix]
    public static void Prefix(NetworkedPlayerInfo player, bool voteTie)
    {
        if (!AmongUsClient.Instance.AmHost || player == null || voteTie)
            return;

        if (Collector.Inventories.Count > 0)
        {
            Collector.ExilePending = true;
            Collector.ExilePosition = player.Object.GetTruePosition();
        }

        if (Usurper.States.Count > 0)
        {
            Usurper.ExiledPlayerId = player.PlayerId;
            Usurper.ExilePosition = player.Object.GetTruePosition();
        }
    }
}
