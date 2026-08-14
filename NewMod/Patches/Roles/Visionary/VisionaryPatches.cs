using System;
using System.IO;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Utilities;

namespace NewMod.Patches.Roles.Visionary;

public static class VisionaryVentPatch
{
    [RegisterEvent]
    public static void OnEnterVent(EnterVentEvent evt)
    {
        if (!Utils.IsRoleActive("Prankster")) return;

        var player = evt.Player;
        var chancePercentage = (int)(0.2f * 100);

        if (Helpers.CheckChance(chancePercentage))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            var filePath = Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");
            Coroutines.Start(Utils.CaptureScreenshot(filePath));

            if (player.AmOwner) Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: Visionary might have seen you vent!</color>"));
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    public static void Postfix(PlayerPhysics __instance, int ventId)
    {
        if (!Utils.IsRoleActive("Prankster")) return;

        var chancePercentage = (int)(0.2f * 100);
        if (Helpers.CheckChance(chancePercentage))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            var filePath = Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");
            Coroutines.Start(Utils.CaptureScreenshot(filePath));

            if (__instance.myPlayer.AmOwner) Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: Visionary might have seen you exit vent!</color>"));
        }
    }
}

public static class VisionaryMurderPatch
{
    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (!Utils.IsRoleActive("Prankster")) return;

        var source = evt.Source;
        var chancePercentage = (int)(0.2f * 100);

        if (Helpers.CheckChance(chancePercentage))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            var filePath = Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");
            Coroutines.Start(Utils.CaptureScreenshot(filePath));

            if (source.AmOwner) Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Warning: The Visionary may have captured your crime!</color>"));
        }
    }
}