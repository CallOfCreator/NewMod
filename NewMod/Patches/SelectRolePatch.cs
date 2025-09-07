using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod;
using NewMod.Options;
using NewMod.Roles;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class SelectRolePatch
    {
        public static void Postfix(RoleManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: START --------------");
            Logger<NewMod>.Instance.LogMessage(
    $"SelectRoles Postfix entered on {(AmongUsClient.Instance.AmHost ? "HOST" : "CLIENT")} (clientId={AmongUsClient.Instance.ClientId})");


            var opts = OptionGroupSingleton<GeneralOption>.Instance;
            int target = Mathf.RoundToInt(opts.TotalNeutrals);
            Logger<NewMod>.Instance.LogMessage($"Config -> TotalNeutrals={opts.TotalNeutrals} (target={target}), KeepCrewMajority={opts.KeepCrewMajority}, PreferVariety={opts.PreferVariety}");

            var all = GameData.Instance.AllPlayers.ToArray()
                .Where(p => !p.IsDead && !p.Disconnected && p.Object)
                .ToList();
            Logger<NewMod>.Instance.LogMessage($"Alive players eligible (all): {all.Count}");

            var neutrals = all.Where(p =>
            {
                var rb = p.Object.Data.Role;
                return rb is ICustomRole cr && cr is INewModRole nm &&
                       (nm.Faction == NewModFaction.Apex || nm.Faction == NewModFaction.Entropy);
            }).Select(p => p.Object).ToList();
            Logger<NewMod>.Instance.LogMessage($"Current neutrals (Apex or Entropy): {neutrals.Count}");

            if (opts.KeepCrewMajority)
            {
                int crewCount = all.Count(p =>
                {
                    var rb = p.Object.Data.Role;
                    if (!rb) return false;
                    return (rb is CrewmateRole) || (!rb.IsImpostor && rb.TeamType == RoleTeamTypes.Crewmate);
                });

                int maxAllowed = Math.Max(0, (int)Math.Floor((crewCount - 1) / 2.0));
                int before = target;
                target = Math.Min(target, maxAllowed);
                Logger<NewMod>.Instance.LogMessage($"KeepCrewMajority -> crewCount={crewCount}, maxAllowedNeutrals={maxAllowed}, target {before} => {target}");
            }

            int have = neutrals.Count;
            Logger<NewMod>.Instance.LogMessage($"Neutral count check -> have={have}, target={target}");

            if (have == target)
            {
                Logger<NewMod>.Instance.LogMessage("No changes needed, exiting early.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (no-op) --------------");
                return;
            }

            if (have > target)
            {
                int remove = have - target;
                Logger<NewMod>.Instance.LogMessage($"Too many neutrals -> remove={remove}. Shuffling current neutrals...");
                neutrals.Shuffle();

                for (int i = 0; i < remove && i < neutrals.Count; i++)
                {
                    var ply = neutrals[i];
                    Logger<NewMod>.Instance.LogMessage($"Demoting to Crewmate -> {ply.PlayerId}");
                    ply.RpcSetRole(RoleTypes.Crewmate);
                }

                Logger<NewMod>.Instance.LogMessage("Demotion phase complete.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (demotions) --------------");
                return;
            }

            int need = target - have;
            Logger<NewMod>.Instance.LogMessage($"Need more neutrals -> need={need}");

            var crewElig = all.Where(p =>
            {
                var rb = p.Object.Data.Role;
                if (!rb) return false;
                bool isCrew = (rb is CrewmateRole) || (!rb.IsImpostor && rb.TeamType == RoleTeamTypes.Crewmate);
                if (!isCrew) return false;

                if (rb is ICustomRole cr && cr is INewModRole nm2)
                    return nm2.Faction != NewModFaction.Apex && nm2.Faction != NewModFaction.Entropy;

                return true;
            }).Select(p => p.Object).ToList();
            Logger<NewMod>.Instance.LogMessage($"Crew eligible for conversion -> count={crewElig.Count}");

            if (crewElig.Count == 0)
            {
                Logger<NewMod>.Instance.LogMessage("No crew eligible to convert. Exiting.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (no elig crew) --------------");
                return;
            }

            var active = CustomRoleUtils.GetActiveRoles().ToList();
            Logger<NewMod>.Instance.LogMessage($"Active custom roles snapshot -> count={active.Count}");

            var candidates = new List<Candidate>();

            foreach (var r in CustomRoleManager.CustomMiraRoles)
            {
                if (r is not INewModRole nm) continue;
                if (nm.Faction != NewModFaction.Apex && nm.Faction != NewModFaction.Entropy) continue;

                var roleType = (RoleTypes)RoleId.Get(r.GetType());
                int already = active.Count(x => x && x.Role == roleType);
                int left = r.Configuration.MaxRoleCount - already;
                if (left <= 0) continue;

                int chance = r.GetChance() ?? r.Configuration.DefaultChance;
                float weight = chance;
                if (weight <= 0f) continue;

                candidates.Add(new Candidate { Role = r, Left = left, Weight = weight, RoleType = roleType });
                Logger<NewMod>.Instance.LogMessage($"Candidate -> {r.GetType().Name} type={(ushort)roleType} left={left} weight={weight} already={already} max={r.Configuration.MaxRoleCount}");
            }

            Logger<NewMod>.Instance.LogMessage($"Candidate pool built -> count={candidates.Count}");
            if (candidates.Count == 0)
            {
                Logger<NewMod>.Instance.LogMessage("No candidates to assign. Exiting.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (no candidates) --------------");
                return;
            }

            var picks = new List<ICustomRole>();
            if (opts.PreferVariety)
            {
                Logger<NewMod>.Instance.LogMessage("PreferVariety enabled -> taking one of each highest weight until need is met.");
                var ordered = candidates.OrderByDescending(x => x.Weight).ToList();
                for (int i = 0; i < ordered.Count && picks.Count < need; i++)
                {
                    if (ordered[i].Left <= 0) continue;
                    picks.Add(ordered[i].Role);
                    Logger<NewMod>.Instance.LogMessage($"Variety pick -> {ordered[i].Role.GetType().Name}");
                    var e = ordered[i]; e.Left -= 1; ordered[i] = e;
                }
                candidates = ordered;
            }

            while (picks.Count < need)
            {
                var avail = candidates.Where(c => c.Left > 0 && c.Weight > 0f).ToList();
                if (avail.Count == 0)
                {
                    Logger<NewMod>.Instance.LogMessage("No more available candidates with slots and weight. Breaking.");
                    break;
                }

                float total = avail.Sum(c => c.Weight);
                float rnum = UnityEngine.Random.Range(0f, total);
                float acc = 0f;
                var chosen = avail[0];

                for (int i = 0; i < avail.Count; i++)
                {
                    acc += avail[i].Weight;
                    if (rnum <= acc) { chosen = avail[i]; break; }
                }

                picks.Add(chosen.Role);
                Logger<NewMod>.Instance.LogMessage($"Weighted pick -> {chosen.Role.GetType().Name} (roll={rnum:F2} / total={total:F2})");

                int gi = candidates.FindIndex(x => x.RoleType == chosen.RoleType);
                if (gi >= 0)
                {
                    candidates[gi] = new Candidate
                    {
                        Role = candidates[gi].Role,
                        Left = candidates[gi].Left - 1,
                        Weight = candidates[gi].Weight,
                        RoleType = candidates[gi].RoleType
                    };
                    Logger<NewMod>.Instance.LogMessage($"Decrement slot -> {candidates[gi].Role.GetType().Name} now left={candidates[gi].Left}");
                }
            }

            Logger<NewMod>.Instance.LogMessage($"Final picks -> count={picks.Count}. Starting assignment to crewElig={crewElig.Count}");

            for (int i = 0; i < picks.Count && crewElig.Count > 0; i++)
            {
                int idx = HashRandom.FastNext(crewElig.Count);
                var pc = crewElig[idx];
                crewElig.RemoveAt(idx);

                var rt = (RoleTypes)RoleId.Get(picks[i].GetType());
                Logger<NewMod>.Instance.LogMessage($"Assign -> playerId={pc.PlayerId} role={(ushort)rt} ({picks[i].GetType().Name})");
                pc.RpcSetRole(rt);
            }

            Logger<NewMod>.Instance.LogMessage("Assignment phase complete.");
            Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END --------------");
        }
        struct Candidate
        {
            public ICustomRole Role;
            public int Left;
            public float Weight;
            public RoleTypes RoleType;
        }
    }
}
