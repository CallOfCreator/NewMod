using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using NewMod.Components;
using NewMod.GeneralEvents;
using NewMod.Modifiers;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Twitch;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewMod;

public static class NewModEventHandler
{
    public static void RegisterEventsLogs()
    {
        var type = typeof(MiraEventManager);
        var field = type.GetField("EventWrappers", BindingFlags.NonPublic | BindingFlags.Static);
        var wrappersObject = field.GetValue(null);
        if (wrappersObject is not IDictionary wrappersByEvent || wrappersByEvent.Count == 0) return;

        var builder = new StringBuilder();
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

            builder.AppendLine(CultureInfo.CurrentCulture, $"{eventType.FullName}  (handlers: {lines.Count})");
            foreach (var line in lines) builder.AppendLine(line);
        }

        Info(builder.ToString());
    }

    public static void ResetMatchState()
    {
        Utils.ResetKillTracking();
        EnergyThief.ResetState();
        InjectorUtilities.Reset();
        Utils.savedPlayerRoles.Clear();

        PranksterUtilities.ResetReportCount();
        WraithCallerUtilities.ClearAll();
        Shade.ShadeKills.Clear();
        Revenant.Phases.Clear();
        Revenant.PhaseExpiresAt.Clear();
        Revenant.Bodies.Clear();
        Revenant.PendingDoomTargets.Clear();
        NecromancerRole.RevivedPlayers.Clear();
        VerifierUtilities.Reset(true, false);

        StickyModifier.ResetState();
        FearPulseArea.ResetState();

        foreach (var shield in ShieldArea._active.ToArray())
        {
            if (shield)
                Object.Destroy(shield.gameObject);
        }

        ShieldArea._active.Clear();

        Tyrant.ResetState();
        TerminatorRole.ResetState();
        MirrorBladeRole.ArmedReflections.Clear();
        MirrorBladeRole.Reflecting = false;

        Beacon.Reset();

        GeneralEventManager.Reset();
    }

    [RegisterEvent]
    public static void OnRoleAssigned(SetRoleEvent evt)
    {
        if (!evt.Player.AmOwner || evt.Player.Data.Role.IsImpostor)
            return;

        PlayerTask header = null;
        foreach (var task in evt.Player.myTasks)
        {
            if (task && task.name == "ImpostorRole")
            {
                header = task;
                break;
            }
        }

        if (!header)
            return;

        evt.Player.myTasks.Remove(header);
        Object.Destroy(header.gameObject);
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            for (var i = PlayerControl.LocalPlayer.myTasks.Count - 1; i >= 0; i--)
            {
                var task = PlayerControl.LocalPlayer.myTasks[i];
                if (task && task.name == "ImpostorRole")
                {
                    PlayerControl.LocalPlayer.myTasks.RemoveAt(i);
                    Object.Destroy(task.gameObject);

                    TwitchManager.Instance.TwitchPopup.Show();
                }
            }
        }

        if (Application.platform == RuntimePlatform.Android)
            return;

        VisionaryUtilities.DeleteAllScreenshots();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        Utils.RecordOnKill(evt.Source, evt.Target);
    }
}
