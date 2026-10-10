// Inspired by: https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs#L12

using System;
using Discord;
using HarmonyLib;
using MiraAPI;
using UnityEngine;

namespace NewMod;

[HarmonyPatch]
public static class NewModDiscordPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public static void UpdateActivityPrefix([HarmonyArgument(0)] ref Activity activity)
    {
        if (Application.platform == RuntimePlatform.Android) return;
        if (activity == null) return;

        var inLobby = activity.State == "In Lobby";
        activity.Details = $"NewMod v{NewMod.ModVersion} (Dev)";
        activity.State = $"Playing Among Us | NewMod v{NewMod.ModVersion}";
        activity.Assets = new ActivityAssets { LargeImage = "nm", SmallText = "Made with MiraAPI" };

        try
        {
            if (inLobby)
            {
                var maxPlayers = GameOptionsManager.Instance?.currentNormalGameOptions?.MaxPlayers ?? 10;
                var lobbyCode = GameStartManager.Instance?.GameRoomNameCode?.text;
                var miraVersion = MiraApiPlugin.Version;
                var platform = Application.platform;

                activity.Details += $" | Lobby: {lobbyCode} | Max: {maxPlayers} | MiraAPI: {miraVersion} | {platform}";
            }

            if (MeetingHud.Instance) activity.Details += " | In Meeting";
        }
        catch (Exception e)
        {
            Error($"Discord RPC activity update failed: {e.Message}\n{e.StackTrace}");
        }
    }
}
