using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Modifiers.S1;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace NewMod.Utilities;

public static class WraithCallerUtilities
{
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
        RpcConfirmKill(PlayerControl.LocalPlayer, evt.Source.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.WraithCallerConfirmKill, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmKill(PlayerControl host, byte ownerId)
    {
        if (!host.IsHost())
            return;

        AddKillNPC(ownerId);
    }

    public static void ClearAll()
    {
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
        if (!AmongUsClient.Instance.AmHost)
            return;

        var target = Utils.PlayerById(targetId);
        if (!target || target.HasModifier<InVoid>())
            return;

        var start = source.GetTruePosition();

        RpcSummonNPC(source, target.PlayerId, start.x, start.y);
    }

    [MethodRpc((uint)CustomRPC.SummonNPC)]
    public static void RpcSummonNPC(PlayerControl source, byte targetId, float x, float y)
    {
        var target = Utils.PlayerById(targetId);
        if (!target)
            return;

        AddSentNPC(source.PlayerId);

        var npcId = GetSentNPC(source.PlayerId);
        var holder = new GameObject("WraithNPC_Holder");
        var npc = holder.AddComponent<WraithCallerNpc>();

        ActiveNpcs[((uint)source.PlayerId << 16) | (uint)npcId] = npc;

        npc.Initialize(source, target, new Vector2(x, y), npcId);
    }
}