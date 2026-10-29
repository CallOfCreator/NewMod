// Inspired by: https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs#L12

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
    public static void UpdateActivityPrefix([HarmonyArgument(0)] Activity activity)
    {
        if (Application.platform == RuntimePlatform.Android) return;
        if (activity == null) return;

        var inLobby = activity.State == "In Lobby";
        activity.Details = $"NewMod v{NewMod.ModVersion} (Dev)";
        activity.State = $"Playing Among Us | NewMod v{NewMod.ModVersion}";
        activity.Assets = new ActivityAssets { LargeImage = "nm", SmallText = "Made with MiraAPI" };

        if (inLobby)
        {
            var maxPlayers = GameOptionsManager.Instance?.currentNormalGameOptions?.MaxPlayers ?? 10;
            var lobbyCode = GameStartManager.InstanceExists ? GameStartManager.Instance.GameRoomNameCode?.text : null;

            activity.Details += $" | Lobby: {lobbyCode} | Max: {maxPlayers} | MiraAPI: {MiraApiPlugin.Version} | {Application.platform}";
        }

        if (MeetingHud.Instance) activity.Details += " | In Meeting";
    }
}
