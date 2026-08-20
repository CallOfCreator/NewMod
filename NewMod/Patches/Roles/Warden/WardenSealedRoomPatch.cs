using HarmonyLib;
using NewMod.Roles.CrewmateRoles.S1;

namespace NewMod.Patches;

[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.DoorsUpdate))]
public static class WardenSealedRoomDoorUiPatch
{
    [HarmonyPostfix]
    public static void Postfix(MapRoom __instance)
    {
        if (!__instance.door)
            return;

        var sealedRoom = WardenRole.SealActive && __instance.room == WardenRole.SealedRoom;

        if (sealedRoom)
            __instance.door.material.SetFloat("_Percent", 1f);

        var button = __instance.door.GetComponent<ButtonBehavior>();

        if (button)
            button.enabled = !sealedRoom;
    }
}

[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageDoors))]
public static class WardenSealedRoomDoorClickPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MapRoom __instance)
    {
        return !WardenRole.SealActive || __instance.room != WardenRole.SealedRoom;
    }
}