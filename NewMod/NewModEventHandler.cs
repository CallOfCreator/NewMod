using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using NewMod.Buttons.Roles;
using NewMod.Components;
using NewMod.GeneralEvents;
using NewMod.Modifiers;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Reactor.Utilities;
using Twitch;
using UnityEngine;
using UnityEngine.Events;
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

            builder.AppendLine($"{eventType.FullName}  (handlers: {lines.Count})");
            foreach (var line in lines) builder.AppendLine(line);
        }

        Info(builder.ToString());
    }

    public static void ResetMatchState()
    {
        Utils.ResetKillTracking();
        EnergyThief.ResetState();
        Utils.ResetMissionSuccessCount();
        Utils.ResetMissionFailureCount();
        Utils.ResetInjections();
        Utils.ResetStrikeCount();
        Utils.savedPlayerRoles.Clear();
        Utils.MissionTimer.Clear();
        Utils.savedTasks.Clear();

        PranksterUtilities.ResetReportCount();
        WraithCallerUtilities.ClearAll();
        Shade.ShadeKills.Clear();
        Revenant.Phases.Clear();
        Revenant.PhaseExpiresAt.Clear();
        Revenant.Bodies.Clear();
        Revenant.PendingDoomTargets.Clear();
        NecromancerRole.RevivedPlayers.Clear();
        VerifierUtilities.Reset(true, false);

        CoroutinesHelper.bodiesCreated.Clear();
        CoroutinesHelper.drainCount.Clear();
        StickyModifier.ResetState();
        FearPulseArea.ResetState();
        AegisUtilities.ActiveOwners.Clear();

        foreach (var shield in ShieldArea._active.ToArray())
            if (shield)
                Object.Destroy(shield.gameObject);

        ShieldArea._active.Clear();

        Tyrant.ResetState();
        OverloadRole.ResetState();
        TerminatorRole.ResetState();
        MirrorBladeRole.ArmedReflections.Clear();
        MirrorBladeRole._reflecting = false;
        SpecialAgent.AssignedPlayer = null;

        Beacon.charges = 0;
        Beacon.grantedFromTasks = 0;
        Beacon.lastCompletedTasks = 0;
        Beacon.cooldownUntil = 0f;
        Beacon.pulseUntil = 0f;

        GeneralEventManager.Reset();
    }

    [RegisterEvent]
    public static void OnRoleAssigned(SetRoleEvent evt)
    {
        if (!evt.Player.AmOwner || evt.Player.Data.Role.IsImpostor)
            return;

        PlayerTask header = null;
        foreach (var task in evt.Player.myTasks)
            if (task && task.name == "ImpostorRole")
            {
                header = task;
                break;
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

        if (Application.platform == RuntimePlatform.Android)
            return;

        VisionaryUtilities.DeleteAllScreenshots();
    }
    
    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (!evt.Source.AmOwner || evt.Source.Data.Role is not OverloadRole || evt.Target != OverloadRole.chosenPrey) return;

        //TODO: Use the newest MiraAPI roles for button mapping
        if (evt.Target.Data.Role is ICustomRole customRole && Utils.RoleToButtonsMap.TryGetValue(customRole.GetType(), out var buttonsType))
        {
            OverloadRole.CachedButtons = [.. CustomButtonManager.Buttons.Where(b => buttonsType.Contains(b.GetType()))];
            Message($"CachedButton: {buttonsType.GetType().Name}");
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var source = evt.Source;
        var target = evt.Target;
        Utils.RecordOnKill(source, target);

        if (!source.AmOwner || source.Data.Role is not OverloadRole || target != OverloadRole.chosenPrey) return;

        foreach (var pc in PlayerControl.AllPlayerControls.ToArray().Where(p => p.AmOwner && p.Data.Role is OverloadRole))
            if (target.Data.Role is ICustomRole customRole)
            {
                foreach (var button in OverloadRole.CachedButtons)
                {
                    CustomButtonSingleton<OverloadButton>.Instance.Absorb(button);
                    Debug($"[Overload] Successfully absorbed ability: {button.Name}");
                }
            }
            else if (target.Data.Role is not ICustomRole)
            {
                var btn = Object.Instantiate(HudManager.Instance.AbilityButton, HudManager.Instance.AbilityButton.transform.parent);
                btn.SetFromSettings(target.Data.Role.Ability);
                var pb = btn.GetComponent<PassiveButton>();
                pb.OnClick.RemoveAllListeners();
                pb.OnClick.AddListener((UnityAction)target.Data.Role.UseAbility);
            }

        OverloadRole.CachedButtons.Clear();
        OverloadRole.AbsorbedAbilityCount++;
        OverloadRole.chosenPrey = null;
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=green>Charge {OverloadRole.AbsorbedAbilityCount}/{OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge}</color>"));

        if (OverloadRole.AbsorbedAbilityCount >= OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#00FF7F>Objective completed: Final Ability unlocked!</color>"));
        else
            Coroutines.Start(OverloadRole.CoShowMenu(1f));
    }
}