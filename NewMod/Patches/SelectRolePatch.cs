using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Roles;
using NewMod.Options;
using NewMod.Roles;
using UnityEngine;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    static class SelectRolePatch
    {
        public static void Postfix(RoleManager __instance)
        {
            var opts = OptionGroupSingleton<GeneralOption>.Instance;
            int target = Mathf.RoundToInt(opts.TotalNeutrals);

            var all = GameData.Instance.AllPlayers.ToArray().Where(p => !p.Disconnected).ToList();
            var neutrals = all.Where(p =>
            {
                var rb = p.Object.Data.Role;
                return rb is ICustomRole cr && cr is INewModRole nm &&
                       (nm.Faction == NewModFaction.Apex || nm.Faction == NewModFaction.Entropy);
            }).Select(p => p.Object).ToList();

            if (opts.KeepCrewMajority)
            {
                int crewCount = all.Count(p => p.Role is CrewmateRole);
                int maxAllowed = Math.Max(0, (int)Math.Floor((crewCount - 1) / 2.0));
                target = Math.Min(target, maxAllowed);
            }

            int have = neutrals.Count;
            if (have == target) return;

            if (have > target)
            {
                int remove = have - target;
                neutrals.Shuffle();
                for (int i = 0; i < remove && i < neutrals.Count; i++)
                    neutrals[i].RpcSetRole(RoleTypes.Crewmate);
                return;
            }

            int need = target - have;
            var crewElig = all.Where(p => p.Role is CrewmateRole)
                              .Select(p => p.Object)
                              .Where(pc => pc && !(pc.Data?.Role is ICustomRole cr && cr is INewModRole nm2 &&
                                                   (nm2.Faction == NewModFaction.Apex || nm2.Faction == NewModFaction.Entropy)))
                              .ToList();
            if (crewElig.Count == 0) return;

            var active = CustomRoleUtils.GetActiveRoles().ToList();

            var candidates = new List<Candidate>();

            foreach (var r in CustomRoleManager.CustomMiraRoles)
            {
                if (r is not INewModRole nm) continue;
                if (nm.Faction != NewModFaction.Apex && nm.Faction != NewModFaction.Entropy) continue;

                var roleType = (RoleTypes)RoleId.Get(r.GetType());
                int already = active.Count(x => x && x.Role == roleType);
                int left = Math.Max(0, Mathf.Clamp(r.Configuration.MaxRoleCount - already, 0, 10));
                if (left <= 0) continue;

                int chance = r.GetChance() ?? r.Configuration.DefaultChance;
                float weight = Mathf.Max(0f, chance);
                if (weight <= 0f) continue;

                candidates.Add(new Candidate { Role = r, Left = left, Weight = weight, RoleType = roleType });
            }
            if (candidates.Count == 0) return;

            var picks = new List<ICustomRole>();
            if (opts.PreferVariety)
            {
                var ordered = candidates.OrderByDescending(x => x.Weight).ToList();
                for (int i = 0; i < ordered.Count && picks.Count < need; i++)
                {
                    if (ordered[i].Left <= 0) continue;
                    picks.Add(ordered[i].Role);
                    var entry = ordered[i];
                    entry.Left -= 1;
                    ordered[i] = entry;
                }
                candidates = ordered;
            }

            while (picks.Count < need)
            {
                var avail = candidates.Where(c => c.Left > 0 && c.Weight > 0f).ToList();
                if (avail.Count == 0) break;

                float total = avail.Sum(c => c.Weight);
                float rnum = UnityEngine.Random.Range(0f, total);
                float acc = 0f;
                int chosenIdx = 0;
                for (int i = 0; i < avail.Count; i++)
                {
                    acc += avail[i].Weight;
                    if (rnum <= acc) { chosenIdx = i; break; }
                }

                var chosen = avail[chosenIdx];
                picks.Add(chosen.Role);

                int globalIdx = candidates.FindIndex(x => x.RoleType == chosen.RoleType);
                if (globalIdx >= 0) candidates[globalIdx] = new Candidate
                {
                    Role = candidates[globalIdx].Role,
                    Left = candidates[globalIdx].Left - 1,
                    Weight = candidates[globalIdx].Weight,
                    RoleType = candidates[globalIdx].RoleType
                };
            }

            for (int i = 0; i < picks.Count && crewElig.Count > 0; i++)
            {
                int idx = HashRandom.FastNext(crewElig.Count);
                var pc = crewElig[idx];
                crewElig.RemoveAt(idx);
                var rt = (RoleTypes)RoleId.Get(picks[i].GetType());
                pc.RpcSetRole(rt);
            }
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
