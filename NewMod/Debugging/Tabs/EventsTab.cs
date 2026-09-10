using NewMod.GeneralEvents;
using UnityEngine;

namespace NewMod.Debugging.Tabs;

public class EventsTab : IDebugTab
{
     public string Name => "EVENTS";
    public bool ShouldShow => ShipStatus.Instance != null && PlayerControl.LocalPlayer;

    public void BuildGUI()
    {
        GUILayout.Label("CYCLE");

        if (GUILayout.Button("Start"))
        {
            GeneralEventManager.StartCycle();
        }

        if (GUILayout.Button("Stop"))
        {
            GeneralEventManager.StopCycle();
        }

        if (GUILayout.Button("End Current Event"))
        {
            GeneralEventManager.ForceEnd();
        }
        
        GUILayout.Label("FORCE EVENT");

        foreach (var generalEvent in GeneralEventManager.RegisteredEvents)
        {
            if (GUILayout.Button(generalEvent.Title)) GeneralEventManager.ForceEvent(generalEvent.GetType());
        }
    }
}