using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;
using NewMod.Options;
using NewMod.Roles;
using NewMod.Roles.ImpostorRoles.S1;
using Reactor.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.Patches;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class SelectRolePatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(RoleManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost || !CustomGameModeManager.IsClassic())
            return;

        Coroutines.Start(CoAdjustNeutrals());
    }

    private static IEnumerator CoAdjustNeutrals()
    {
        yield return null;
        yield return new WaitForSeconds(0.05f);

        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !CustomGameModeManager.IsClassic())
            yield break;

        var options = OptionGroupSingleton<GeneralOption>.Instance;
        var target = Mathf.RoundToInt(options.TotalNeutrals);

        var allPlayers = new List<PlayerControl>();
        foreach (var player in GameData.Instance.AllPlayers)
        {
            if (player && player.Object && !player.Disconnected && !player.IsDead && !player.Object.notRealPlayer)
                allPlayers.Add(player.Object);
        }

        Instance.LogMessage("-------------- NEUTRAL ADJUST: START --------------");
        Instance.LogMessage($"Players={allPlayers.Count}, TotalNeutrals={options.TotalNeutrals} target={target}, KeepCrewMajority={options.KeepCrewMajority}, PreferVariety={options.PreferVariety}");

        var neutrals = allPlayers.Where(player => player.Data?.Role is INewModRole { Team: ModdedRoleTeams.Custom }).ToList();

        var eligibleCrew = allPlayers.Where(player =>
        {
            var playerRole = player.Data?.Role;
            if (playerRole == null || playerRole is ICustomRole { Team: ModdedRoleTeams.Custom })
                return false;

            return playerRole is CrewmateRole || (!playerRole.IsImpostor && playerRole.TeamType == RoleTeamTypes.Crewmate);
        }).ToList();

        if (options.KeepCrewMajority)
        {
            var crewAndNeutralCount = eligibleCrew.Count + neutrals.Count;
            var maxAllowed = Math.Max(0, (crewAndNeutralCount - 1) / 2);
            var before = target;
            target = Math.Min(target, maxAllowed);
            Instance.LogMessage($"KeepCrewMajority applied -> crewAndNeutralCount={crewAndNeutralCount}, maxNeutrals={maxAllowed}, adjustedTarget={target} (was {before})");
        }

        var have = neutrals.Count;
        Instance.LogMessage($"Currently neutrals={have}");

        if (have == target)
        {
            Instance.LogMessage("No change needed.");
            Instance.LogMessage("-------------- NEUTRAL ADJUST: END (no-op) --------------");
            yield break;
        }

        if (have > target)
        {
            var remove = have - target;
            Instance.LogMessage($"Too many neutrals, demoting {remove}");

            neutrals.Shuffle();
            for (var i = 0; i < remove && i < neutrals.Count; i++)
            {
                var player = neutrals[i];
                if (player == null) continue;

                Instance.LogMessage($"→ Demoting {player.Data.PlayerName} to Crewmate");
                player.RpcSetRole(RoleTypes.Crewmate, true);
            }

            Instance.LogMessage("-------------- NEUTRAL ADJUST: END (demotions) --------------");
            yield break;
        }

        var need = Math.Min(target - have, eligibleCrew.Count);
        Instance.LogMessage($"Need to assign {need} more neutrals.");

        Instance.LogMessage($"Crew eligible for neutral conversion: {eligibleCrew.Count}");
        if (eligibleCrew.Count == 0)
        {
            Instance.LogMessage("No eligible crew found, aborting.");
            Instance.LogMessage("-------------- NEUTRAL ADJUST: END (no candidates) --------------");
            yield break;
        }

        var active = allPlayers.Select(player => player.Data.Role).ToList();
        var candidates = new List<Candidate>();

        foreach (var r in CustomRoleManager.CustomMiraRoles)
        {
            if (r is not INewModRole role)
                continue;

            if (role.Team != ModdedRoleTeams.Custom || r.Configuration.HideSettings)
                continue;

            if (r is not RoleBehaviour roleBehaviour || RoleManager.IsGhostRole(roleBehaviour.Role) || !CustomRoleUtils.CanSpawnOnCurrentMode(roleBehaviour))
                continue;

            var roleType = (RoleTypes)RoleId.Get(r.GetType());
            var already = active.Count(x => x && x.Role == roleType);
            var left = r.GetCount().GetValueOrDefault() - already;
            if (left <= 0) continue;

            var chance = r.GetChance().GetValueOrDefault();
            if (chance <= 0) continue;

            candidates.Add(new Candidate { Role = r, Left = left, Weight = chance, RoleType = roleType });
        }

        Instance.LogMessage($"Built neutral candidate list: {candidates.Count}");
        if (candidates.Count == 0)
        {
            Instance.LogMessage("No candidates available, exiting.");
            Instance.LogMessage("-------------- NEUTRAL ADJUST: END --------------");
            yield break;
        }

        var picks = new List<ICustomRole>();

        if (options.PreferVariety)
        {
            var ordered = candidates.OrderByDescending(x => x.Weight).ToList();
            for (var i = 0; i < ordered.Count && picks.Count < need; i++)
            {
                var c = ordered[i];
                if (c.Left <= 0) continue;
                picks.Add(c.Role);
                c.Left--;
                ordered[i] = c;
            }

            candidates = ordered;
        }

        while (picks.Count < need)
        {
            var available = candidates.Where(c => c.Left > 0 && c.Weight > 0f).ToList();
            if (available.Count == 0) break;

            var total = available.Sum(c => c.Weight);
            var roll = Random.Range(0f, total);
            var cumulativeWeight = 0f;
            var chosen = available[0];

            foreach (var c in available)
            {
                cumulativeWeight += c.Weight;
                if (roll <= cumulativeWeight)
                {
                    chosen = c;
                    break;
                }
            }

            picks.Add(chosen.Role);

            var index = candidates.FindIndex(x => x.RoleType == chosen.RoleType);
            if (index >= 0)
            {
                var candidate = candidates[index];
                candidate.Left--;
                candidates[index] = candidate;
            }
        }

        Instance.LogMessage($"Assigning {picks.Count} neutrals...");

        foreach (var role in picks)
        {
            if (eligibleCrew.Count == 0) break;

            var index = HashRandom.FastNext(eligibleCrew.Count);
            var player = eligibleCrew[index];
            eligibleCrew.RemoveAt(index);

            var roleType = (RoleTypes)RoleId.Get(role.GetType());
            player.RpcSetRole(roleType, true);
        }

        Instance.LogMessage("Neutral assignment complete.");
        Instance.LogMessage("-------------- NEUTRAL ADJUST: END --------------");
    }

    public struct Candidate
    {
        public ICustomRole Role;
        public int Left;
        public float Weight;
        public RoleTypes RoleType;
    }
}

// Thanks to:https://github.com/AU-Avengers/TOU-Mira/blob/main/TownOfUs/Patches/RoleManagerPatches.cs#L1070
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetRole))]
public static class CoSetRoleOverridePatch
{
    [HarmonyPrefix]
    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes role, [HarmonyArgument(1)] bool canOverrideRole)
    {
        if (canOverrideRole) __instance.roleAssigned = false;
    }
}

[HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
public static class RoleKillTargetPatch
{
    public static void Prefix(ref PlayerControl target)
    {
        var player = PlayerControl.LocalPlayer;
        if (player && (MirrorBladeRole.ArmedReflections.Contains(player.PlayerId) || player.HasModifier<InVoid>()))
            target = null;
    }
}
