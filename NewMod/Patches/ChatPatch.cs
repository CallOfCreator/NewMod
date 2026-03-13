using AmongUs.Data;
using HarmonyLib;
using InnerNet;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        public static bool Prefix(ChatController __instance)
        {
            __instance.timeSinceLastMessage = 0f;

            if (__instance.quickChatMenu.CanSend)
            {
                __instance.SendQuickChat();
            }
            else
            {
                if (__instance.quickChatMenu.IsOpen || string.IsNullOrWhiteSpace(__instance.freeChatField.Text) || DataManager.Settings.Multiplayer.ChatMode != QuickChatModes.FreeChatOrQuickChat)
                {
                    return false;
                }
                __instance.SendFreeChat();
            }

            __instance.timeSinceLastMessage = 0f;
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();

            return false;
        }
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Start))]
        [HarmonyPostfix]
        public static void StartPostfix(TextBoxTMP __instance)
        {
            __instance.AllowSymbols = true;
            __instance.allowAllCharacters = true;
        }
    }
}