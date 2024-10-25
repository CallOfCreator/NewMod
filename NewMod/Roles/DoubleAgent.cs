using MiraAPI.Roles;
using UnityEngine;
using MiraAPI.Utilities.Assets;

namespace NewMod.Roles;

[RegisterCustomRole]
public class DoubleAgent : CrewmateRole, ICustomRole
{
    public string RoleName => "Double Agent";
    public string RoleDescription => $"A Crewmate posing as an Impostor: You can't kill or vent, but you can sabotage and confuse the real Impostors. Complete all tasks and sabotage to win\n\nTeam: {Team}.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        OptionsScreenshot = MiraAssets.Empty,
        Icon = MiraAssets.Empty,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = true,
        CanUseSabotage = true,
        DefaultChance = 50,
        DefaultRoleCount = 2,
        CanModifyChance = true,
        RoleHintType = RoleHintType.RoleTab
    };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == (GameOverReason)NewModEndReasons.DoubleAgentWin;
    }
}