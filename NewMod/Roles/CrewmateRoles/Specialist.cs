using System;
using System.Collections.Generic;
using MiraAPI.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;
using MiraAPI.Utilities.Assets;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events;

namespace NewMod.Roles.CrewmateRoles;

public class Specialist : CrewmateRole, ICustomRole
{
    public string RoleName => "Specialist";
    public string RoleDescription => "Complete tasks, gain power.";
    public string RoleLongDescription => "Each task you complete grants you a random ability.";
    public Color RoleColor => new(0.0f, 0.8f, 1.0f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        OptionsScreenshot = MiraAssets.Empty,
        Icon = MiraAssets.Empty,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = true,
        CanUseSabotage = false,
        DefaultChance = 50,
        DefaultRoleCount = 1,
        CanModifyChance = true,
        RoleHintType = RoleHintType.RoleTab
    };
    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        PlayerControl player = evt.Player;
        if (!(player.Data.Role is Specialist)) return;

        List<Action> abilityAction = new List<Action>
        {
            () =>
            {
                var target = Utils.GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected && p != player);
                if (target != null)
                {
                   Utils.RpcRandomDrainActions(player, target);
                   Coroutines.Start(CoroutinesHelper.CoNotify(
                    $"<color=green>Energy Drain activated on {target.Data.PlayerName}!</color>"));
                }
            },
            () =>
            {
                var closestBody = Utils.GetClosestBody();
                var player = Utils.PlayerById(closestBody.ParentId);
                if (closestBody != null)
                {
                   Utils.RpcRevive(closestBody);
                   Coroutines.Start(CoroutinesHelper.CoNotify(
                    $"<color=green>Player {player.Data.PlayerName} has been revived.</color>"));
                }
            },
            () =>
            {
                PranksterUtilities.CreatePranksterDeadBody(player, player.PlayerId);
                Coroutines.Start(CoroutinesHelper.CoNotify(
                   "<color=green>Fake Body created!</color>"));
            },
            () =>
            {
                var randPlayer = Utils.GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);
                if (randPlayer != null && randPlayer.Data.Role is not ICustomRole)
                {
                   var role = randPlayer.Data.Role;
                   role.UseAbility();
                }
            },
            () =>
            {
                Utils.RpcAssignMission(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
                Coroutines.Start(CoroutinesHelper.CoNotify(
                      "<color=red>You have been assigned a mission. Complete it or die.</color>"));
            }
        };

        if (abilityAction.Count == 0)
        {
            return;
        }
        int randomIndex = UnityEngine.Random.Range(0, abilityAction.Count);
        abilityAction[randomIndex].Invoke();
    }
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == GameOverReason.CrewmatesByTask;
    }
}
