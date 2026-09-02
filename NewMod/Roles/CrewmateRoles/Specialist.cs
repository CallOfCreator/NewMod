using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.RoleLogic;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class Specialist : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, SpecialistScanState> ScanStates = [];

    public string RoleName => "Specialist";
    public string RoleDescription => "Complete tasks to earn scans, then choose what field intel you need.";
    public string RoleLongDescription => "Each completed task grants one scan. Cycle between Presence, Forensics, and Vital, then spend a charge to run the selected scan.";
    public Color RoleColor => new(0f, 0.8f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            OptionsScreenshot = MiraAssets.Empty,
            Icon = MiraAssets.Empty,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = true,
            CanUseSabotage = false,
            DefaultChance = 30,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        var state = ScanStates[PlayerControl.LocalPlayer.PlayerId];
        text.AppendLine($"<size=65%>Mode: <color=#00CCFF>{state.Mode}</color></size>");
        text.AppendLine($"<size=65%>Scans: <color=#00CCFF>{state.Charges}</color></size>");
        return text;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        ScanStates.Clear();
        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.Role is Specialist)
                ScanStates[player.PlayerId] = new SpecialistScanState();
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        if (!evt.Player.AmOwner || evt.Player.Data.Role is not Specialist)
            return;

        ScanStates[evt.Player.PlayerId].Earn();
        Helpers.CreateAndShowNotification("Specialist scan ready.", new Color(0f, 0.8f, 1f));
    }

    public static void CycleMode()
    {
        var mode = ScanStates[PlayerControl.LocalPlayer.PlayerId].Cycle();
        Helpers.CreateAndShowNotification($"Scan mode: {mode}", new Color(0f, 0.8f, 1f));
    }

    public static void Scan()
    {
        var specialist = PlayerControl.LocalPlayer;
        var state = ScanStates[specialist.PlayerId];
        if (!state.TrySpend())
            return;

        var result = state.Mode switch
        {
            SpecialistScanMode.Presence => PresenceScan(specialist),
            SpecialistScanMode.Forensics => ForensicsScan(specialist),
            _ => $"Vital scan: {Helpers.GetAlivePlayers().Count} players remain alive."
        };

        Helpers.CreateAndShowNotification(result, new Color(0f, 0.8f, 1f));
    }

    private static string PresenceScan(PlayerControl specialist)
    {
        var nearby = Helpers.GetAlivePlayers().Count(player => player != specialist && Vector2.Distance(player.GetTruePosition(), specialist.GetTruePosition()) <= 5f);
        return $"Presence scan: {nearby} living player{(nearby == 1 ? "" : "s")} nearby.";
    }

    private static string ForensicsScan(PlayerControl specialist)
    {
        var body = Object.FindObjectsOfType<DeadBody>().OrderBy(deadBody => Vector2.Distance(specialist.GetTruePosition(), deadBody.TruePosition)).FirstOrDefault();
        return body ? $"Forensics scan: nearest body is {Vector2.Distance(specialist.GetTruePosition(), body.TruePosition):0.0}m away." : "Forensics scan: no bodies detected.";
    }
}

