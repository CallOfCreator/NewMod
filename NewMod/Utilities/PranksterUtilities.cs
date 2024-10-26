using UnityEngine;
using NewMod.Roles.NeutralRoles;
using System.Collections.Generic;

namespace NewMod.Utilities
{
    public static class PranksterUtilities
    {
        private const string PranksterBodyName = "PranksterCloneBody";
        public static void CreatePranksterDeadBody()
        {
            DeadBody deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.name = PranksterBodyName;
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
    }
}
