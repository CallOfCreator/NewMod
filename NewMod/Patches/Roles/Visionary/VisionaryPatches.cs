using HarmonyLib;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Patches.Roles.Visionary
{
    public static class VisionaryVentPatch
    {
        public static void OnEnterVent(EnterVentEvent evt)
        {
            PlayerControl player = evt.Player;
            float chance = 0.3f;
            if (Random.Range(0f, 1f) < chance)
            {
                string timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string filePath = System.IO.Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");
                Coroutines.Start(Utils.CaptureScreenshot(filePath));
                if (player.AmOwner)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: Visionary might have seen you vent!</color>"));
                }
            }
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
        public static void Postfix(PlayerPhysics __instance, int id)
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
        }
    }
    public static class VisionaryMurderPatch
    {
        public static void OnBeforeMurder(BeforeMurderEvent evt)
        {
            PlayerControl source = evt.Source;
            float chance = 0.5f;

            if (Random.Range(0f, 1f) < chance)
            {
                var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string filePath = System.IO.Path.Combine(
                    VisionaryUtilities.ScreenshotDirectory,
                    $"screenshot_{timestamp}.png"
                );
                Coroutines.Start(Utils.CaptureScreenshot(filePath));

                if (source.AmOwner)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: The Visionary may have captured your crime!</color>"));
                }
            }
        }
    }
}
