using MiraAPI.GameEnd;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class SpecialAgent : CrewmateRole, ICustomRole
{
    public static PlayerControl AssignedPlayer { get; set; }
    public string RoleName => "Special Agent";
    public string RoleDescription => "Assign timed missions to other players.";
    public string RoleLongDescription => "Assign missions that targets must complete before time runs out.\nEarn enough successful mission results to win.";
    public Color RoleColor => Color.gray;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            AffectedByLightOnAirship = false,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = false,
            Icon = MiraAssets.Empty,
            OptionsScreenshot = MiraAssets.Empty,
            DefaultChance = 40,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<SpecialAgentGameOver>();
    }
}