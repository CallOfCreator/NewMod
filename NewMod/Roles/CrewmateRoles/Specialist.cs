using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiraAPI.Utilities;
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

    public static readonly List<(Vector2 Position, float Time)> Disturbances = new();

    public string RoleName => "Specialist";
    public string RoleDescription => "Complete tasks to earn different kinds of scans.";
    public string RoleLongDescription => "Each task gives you one scan, up to three. Choose what to check before scanning.\nPresence counts nearby players. Forensics checks for bodies.\nDisturbance tells you whether someone was killed nearby recently.";
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

        text.Append($"\n<size=65%>{RoleColor.ToTextColor()}Mode: {state.Mode} | Scans: {state.Charges}/3</color>\nRange: 5 units | Disturbance history: 15s\nScanning takes 1.5s; stay still. <color=#FFCF70>Comms prevents scans.</color></size>");
        return text;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        ScanStates.Clear();
        Disturbances.Clear();
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
        Disturbances.Clear();
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
        if (!state.Spend())
            return;

        var position = specialist.GetTruePosition();
        string result;
        switch (state.Mode)
        {
            case SpecialistScanMode.Presence:
                var nearby = Helpers.GetClosestPlayers(specialist, 5f).Count(player => !player.Data.IsDead && !player.Data.Disconnected && !player.inVent);
                result = $"Presence: {nearby} living player(s) nearby.";
                break;
            case SpecialistScanMode.Forensics:
                var bodies = Helpers.GetNearestDeadBodies(position, 5f, Helpers.CreateFilter(Constants.NotShipMask));
                result = bodies.Count == 0 ? "Forensics: no bodies within 5 units." : bodies.Any(body => Vector2.Distance(position, body.TruePosition) <= 2f) ? "Forensics: a body is very close (within 2 units)." : "Forensics: a body is nearby (2-5 units).";
                break;
            default:
                Disturbances.RemoveAll(entry => Time.time - entry.Time > 15f);
                result = Disturbances.Any(entry => Vector2.Distance(position, entry.Position) <= 5f) ? "Disturbance: violence occurred nearby in the last 15s." : "Disturbance: no recent violence detected nearby.";
                break;
        }

        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#00CCFF>Specialist:</color> {result}"));
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        Disturbances.RemoveAll(entry => Time.time - entry.Time > 15f);
        Disturbances.Add((evt.Target.GetTruePosition(), Time.time));
    }
}