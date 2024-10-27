using AmongUs.GameOptions;
using UnityEngine;
using HarmonyLib;
using MiraAPI.Roles;
using Il2CppSystem.Text;

namespace NewMod.Patches
{
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

                var winCount = StatsManager.Instance.GetRoleWinCount(roleType);

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

            foreach (StringNames stringName in StatsPopup.RoleSpecificStatsToShow)
            {
                StatsPopup.AppendStat(stringBuilder, stringName, StatsManager.Instance.GetStat(stringName));
            }

            __instance.StatsText.text = stringBuilder.ToString();

            return false;
        }
    }
}
