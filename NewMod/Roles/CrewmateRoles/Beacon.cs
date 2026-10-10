using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
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
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Beacon");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Beacon.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Beacon.TabDescription");
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
            Icon = NewModAsset.RadarIcon,
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tab = INewModRole.GetRoleTabText(this);
        var opts = OptionGroupSingleton<BeaconOptions>.Instance;

        tab.Append(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Beacon.Tab.Details"), RoleColor.ToTextColor(), charges, opts.MaxCharges, opts.TasksPerCharge, opts.PulseDuration, opts.PulseCooldown));

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
        {
            if (t && t.IsComplete)
                done++;
        }

        return done;
    }
}
