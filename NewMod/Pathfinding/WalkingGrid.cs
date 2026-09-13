using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewMod.Pathfinding;

public enum PathTraversal
{
    Ladder,
    Zipline,
    Decontamination,
    Door,
    MovingPlatform,
    Vent
}

public sealed record PathLink(Vector2 Start, Vector2 End, PathTraversal Type);

// Inspired by Nebula's endpoint connections and collision navigation
public sealed class WalkingGrid
{
    public const int StartNode = -1;
    public const int GoalNode = -2;
    public readonly Func<Vector2, float> penalty;
    public readonly Dictionary<Vector2, float> penalties = new();
    public readonly IReadOnlyList<PathLink> links;
    public readonly Vector2 start;
    public readonly Vector2 goal;
    public readonly Rect bounds;
    public readonly float spacing;
    public readonly float connectionRange;
    public readonly int width;
    public readonly int height;
    public readonly Func<Vector2, bool> isClear;
    public readonly Func<Vector2, Vector2, bool> canMove;
    public readonly Dictionary<int, bool> clearance = new();
    public readonly Dictionary<(int, int), bool> connections = new();

    public WalkingGrid(Vector2 start, Vector2 goal, Rect bounds, float spacing, float connectionRange, Func<Vector2, bool> isClear, Func<Vector2, Vector2, bool> canMove, IReadOnlyList<PathLink> links = null!, Func<Vector2, float> penalty = null!)
    {
        this.penalty = penalty;
        this.links = links ?? Array.Empty<PathLink>();
        this.start = start;
        this.goal = goal;
        this.bounds = bounds;
        this.spacing = spacing;
        this.connectionRange = connectionRange;
        this.isClear = isClear;
        this.canMove = canMove;
        width = Mathf.CeilToInt(bounds.width / spacing) + 1;
        height = Mathf.CeilToInt(bounds.height / spacing) + 1;
    }

    public Vector2 Position(int node)
    {
        return node switch
        {
            StartNode => start,
            GoalNode => goal,
            < GoalNode => (-node - 3) % 2 == 0 ? links[(-node - 3) / 2].Start : links[(-node - 3) / 2].End,
            _ => bounds.min + new Vector2(node % width * spacing, node / width * spacing)
        };
    }

    public float Estimate(int node)
    {
        return Vector2.Distance(Position(node), goal);
    }

    public IEnumerable<PathEdge<int>> Edges(int node)
    {
        if (node == GoalNode)
            yield break;

        var position = Position(node);
        if ((node == StartNode || Vector2.Distance(position, goal) <= connectionRange) && Connected(node, GoalNode))
            yield return new PathEdge<int>(GoalNode, WalkingCost(position, goal));

        if (node < GoalNode && (-node - 3) % 2 == 0)
            yield return new PathEdge<int>(node - 1, Vector2.Distance(position, Position(node - 1)));

        for (var i = 0; i < links.Count; i++)
        {
            var entry = -3 - i * 2;
            if (entry != node && Vector2.Distance(position, links[i].Start) <= connectionRange && Connected(node, entry))
                yield return new PathEdge<int>(entry, WalkingCost(position, links[i].Start));
        }

        var x = node < 0 ? Mathf.RoundToInt((position.x - bounds.xMin) / spacing) : node % width;
        var y = node < 0 ? Mathf.RoundToInt((position.y - bounds.yMin) / spacing) : node / width;
        var range = node < 0 ? Mathf.CeilToInt(connectionRange / spacing) : 1;

        for (var dy = -range; dy <= range; dy++)
        for (var dx = -range; dx <= range; dx++)
        {
            var nx = x + dx;
            var ny = y + dy;
            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                continue;

            var next = ny * width + nx;
            if (next == node || !IsClear(next))
                continue;

            var distance = Vector2.Distance(position, Position(next));
            if (node < 0 && distance > connectionRange)
                continue;

            if (Connected(node, next))
                yield return new PathEdge<int>(next, WalkingCost(position, Position(next)));
        }
    }

    public float WalkingCost(Vector2 from, Vector2 to)
    {
        var distance = Vector2.Distance(from, to);
        if (penalty == null) return distance;
        var samples = Math.Max(1, Mathf.CeilToInt(distance / spacing));
        var sum = 0f;
        for (var i = 0; i <= samples; i++)
        {
            var t = (float)i / samples;
            var point = new Vector2(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t);
            if (!penalties.TryGetValue(point, out var value))
            {
                value = Math.Max(0f, penalty(point));
                penalties[point] = value;
            }

            sum += value * (i == 0 || i == samples ? 0.5f : 1f);
        }

        return distance * (1f + sum / samples);
    }

    public static bool TryApproach(Vector2 anchor, Vector2 away, float range, Func<Vector2, bool> isClear, Func<Vector2, Vector2, bool> visible, out Vector2 point)
    {
        point = anchor;
        if (isClear(anchor))
            return true;
        var direction = MathF.Atan2(away.y, away.x);
        for (var ring = 1; ring <= 10; ring++)
        for (var sample = 0; sample <= 16; sample++)
        {
            var turn = (sample + 1) / 2 * (sample % 2 == 0 ? -1 : 1);
            var angle = direction + turn * MathF.PI / 16f;
            var radius = range * 0.95f * ring / 10f;
            var candidate = anchor + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
            if (!isClear(candidate) || !visible(candidate, anchor))
                continue;
            point = candidate;
            return true;
        }

        return false;
    }

    public bool SmoothPath(IReadOnlyList<int> path, Func<int, bool> canTraverse, out List<int> result)
    {
        result = new List<int> { path[0] };
        var accumulated = 0f;
        for (var i = 1; i < path.Count; i++)
        {
            var previous = path[i - 1];
            var next = path[i];
            var link = LinkIndex(previous, next);
            if (link >= 0)
            {
                if (!canMove(Position(result[^1]), Position(previous)) || !canTraverse(link))
                    return false;
                if (result[^1] != previous)
                    result.Add(previous);
                result.Add(next);
                accumulated = 0f;
                continue;
            }

            var edgeCost = WalkingCost(Position(previous), Position(next));
            accumulated += edgeCost;
            if (canMove(Position(result[^1]), Position(next)) && WalkingCost(Position(result[^1]), Position(next)) <= accumulated + 0.001f)
                continue;
            if (!canMove(Position(result[^1]), Position(previous)) || !canMove(Position(previous), Position(next)))
                return false;
            result.Add(previous);
            accumulated = edgeCost;
        }

        if (result[^1] != path[^1])
            result.Add(path[^1]);
        return true;
    }

    public int LinkIndex(int from, int to)
    {
        return from < GoalNode && (-from - 3) % 2 == 0 && to == from - 1 ? (-from - 3) / 2 : -1;
    }

    public bool IsClear(int node)
    {
        if (clearance.TryGetValue(node, out var clear))
            return clear;
        var point = Position(node);
        clear = bounds.Contains(point) && isClear(point);
        clearance[node] = clear;
        return clear;
    }

    public bool Connected(int from, int to)
    {
        var key = from < to ? (from, to) : (to, from);
        if (connections.TryGetValue(key, out var connected))
            return connected;
        connected = canMove(Position(from), Position(to));
        connections[key] = connected;
        return connected;
    }
}