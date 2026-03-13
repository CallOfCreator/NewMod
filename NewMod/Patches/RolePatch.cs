using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Options;
using NewMod.Roles;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class SelectRolePatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(RoleManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (GameManager.Instance.IsHideAndSeek()) return;

            Coroutines.Start(CoAdjustNeutrals());
        }

        private static IEnumerator CoAdjustNeutrals()
        {
            yield return null;
            yield return new WaitForSeconds(0.05f);

            var opts = OptionGroupSingleton<GeneralOption>.Instance;
            int target = Mathf.RoundToInt(opts.TotalNeutrals);

            var allInfos = GameData.Instance.AllPlayers.ToArray()
                .Where(p => p != null && p.Object != null && !p.Disconnected)
                .ToList();

            var allPlayers = allInfos.Select(p => p.Object).ToList();

            Logger<NewMod>.Instance.LogMessage("-------------- NEUTRAL ADJUST: START --------------");
            Logger<NewMod>.Instance.LogMessage($"Players={allPlayers.Count}, TotalNeutrals={opts.TotalNeutrals} target={target}, KeepCrewMajority={opts.KeepCrewMajority}, PreferVariety={opts.PreferVariety}");

            var neutrals = allPlayers
                .Where(pc =>
                {
                    var rb = pc.Data?.Role;
                    if (rb is ICustomRole cr && cr is INewModRole nm)
                        return nm.Faction == NewModFaction.Apex || nm.Faction == NewModFaction.Entropy;
                    return false;
                })
                .ToList();

            if (opts.KeepCrewMajority)
            {
                int crewCount = allPlayers.Count(pc =>
                {
                    var rb = pc.Data?.Role;
                    if (rb == null) return false;
                    return rb is CrewmateRole || (!rb.IsImpostor && rb.TeamType == RoleTeamTypes.Crewmate);
                });

                int maxAllowed = Math.Max(0, (int)Math.Floor((crewCount - 1) / 2.0));
                int before = target;
                target = Math.Min(target, maxAllowed);
                Logger<NewMod>.Instance.LogMessage($"KeepCrewMajority applied -> crewCount={crewCount}, maxNeutrals={maxAllowed}, adjustedTarget={target} (was {before})");
            }

            int have = neutrals.Count;
            Logger<NewMod>.Instance.LogMessage($"Currently neutrals={have}");

            if (have == target)
            {
                Logger<NewMod>.Instance.LogMessage("No change needed.");
                Logger<NewMod>.Instance.LogMessage("-------------- NEUTRAL ADJUST: END (no-op) --------------");
                yield break;
            }

            if (have > target)
            {
                int remove = have - target;
                Logger<NewMod>.Instance.LogMessage($"Too many neutrals, demoting {remove}");

                neutrals.Shuffle();
                for (int i = 0; i < remove && i < neutrals.Count; i++)
                {
                    var pc = neutrals[i];
                    if (pc == null) continue;

                    Logger<NewMod>.Instance.LogMessage($"→ Demoting {pc.Data.PlayerName} to Crewmate");
                    pc.RpcSetRole(RoleTypes.Crewmate, true);
                }

                Logger<NewMod>.Instance.LogMessage("-------------- NEUTRAL ADJUST: END (demotions) --------------");
                yield break;
            }

            int need = target - have;
            Logger<NewMod>.Instance.LogMessage($"Need to assign {need} more neutrals.");

            var crewElig = allPlayers
                .Where(pc =>
                {
                    var rb = pc.Data?.Role;
                    if (rb == null) return false;

                    bool isCrew = rb is CrewmateRole || (!rb.IsImpostor && rb.TeamType == RoleTeamTypes.Crewmate);
                    if (!isCrew) return false;

                    if (rb is ICustomRole cr && cr is INewModRole nm2)
                        return nm2.Faction != NewModFaction.Apex && nm2.Faction != NewModFaction.Entropy;

                    return true;
                })
                .ToList();

            Logger<NewMod>.Instance.LogMessage($"Crew eligible for neutral conversion: {crewElig.Count}");
            if (crewElig.Count == 0)
            {
                Logger<NewMod>.Instance.LogMessage("No eligible crew found, aborting.");
                Logger<NewMod>.Instance.LogMessage("-------------- NEUTRAL ADJUST: END (no candidates) --------------");
                yield break;
            }

            var active = CustomRoleUtils.GetActiveRoles().ToList();
            var candidates = new List<Candidate>();

            foreach (var r in CustomRoleManager.CustomMiraRoles)
            {
                if (r is not INewModRole nm) continue;
                if (nm.Faction != NewModFaction.Apex && nm.Faction != NewModFaction.Entropy) continue;

                var roleType = (RoleTypes)RoleId.Get(r.GetType());
                int already = active.Count(x => x && x.Role == roleType);
                int left = r.Configuration.MaxRoleCount - already;
                if (left <= 0) continue;

                int chance = r.GetChance().Value;
                if (chance <= 0) continue;

                candidates.Add(new Candidate
                {
                    Role = r,
                    Left = left,
                    Weight = chance,
                    RoleType = roleType
                });
            }

            Logger<NewMod>.Instance.LogMessage($"Built neutral candidate list: {candidates.Count}");
            if (candidates.Count == 0)
            {
                Logger<NewMod>.Instance.LogMessage("No candidates available, exiting.");
                Logger<NewMod>.Instance.LogMessage("-------------- NEUTRAL ADJUST: END --------------");
                yield break;
            }

            var picks = new List<ICustomRole>();

            if (opts.PreferVariety)
            {
                var ordered = candidates.OrderByDescending(x => x.Weight).ToList();
                for (int i = 0; i < ordered.Count && picks.Count < need; i++)
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

                float total = available.Sum(c => c.Weight);
                float rnum = UnityEngine.Random.Range(0f, total);
                float acc = 0f;
                var chosen = available[0];

                foreach (var c in available)
                {
                    acc += c.Weight;
                    if (rnum <= acc) { chosen = c; break; }
                }

                picks.Add(chosen.Role);

                int idx = candidates.FindIndex(x => x.RoleType == chosen.RoleType);
                if (idx >= 0)
                {
                    var tmp = candidates[idx];
                    tmp.Left--;
                    candidates[idx] = tmp;
                }
            }

            Logger<NewMod>.Instance.LogMessage($"Assigning {picks.Count} neutrals...");

            foreach (var role in picks)
            {
                if (crewElig.Count == 0) break;

                int idx = HashRandom.FastNext(crewElig.Count);
                var pc = crewElig[idx];
                crewElig.RemoveAt(idx);

                var rt = (RoleTypes)RoleId.Get(role.GetType());
                pc.RpcSetRole(rt, true);
            }

            Logger<NewMod>.Instance.LogMessage("Neutral assignment complete.");
            Logger<NewMod>.Instance.LogMessage("-------------- NEUTRAL ADJUST: END --------------");
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
            if (canOverrideRole)
            {
                __instance.roleAssigned = false;
            }
        }
    }
}