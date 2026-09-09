using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using NewMod.RoleLogic;
using NewMod.Utilities;
using Reactor.Utilities;
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
        if (!ScanStates.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var state))
            return text;

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
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is Specialist)
        {
            ScanStates[evt.Player.PlayerId] = new SpecialistScanState();
            return;
        }

        ScanStates.Remove(evt.Player.PlayerId);
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ScanStates.Clear();
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        if (!evt.Player.AmOwner || evt.Player.Data.Role is not Specialist)
            return;

        ScanStates[evt.Player.PlayerId].Earn();
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#00CCFF>Specialist:</color> scan ready."));
    }

    public static void CycleMode()
    {
        var mode = ScanStates[PlayerControl.LocalPlayer.PlayerId].Cycle();
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#00CCFF>Scan mode:</color> {mode}"));
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
            _ => VitalScan()
        };

        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#00CCFF>Specialist:</color> {result}"));
    }

    private static string PresenceScan(PlayerControl specialist)
    {
        var nearby = 0;
        var position = specialist.GetTruePosition();
        foreach (var player in PlayerControl.AllPlayerControls)
            if (player != specialist && !player.Data.IsDead && !player.Data.Disconnected && Vector2.Distance(player.GetTruePosition(), position) <= 5f)
                nearby++;

        return $"Presence scan: {nearby} living player{(nearby == 1 ? "" : "s")} nearby.";
    }

    private static string ForensicsScan(PlayerControl specialist)
    {
        var nearestDistance = float.PositiveInfinity;
        var position = specialist.GetTruePosition();
        foreach (var body in FindObjectsOfType<DeadBody>())
        {
            var distance = Vector2.Distance(position, body.TruePosition);
            if (distance < nearestDistance)
                nearestDistance = distance;
        }

        return float.IsPositiveInfinity(nearestDistance) ? "Forensics scan: no bodies detected." : $"Forensics scan: nearest body is {nearestDistance:0.0}m away.";
    }

    private static string VitalScan()
    {
        var alive = 0;
        foreach (var player in PlayerControl.AllPlayerControls)
            if (!player.Data.IsDead && !player.Data.Disconnected)
                alive++;

        return $"Vital scan: {alive} players remain alive.";
    }
}