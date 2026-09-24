using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using NewMod.Options;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteArea_SetCosmetics_Patch
{
    public static Dictionary<byte, string> _alias;
    public static HashSet<string> _used;

    public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
    {
        var opts = OptionGroupSingleton<GeneralOption>.Instance;
        var anonNames = opts.EnableAnonymousNamesInMeetings;
        var anonIcons = anonNames;

        _alias ??= [];
        _used ??= [];

        var playerId = playerInfo.PlayerId;
        var baseName = playerInfo.PlayerName;

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
            var randomColor = Random.Range(0, Palette.PlayerColors.Length);

            __instance.PlayerIcon.SetBodyColor(randomColor);
            __instance.PlayerIcon.SetHat("hat_Nohat", 0);
            __instance.PlayerIcon.SetSkin("", randomColor);
            __instance.PlayerIcon.SetVisor("", randomColor);
        }

        __instance.NameText.text = PlayerRoleNamePatch.FormatName(baseName, playerInfo);
    }
}