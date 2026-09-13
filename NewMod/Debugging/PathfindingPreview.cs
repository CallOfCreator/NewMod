using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using NewMod.Pathfinding;
using NewMod.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Debugging;

[RegisterInIl2Cpp]
public sealed class PathfindingPreview(nint ptr) : MonoBehaviour(ptr)
{
    public readonly List<LineRenderer> traversalLines = new();
    public string traversalReport = "";
    public PathfindingNpc npc;
    public Vector2 start;
    public Vector2 goal;
    public bool hasStart;
    public bool hasGoal;
    public LineRenderer route;
    public LineRenderer startMarker;
    public LineRenderer goalMarker;
    public MapPathRequest request;
    public string message = "Mark a start, move to a destination, then mark the goal.";

    public static PathfindingPreview Current => ShipStatus.Instance ? ShipStatus.Instance.GetComponent<PathfindingPreview>() : null;
    public string StatusText => request?.Status == PathStatus.Searching ? $"Searching: {request.ExpandedNodes} nodes" : message;
    public string TraversalReport => traversalReport;
    public string StartText => hasStart ? $"Start: {start.x:0.00}, {start.y:0.00}" : "Start: not set";
    public string GoalText => hasGoal ? $"Goal: {goal.x:0.00}, {goal.y:0.00}" : "Goal: not set";

    public static void MarkStart()
    {
        var preview = GetOrCreate();
        preview.StopSearch();
        preview.start = PlayerControl.LocalPlayer.GetTruePosition();
        preview.hasStart = true;
        DrawMarker(preview.startMarker, preview.start);
        preview.message = "Start marked. Move to the destination and mark the goal.";
    }

    public static void MarkGoal()
    {
        var preview = GetOrCreate();
        preview.StopSearch();
        preview.goal = PlayerControl.LocalPlayer.GetTruePosition();
        preview.hasGoal = true;
        DrawMarker(preview.goalMarker, preview.goal);
        preview.message = "Goal marked. Find path to test the current map.";
    }

    public static void FindPath()
    {
        var preview = GetOrCreate();
        preview.StopSearch();
        if (!preview.hasStart || !preview.hasGoal)
        {
            preview.message = "Mark both the start and goal first.";
            return;
        }

        preview.request = new MapPathRequest(ShipStatus.Instance, preview.start, preview.goal,
            new PathOptions(UseVents: PlayerControl.LocalPlayer.Data.Role.CanVent), PlayerControl.LocalPlayer);
    }

    public static void SendNpc() => GetOrCreate().StartNpc(false);
    public static void TourMap() => GetOrCreate().StartNpc(true);

    public static void StopNpc()
    {
        if (!Current || !Current.npc)
            return;
        Current.npc.Dispose();
        Current.npc = null;
        Current.message = "Debug NPC stopped.";
    }

    public void StartNpc(bool tour)
    {
        if (!hasStart || (!tour && !hasGoal))
        {
            message = tour ? "Mark the start first." : "Mark both the start and goal first.";
            return;
        }
        var probe = MapPathfinding.CreateRequest(start, tour ? start : goal);
        if (probe.Status == PathStatus.InvalidEndpoint)
        {
            message = "NPC start or goal is not on clear floor.";
            return;
        }
        probe.Cancel();
        StopNpc();
        StopSearch();
        var holder = new GameObject("NewModPathfindingNpc");
        holder.transform.SetParent(transform, false);
        npc = holder.AddComponent<PathfindingNpc>();
        npc.Initialize(this, tour);
    }

    [HideFromIl2Cpp]
    public void DrawPath(MapPath path)
    {
        route.positionCount = path.Points.Length > 1 ? path.Points.Length : 0;
        for (var i = 0; i < route.positionCount; i++)
            route.SetPosition(i, new Vector3(path.Points[i].x, path.Points[i].y, -5f));
    }

    public static void Clear()
    {
        var preview = Current;
        if (!preview)
            return;
        preview.StopSearch();
        StopNpc();
        preview.hasStart = preview.hasGoal = false;
        preview.startMarker.positionCount = preview.goalMarker.positionCount = 0;
        preview.ClearTraversalLines();
        preview.traversalReport = "";
        preview.message = "Path preview cleared.";
    }

    public static void CopyTraversalReport()
    {
        if (Current)
            GUIUtility.systemCopyBuffer = Current.traversalReport;
    }

    public static void InspectTraversals() => GetOrCreate().ScanTraversals();
    public static void TestNearestLadder() => GetOrCreate().ScanTraversals(PathTraversal.Ladder);
    public static void TestNearestZipline() => GetOrCreate().ScanTraversals(PathTraversal.Zipline);

    public void ClearTraversalLines()
    {
        foreach (var line in traversalLines)
            if (line)
                Destroy(line.gameObject);
        traversalLines.Clear();
    }

    [HideFromIl2Cpp]
    public void ScanTraversals(PathTraversal? test = null)
    {
        StopSearch();
        ClearTraversalLines();
        var position = PlayerControl.LocalPlayer.GetTruePosition();
        var probe = MapPathfinding.CreateRequest(position, position);
        var report = new StringBuilder($"Traversal scan | map: {ShipStatus.Instance.name} | player: {position} | probe: {probe.Status}\n");
        var ladders = ShipStatus.Instance.GetComponentsInChildren<Ladder>(true);
        var consoles = ShipStatus.Instance.GetComponentsInChildren<ZiplineConsole>(true);
        report.AppendLine($"Found {ladders.Length} ladder ends and {consoles.Length} zipline consoles.");
        Component nearest = null;
        var distance = float.MaxValue;
        foreach (var console in consoles)
        {
            var from = (Vector2)console.transform.position;
            report.AppendLine($"Zipline {console.name} top={console.atTop} active={console.isActiveAndEnabled}");
            if (console.zipline)
            {
                var zipline = console.zipline;
                var landing = console.atTop ? zipline.landingPositionBottom : zipline.landingPositionTop;
                if (landing)
                {
                    report.AppendLine($"  Landing world={landing.position} local={landing.localPosition} zipline origin={zipline.transform.position}");
                    DescribeTraversal(probe, report, console, from, zipline.transform.TransformPoint(landing.position),
                        $"zipline active={zipline.isActiveAndEnabled}, destination active={console.destination && console.destination.isActiveAndEnabled}");
                }
                else
                    report.AppendLine("  Missing landing transform.");
            }
            else
                report.AppendLine("  Missing zipline behaviour.");
            if (test == PathTraversal.Zipline && Vector2.Distance(position, from) < distance)
            {
                distance = Vector2.Distance(position, from);
                nearest = console;
            }
        }
        foreach (var ladder in ladders)
        {
            var from = (Vector2)ladder.transform.position;
            report.AppendLine($"Ladder {ladder.name} id={ladder.Id} top={ladder.IsTop} active={ladder.isActiveAndEnabled}");
            if (ladder.Destination)
                DescribeTraversal(probe, report, ladder, from, ladder.Destination.transform.position,
                    $"destination active={ladder.Destination.isActiveAndEnabled}");
            else
                report.AppendLine("  Missing destination.");
            if (test == PathTraversal.Ladder && Vector2.Distance(position, from) < distance)
            {
                distance = Vector2.Distance(position, from);
                nearest = ladder;
            }
        }
        traversalReport = report.ToString();
        Info(traversalReport);
        probe.Cancel();
        if (test == null)
        {
            message = "Traversal scan complete. Green: connected | Yellow: isolated | Red: rejected. Details below and in the log.";
            return;
        }
        var selected = -1;
        for (var i = 0; i < probe.linkSources.Count; i++)
            if (probe.linkSources[i] == nearest)
            {
                selected = i;
                break;
            }
        if (selected < 0)
        {
            message = nearest ? $"Nearest {test} was rejected. See the scan report." : $"No {test} found on this map.";
            return;
        }
        start = probe.links[selected].Start;
        goal = probe.links[selected].End;
        hasStart = hasGoal = true;
        DrawMarker(startMarker, start);
        DrawMarker(goalMarker, goal);
        FindPath();
    }

    [HideFromIl2Cpp]
    public void DescribeTraversal(MapPathRequest probe, StringBuilder report, Component source, Vector2 from, Vector2 to, string state)
    {
        var selected = -1;
        for (var i = 0; i < probe.linkSources.Count; i++)
            if (probe.linkSources[i] == source)
            {
                selected = i;
                break;
            }
        var entryNeighbours = 0;
        var exitNeighbours = 0;
        if (selected >= 0 && probe.grid != null)
        {
            foreach (var edge in probe.grid.Edges(-3 - selected * 2))
                if (edge.Node >= 0) entryNeighbours++;
            foreach (var edge in probe.grid.Edges(-4 - selected * 2))
                if (edge.Node >= 0) exitNeighbours++;
        }
        report.AppendLine($"  {state}; included={selected >= 0}; walking neighbours entry={entryNeighbours}, exit={exitNeighbours}");
        if (selected >= 0)
        {
            report.AppendLine($"  Connected entry={probe.links[selected].Start}, exit={probe.links[selected].End}");
            report.AppendLine($"  Approach offsets entry={Vector2.Distance(from, probe.links[selected].Start):0.00}, exit={Vector2.Distance(to, probe.links[selected].End):0.00}");
        }
        report.AppendLine($"  Entry {DescribeEndpoint(probe, from)}");
        report.AppendLine($"  Exit  {DescribeEndpoint(probe, to)}");
        var color = selected < 0 ? Color.red : entryNeighbours == 0 || exitNeighbours == 0 ? Color.yellow : Color.green;
        if (selected >= 0)
        {
            from = probe.links[selected].Start;
            to = probe.links[selected].End;
        }
        var line = CreateLine("TraversalDebug", color);
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(from.x, from.y, -5.1f));
        line.SetPosition(1, new Vector3(to.x, to.y, -5.1f));
        traversalLines.Add(line);
    }

    [HideFromIl2Cpp]
    public string DescribeEndpoint(MapPathRequest probe, Vector2 point)
    {
        var inBounds = probe.grid == null ? "unknown" : probe.grid.bounds.Contains(point).ToString();
        if (probe.IsClear(point))
            return $"{point} clear; in bounds={inBounds}";
        var collider = probe.overlaps[0];
        return $"{point} BLOCKED by {collider.name} ({collider.GetIl2CppType().Name}, layer={LayerMask.LayerToName(collider.gameObject.layer)}, bounds={collider.bounds}); in bounds={inBounds}";
    }

    public void Awake()
    {
        route = CreateLine("Path", new Color(0.3f, 0.9f, 1f));
        startMarker = CreateLine("Start", new Color(0.35f, 1f, 0.5f));
        goalMarker = CreateLine("Goal", new Color(1f, 0.3f, 0.7f));
        startMarker.loop = goalMarker.loop = true;
    }

    public void OnDisable()
    {
        if (npc) npc.Dispose();
        npc = null;
        request?.Cancel();
        request = null;
    }

    public void Update()
    {
        if (request == null)
            return;

        request.Step();
        if (request.Status == PathStatus.Searching)
            return;

        var result = request.Result;
        message = result.Status switch
        {
            PathStatus.Found => $"{result.Points.Length} waypoints | {result.Length:0.0} units | {result.ExpandedNodes} nodes explored | {result.Crossings.Length} crossings",
            PathStatus.Unreachable => "No route found. Check doors or try a smaller grid spacing through the API.",
            PathStatus.InvalidEndpoint => "Start or goal overlaps a wall, or lies outside the map. Mark it in open floor space.",
            PathStatus.MapChanged => "The map or doors changed during the search. Find path again.",
            PathStatus.LimitReached => "Search limit reached. Try closer endpoints or increase the API node limit.",
            _ => "Search cancelled."
        };

        route.positionCount = result.Points.Length > 1 ? result.Points.Length : 0;
        for (var i = 0; i < route.positionCount; i++)
            route.SetPosition(i, new Vector3(result.Points[i].x, result.Points[i].y, -5f));
        request = null;
    }

    public void StopSearch()
    {
        request?.Cancel();
        request = null;
        route.positionCount = 0;
    }

    public static PathfindingPreview GetOrCreate()
    {
        return Current ? Current : ShipStatus.Instance.gameObject.AddComponent<PathfindingPreview>();
    }

    public LineRenderer CreateLine(string name, Color color)
    {
        var visual = new GameObject($"NewModPathfinding_{name}") { layer = LayerMask.NameToLayer("UI") };
        visual.transform.SetParent(transform, false);
        var line = visual.AddComponent<LineRenderer>();
        line.sharedMaterial = Utils.GetCircleMat();
        line.useWorldSpace = true;
        line.widthMultiplier = 0.045f;
        line.startColor = line.endColor = color;
        line.numCornerVertices = 3;
        line.numCapVertices = 3;
        line.sortingOrder = 100;
        line.positionCount = 0;
        return line;
    }

    public static void DrawMarker(LineRenderer line, Vector2 position)
    {
        line.positionCount = 24;
        for (var i = 0; i < line.positionCount; i++)
        {
            var angle = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(position.x + Mathf.Cos(angle) * 0.14f, position.y + Mathf.Sin(angle) * 0.14f, -5f));
        }
    }
}
