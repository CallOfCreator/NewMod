using System.Collections.Generic;
using NewMod.Components;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace NewMod.Utilities
{
    public static class WraithCallerUtilities
    {
        public static readonly Dictionary<byte, int> Sent = [];
        public static readonly Dictionary<byte, int> Kills = [];

        public static int GetSentNPC(byte ownerId)
        {
            return Sent.TryGetValue(ownerId, out var v) ? v : 0;
        }

        public static int GetKillsNPC(byte ownerId)
        {
            return Kills.TryGetValue(ownerId, out var v) ? v : 0;
        }

        public static void AddSentNPC(byte ownerId, int amount = 1)
        {
            Sent[ownerId] = GetSentNPC(ownerId) + amount;
        }

        public static void AddKillNPC(byte ownerId, int amount = 1)
        {
            Kills[ownerId] = GetKillsNPC(ownerId) + amount;
        }

        public static void ClearAll()
        {
            Sent.Clear();
            Kills.Clear();
        }

        public static void RequestSummonNPC(PlayerControl owner, PlayerControl target)
        {
            RpcRequestSummonNPC(owner, target.PlayerId);
        }

        [MethodRpc((uint)CustomRPC.RequestSummon)]
        public static void RpcRequestSummonNPC(PlayerControl source, byte targetId)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            var target = Utils.PlayerById(targetId);
            if (!target) return;

            var start = source.GetTruePosition();

            RpcSummonNPC(source, target.PlayerId, start.x, start.y);
        }

        [MethodRpc((uint)CustomRPC.SummonNPC)]
        public static void RpcSummonNPC(PlayerControl source, byte targetId, float x, float y)
        {
            var target = Utils.PlayerById(targetId);

            AddSentNPC(source.PlayerId);

            var holder = new GameObject("WraithNPC_Holder");
            var npc = holder.AddComponent<WraithCallerNpc>();
            npc.Initialize(source, target, new Vector2(x, y));
        }
    }
}