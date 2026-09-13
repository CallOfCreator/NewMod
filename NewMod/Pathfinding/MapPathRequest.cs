using System;
using System.Collections;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace NewMod.Pathfinding;

public sealed class MapPathRequest
{
    public readonly List<PathLink> links = new();
    public readonly List<Component> linkSources = new();
    public readonly Il2CppStructArray<RaycastHit2D> crossingHits = new(32);
    public readonly PlayerControl actor;
    public readonly ShipStatus ship;
    public readonly PathOptions options;
    public readonly OpenableDoor[] doors;
    public readonly bool[] doorStates;
    public readonly ContactFilter2D filter;
    public readonly Il2CppStructArray<RaycastHit2D> hits = new(1);
    public readonly Il2CppReferenceArray<Collider2D> overlaps = new(1);
    public readonly WalkingGrid grid;
    public readonly AStarSearch<int> search;

    public MapPath Result { get; internal set; }
    public PathStatus Status => Result?.Status ?? search.Status;
    public int ExpandedNodes => search?.ExpandedNodes ?? 0;

    public MapPathRequest(ShipStatus ship, Vector2 start, Vector2 goal, PathOptions options, PlayerControl actor = null)
    {
        if (!float.IsFinite(options.CellSize) || options.CellSize <= 0f || !float.IsFinite(options.Radius) || options.Radius <= 0f || !float.IsFinite(options.ConnectionRange) || options.ConnectionRange < options.CellSize || options.NodeLimit <= 0 || options.BatchSize <= 0 || !float.IsFinite(options.WallWeight) || options.WallWeight < 0f || !float.IsFinite(options.WallDistance) || options.WallDistance <= 0f)
            throw new ArgumentOutOfRangeException(nameof(options));

        this.actor = actor;
        this.ship = ship;
        this.options = options;
        filter = new ContactFilter2D { useLayerMask = true, layerMask = Constants.ShipAndAllObjectsMask, useTriggers = false };
        if (!ship || !float.IsFinite(start.x) || !float.IsFinite(start.y) || !float.IsFinite(goal.x) || !float.IsFinite(goal.y))
        {
            Finish(PathStatus.InvalidEndpoint);
            return;
        }

        doors = ship.AllDoors;
        doorStates = new bool[doors.Length];
        for (var i = 0; i < doors.Length; i++)
            doorStates[i] = doors[i] && doors[i].IsOpen;

        var bounds = new Bounds();
        var hasBounds = false;
        foreach (var room in ship.AllRooms)
        {
            if (!room.roomArea)
                continue;
            if (hasBounds)
                bounds.Encapsulate(room.roomArea.bounds);
            else
                bounds = room.roomArea.bounds;
            hasBounds = true;
        }

        bounds.Expand(options.CellSize * 2f);
        var area = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        if (!hasBounds || !area.Contains(start) || !area.Contains(goal) || !IsClear(start) || !IsClear(goal))
        {
            Finish(PathStatus.InvalidEndpoint);
            return;
        }

        CollectLinks();
        grid = new WalkingGrid(start, goal, area, options.CellSize, options.ConnectionRange, IsClear, CanMove, links, options.WallWeight > 0f ? WallPenalty : null);
        search = new AStarSearch<int>(WalkingGrid.StartNode, WalkingGrid.GoalNode, grid.Edges, grid.Estimate, options.NodeLimit);
    }

    public IEnumerator Run()
    {
        while (Status == PathStatus.Searching)
        {
            Step();
            if (Status == PathStatus.Searching)
                yield return null;
        }
    }

    public void Step()
    {
        if (Result != null)
            return;
        if (MapChanged())
        {
            Finish(PathStatus.MapChanged);
            return;
        }

        search.Step(options.BatchSize);
        if (search.Status == PathStatus.Found)
            BuildPath();
        else if (search.Status != PathStatus.Searching)
            Finish(search.Status);
    }

    public void Cancel()
    {
        if (Result == null)
        {
            search.Cancel();
            Finish(PathStatus.Cancelled);
        }
    }

    public bool IsClear(Vector2 point)
    {
        return Physics2D.OverlapCircle(point, options.Radius, filter, overlaps) == 0;
    }

    public float WallPenalty(Vector2 point)
    {
        for (var i = 1; i <= 3; i++)
            if (Physics2D.OverlapCircle(point, options.Radius + options.WallDistance * i / 3f, filter, overlaps) > 0)
                return options.WallWeight * (4 - i) / 3f;
        return 0f;
    }

    public bool CanMove(Vector2 from, Vector2 to)
    {
        var delta = to - from;
        return IsClear(from) && IsClear(to) && (delta.sqrMagnitude < 0.000001f || Physics2D.CircleCast(from, options.Radius, delta.normalized, filter, hits, delta.magnitude) == 0);
    }

    public bool MapChanged()
    {
        if (!ship || ship != ShipStatus.Instance)
            return true;
        for (var i = 0; i < doors.Length; i++)
            if (!doors[i] || doors[i].IsOpen != doorStates[i])
                return true;
        return false;
    }

    public void CollectLinks()
    {
        if (options.UseLadders)
            foreach (var ladder in ship.GetComponentsInChildren<Ladder>())
                if (ladder.isActiveAndEnabled && ladder.Destination && ladder.Destination.isActiveAndEnabled)
                    AddTransportLink(ladder.transform.position, ladder.Destination.transform.position, ladder.UsableDistance, ladder.Destination.UsableDistance, PathTraversal.Ladder, ladder);

        if (options.UseZiplines)
            foreach (var console in ship.GetComponentsInChildren<ZiplineConsole>())
            {
                if (!console.isActiveAndEnabled || !console.zipline || !console.zipline.isActiveAndEnabled || !console.destination || !console.destination.isActiveAndEnabled)
                    continue;
                var zipline = console.zipline;
                var landing = console.atTop ? zipline.landingPositionBottom : zipline.landingPositionTop;
                AddTransportLink(console.transform.position, zipline.transform.TransformPoint(landing.position), console.UsableDistance, console.destination.UsableDistance, PathTraversal.Zipline, console);
            }

        if (options.UseVents)
            foreach (var vent in ship.AllVents)
            {
                if (!VentAvailable(vent)) continue;
                foreach (var next in new[] { vent.Left, vent.Right, vent.Center })
                    if (VentAvailable(next))
                        AddTransportLink(vent.transform.position + vent.Offset, next.transform.position + next.Offset, vent.UsableDistance, next.UsableDistance, PathTraversal.Vent, vent);
            }

        if (options.UseMovingPlatforms)
            foreach (var platform in ship.GetComponentsInChildren<MovingPlatformBehaviour>())
            {
                if (!platform.isActiveAndEnabled || !platform.transform.parent || (GameOptionsManager.Instance.CurrentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek && platform.DisabledPosition != Vector3.zero))
                    continue;
                var parent = platform.transform.parent;
                var left = (Vector2)parent.TransformPoint(platform.LeftUsePosition);
                var right = (Vector2)parent.TransformPoint(platform.RightUsePosition);
                AddTransportLink(left, right, options.ConnectionRange, options.ConnectionRange, PathTraversal.MovingPlatform, platform);
                AddTransportLink(right, left, options.ConnectionRange, options.ConnectionRange, PathTraversal.MovingPlatform, platform);
            }

        if (options.WaitForDoors || options.AutoOpenDoors)
            foreach (var door in doors)
                AddDoorLink(door, PathTraversal.Door);

        if (options.UseDecontamination)
            foreach (var decon in ship.GetComponentsInChildren<DeconSystem>())
                if (decon.isActiveAndEnabled)
                {
                    AddDoorLink(decon.UpperDoor);
                    AddDoorLink(decon.LowerDoor);
                }
    }

    public bool VentAvailable(Vent vent)
    {
        if (!vent || !vent.isActiveAndEnabled) return false;
        if (actor && (actor.Data == null || actor.Data.IsDead || !actor.Data.Role.CanVent || !actor.Data.Role.CanUse(vent.Cast<IUsable>()) || actor.MustCleanVent(vent.Id))) return false;
        if (ship.Systems.TryGetValue(SystemTypes.Ventilation, out var system))
        {
            var ventilation = system.TryCast<VentilationSystem>();
            if (ventilation != null && ventilation.IsVentCurrentlyBeingCleaned(vent.Id)) return false;
        }

        return true;
    }

    public void AddTransportLink(Vector2 from, Vector2 to, float entryRange, float exitRange, PathTraversal type, Component source)
    {
        if (!WalkingGrid.TryApproach(from, from - to, entryRange, IsClear, CanReachInteraction, out var entry) || !WalkingGrid.TryApproach(to, to - from, exitRange, IsClear, CanReachInteraction, out var exit))
            return;
        AddLink(entry, exit, type, source);
    }

    public bool CanReachInteraction(Vector2 from, Vector2 to)
    {
        return !PhysicsHelpers.AnythingBetween(from, to, Constants.ShipOnlyMask, false);
    }

    public void AddLink(Vector2 from, Vector2 to, PathTraversal type, Component source)
    {
        if (!IsClear(from) || !IsClear(to))
            return;
        links.Add(new PathLink(from, to, type));
        linkSources.Add(source);
    }

    public void AddDoorLink(SomeKindaDoor door, PathTraversal type = PathTraversal.Decontamination)
    {
        if (!door || !door.isActiveAndEnabled)
            return;
        var manual = door.TryCast<ManualDoor>();
        var plain = door.TryCast<PlainDoor>();
        var collider = manual ? manual.myCollider : plain ? plain.myCollider : null;
        if (!collider)
            return;
        var bounds = collider.bounds;
        var margin = options.Radius + options.CellSize;
        var offset = bounds.size.x > bounds.size.y ? new Vector2(0f, bounds.extents.y + margin) : new Vector2(bounds.extents.x + margin, 0f);
        var from = (Vector2)bounds.center - offset;
        var to = (Vector2)bounds.center + offset;
        if (!CanCrossDoor(from, to, door))
            return;
        AddLink(from, to, type, door);
        AddLink(to, from, type, door);
    }

    public bool CanCrossDoor(Vector2 from, Vector2 to, SomeKindaDoor door)
    {
        if (!IsClear(from) || !IsClear(to))
            return false;
        var manual = door.TryCast<ManualDoor>();
        var plain = door.TryCast<PlainDoor>();
        var collider = manual ? manual.myCollider : plain ? plain.myCollider : null;
        var shadow = manual ? manual.shadowCollider : plain ? plain.shadowCollider : null;
        var delta = to - from;
        var count = Physics2D.CircleCast(from, options.Radius, delta.normalized, filter, crossingHits, delta.magnitude);
        if (count == crossingHits.Length)
            return false;
        for (var i = 0; i < count; i++)
            if (crossingHits[i].collider != collider && crossingHits[i].collider != shadow)
                return false;
        return true;
    }

    public bool CanTraverse(int index)
    {
        var source = linkSources[index];
        if (!source || !source.Cast<Behaviour>().isActiveAndEnabled)
            return false;
        var link = links[index];
        return link.Type is PathTraversal.Decontamination or PathTraversal.Door ? CanCrossDoor(link.Start, link.End, source.Cast<SomeKindaDoor>()) : IsClear(link.Start) && IsClear(link.End);
    }

    public void BuildPath()
    {
        if (!grid.SmoothPath(search.Path, CanTraverse, out var nodes))
        {
            Finish(PathStatus.MapChanged);
            return;
        }

        var points = new List<Vector2>();
        var crossings = new List<PathCrossing>();
        for (var i = 0; i < nodes.Count; i++)
        {
            points.Add(grid.Position(nodes[i]));
            if (i == 0)
                continue;
            var link = grid.LinkIndex(nodes[i - 1], nodes[i]);
            if (link >= 0)
                crossings.Add(new PathCrossing(i - 1, links[link].Type, linkSources[link]));
        }

        var length = 0f;
        for (var i = 1; i < points.Count; i++)
            length += Vector2.Distance(points[i - 1], points[i]);
        Result = new MapPath(PathStatus.Found, points.ToArray(), length, ExpandedNodes) { Crossings = crossings.ToArray(), AutoOpenDoors = options.AutoOpenDoors };
    }

    public void Finish(PathStatus status)
    {
        Result = new MapPath(status, Array.Empty<Vector2>(), 0f, ExpandedNodes);
    }
}