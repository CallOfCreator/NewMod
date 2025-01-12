using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Reactor.Networking.Attributes;

namespace NewMod.Utilities
{
    public static class PranksterUtilities
    {
        // The name assigned to the prankster clone body
        private const string PranksterBodyName = "PranksterCloneBody";

        // Dictionary tracking how many times each player has reported a Prankster body
        private static readonly Dictionary<byte, int> ReportCounts = new();

        /// <summary>
        /// Creates a "prankster clone" dead body at the local player's position.
        /// </summary>
        [MethodRpc((uint)CustomRPC.FakeBody, LocalHandling = Reactor.Networking.Rpc.RpcLocalHandling.After)]
        public static void CreatePranksterDeadBody(PlayerControl player, byte parentId)
        {
            var deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.name = PranksterBodyName;
            deadBody.ParentId = parentId;
            // Shuffle the player colors
            var shuffledColors = Utils.ShuffleArrays(Palette.PlayerColors.ToArray());

            for (int i = 0; i < deadBody.bodyRenderers.Length; i++)
            {
                deadBody.bodyRenderers[i].color = shuffledColors[i % shuffledColors.Length];
            }
            deadBody.transform.position = player.GetTruePosition();
        }

        /// <summary>
        /// Checks if the given DeadBody object is a prankster clone body.
        /// </summary>
        /// <param name="body">The DeadBody object to check.</param>
        /// <returns>True if the body is a prankster clone body; otherwise, false.</returns>
        public static bool IsPranksterBody(DeadBody body)
        {
            return body.name.Equals(PranksterBodyName, System.StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Finds all prankster clone bodies currently in the game scene.
        /// </summary>
        /// <returns>A list of DeadBody objects representing all prankster clone bodies found.</returns>
        public static List<DeadBody> FindAllPranksterBodies()
        {
            var allDeadBodies = Object.FindObjectsOfType<DeadBody>();
            var pranksterBodies = new List<DeadBody>();

            foreach (var body in allDeadBodies)
            {
                if (IsPranksterBody(body))
                {
                    pranksterBodies.Add(body);
                }
            }
            return pranksterBodies;
        }

        /// <summary>
        /// Increments the report count for a specified player when they report a prankster body.
        /// </summary>
        /// <param name="playerId">The ID of the player reporting the body.</param>
        public static void IncrementReportCount(byte playerId)
        {
            ReportCounts[playerId] = GetReportCount(playerId) + 1;
        }

        /// <summary>
        /// Clears all recorded report counts for prankster bodies.
        /// </summary>
        public static void ResetReportCount()
        {
            ReportCounts.Clear();
        }

        /// <summary>
        /// Retrieves the number of times the specified player has reported a prankster body.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The report count for the player, or 0 if the player has not reported a prankster body.</returns>
        public static int GetReportCount(byte playerId)
        {
            return ReportCounts.TryGetValue(playerId, out int value) ? value : 0;
        }
    }
}
