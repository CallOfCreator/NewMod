using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using NewMod.Seasons;

namespace NewMod.Achievements;

public static class NewModAchievementTracker
{
    private static bool _onlineMatchActive;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        _onlineMatchActive = false;

        if (!SeasonManager.AvailableAchievementTabTypes.Contains(typeof(PreseasonAchievementsTab)))
        {
            return;
        }

        PreseasonAchievementsTab.WelcomeToNewMod.Unlock();

        _onlineMatchActive = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        if (!_onlineMatchActive)
            return;

        _onlineMatchActive = false;

        if (!SeasonManager.AvailableAchievementTabTypes.Contains(typeof(PreseasonAchievementsTab)))
        {
            return;
        }

        if (!PreseasonAchievementsTab.ThreeInARow.Unlocked)
            PreseasonAchievementsTab.ThreeInARow.Increment(1);
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

            if (!PreseasonAchievementsTab.ThreeInARow.Unlocked)
                PreseasonAchievementsTab.ThreeInARow.SetValue(0, false);
        }
    }
}