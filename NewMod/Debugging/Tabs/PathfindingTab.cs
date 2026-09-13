using UnityEngine;

namespace NewMod.Debugging.Tabs;

public sealed class PathfindingTab : IDebugTab
{
    public string Name => "PATHFINDING";
    public bool ShouldShow => ShipStatus.Instance && PlayerControl.LocalPlayer;

    public void BuildGUI()
    {
        var preview = PathfindingPreview.Current;
        GUILayout.Label("Routes using current walls and map connections");
        GUILayout.Label(preview ? preview.StartText : "Start: not set");
        GUILayout.Label(preview ? preview.GoalText : "Goal: not set");
        if (GUILayout.Button("Mark start here")) PathfindingPreview.MarkStart();
        if (GUILayout.Button("Mark goal here")) PathfindingPreview.MarkGoal();
        if (GUILayout.Button("Find path")) PathfindingPreview.FindPath();
        if (GUILayout.Button("Send NPC")) PathfindingPreview.SendNpc();
        if (GUILayout.Button("Tour map and return")) PathfindingPreview.TourMap();
        if (GUILayout.Button("Stop NPC")) PathfindingPreview.StopNpc();
        if (GUILayout.Button("Inspect ladders / ziplines")) PathfindingPreview.InspectTraversals();
        if (GUILayout.Button("Copy traversal report")) PathfindingPreview.CopyTraversalReport();
        if (GUILayout.Button("Test nearest ladder")) PathfindingPreview.TestNearestLadder();
        if (GUILayout.Button("Test nearest zipline")) PathfindingPreview.TestNearestZipline();
        if (GUILayout.Button("Clear preview")) PathfindingPreview.Clear();
        GUILayout.Label(preview ? preview.StatusText : "Mark a start, move to a destination, then mark the goal.");
        if (preview && preview.TraversalReport.Length > 0) GUILayout.Label(preview.TraversalReport);
        GUILayout.Label("Green: start | Pink: goal | Cyan: route. Crossings require using the ladder, zipline or decon door.");
    }
}
