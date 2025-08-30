using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NewMod.Features
{
    public static class CustomPlayerTag
    {
        public enum TagType : byte
        {
            Player, NPC, Dev, Creator, Tester, Staff, Contributor, Host, AOUDev
        }

        public static readonly Dictionary<TagType, string> DefaultHex = new()
        {
            { TagType.Player,      "c0c0c0" },
            { TagType.NPC,         "7D3C98" },
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
            TagType.NPC => "NPC",
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
            return $"\n<size=1.7><color=#{color}>{DisplayName(t)}</color></size>";
        }

        private static bool IsNpc(PlayerControl pc)
        {
            return pc != null && (pc.notRealPlayer || pc.isDummy);
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
        public static class RpcSetNamePatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref string name)
            {
                if (!AmongUsClient.Instance.AmHost) return true;
                if (!IsNpc(__instance)) return true;
                if (string.IsNullOrEmpty(name)) name = string.Empty;
                if (name.Contains("\n<size=", StringComparison.Ordinal)) return true;

                string baseName = name.Split('\n')[0];
                string decorated = baseName + Format(TagType.NPC, DefaultHex[TagType.NPC]);

                name = decorated;
                return false;
            }
        }
    }
}
