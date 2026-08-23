using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using NewMod.Buttons.Revenant;
using NewMod.Components;
using NewMod.Modifiers;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod;

public static class NewModEventHandler
{
    public static void RegisterEventsLogs()
    {
        var type = typeof(MiraEventManager);
        var field = type.GetField("EventWrappers", BindingFlags.NonPublic | BindingFlags.Static);
        var wrappersObject = field.GetValue(null);
        if (wrappersObject is not IDictionary wrappersByEvent || wrappersByEvent.Count == 0)
        {
            return;
        }

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("=== Registered NewMod Events ===");

        foreach (DictionaryEntry entry in wrappersByEvent)
        {
            var eventType = entry.Key as Type;
            var lines = new List<string>();

            if (entry.Value is IEnumerable wrappers)
            {
                foreach (var wrapper in wrappers)
                {
                    if (wrapper == null) continue;

                    var wrapperType = wrapper.GetType();
                    var eventHandlerProperty = wrapperType.GetProperty("EventHandler", BindingFlags.Public | BindingFlags.Instance);
                    var priorityProperty = wrapperType.GetProperty("Priority", BindingFlags.Public | BindingFlags.Instance);
                    var handler = eventHandlerProperty.GetValue(wrapper) as Delegate;
                    var priority = priorityProperty.GetValue(wrapper) as int? ?? 0;
                    var method = handler.Method;

                    lines.Add($" [{priority}] {method.DeclaringType.FullName}.{method.Name}()");
                }
            }

            builder.AppendLine($"{eventType.FullName}  (handlers: {lines.Count})");
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }
        }

        NewMod.Instance.Log.LogInfo(builder.ToString());
    }

    public static void ResetMatchState()
    {
        Utils.ResetKillTracking();
        Utils.ResetDrainCount();
        Utils.ResetMissionSuccessCount();
        Utils.ResetMissionFailureCount();
        Utils.ResetInjections();
        Utils.ResetStrikeCount();
        Utils.waitingPlayers.Clear();
        Utils.savedPlayerRoles.Clear();
        Utils.MissionTimer.Clear();
        Utils.savedTasks.Clear();

        PranksterUtilities.ResetReportCount();
        WraithCallerUtilities.ClearAll();
        Shade.ShadeKills.Clear();
        Revenant.ResetAllStates();
        NecromancerRole.RevivedPlayers.Clear();

        CoroutinesHelper.bodiesCreated.Clear();
        CoroutinesHelper.drainCount.Clear();
        PendingEffectManager.pendingEffects.Clear();
        DoomAwakening.killedPlayers.Clear();

        StickyModifier.ResetState();
        FearPulseArea.AffectedPlayers.Clear();
        FearPulseArea._speedNotifShown.Clear();
        FearPulseArea._visionNotifShown.Clear();
        AegisUtilities.ActiveOwners.Clear();

        foreach (var shield in ShieldArea._active.ToArray())
        {
            if (shield)
            {
                UnityEngine.Object.Destroy(shield.gameObject);
            }
        }

        ShieldArea._active.Clear();

        Tyrant.ResetState();
        OverloadRole.ResetState();
        SpecialAgent.AssignedPlayer = null;

        Beacon.charges = 0;
        Beacon.grantedFromTasks = 0;
        Beacon.lastCompletedTasks = 0;
        Beacon.cooldownUntil = 0f;
        Beacon.pulseUntil = 0f;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
        {
            return;
        }

        HudManager.Instance.Chat.enabled = false;
        VisionaryUtilities.DeleteAllScreenshots();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        if (evt.MeetingHud != null)
        {
            HudManager.Instance.Chat.enabled = true;
        }
    }

    [RegisterEvent(100)]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ResetMatchState();
        VisionaryUtilities.DeleteAllScreenshots();
    }
}