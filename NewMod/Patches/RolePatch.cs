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
using Hazel;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class SelectRolePatch
    {
        public static bool Prefix(RoleManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost)
                return true;

            Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: START --------------");
            Logger<NewMod>.Instance.LogMessage($"Running as host (clientId={AmongUsClient.Instance.ClientId})");

            var opts = OptionGroupSingleton<GeneralOption>.Instance;
            int target = Mathf.RoundToInt(opts.TotalNeutrals);
            Logger<NewMod>.Instance.LogMessage($"Settings -> TotalNeutrals={opts.TotalNeutrals} → target={target}, KeepCrewMajority={opts.KeepCrewMajority}, PreferVariety={opts.PreferVariety}");

            var allPlayers = GameData.Instance.AllPlayers.ToArray()
                .Where(p => p?.Object && !p.IsDead && !p.Disconnected)
                .ToList();

            Logger<NewMod>.Instance.LogMessage($"Eligible players: {allPlayers.Count}");

            var neutrals = allPlayers
                .Where(p => p?.Object?.Data?.Role is ICustomRole cr && cr is INewModRole nm &&
                            (nm.Faction == NewModFaction.Apex || nm.Faction == NewModFaction.Entropy))
                .Select(p => p.Object)
                .ToList();

            Logger<NewMod>.Instance.LogMessage($"Currently neutral (Apex/Entropy): {neutrals.Count}");

            if (opts.KeepCrewMajority)
            {
                int crewCount = allPlayers.Count(p =>
                {
                    var rb = p?.Object?.Data?.Role;
                    if (rb == null) return false;
                    return rb is CrewmateRole || (!rb.IsImpostor && rb.TeamType == RoleTeamTypes.Crewmate);
                });

                int maxAllowed = Math.Max(0, (int)Math.Floor((crewCount - 1) / 2.0));
                int before = target;
                target = Math.Min(target, maxAllowed);
                Logger<NewMod>.Instance.LogMessage($"KeepCrewMajority applied -> crewCount={crewCount}, maxNeutrals={maxAllowed}, adjustedTarget={target} (was {before})");
            }

            int have = neutrals.Count;
            if (have == target)
            {
                Logger<NewMod>.Instance.LogMessage("No change needed; exiting cleanly.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (no-op) --------------");
                return false;
            }

            if (have > target)
            {
                int remove = have - target;
                Logger<NewMod>.Instance.LogMessage($"Too many neutrals; demoting {remove}");
                neutrals.Shuffle();

                for (int i = 0; i < remove && i < neutrals.Count; i++)
                {
                    var ply = neutrals[i];
                    if (ply == null) continue;

                    Logger<NewMod>.Instance.LogMessage($"→ Demoting {ply.Data.PlayerName} to Crewmate");
                    ply.RpcSetRole(RoleTypes.Crewmate);
                }

                Logger<NewMod>.Instance.LogMessage("Demotion complete.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (demotions) --------------");
                return false;
            }

            int need = target - have;
            Logger<NewMod>.Instance.LogMessage($"Need to assign {need} more neutrals.");

            var crewElig = allPlayers
                .Where(p =>
                {
                    var rb = p?.Object?.Data?.Role;
                    if (rb == null) return false;
                    bool isCrew = rb is CrewmateRole || (!rb.IsImpostor && rb.TeamType == RoleTeamTypes.Crewmate);
                    if (!isCrew) return false;

                    if (rb is ICustomRole cr && cr is INewModRole nm2)
                        return nm2.Faction != NewModFaction.Apex && nm2.Faction != NewModFaction.Entropy;

                    return true;
                })
                .Select(p => p.Object)
                .ToList();

            Logger<NewMod>.Instance.LogMessage($"Crew eligible for neutral conversion: {crewElig.Count}");

            if (crewElig.Count == 0)
            {
                Logger<NewMod>.Instance.LogMessage("No eligible crew found; aborting neutral assignment.");
                Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END (no candidates) --------------");
                return false;
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
                Logger<NewMod>.Instance.LogMessage("No candidates available; exiting.");
                return false;
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
                var chosen = available.First();

                foreach (var c in available)
                {
                    acc += c.Weight;
                    if (rnum <= acc)
                    {
                        chosen = c;
                        break;
                    }
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
                Logger<NewMod>.Instance.LogMessage($"Assigning {role.GetType().Name} → {pc.Data.PlayerName}");
                pc.RpcSetRole(rt);
            }

            Logger<NewMod>.Instance.LogMessage("Neutral assignment complete.");
            Logger<NewMod>.Instance.LogMessage("-------------- SELECT ROLES: END --------------");
            return false;
        }
        public struct Candidate
        {
            public ICustomRole Role;
            public int Left;
            public float Weight;
            public RoleTypes RoleType;
        }
    }

    // Inspired by https://github.com/AU-Avengers/TOU-Mira/blob/main/TownOfUs/Patches/RoleManagerPatches.cs#L747C3-L768C1
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    public static class RpcSetRolePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType, [HarmonyArgument(1)] bool canOverrideRole = false)
        {
            if (AmongUsClient.Instance.AmClient)
                __instance.StartCoroutine(__instance.CoSetRole(roleType, canOverrideRole));

            var writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable);
            writer.Write((ushort)roleType);
            writer.Write(canOverrideRole);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            Logger<NewMod>.Instance.LogMessage($"RpcSetRole: {__instance.Data.PlayerName} ({roleType})");
            return false;
        }
    }
}
