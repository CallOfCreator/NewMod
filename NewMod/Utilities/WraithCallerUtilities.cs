using System.Collections.Generic;
using System.Linq;
using NewMod.Components;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace NewMod.Utilities
{
    public static class WraithCallerUtilities
    {
        /// <summary>
        /// A dictionary holding the number of NPCs sent by each Wraith Caller.
        /// </summary>
        private static readonly Dictionary<byte, int> Sent = [];

        /// <summary>
        /// A dictionary holding the number of kills achieved by NPCs summoned by each Wraith Caller.
        /// </summary>
        private static readonly Dictionary<byte, int> Kills = [];

        /// <summary>
        /// Returns the number of NPCs sent by the specified owner.
        /// </summary>
        public static int GetSentNPC(byte ownerId)
        {
            return Sent.TryGetValue(ownerId, out var v) ? v : 0;
        }

        /// <summary>
        /// Returns the number of successful kills credited to the owner’s wraiths.
        /// </summary>
        public static int GetKillsNPC(byte ownerId)
        {
            return Kills.TryGetValue(ownerId, out var v) ? v : 0;
        }

        /// <summary>
        /// Increments the number of NPCs sent by the owner.
        /// </summary>
        public static void AddSentNPC(byte ownerId, int amount = 1)
        {
            Sent[ownerId] = GetSentNPC(ownerId) + amount;
        }

        /// <summary>
        /// Increments the number of kills credited to the owner’s wraiths.
        /// </summary>
        public static void AddKillNPC(byte ownerId, int amount = 1)
        {
            Kills[ownerId] = GetKillsNPC(ownerId) + amount;
        }

        /// <summary>
        /// Clears all counters for both NPCs sent and kills achieved.
        /// </summary>
        public static void ClearAll()
        {
            Sent.Clear();
            Kills.Clear();
        }
        [MethodRpc((uint)CustomRPC.RequestSummon)]
        public static void RpcRequestSummonNPC(PlayerControl source, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            var npcId = HostNPC(source);
            RpcSummonNPC(source, target, npcId);
        }
        public static byte HostNPC(PlayerControl source)
        {
            var prefab = AmongUsClient.Instance.PlayerPrefab;
            var npc = Object.Instantiate(prefab);
            npc.PlayerId = (byte)GameData.Instance.GetAvailableId();

            npc.isDummy = false;
            npc.notRealPlayer = true;

            var host = AmongUsClient.Instance.GetHost();
            var npcInfo = GameData.Instance.AddPlayer(npc, host);

            AmongUsClient.Instance.Spawn(npcInfo);
            AmongUsClient.Instance.Spawn(npc);

            npc.NetTransform.RpcSnapTo(source.transform.position);

            return npc.PlayerId;
        }
        [MethodRpc((uint)CustomRPC.SummonNPC)]
        public static void RpcSummonNPC(PlayerControl source, PlayerControl target, byte npcId)
        {
            AddSentNPC(source.PlayerId);

            InitializeNPC(source, target, Utils.PlayerById(npcId));
            return;

        }
        public static void InitializeNPC(PlayerControl owner, PlayerControl target, PlayerControl npc)
        {
            if (!npc.gameObject.TryGetComponent<WraithCallerNpc>(out _))
            {
                var comp = npc.gameObject.AddComponent<WraithCallerNpc>();
                comp.Initialize(owner, target, npc);
            }
        }
    }
}
