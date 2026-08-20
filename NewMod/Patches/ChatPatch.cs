using HarmonyLib;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingChatScreenPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        var chatScreen = HudManager.Instance.Chat.chatScreen;
        chatScreen.transform.localPosition = new Vector3(3.4833f, 2.5f, chatScreen.transform.localPosition.z);
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.LateUpdate))]
public static class MeetingChatLateUpdatePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !MeetingHud.Instance;
    }
}