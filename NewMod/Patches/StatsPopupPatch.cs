using AmongUs.GameOptions;
using UnityEngine;
using HarmonyLib;
using MiraAPI.Roles;
using Il2CppSystem.Text;
using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.Data.Player;
using AmongUs.Data;

namespace NewMod.Patches
{
    public static class CustomStatsManager
    {
        private static readonly string SavePath = Path.Combine(PlatformPaths.persistentDataPath, "customStats.dat");
        public static Dictionary<string, uint> CustomRoleStates = new();

        public static void SaveCustomStats()
        {
            try
            {
                using var writer = new BinaryWriter(File.Open(SavePath, FileMode.Create));
                writer.Write(CustomRoleStates.Count);

                foreach (var entry in CustomRoleStates)
                {
                    writer.Write(entry.Key);
                    writer.Write(entry.Value);
                }

                NewMod.Instance.Log.LogInfo("Custom stats saved successfully.");
            }
            catch (Exception ex)
            {
                NewMod.Instance.Log.LogError($"Failed saving custom stats: {ex}");
            }
        }

        public static void LoadCustomStats()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    NewMod.Instance.Log.LogInfo("No custom stats file found.");
                    return;
                }

                using var reader = new BinaryReader(File.Open(SavePath, FileMode.Open));
                CustomRoleStates.Clear();

                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadUInt32();
                    CustomRoleStates[key] = value;
                }

                NewMod.Instance.Log.LogInfo("Custom stats loaded successfully.");
            }
            catch (Exception ex)
            {
                NewMod.Instance.Log.LogError($"Failed loading custom stats: {ex}");
            }
        }

        public static void IncrementCustomStat(string key)
        {
            if (CustomRoleStates.ContainsKey(key))
            {
                CustomRoleStates[key]++;
            }
            else
            {
                CustomRoleStates[key] = 1;
            }
        }

        public static uint GetCustomStat(string key)
        {
            return CustomRoleStates.ContainsKey(key) ? CustomRoleStates[key] : 0;
        }
    }

    [HarmonyPatch(typeof(PlayerStatsData), nameof(PlayerStatsData.SaveStats))]
    public class SaveStatsPatch
    {
        public static void Postfix()
        {
            CustomStatsManager.SaveCustomStats();
        }
    }

    [HarmonyPatch(typeof(LegacyStatsManager), nameof(LegacyStatsManager.LoadStats))]
    public class LoadStatsPatch
    {
        public static void Postfix()
        {
            CustomStatsManager.LoadCustomStats();
        }
    }

    [HarmonyPatch(typeof(StatsPopup), nameof(StatsPopup.DisplayRoleStats))]
    public class DisplayRoleStatsPatch
    {
        public static bool Prefix(StatsPopup __instance)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var allRoles = RoleManager.Instance.AllRoles;

            foreach (var role in allRoles)
            {
                RoleTypes roleType = role.Role;

                if (roleType == RoleTypes.ImpostorGhost || roleType == RoleTypes.CrewmateGhost)
                {
                    continue;
                }

                uint winCount = DataManager.Player.Stats.GetRoleStat(roleType, RoleStat.Wins);

                string roleName;
                Color roleColor;

                if (role is ICustomRole customRole)
                {
                    roleName = customRole.RoleName;
                    roleColor = customRole.RoleColor;
                }
                else
                {
                    roleName = role.NiceName;
                    roleColor = role.NameColor;
                }

                StatsPopup.AppendStat(stringBuilder, StringNames.StatsRoleWins, winCount, $"<color=#{ColorUtility.ToHtmlStringRGBA(roleColor)}>{roleName}</color>");
            }

            foreach (var entry in StatsPopup.RoleSpecificStatsToShow)
            {
                StatID statID = entry.Key; 
                StringNames stringNames = entry.Value;

                StatsPopup.AppendStat(stringBuilder, stringNames, DataManager.Player.Stats.GetStat(statID));
            }
            __instance.StatsText.text = stringBuilder.ToString();

            return false;
        }
    }
}
