using NewMod.Utilities;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Options.Roles;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class Beacon : CrewmateRole, INewModRole
{
    public static int charges;
    public static int grantedFromTasks;
    public static int lastCompletedTasks;
    public static float pulseUntil;
    public string RoleName => "Beacon";
    public string RoleDescription => "Check where players are on the map.";
    public string RoleLongDescription => "Scan the map to see where living players are at that moment. Players in vents are hidden.\nThe markers stay in place and do not show names. Complete tasks to earn more scans.";
    public Color RoleColor => new(0.494f, 0.341f, 0.761f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = true,
            MaxRoleCount = 1,
            Icon = NewModAsset.RadarIcon
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tab = INewModRole.GetRoleTabText(this);
        var opts = OptionGroupSingleton<BeaconOptions>.Instance;

        tab.Append($"\n<size=65%>{RoleColor.ToTextColor()}Charges: {charges}/{opts.MaxCharges:0} | +1 per {opts.TasksPerCharge:0} tasks</color>\nSnapshot: {opts.PulseDuration:0.#}s | Cooldown: {opts.PulseCooldown:0.#}s\nOpening the map does not spend a charge.\n<color=#FFCF70>Comms blocks scanning.</color> Markers show where players were when you scanned.</size>");

        return tab;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        Reset();
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.AmOwner && evt.Player.Data.Role is Beacon)
            Reset();
    }

    public static void Reset()
    {
        pulseUntil = 0f;
        grantedFromTasks = 0;
        lastCompletedTasks = 0;
        charges = Mathf.Min((int)OptionGroupSingleton<BeaconOptions>.Instance.StartingCharges, (int)OptionGroupSingleton<BeaconOptions>.Instance.MaxCharges);
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        if (!evt.Player.AmOwner || evt.Player.Data.Role is not Beacon) return;
        UpdateChargesFromTasks();
    }

    public static void UpdateChargesFromTasks()
    {
        var settings = OptionGroupSingleton<BeaconOptions>.Instance;
        var completed = GetCompletedTasks();
        if (completed == lastCompletedTasks) return;

        lastCompletedTasks = completed;
        var per = (int)settings.TasksPerCharge;
        var earned = completed / per;
        var delta = Mathf.Min(earned - grantedFromTasks, (int)settings.MaxCharges - charges);
        grantedFromTasks = earned;

        if (delta > 0)
        {
            charges += delta;
            Helpers.CreateAndShowNotification($"+{delta} Beacon {(delta > 1 ? "charges" : "charge")} (tasks)", new Color(0.75f, 0.65f, 1f), spr: NewModAsset.RadarIcon.LoadAsset());
        }
    }

    public static int GetCompletedTasks()
    {
        var lp = PlayerControl.LocalPlayer;
        var done = 0;
        foreach (var t in lp.myTasks)
            if (t && t.IsComplete)
                done++;
        return done;
    }
}