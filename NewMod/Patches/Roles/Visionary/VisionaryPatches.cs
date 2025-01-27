using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using NewMod.Options.Roles.VisionaryOptions;
using NewMod.Roles.CrewmateRoles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Patches.Roles.Visionary
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
    public static class VisionaryVentPatch
    {
        public static bool Prefix(PlayerPhysics __instance, int id)
        {
            float chance = 0.3f;
            if (Random.Range(0f, 1f) < chance)
            {
                var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string filePath = System.IO.Path.Combine(
                    VisionaryUtilities.ScreenshotDirectory, 
                    $"screenshot_{timestamp}.png"
                );
                Coroutines.Start(Utils.CaptureScreenshot(filePath));

                if (__instance.myPlayer.AmOwner)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: Visionary might have seen you vent!</color>"));
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
        [HarmonyPrefix]
        public static bool StartPrefix(PlayerPhysics __instance, int id)
        {
            float chance = 0.3f;
            if (Random.Range(0f, 1f) < chance)
            {
                var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string filePath = System.IO.Path.Combine(
                    VisionaryUtilities.ScreenshotDirectory, 
                    $"screenshot_{timestamp}.png"
                );
                Coroutines.Start(Utils.CaptureScreenshot(filePath));

                if (__instance.myPlayer.AmOwner)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: Visionary might have seen you exit vent!</color>"));
                }
            }
            return true;
        }
    }
    public static class VisionaryMurderPatch
    {
        [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcCustomMurder))]
        [HarmonyPrefix]
        public static bool StartPrefix(PlayerControl target, bool didSucceed, bool resetKillTimer, bool createDeadBody, bool teleportMurderer,  bool showKillAnim,  bool playKillSound)
        {
            float chance = 0.5f;
            if (Random.Range(0f, 1f) < chance)
            {
                var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string filePath = System.IO.Path.Combine(
                    VisionaryUtilities.ScreenshotDirectory, 
                    $"screenshot_{timestamp}.png"
                );
                Coroutines.Start(Utils.CaptureScreenshot(filePath));

                if (PlayerControl.LocalPlayer.AmOwner)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: The Visionary may have captured your crime!</color>"));
                }
            }
            return true;
        }
    }
    public static class VisionaryChatPatch
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
        [HarmonyPrefix]
        public static bool StartPrefix(ChatController __instance, bool visible)
        {
            if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole) return true;
            if (MeetingHud.Instance) return true;

            bool allowChat = VisionaryUtilities.CapturedScreenshotPaths.Count > 2;
            if (allowChat)
            {
                __instance.gameObject.SetActive(true);
            }
            else
            {
                __instance.gameObject.SetActive(false);
            }
            return false;
        }
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        [HarmonyPrefix]
        public static bool StartPrefix(ChatController __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole) return true;
            if (PlayerControl.LocalPlayer.Data.Role is not TheVisionary) return true;
            string chatText = __instance.freeChatField.Text;

            if (chatText.ToLower().StartsWith("/") && chatText.Length > 1)
            {
                string commandPart = chatText[1..].Trim();
                if (int.TryParse(commandPart, out var index))
                {
                    int zeroBased = index - 1;

                    if (zeroBased >= 0 && zeroBased < VisionaryUtilities.CapturedScreenshotPaths.Count)
                    {
                        string path = VisionaryUtilities.CapturedScreenshotPaths[zeroBased];
                        Coroutines.Start(VisionaryUtilities.ShowScreenshotByPath(path, OptionGroupSingleton<VisionaryOptions>.Instance.MaxDisplayDuration));
                        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=green>Showing screenshot #{index}!</color>"));
                    }
                    else
                    {
                        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=red>Screenshot #{index} not found.</color>"));
                    }
                }
            }
            return false;
        }
    }
}
