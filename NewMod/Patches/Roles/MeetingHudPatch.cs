using System;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Events;
using System.Collections.Generic;
using HarmonyLib;
using NewMod.Utilities;
using MiraAPI.Roles;
using MiraAPI.Hud;
using Reactor.Utilities;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using AmongUs.GameOptions;
using NewMod.Roles.NeutralRoles;

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

            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateButtons))]
            public static class MeetingHud_PopulateButtons_Patch
            {
                public static bool Prefix(MeetingHud __instance, byte reporter)
                {
                    var fakeBodies = PranksterUtilities.FindAllPranksterBodies();
                    var realPlayers = GameData.Instance.AllPlayers
                       .ToArray()
                       .Where(p => !fakeBodies.Any(body => body.ParentId == p.PlayerId))
                       .ToList();

                    __instance.playerStates = new Il2CppReferenceArray<PlayerVoteArea>(realPlayers.Count);

                    for (int i = 0; i < realPlayers.Count; i++)
                    {
                        var player = realPlayers[i];
                        PlayerVoteArea voteArea = __instance.CreateButton(player);
                        voteArea.Parent = __instance;
                        voteArea.SetTargetPlayerId(player.PlayerId);
                        voteArea.SetDead(
                            didReport: (player.PlayerId == reporter),
                            isDead: player.Disconnected || player.IsDead,
                            isGuardian: player.Role != null && player.Role.Role == RoleTypes.GuardianAngel
                        );
                        __instance.playerStates[i] = voteArea;
                    }
                    __instance.SortButtons();

                    return false;
                }
            }
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
            public static class MeetingHud_VotingComplete_Patch
            {
                public static void Postfix(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
                {
                    if (tie || exiled == null) return;

                    var exiledPlayer = Utils.PlayerById(exiled.PlayerId);
                    foreach (var overload in PlayerControl.AllPlayerControls.ToArray().Where(p => p.AmOwner && p.Data.Role is OverloadRole))
                    {
                        if (!(exiledPlayer.Data.Role is ICustomRole))
                        {
                            if (exiledPlayer.Data.Role.Ability == null)
                            {
                                Coroutines.Start(CoroutinesHelper.CoNotify("<color=orange>No ability to absorb from this player.</color>"));
                                continue;
                            }
                            if (OverloadRole.AbsorbedAbilityCount >= 3)
                            {
                                Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Maximum abilities absorbed.</color>"));
                                continue;
                            }
                            OverloadRole.AbsorbedAbilityCount++;
                            var role = exiledPlayer.Data.Role;

                            var absorbedButton = Object.Instantiate(HudManager.Instance.AbilityButton, HudManager.Instance.AbilityButton.transform.parent);
                            absorbedButton.SetFromSettings(role.Ability);

                            var pb = absorbedButton.GetComponent<PassiveButton>();
                            pb.OnClick.RemoveAllListeners();
                            pb.OnClick.AddListener((UnityAction)role.UseAbility);
                            
                            Coroutines.Start(CoroutinesHelper.CoNotify(
                                $"<color=green>Ability absorbed from {exiledPlayer.Data.PlayerName}. Total absorbed: {OverloadRole.AbsorbedAbilityCount}</color>"));
                        }
                        else
                        {
                            if (OverloadRole.AbsorbedAbilityCount >= 3)
                            {
                                Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Maximum abilities absorbed.</color>"));
                                continue;
                            }
                            OverloadRole.AbsorbedAbilityCount++;
                            var customRole = (ICustomRole)exiledPlayer.Data.Role;
                            var parentMod = customRole.ParentMod;
                            Debug.Log(parentMod == null);

                            var buttons = parentMod.GetButtons();
                            Debug.Log(buttons.Count);

                            var exiledButton = buttons.First();
                            var newButton = Activator.CreateInstance(exiledButton.GetType()) as CustomActionButton;
                            newButton.CreateButton(HudManager.Instance.AbilityButton.transform.parent);
                            newButton.OverrideName(exiledButton.Name);
                            newButton.OverrideSprite(exiledButton.Sprite.LoadAsset());

                            var passive = newButton.Button.GetComponent<PassiveButton>();
                            passive.OnClick.RemoveAllListeners();
                            passive.OnClick.AddListener((UnityAction)newButton.ClickHandler);

                            Coroutines.Start(CoroutinesHelper.CoNotify(
                                $"<color=green>Custom ability absorbed from {exiledPlayer.Data.PlayerName}. Total absorbed: {OverloadRole.AbsorbedAbilityCount}</color>"));
                        }
                    }
                }
            }
        }
    }
}
