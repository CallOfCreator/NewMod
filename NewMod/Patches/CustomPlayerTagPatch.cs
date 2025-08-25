using HarmonyLib;
using UnityEngine;

namespace NewMod.Patches
{
    [HarmonyPatch]
    public static class CustomPlayerTagPatch
    {
        public const float Padding = 0.02f;
        private static float targetY;

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        [HarmonyPostfix]
        public static void SetNamePostfix(ChatBubble __instance, string playerName, bool isDead, bool voted, Color color)
        {
            __instance.NameText.ForceMeshUpdate();

            float nameBottom = __instance.NameText.textBounds.min.y;
            float nameLocalY = __instance.NameText.transform.localPosition.y;

            targetY = nameLocalY + nameBottom - Padding;
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
        [HarmonyPostfix]
        public static void SetTextPostfix(ChatBubble __instance, string chatText)
        {
            var pos = __instance.TextArea.transform.localPosition;
            pos.y = targetY;

            __instance.TextArea.transform.localPosition = pos;
            __instance.AlignChildren();
        }
        [HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.AddDisconnectMessage))]
        [HarmonyPrefix]
        public static void StartPrefix(NotificationPopper __instance, ref string item)
        {
            item = item.Replace("\r", "").Replace("\n", "");
            while (item.Contains("  ")) item = item.Replace("  ", " ");

            item = item.Replace("<size=1.7>", "<size=1.2>");
            item = item.Replace("</size>", "</size>");

            _ = item.TrimEnd();
        }
        [HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.SetUp))]
        [HarmonyPrefix]
        public static void StartPrefix(ChatNotification __instance, PlayerControl sender, ref string text)
        {
            if (text.Contains('\n'))
                text = text.Replace("\r", "").Replace("\n", " ");

            text = text.Replace("<size=1.7>", "<size=1.2>");

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            text = text.TrimEnd();
        }
    }
}
