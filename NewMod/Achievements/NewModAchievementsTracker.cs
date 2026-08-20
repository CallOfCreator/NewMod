using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace NewMod.Achievements;

public static class NewModAchievementTracker
{
    private static bool _onlineMatchActive;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        NewModAchievementsTab.WelcomeToNewMod.Unlock();
        _onlineMatchActive = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        if (!_onlineMatchActive)
            return;

        _onlineMatchActive = false;

        if (!NewModAchievementsTab.ThreeInARow.Unlocked)
            NewModAchievementsTab.ThreeInARow.Increment(1);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
    public static class GameDataOnDisconnectedPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!_onlineMatchActive)
                return;

            _onlineMatchActive = false;

            if (!NewModAchievementsTab.ThreeInARow.Unlocked)
                NewModAchievementsTab.ThreeInARow.SetValue(0, false);
        }
    }
}