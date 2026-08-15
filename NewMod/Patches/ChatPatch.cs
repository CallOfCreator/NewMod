using HarmonyLib;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.LateUpdate))]
public static class ChatScreenRootPatch
{
    [HarmonyPostfix]
    public static void Postfix(GameObject ___chatScreen, PassiveButton ___chatButton)
    {
        var position = ___chatScreen.transform.localPosition;
        position.y = ___chatButton.transform.localPosition.y;
        ___chatScreen.transform.localPosition = position;
    }
}