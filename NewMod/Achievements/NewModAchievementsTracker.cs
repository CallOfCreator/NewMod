using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using UnityEngine;

namespace NewMod.Achievements;

public static class NewModAchievementTracker
{
    public static bool OnlineMatchActive;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        OnlineMatchActive = false;

        if (Application.platform == RuntimePlatform.Android)
            return;

        OnlineMatchActive = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
        if (OnlineMatchActive)
            NewModAchievementsTab.WelcomeToNewMod.Unlock();
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        if (!OnlineMatchActive)
            return;

        OnlineMatchActive = false;

        if (Application.platform == RuntimePlatform.Android)
            return;

        if (!NewModAchievementsTab.ThreeInARow.Unlocked)
            NewModAchievementsTab.ThreeInARow.Increment(1, false);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
    public static class GameDataOnDisconnectedPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!OnlineMatchActive)
                return;

            OnlineMatchActive = false;

            if (!NewModAchievementsTab.ThreeInARow.Unlocked)
                NewModAchievementsTab.ThreeInARow.SetValue(0, false);
        }
    }
}
