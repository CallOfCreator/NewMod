using System.Collections.Generic;
using HarmonyLib;
using NewMod.Utilities;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using AmongUs.GameOptions;

namespace NewMod.Patches.Roles
{
    public static class MeetingHudPatches
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
        public static class MeetingHud_OnDestroy_Patch
        {
            public static void Postfix(MeetingHud __instance)
            {
                PendingEffectManager.ApplyPendingEffects();
            }
        }
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
        public static class MeetingHud_CoIntro_Patch
        {
            public static bool Prefix(ref Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
            {
                List<DeadBody> pranksterBodies = PranksterUtilities.FindAllPranksterBodies();
                deadBodies = new Il2CppReferenceArray<NetworkedPlayerInfo>(
                deadBodies
                    .Where(deadBody => !pranksterBodies.Any(pb => pb.ParentId == deadBody.PlayerId))
                    .ToArray());

                return true;
            }
        }
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateButtons))]
        public static class MeetingHud_PopulateButtons_Patch
        {
            public static bool Prefix(MeetingHud __instance, byte reporter)
            {
                var fakeBodies = PranksterUtilities.FindAllPranksterBodies();
                var voteArea = GameData.Instance.AllPlayers
                    .ToArray()
                    .Where(player => 
                        !player.IsDead && 
                        !fakeBodies.Any(body => body.ParentId == player.PlayerId)
                    )
                    .Select(player =>
                    {
                        PlayerVoteArea voteArea = __instance.CreateButton(player);
                        voteArea.SetTargetPlayerId(player.PlayerId);
                        voteArea.SetDead(false, player.Disconnected || player.IsDead, player.Role?.Role == RoleTypes.GuardianAngel);
                        return voteArea;
                    });
                __instance.playerStates = new Il2CppReferenceArray<PlayerVoteArea>(voteArea.ToArray());
                __instance.SortButtons();
                return false;
            }
        }
    }
}
