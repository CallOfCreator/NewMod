using System;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewMod.Utilities;

public static class PranksterUtilities
{
    private const string PranksterBodyName = "PranksterCloneBody";
    private static readonly Dictionary<byte, int> ReportCounts = new();

    [MethodRpc((uint)CustomRPC.FakeBody, LocalHandling = RpcLocalHandling.After)]
    public static void CreatePranksterDeadBody(PlayerControl player, byte parentId)
    {
        var deadBody = Object.Instantiate(GameManager.Instance.GetDeadBody(player.Data.Role));
        deadBody.name = PranksterBodyName;
        deadBody.ParentId = parentId;

        foreach (var renderer in deadBody.bodyRenderers) player.SetPlayerMaterialColors(renderer);
        deadBody.transform.position = player.GetTruePosition();
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
        var position = evt.Body.TruePosition;
        RpcRequestFakeReport(evt.Reporter, evt.Body.ParentId, position.x, position.y);
    }

    [MethodRpc((uint)CustomRPC.PranksterRequestFakeReport, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRequestFakeReport(PlayerControl source, byte pranksterId, float x, float y)
    {
        if (!AmongUsClient.Instance.AmHost || !AmongUsClient.Instance.IsGameStarted || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance)
            return;

        var position = new Vector2(x, y);
        foreach (var body in FindAllPranksterBodies())
        {
            if (body.Reported || body.ParentId != pranksterId || Vector2.Distance(body.TruePosition, position) > 0.05f)
                continue;

            if (Vector2.Distance(source.GetTruePosition(), body.TruePosition) > source.MaxReportDistance)
                return;

            source.RpcCustomMurder(source, teleportMurderer: false, showKillAnim: true);
            RpcConfirmFakeReport(PlayerControl.LocalPlayer, pranksterId, x, y);
            return;
        }
    }

    [MethodRpc((uint)CustomRPC.PranksterConfirmFakeReport, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmFakeReport(PlayerControl host, byte pranksterId, float x, float y)
    {
        if (!host.IsHost())
            return;

        ReportCounts[pranksterId] = GetReportCount(pranksterId) + 1;
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
    }

    public static int GetReportCount(byte playerId)
    {
        return ReportCounts.TryGetValue(playerId, out var value) ? value : 0;
    }
}