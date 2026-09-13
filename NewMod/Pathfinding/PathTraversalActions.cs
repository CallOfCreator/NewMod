using System;
using System.Collections;
using UnityEngine;
using Reactor.Networking.Attributes;

namespace NewMod.Pathfinding;

public static class PathTraversalActions
{
    [MethodRpc((uint)CustomRPC.PathfindingOpenDoor)]
    public static void RpcOpenDoor(PlayerControl source, int doorId)
    {
        if (!AmongUsClient.Instance.AmHost || !ShipStatus.Instance || MeetingHud.Instance || source.Data.IsDead || source.Data.Disconnected) return;
        var ship = ShipStatus.Instance;
        if (!ship.Systems.TryGetValue(SystemTypes.Doors, out var system)) return;
        for (var i = 0; i < ship.AllDoors.Length; i++)
        {
            var door = ship.AllDoors[i];
            if (!door || door.Id != doorId || door.IsOpen || !door.isActiveAndEnabled) continue;
            var manual = door.TryCast<ManualDoor>();
            var plain = door.TryCast<PlainDoor>();
            var collider = manual ? manual.myCollider : plain ? plain.myCollider : null;
            if (!collider || Vector2.Distance(collider.ClosestPoint(source.GetTruePosition()), source.GetTruePosition()) > 1.5f)
                return;
            var automatic = system.TryCast<AutoDoorsSystemType>();
            if (automatic != null)
            {
                door.SetDoorway(true);
                automatic.dirtyBits |= 1U << i;
            }
            else if (system.TryCast<DoorsSystemType>() != null && door.Id >= 0 && door.Id <= 31)
            {
                ship.UpdateSystem(SystemTypes.Doors, source, (byte)(door.Id | 64));
            }

            return;
        }
    }

    public static IEnumerator Traverse(PlayerControl player, MapPath path, PathCrossing crossing, Action<bool> completed, float timeout = 30f)
    {
        if (!player || player.Data.IsDead || !crossing.Source || (!player.AmOwner && !AmongUsClient.Instance.AmHost) || crossing.PointIndex < 0 || crossing.PointIndex + 1 >= path.Points.Length || Vector2.Distance(player.GetTruePosition(), path.Points[crossing.PointIndex]) > 0.75f)
        {
            completed(false);
            yield break;
        }

        var elapsed = 0f;
        if (crossing.Type == PathTraversal.Door && path.AutoOpenDoors)
            RpcOpenDoor(player, crossing.Source.Cast<OpenableDoor>().Id);
        if (crossing.Type == PathTraversal.MovingPlatform)
        {
            var platform = crossing.Source.Cast<MovingPlatformBehaviour>();
            while (platform && platform.InUse && elapsed < timeout && !MeetingHud.Instance)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!platform || platform.InUse || MeetingHud.Instance || Vector2.Distance(platform.transform.position, player.transform.position) > 3f)
            {
                completed(false);
                yield break;
            }

            var allowed = false;
            foreach (var console in ShipStatus.Instance.GetComponentsInChildren<PlatformConsole>())
                if (console.Platform == platform)
                {
                    console.CanUse(player.Data, out var canUse, out _);
                    allowed |= canUse;
                }

            if (!allowed)
            {
                completed(false);
                yield break;
            }

            player.RpcUsePlatform();
        }
        else if (crossing.Type == PathTraversal.Ladder)
        {
            var ladder = crossing.Source.Cast<Ladder>();
            ladder.CanUse(player.Data, out var allowed, out _);
            if (!allowed || ladder.IsCoolingDown())
            {
                completed(false);
                yield break;
            }

            player.MyPhysics.RpcClimbLadder(ladder);
        }
        else if (crossing.Type == PathTraversal.Zipline)
        {
            var console = crossing.Source.Cast<ZiplineConsole>();
            console.CanUse(player.Data, out var allowed, out _);
            if (!allowed || console.IsCoolingDown())
            {
                completed(false);
                yield break;
            }

            player.CmdCheckUseZipline(player, console.zipline, console.atTop);
        }
        else if (crossing.Type == PathTraversal.Vent)
        {
            var vent = crossing.Source.Cast<Vent>();
            var request = MapPathfinding.CreateRequest(player, path.Points[crossing.PointIndex + 1]);
            Vent exit = null;
            foreach (var neighbour in new[] { vent.Left, vent.Right, vent.Center })
                if (request.VentAvailable(neighbour) && Vector2.Distance(neighbour.transform.position + neighbour.Offset, path.Points[crossing.PointIndex + 1]) <= neighbour.UsableDistance)
                    exit = neighbour;
            request.Cancel();
            vent.CanUse(player.Data, out var allowed, out _);
            if (!allowed || !exit)
            {
                completed(false);
                yield break;
            }

            player.MyPhysics.RpcEnterVent(vent.Id);
            do
            {
                yield return null;
                elapsed += Time.deltaTime;
            } while (player && !MeetingHud.Instance && (!player.inVent || player.walkingToVent) && elapsed < timeout);

            if (!player || !player.inVent || MeetingHud.Instance || elapsed >= timeout)
            {
                completed(false);
                yield break;
            }

            player.MyPhysics.RpcExitVent(exit.Id);
        }
        else
        {
            var request = MapPathfinding.CreateRequest(player.GetTruePosition(), path.Points[crossing.PointIndex + 1]);
            while (player && !MeetingHud.Instance && elapsed < timeout && !request.CanMove(player.GetTruePosition(), path.Points[crossing.PointIndex + 1]))
            {
                if (crossing.Type == PathTraversal.Decontamination)
                    foreach (var system in ShipStatus.Instance.GetComponentsInChildren<DeconSystem>())
                        if (system.CurState == DeconSystem.States.Idle && (system.UpperDoor == crossing.Source || system.LowerDoor == crossing.Source))
                        {
                            var upper = system.UpperDoor == crossing.Source;
                            if (system.RoomArea.OverlapPoint(player.GetTruePosition())) system.OpenFromInside(upper);
                            else system.OpenDoor(upper);
                        }

                elapsed += Time.deltaTime;
                yield return null;
            }

            request.Cancel();
            completed(player && !MeetingHud.Instance && elapsed < timeout);
            yield break;
        }

        do
        {
            yield return null;
            elapsed += Time.deltaTime;
        } while (player && !MeetingHud.Instance && elapsed < timeout && (player.onLadder || player.inMovingPlat || player.inVent || !player.moveable || Vector2.Distance(player.GetTruePosition(), path.Points[crossing.PointIndex + 1]) > 1f));

        completed(player && !MeetingHud.Instance && elapsed < timeout);
    }
}