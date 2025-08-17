/*
using HarmonyLib;
using UnityEngine;

namespace NewMod.Patches
{
    /// <summary>
    /// Allows players to paste clipboard text into chat using Ctrl+V.
    /// Inspired by:https://github.com/nyomo/TownOfPlus/blob/origin/TownOfPlus/Patches/ChatPlus.cs#L80-L158
    /// </summary>
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static class ClipboardPatch
    {
        public static void Prefix(ChatController __instance)
        {
            if (!HudManager.Instance.Chat.IsOpenOrOpening) return;

            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrlPressed && Input.GetKeyDown(KeyCode.V))
            {
                string clipboard = GUIUtility.systemCopyBuffer;

                if (!string.IsNullOrWhiteSpace(clipboard))
                {
                    clipboard = clipboard.Replace("<", "")
                                         .Replace(">", "")
                                         .Replace("\r", "");

                    if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                        clipboard = clipboard.Replace("\n", "");

                    __instance.freeChatField.textArea.SetText(__instance.freeChatField.textArea.text + clipboard);
                }
            }
        }
    }
}
*/
