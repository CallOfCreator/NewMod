using NewMod.Debugging.Tabs;

namespace NewMod.Debugging;

// Thanks to Submerged

public static class DebugMode
{
    public static void Initialize(NewMod plugin)
    {
        plugin.AddComponent<DebugWindow>();
        DebugWindow.Instance.Tabs.Add(new Tabs.PlayerTab());
        DebugWindow.Instance.Tabs.Add(new MatchTab());
        DebugWindow.Instance.Tabs.Add(new EnergyTab());
        DebugWindow.Instance.Tabs.Add(new EventsTab());
        DebugWindow.Instance.Tabs.Add(new EffectsTab());
    }
}