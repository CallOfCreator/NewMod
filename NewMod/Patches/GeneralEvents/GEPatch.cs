using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using NewMod.GeneralEvents;

namespace NewMod.Patches.GeneralEvents;

public static class GeneralEventCycleEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        GeneralEventManager.Reset();
        GeneralEventManager.StartCycle();
        NewMod.Instance.Log.LogMessage("Started Cycle");
    }
}