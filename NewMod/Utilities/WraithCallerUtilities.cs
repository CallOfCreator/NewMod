    using System.Collections.Generic;
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

            /// <summary>
            /// RPC for <see cref="SummonNPC"/>. Runs on all clients to keep state in sync.
            /// </summary>
            /// <param name="source">The Wraith Caller owner who summoned the NPC.</param>
            /// <param name="target">The intended target player.</param>

            [MethodRpc((uint)CustomRPC.SummonNPC)]
            public static void RpcSummonNPC(PlayerControl source, PlayerControl target)
            {
                AddSentNPC(source.PlayerId);
                SummonNPC(source, target);
            }

            /// <summary>
            /// Spawns and initializes the wraith NPC that will hunt the target.
            /// Runs locally on each client after the RPC dispatch.
            /// </summary>
            /// <param name="wraith">The Wraith Caller (owner) who summoned the NPC.</param>
            /// <param name="target">The intended target player.</param>
            public static void SummonNPC(PlayerControl wraith, PlayerControl target)
            {
                var npcObj = new GameObject("WraithNPC_Holder");
                var wraithNpc = npcObj.AddComponent<WraithCallerNpc>();

                wraithNpc.Initialize(wraith, target);
            }
        }
    }
