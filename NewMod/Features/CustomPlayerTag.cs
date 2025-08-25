using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using Reactor.Utilities;

namespace NewMod.Features
{
    public static class CustomPlayerTag
    {
        public enum TagType : byte
        {
            Player,
            Dev,
            Creator,
            Tester,
            Staff,
            Contributor,
            Host,
            AOUDev
        }

        public static readonly Dictionary<TagType, string> DefaultHex = new()
        {
            { TagType.Player,      "c0c0c0" },
            { TagType.Dev,         "ff4d4d" },
            { TagType.Creator,     "ffb000" },
            { TagType.Tester,      "00e0ff" },
            { TagType.Staff,       "9b59b6" },
            { TagType.Contributor, "7ee081" },
            { TagType.Host,        "ff7f50" },
            { TagType.AOUDev,      "00ffb3" }
        };

        public static string DisplayName(TagType t) => t switch
        {
            TagType.Player => "Player",
            TagType.Dev => "Developer",
            TagType.Creator => "Creator",
            TagType.Tester => "Tester",
            TagType.Staff => "Staff",
            TagType.Contributor => "Contributor",
            TagType.Host => "Host",
            TagType.AOUDev => "AOU Dev",
            _ => ""
        };

        public static string Format(TagType t, string hex)
        {
            string color = string.IsNullOrWhiteSpace(hex)
                ? (DefaultHex.TryGetValue(t, out var h) ? h : "ffffff")
                : hex;
            string label = DisplayName(t);
            return $"\n<size=1.7><color=#{color}>{label}</color></size>";
        }
        public static TagType GetTag(string friendCode)
        {
            if (string.Equals(friendCode, "puncool#9009", StringComparison.OrdinalIgnoreCase)) return TagType.Creator;
            if (string.Equals(friendCode, "peaktipple#8186", StringComparison.OrdinalIgnoreCase)) return TagType.Dev;
            if (string.Equals(friendCode, "shinyrake#9382", StringComparison.OrdinalIgnoreCase)) return TagType.Dev;
            if (string.Equals(friendCode, "dimpledue#6629", StringComparison.OrdinalIgnoreCase)) return TagType.AOUDev;
            return TagType.Player;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
        public static class RpcSetNamePatch
        {
            public static bool Prefix(PlayerControl __instance, ref string name)
            {
                var friendCode = __instance.FriendCode;
                TagType tag = GetTag(friendCode);

                var host = GameData.Instance.GetHost();
                bool isHost = host.PlayerId == __instance.PlayerId;

                string baseName = name.Split('\n')[0];

                string newName = baseName;
                if (isHost)
                    newName += Format(TagType.Host, DefaultHex[TagType.Host]);
                if (tag != TagType.Player)
                    newName += Format(tag, DefaultHex[tag]);
                else
                    newName += Format(TagType.Player, DefaultHex[TagType.Player]);

                Logger<NewMod>.Instance.LogInfo($"Player {__instance.PlayerId} '{baseName}' " + $"FriendCode={friendCode}, Host={isHost}, Tag={DisplayName(tag)} " + $"FinalName='{newName}'");

                __instance.SetName(newName);

                var writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, -1);
                writer.Write(__instance.Data.NetId);
                writer.Write(newName);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                return false;
            }
        }

        [RegisterEvent]
        public static void OnMeetingStart(StartMeetingEvent evt)
        {
            var host = GameData.Instance.GetHost();

            foreach (var ps in evt.MeetingHud.playerStates)
            {
                string baseName = ps.NameText.text.Split('\n')[0];
                bool isHost = ps.TargetPlayerId == host.PlayerId;

                TagType tag = GetTag(GameData.Instance.GetPlayerById(ps.TargetPlayerId).FriendCode);
                string newName = baseName;

                if (isHost)
                    newName += Format(TagType.Host, DefaultHex[TagType.Host]);

                if (tag != TagType.Player)
                    newName += Format(tag, DefaultHex[tag]);
                else
                    newName += Format(TagType.Player, DefaultHex[TagType.Player]);

                ps.NameText.text = newName;
            }
        }
    }
}
