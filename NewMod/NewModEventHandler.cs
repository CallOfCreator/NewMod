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
using Object = UnityEngine.Object;

namespace NewMod;

public static class NewModEventHandler
{
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
                }
            }
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        Utils.RecordOnKill(evt.Source, evt.Target);
    }
}
