using HarmonyLib;

namespace NewMod.Patches;

[HarmonyPatch(typeof(FriendsListManager), nameof(FriendsListManager.SetFriendButtonColor))]
public static class FriendListPatch
{
    [HarmonyPrefix]
    public static bool Prefix(FriendsListManager __instance)
    {
        // Without this patch, the Friends List button will spam null references
        return __instance.FriendsListButton;
    }
}