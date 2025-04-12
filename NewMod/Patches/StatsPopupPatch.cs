using AmongUs.GameOptions;
using UnityEngine;
using HarmonyLib;
using MiraAPI.Roles;
using Il2CppSystem.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NewMod.Patches
{
    public static class CustomStatsManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "customStats.dat");
        public static Dictionary<string, int> CustomRoleWins = new();
        public static void SaveCustomStats()
        {
            try
            {
                using var writer = new BinaryWriter(File.Open(SavePath, FileMode.Create));

                var allRoles = RoleManager.Instance.AllRoles;
                writer.Write(allRoles.Count);

                foreach (var role in allRoles)
                {
                    RoleTypes roleType = role.Role;
                    int winCount = 0;

                    if (role is ICustomRole customRole)
                    {
                        string roleName = customRole.RoleName;
                        winCount = CustomRoleWins.ContainsKey(roleName) ? CustomRoleWins[roleName] : 0;
                        writer.Write(roleName);
                    }
                    else
                    {
                        winCount = (int)StatsManager.Instance.GetRoleWinCount(roleType);
                        writer.Write(roleType.ToString());
                    }
                    writer.Write(winCount);
                }
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
                    return;
                }

                using var reader = new BinaryReader(File.Open(SavePath, FileMode.Open));

                int roleCount = reader.ReadInt32();
                for (int i = 0; i < roleCount; i++)
                {
                    string roleIdentifier = reader.ReadString();
                    int winCount = reader.ReadInt32();

                    if (Enum.TryParse(roleIdentifier, out RoleTypes roleType))
                    {
                        SetVanillaRoleWinCount(roleType, winCount);
                    }
                    else
                    {
                        CustomRoleWins[roleIdentifier] = winCount;
                    }
                }
            }
            catch (Exception ex)
            {
                NewMod.Instance.Log.LogError(ex.ToString());
            }
        }
        private static void SetVanillaRoleWinCount(RoleTypes role, int winCount)
        {
            try
            {
                FieldInfo statsField = typeof(StatsManager).GetField("stats", BindingFlags.NonPublic | BindingFlags.Instance);
                var statsInstance = statsField.GetValue(StatsManager.Instance) as StatsManager.Stats;
            
                FieldInfo roleWinsField = typeof(StatsManager.Stats).GetField("roleWins", BindingFlags.NonPublic | BindingFlags.Instance);
                var roleWinsDict = roleWinsField.GetValue(statsInstance) as Dictionary<RoleTypes, uint>;
                roleWinsDict[role] = (uint)winCount;
            }
            catch (Exception ex)
            {
                NewMod.Instance.Log.LogError(ex.ToString());
            }
        }

        public static void IncrementRoleWin(ICustomRole customRole)
        {
            string roleName = customRole.RoleName;
            if (CustomRoleWins.ContainsKey(roleName))
            {
                CustomRoleWins[roleName]++;
            }
            else
            {
                CustomRoleWins[roleName] = 1;
            }
        }
        public static int GetRoleWins(ICustomRole customRole)
        {
            return CustomRoleWins.ContainsKey(customRole.RoleName) ? CustomRoleWins[customRole.RoleName] : 0;
        }
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.SaveStats))]
    public class SaveStatsPatch
    {
        public static void Postfix()
        {
            CustomStatsManager.SaveCustomStats();
        }
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.LoadStats))]
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
                string roleName;
                Color roleColor;
                int winCount;

                if (role is ICustomRole customRole)
                {
                    roleName = customRole.RoleName;
                    roleColor = customRole.RoleColor;
                    winCount = CustomStatsManager.GetRoleWins(customRole);
                }
                else
                {
                    roleName = role.NiceName;
                    roleColor = role.NameColor;
                    winCount = (int)StatsManager.Instance.GetRoleWinCount(roleType);
                }

                StatsPopup.AppendStat(stringBuilder, StringNames.StatsRoleWins, winCount, $"<color=#{ColorUtility.ToHtmlStringRGBA(roleColor)}>{roleName}</color>");
            }

            foreach (StringNames stringName in StatsPopup.RoleSpecificStatsToShow)
            {
                StatsPopup.AppendStat(stringBuilder, stringName, StatsManager.Instance.GetStat(stringName));
            }
            __instance.StatsText.text = stringBuilder.ToString();

            return false;
        }
    }
}
