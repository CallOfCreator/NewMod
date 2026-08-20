using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using NewMod.Utilities;

namespace NewMod.Patches.Roles;

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
            if (!Utils.IsRoleActive("Prankster")) return true;

            var pranksterBodies = PranksterUtilities.FindAllPranksterBodies();
            deadBodies = new Il2CppReferenceArray<NetworkedPlayerInfo>(deadBodies.Where(deadBody => !pranksterBodies.Any(pb => pb.ParentId == deadBody.PlayerId)).ToArray());

            return true;
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateButtons))]
        public static class MeetingHud_PopulateButtons_Patch
        {
            public static bool Prefix(MeetingHud __instance)
            {
                if (!Utils.IsRoleActive("Prankster")) return true;

                var fakeBodies = PranksterUtilities.FindAllPranksterBodies();
                var realPlayers = GameData.Instance.AllPlayers.ToArray().Where(p => !fakeBodies.Any(body => body.ParentId == p.PlayerId)).ToList();

                __instance.playerStates = new Il2CppReferenceArray<PlayerVoteArea>(realPlayers.Count);

                for (var i = 0; i < realPlayers.Count; i++)
                {
                    var player = realPlayers[i];
                    var voteArea = __instance.CreateButton(player);
                    voteArea.Parent = __instance;
                    voteArea.SetPlayerId(player.PlayerId);
                    voteArea.SetDead(player.Disconnected || player.IsDead);
                    __instance.playerStates[i] = voteArea;
                }

                __instance.SortButtons();

                return false;
            }
        }
    }
}