using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Options;
using NewMod.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
    public static class PlayerVoteArea_SetCosmetics_Patch
    {
        public static Dictionary<byte, string> _alias;
        public static HashSet<string> _used;
        public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
        {
            var opts = OptionGroupSingleton<GeneralOption>.Instance;
            var lp = PlayerControl.LocalPlayer;
            bool revealRolesForDead = opts.ShouldDeadPlayersSeeRoles && lp?.Data?.IsDead == true;
            bool anonNames = opts.EnableAnonymousNamesInMeetings;
            bool anonIcons = anonNames;

            _alias ??= [];
            _used ??= [];

            byte playerId = playerInfo.PlayerId;
            string baseName = playerInfo.PlayerName;

            if (anonNames && !(playerInfo.Object?.notRealPlayer ?? true) && !(playerInfo.Object?.isDummy ?? true))
            {
                if (!_alias.TryGetValue(playerId, out var code))
                {
                    code = Helpers.RandomString(5);
                    while (!_used.Add(code)) code = Helpers.RandomString(5);
                    _alias[playerId] = code;
                }
                baseName = _alias[playerId];
            }

            if (anonIcons && __instance.PlayerIcon != null)
            {
                int randomColor = Random.Range(0, Palette.PlayerColors.Length);

                __instance.PlayerIcon.SetBodyColor(randomColor);
                __instance.PlayerIcon.SetHat("hat_Nohat", 0);
                __instance.PlayerIcon.SetSkin("", randomColor);
                __instance.PlayerIcon.SetVisor("", randomColor);
            }

            if (revealRolesForDead)
            {
                var role = playerInfo.Role;
                string roleText;
                string hex;

                if (role != null && CustomRoleManager.GetCustomRoleBehaviour(role.Role, out ICustomRole customRole))
                {
                    roleText = customRole is INewModRole nm
                        ? $"{nm.RoleName} [{Utils.GetFactionDisplay(nm)}]"
                        : customRole.RoleName;
                    hex = ColorUtility.ToHtmlStringRGB(customRole.RoleColor);
                }
                else
                {
                    bool isImp = role?.IsImpostor == true;
                    roleText = isImp ? "Impostor" : "Crewmate";
                    hex = isImp ? "FF4D4D" : "00E0FF";
                }
                __instance.NameText.text = $"{baseName}\n<color=#{hex}>{roleText}</color>";
            }
            else
            {
                __instance.NameText.text = baseName;
            }
        }
    }
}
