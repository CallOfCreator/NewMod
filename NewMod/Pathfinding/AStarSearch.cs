using System;
using System.Collections.Generic;

namespace NewMod.Pathfinding;

public enum PathStatus
{
    Searching,
    Found,
    Unreachable,
    LimitReached,
    Cancelled,
    InvalidEndpoint,
    MapChanged
}

public readonly record struct PathEdge<T>(T Node, float Cost);

public sealed class AStarSearch<T> where T : notnull
{
    private readonly T goal;
    private readonly Func<T, IEnumerable<PathEdge<T>>> getEdges;
    private readonly Func<T, float> estimate;
    private readonly int nodeLimit;
    private readonly PriorityQueue<(T Node, float Cost), float> frontier = new();
    private readonly Dictionary<T, float> costs = new();
    private readonly Dictionary<T, T> parents = new();

    public PathStatus Status { get; private set; } = PathStatus.Searching;
    public IReadOnlyList<T> Path { get; private set; } = Array.Empty<T>();
    public float Cost { get; private set; } = float.PositiveInfinity;
    public int ExpandedNodes { get; private set; }

    public AStarSearch(T start, T goal, Func<T, IEnumerable<PathEdge<T>>> getEdges, Func<T, float> estimate, int nodeLimit = 30000)
    {
        if (nodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(nodeLimit));

        this.goal = goal;
        this.getEdges = getEdges;
        this.estimate = estimate;
        this.nodeLimit = nodeLimit;
        costs[start] = 0f;
        frontier.Enqueue((start, 0f), Estimate(start));
    }

    public void Step(int budget = 128)
    {
        if (budget <= 0)
            throw new ArgumentOutOfRangeException(nameof(budget));

        while (Status == PathStatus.Searching && budget-- > 0)
        {
            if (!frontier.TryDequeue(out var current, out _))
            {
                Status = PathStatus.Unreachable;
                return;
            }

            if (current.Cost != costs[current.Node])
                continue;

            if (EqualityComparer<T>.Default.Equals(current.Node, goal))
            {
                var path = new List<T> { goal };
                var node = goal;
                while (parents.TryGetValue(node, out var parent))
                {
                    path.Add(parent);
                    node = parent;
                }

                path.Reverse();
                Path = path;
                Cost = current.Cost;
                Status = PathStatus.Found;
                return;
            }

            if (ExpandedNodes >= nodeLimit)
            {
                Status = PathStatus.LimitReached;
                return;
            }

            ExpandedNodes++;
            foreach (var edge in getEdges(current.Node))
            {
                if (!float.IsFinite(edge.Cost) || edge.Cost < 0f)
                    throw new ArgumentOutOfRangeException(nameof(edge.Cost));

                var cost = current.Cost + edge.Cost;
                if (costs.TryGetValue(edge.Node, out var previous) && previous <= cost)
                    continue;

                var score = cost + Estimate(edge.Node);
                if (!float.IsFinite(score))
                    throw new ArgumentOutOfRangeException(nameof(edge.Cost));

                costs[edge.Node] = cost;
                parents[edge.Node] = current.Node;
                frontier.Enqueue((edge.Node, cost), score);
            }
        }
    }

    public void Cancel()
    {
        if (Status == PathStatus.Searching)
            Status = PathStatus.Cancelled;
    }

    public float Estimate(T node)
    {
        var value = estimate(node);
        if (!float.IsFinite(value) || value < 0f)
            throw new ArgumentOutOfRangeException(nameof(estimate));
        return value;
    }
}