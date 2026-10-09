using NewMod.Modifiers.S1;
using System;
using System.Linq;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using Reactor.Utilities;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewMod.Utilities;

public static class PranksterUtilities
{
    public const string PranksterBodyName = "PranksterCloneBody";
    public static readonly Dictionary<byte, int> ReportCounts = new();
    public static readonly HashSet<(byte Prankster, byte Reporter)> FooledPlayers = [];
    public static readonly Dictionary<byte, float> ReportRecovery = [];

    [MethodRpc((uint)CustomRPC.FakeBody, LocalHandling = RpcLocalHandling.After)]
    public static void CreatePranksterDeadBody(PlayerControl source, byte parentId)
    {
        if (source.Data.Role is not Prankster || source.Data.IsDead || source.Data.Disconnected || parentId != source.PlayerId || MeetingHud.Instance || ExileController.Instance || FindAllPranksterBodies().Any(body => body.ParentId == parentId)) return;
        OverclockedModifier.Pulse(source);
        var deadBody = Object.Instantiate(GameManager.Instance.GetDeadBody(source.Data.Role));
        deadBody.name = PranksterBodyName;
        deadBody.ParentId = parentId;

        foreach (var renderer in deadBody.bodyRenderers) source.SetPlayerMaterialColors(renderer);
        deadBody.transform.position = source.GetTruePosition();
    }

    public static bool IsPranksterBody(DeadBody body)
    {
        return body.name.Equals(PranksterBodyName, StringComparison.OrdinalIgnoreCase);
    }

    public static List<DeadBody> FindAllPranksterBodies()
    {
        var allDeadBodies = Object.FindObjectsOfType<DeadBody>();
        var pranksterBodies = new List<DeadBody>();

        foreach (var body in allDeadBodies)
            if (IsPranksterBody(body))
                pranksterBodies.Add(body);

        return pranksterBodies;
    }

    [RegisterEvent]
    public static void OnReportBody(ReportBodyEvent evt)
    {
        if (!evt.Reporter.AmOwner || !evt.Body || !IsPranksterBody(evt.Body))
            return;

        evt.Cancel();
        evt.Body.Reported = false;
        var position = evt.Body.TruePosition;
        RpcRequestFakeReport(evt.Reporter, evt.Body.ParentId, position.x, position.y);
    }

    [MethodRpc((uint)CustomRPC.PranksterRequestFakeReport, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRequestFakeReport(PlayerControl source, byte pranksterId, float x, float y)
    {
        if (!AmongUsClient.Instance.AmHost || !AmongUsClient.Instance.IsGameStarted || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance)
            return;

        if (Time.time < ReportRecovery.GetValueOrDefault(source.PlayerId)) return;
        var position = new Vector2(x, y);
        foreach (var body in FindAllPranksterBodies())
        {
            if (body.Reported || body.ParentId != pranksterId || Vector2.Distance(body.TruePosition, position) > 0.05f)
                continue;

            if (Vector2.Distance(source.GetTruePosition(), body.TruePosition) > source.MaxReportDistance)
                return;

            ReportRecovery[source.PlayerId] = Time.time + 2f;
            RpcConfirmFakeReport(PlayerControl.LocalPlayer, pranksterId, source.PlayerId, x, y);
            return;
        }
    }

    [MethodRpc((uint)CustomRPC.PranksterConfirmFakeReport, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmFakeReport(PlayerControl source, byte pranksterId, byte reporterId, float x, float y)
    {
        if (!source.IsHost())
            return;

        if (pranksterId != reporterId && FooledPlayers.Add((pranksterId, reporterId)))
            ReportCounts[pranksterId] = GetReportCount(pranksterId) + 1;
        if (PlayerControl.LocalPlayer.PlayerId == reporterId)
            Coroutines.Start(CoroutinesHelper.CoNotify("That body was a prank. You are unharmed."));
        var position = new Vector2(x, y);

        foreach (var body in FindAllPranksterBodies())
        {
            if (body.ParentId != pranksterId || Vector2.Distance(body.TruePosition, position) > 0.05f)
                continue;

            Object.Destroy(body.gameObject);
            return;
        }
    }

    public static void ResetReportCount()
    {
        ReportCounts.Clear();
        FooledPlayers.Clear();
        ReportRecovery.Clear();
    }

    public static int GetReportCount(byte playerId)
    {
        return ReportCounts.TryGetValue(playerId, out var value) ? value : 0;
    }

    [MethodRpc((uint)CustomRPC.PranksterInspectBody)]
    public static void RpcInspectBody(PlayerControl source, byte bodyId, float x, float y)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance) return;
        var position = new Vector2(x, y);
        foreach (var body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body.ParentId != bodyId || Vector2.Distance(body.TruePosition, position) > 0.05f || Vector2.Distance(source.GetTruePosition(), body.TruePosition) > OptionGroupSingleton<PranksterOptions>.Instance.InspectRange || PhysicsHelpers.AnythingBetween(source.GetTruePosition(), body.TruePosition, Constants.ShipAndObjectsMask, false)) continue;
            RpcConfirmInspection(PlayerControl.LocalPlayer, source.PlayerId, bodyId, x, y, IsPranksterBody(body));
            return;
        }
    }

    [MethodRpc((uint)CustomRPC.PranksterConfirmInspection)]
    public static void RpcConfirmInspection(PlayerControl source, byte inspectorId, byte bodyId, float x, float y, bool fake)
    {
        if (!source.IsHost()) return;
        if (fake)
            foreach (var body in FindAllPranksterBodies())
                if (body.ParentId == bodyId && Vector2.Distance(body.TruePosition, new Vector2(x, y)) <= 0.05f)
                {
                    Object.Destroy(body.gameObject);
                    break;
                }

        if (PlayerControl.LocalPlayer.PlayerId == inspectorId)
            Coroutines.Start(CoroutinesHelper.CoNotify(fake ? "Fake body removed. The Prankster earned nothing." : "This body is real."));
    }
}