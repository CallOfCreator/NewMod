using HarmonyLib;
using Discord;
using MiraAPI;
using UnityEngine;

namespace NewMod
{
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public static class DiscordPlayStatusPatch
    {
        public static void Prefix([HarmonyArgument(0)] Activity activity)
        {
            if (activity == null) return;
            
            var isBeta = true;

            string details = $"New Mod v{NewMod.ModVersion}" + (isBeta ? " (Beta)" : "(dev)");

            activity.Details = details;

            try 
            {
                if (activity.State == "In Menus")
                {
                    int maxPlayers = GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers;
                    var lobbyCode = GameStartManager.Instance.GameRoomNameCode.text;
                    var miraAPIVersion = MiraApiPlugin.Version;
                    var platform = Application.platform;

                   details += $" Players: {maxPlayers} | Lobby Code: {lobbyCode} | MiraAPI Version {miraAPIVersion} | Platform: {platform}";
                }

                else if (activity.State == "In Game")
                {
                    if (MeetingHud.Instance)
                    {
                        details +=  " | \nIn Meeting";
                    }
                }
                
                activity.Assets.SmallText = "NewMod Made With MiraAPI";
            }
            catch (System.Exception e)
            {
                NewMod.Instance.Log.LogError($"Error updating Discord activity: {e.Message}\nStackTrace: {e.StackTrace}");
            }
        }
    }
}
