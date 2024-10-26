using UnityEngine;
using NewMod.Roles.NeutralRoles;
using System.Collections.Generic;

namespace NewMod.Utilities
{
    public static class PranksterUtilities
    {
        private const string PranksterBodyName = "PranksterCloneBody";
        private static Dictionary<byte, int> ReportCounts = new();
        public static void CreatePranksterDeadBody()
        {
            DeadBody deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.name = PranksterBodyName;
            deadBody.ParentId = PlayerControl.LocalPlayer.PlayerId;
            deadBody.transform.position = PlayerControl.LocalPlayer.transform.position;
        }
        public static bool IsPranksterBody(DeadBody body)
        {
            return body.name.Equals(PranksterBodyName, System.StringComparison.OrdinalIgnoreCase);
        }
        public static List<DeadBody> FindAllPranksterBodies()
        {
            DeadBody[] allDeadBodies = Object.FindObjectsOfType<DeadBody>();
            List<DeadBody> pranksterBodies = new List<DeadBody>();

            foreach (var body in allDeadBodies)
            {
                if (IsPranksterBody(body))
                {
                    pranksterBodies.Add(body);
                }
            }

            return pranksterBodies;
        }
        public static void IncrementReportCount(byte playerId)
        {
            if (!ReportCounts.ContainsKey(playerId))
            {
                ReportCounts[playerId] = 1;
            }
            else 
            {
                 ReportCounts[playerId]++;
            }
        }
        public static int GetReportCount(byte playerId)
        {
            return ReportCounts.ContainsKey(playerId) ? ReportCounts[playerId] : 0; 
        }
    }
}
