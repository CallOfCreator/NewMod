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
    public static float cooldownUntil;
    public static float pulseUntil;
    public string RoleName => "Beacon";
    public string RoleDescription => "Spend charges to scan the whole map.";
    public string RoleLongDescription => "Open your map to spend a charge and reveal player positions.\nComplete tasks to restore limited charges.";
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

        var pulseDur = opts.PulseDuration;
        var cd = opts.PulseCooldown;
        var taskPerCh = opts.TasksPerCharge;
        var maxCharges = opts.MaxCharges;

        tab.AppendLine($"<size=65%><color=#{ColorUtility.ToHtmlStringRGB(RoleColor)}>Recon Support</color></size>");
        tab.AppendLine();
        tab.AppendLine($"<size=65%>Charges: <b><color=#{ColorUtility.ToHtmlStringRGB(Color.cyan)}>{charges}</color></b> / {maxCharges}  (+1 per {taskPerCh} tasks)</size>");
        tab.AppendLine($"<size=65%>Pulse Duration: <color=#{ColorUtility.ToHtmlStringRGB(Color.cyan)}>{pulseDur:F0}s</color> • Cooldown: <color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{cd:F0}s</color></size>");
        tab.AppendLine();
        tab.AppendLine("<size=65%><color=#FFD54F>Tip:</color> Use pulses after lights or suspected kills to catch rotations.</size>");

        return tab;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (Application.platform == RuntimePlatform.Android && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            return;

        pulseUntil = 0f;
        cooldownUntil = 0f;
        grantedFromTasks = 0;
        lastCompletedTasks = 0;
        charges = (int)OptionGroupSingleton<BeaconOptions>.Instance.StartingCharges;
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not Beacon) return;
        UpdateChargesFromTasks();
    }

    public static void UpdateChargesFromTasks()
    {
        var settings = OptionGroupSingleton<BeaconOptions>.Instance;
        var completed = GetCompletedTasks();
        if (completed == lastCompletedTasks) return;

        lastCompletedTasks = completed;
        var per = (int)settings.TasksPerCharge;
        var earned = Mathf.Min(completed / per, (int)settings.MaxCharges);
        var delta = earned - grantedFromTasks;

        if (delta > 0)
        {
            charges = Mathf.Clamp(charges + delta, 0, (int)settings.MaxCharges);
            grantedFromTasks = earned;
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