using System;
using System.Collections;
using UnityEngine;

namespace NewMod.Pathfinding;

public sealed record PathOptions(float CellSize = 0.35f, float Radius = 0.18f, float ConnectionRange = 1f, int NodeLimit = 30000, int BatchSize = 96, bool UseLadders = true, bool UseZiplines = true, bool UseDecontamination = true, bool UseMovingPlatforms = true, bool WaitForDoors = false, bool UseVents = false, float WallWeight = 1.5f, float WallDistance = 0.6f, bool AutoOpenDoors = false); //default values :)

public sealed record PathCrossing(int PointIndex, PathTraversal Type, Component Source);

public sealed record MapPath(PathStatus Status, Vector2[] Points, float Length, int ExpandedNodes)
{
    public bool AutoOpenDoors { get; init; }
    public PathCrossing[] Crossings { get; init; } = Array.Empty<PathCrossing>();
}

public static class MapPathfinding
{
    public static MapPathRequest CreateRequest(PlayerControl player, Vector2 goal, PathOptions options = null)
    {
        var canVent = player.Data != null && !player.Data.IsDead && player.Data.Role.CanVent;
        options ??= new PathOptions(UseVents: canVent);
        return new MapPathRequest(ShipStatus.Instance, player.GetTruePosition(), goal, options with { UseVents = options.UseVents && canVent }, player);
    }

    public static IEnumerator FindPath(PlayerControl player, Vector2 goal, Action<MapPath> completed, PathOptions options = null)
    {
        var request = CreateRequest(player, goal, options);
        yield return request.Run();
        completed(request.Result);
    }

    public static IEnumerator Traverse(PlayerControl player, MapPath path, PathCrossing crossing, Action<bool> completed, float timeout = 30f)
    {
        return PathTraversalActions.Traverse(player, path, crossing, completed, timeout);
    }

    public static IEnumerator FindPath(Vector2 start, Vector2 goal, Action<MapPath> completed, PathOptions options = null)
    {
        var request = CreateRequest(start, goal, options);
        yield return request.Run();
        completed(request.Result);
    }

    public static MapPathRequest CreateRequest(Vector2 start, Vector2 goal, PathOptions options = null)
    {
        return new MapPathRequest(ShipStatus.Instance, start, goal, options ?? new PathOptions());
    }
}