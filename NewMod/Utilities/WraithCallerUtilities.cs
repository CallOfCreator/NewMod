using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using Reactor.Utilities;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Modifiers.S1;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace NewMod.Utilities;

public static class WraithCallerUtilities
{
    public static readonly HashSet<byte> Traces = [];
    public static readonly HashSet<(byte Caller, byte Body)> CollectedTraces = [];
    public static readonly Dictionary<byte, float> NextSummon = [];
    public static readonly Dictionary<byte, int> Sent = [];
    public static readonly Dictionary<byte, int> Kills = [];
    public static readonly Dictionary<uint, WraithCallerNpc> ActiveNpcs = [];

    public static int GetSentNPC(byte ownerId)
    {
        return Sent.TryGetValue(ownerId, out var value) ? value : 0;
    }

    public static int GetKillsNPC(byte ownerId)
    {
        return Kills.TryGetValue(ownerId, out var value) ? value : 0;
    }

    public static void AddSentNPC(byte ownerId, int amount = 1)
    {
        Sent[ownerId] = GetSentNPC(ownerId) + amount;
    }

    public static void AddKillNPC(byte ownerId, int amount = 1)
    {
        Kills[ownerId] = GetKillsNPC(ownerId) + amount;
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || !evt.Source || !evt.Target || !evt.Target.Data || !evt.Target.Data.IsDead)
            return;

        var npc = ActiveNpcs.Values.FirstOrDefault(npc => npc && npc.isActive && npc.ResolvingKill && !npc.Reflected && npc.Owner == evt.Source && npc.Target == evt.Target);

        if (!npc)
            return;

        npc.ResolvingKill = false;
        npc.HuntSucceeded = true;
        RpcConfirmKill(PlayerControl.LocalPlayer, evt.Source.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.WraithCallerConfirmKill, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmKill(PlayerControl source, byte ownerId)
    {
        if (!source.IsHost())
            return;

        AddKillNPC(ownerId);
    }

    public static void ClearAll()
    {
        foreach (var npc in ActiveNpcs.Values.ToArray())
            if (npc)
            {
                npc.HuntSucceeded = true;
                npc.Dispose();
            }
        Traces.Clear();
        CollectedTraces.Clear();
        NextSummon.Clear();
        Sent.Clear();
        Kills.Clear();
        ActiveNpcs.Clear();
    }

    public static void RequestSummonNPC(PlayerControl owner, PlayerControl target)
    {
        RpcRequestSummonNPC(owner, target.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.RequestSummon)]
    public static void RpcRequestSummonNPC(PlayerControl source, byte targetId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not WraithCaller || source.Data.IsDead || source.Data.Disconnected ||
            MeetingHud.Instance || ExileController.Instance || !Traces.Contains(source.PlayerId) ||
            NextSummon.GetValueOrDefault(source.PlayerId) > Time.time ||
            ActiveNpcs.Values.Any(npc => npc && npc.isActive && npc.Owner == source))
            return;

        var target = Utils.PlayerById(targetId);
        if (!target || target == source || target.Data.IsDead || target.Data.Disconnected || target.HasModifier<InVoid>())
            return;

        var start = source.GetTruePosition();

        NextSummon[source.PlayerId] = Time.time + OptionGroupSingleton<WraithCallerOptions>.Instance.CallWraithCooldown;
        RpcSummonNPC(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId, start.x, start.y);
    }

    [MethodRpc((uint)CustomRPC.SummonNPC)]
    public static void RpcSummonNPC(PlayerControl source, byte ownerId, byte targetId, float x, float y)
    {
        if (!source.IsHost()) return;
        var owner = Utils.PlayerById(ownerId);
        var target = Utils.PlayerById(targetId);
        if (!owner || !target)
            return;

        Traces.Remove(ownerId);
        AddSentNPC(ownerId);

        var npcId = GetSentNPC(ownerId);
        var holder = new GameObject("WraithNPC_Holder");
        var npc = holder.AddComponent<WraithCallerNpc>();

        ActiveNpcs[((uint)ownerId << 16) | (uint)npcId] = npc;

        npc.Initialize(owner, target, new Vector2(x, y), npcId);
    }

    [MethodRpc((uint)CustomRPC.WraithCollectTrace)]
    public static void RpcCollectTrace(PlayerControl source, byte bodyId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not WraithCaller || source.Data.IsDead || source.Data.Disconnected ||
            MeetingHud.Instance || ExileController.Instance || Traces.Contains(source.PlayerId) ||
            CollectedTraces.Contains((source.PlayerId, bodyId)) ||
            ActiveNpcs.Values.Any(npc => npc && npc.isActive && npc.Owner == source))
            return;

        var body = Helpers.GetBodyById(bodyId);
        if (!body || body.Reported || PranksterUtilities.IsPranksterBody(body) ||
            Vector2.Distance(source.GetTruePosition(), body.TruePosition) > OptionGroupSingleton<WraithCallerOptions>.Instance.TraceRange ||
            PhysicsHelpers.AnythingBetween(source.GetTruePosition(), body.TruePosition, Constants.ShipAndObjectsMask, false))
            return;

        RpcConfirmTrace(PlayerControl.LocalPlayer, source.PlayerId, bodyId);
    }

    [MethodRpc((uint)CustomRPC.WraithConfirmTrace)]
    public static void RpcConfirmTrace(PlayerControl source, byte ownerId, byte bodyId)
    {
        if (!source.IsHost()) return;
        CollectedTraces.Add((ownerId, bodyId));
        Traces.Add(ownerId);
        if (PlayerControl.LocalPlayer.PlayerId == ownerId)
            Coroutines.Start(CoroutinesHelper.CoNotify("Trace collected. Choose a target to send your Wraith."));
    }

    [MethodRpc((uint)CustomRPC.WraithRestoreTrace)]
    public static void RpcRestoreTrace(PlayerControl source, byte ownerId)
    {
        if (source.IsHost()) Traces.Add(ownerId);
    }

}