// Inspired by: https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs#L12
using System;
using Discord;
using HarmonyLib;
using MiraAPI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewMod
{
    [HarmonyPatch]
    public static class NewModDiscordPatch
    {
        private static Discord.Discord discord;
        public static ActivityManager activityManager;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.Start))]
        public static bool StartPrefix(DiscordManager __instance)
        {
            if (Application.platform == RuntimePlatform.Android) return true;

            InitializeDiscord(__instance);
            return false;
        }
        
        private static void InitializeDiscord(DiscordManager __instance)
        {
            const long clientId = 1405946628115791933;

            discord = new Discord.Discord(clientId, (ulong)CreateFlags.Default);
            activityManager = discord.GetActivityManager();

            activityManager.RegisterSteam(945360U);
            activityManager.add_OnActivityJoin((Action<string>)__instance.HandleJoinRequest);

            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            {
                __instance.OnSceneChange(scene.name);
            }));
            __instance.presence = discord;
            __instance.SetInMenus();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
        public static void UpdateActivityPrefix([HarmonyArgument(0)] ref Activity activity)
        {
            if (Application.platform == RuntimePlatform.Android) return;
            if (activity == null) return;

            var isBeta = false;
            string details = $"NewMod v{NewMod.ModVersion}" + (isBeta ? " (Beta)" : " (Dev)");

            activity.Details = details;
            activity.State = $"Playing Among Us | NewMod v{NewMod.ModVersion}";
            activity.Assets = new ActivityAssets()
            {
                LargeImage = "nm",
                SmallText = "Made with MiraAPI"
            };

            try
            {
                if (activity.State.Contains("Menus"))
                {
                    int maxPlayers = GameOptionsManager.Instance?.currentNormalGameOptions?.MaxPlayers ?? 10;
                    var lobbyCode = GameStartManager.Instance?.GameRoomNameCode?.text;
                    var miraVersion = MiraApiPlugin.Version;
                    var platform = Application.platform;

                    activity.Details += $" | Lobby: {lobbyCode} | Max: {maxPlayers} | MiraAPI: {miraVersion} | {platform}";
                }

                if (MeetingHud.Instance)
                {
                    activity.Details += " | In Meeting";
                }
            }
            catch (Exception e)
            {
                NewMod.Instance.Log.LogError($"Discord RPC activity update failed: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
