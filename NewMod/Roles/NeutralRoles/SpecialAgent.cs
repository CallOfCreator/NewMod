using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

[RegisterCustomRole]
public class SpecialAgent : CrewmateRole, ICustomRole
{
    public string RoleName => "Special Agent";
    public string RoleDescription => "Assigns secret missions to players, who must complete them or face consequences.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Color.gray;
    public ModdedRoleTeams Team => ModdedRoleTeams.Neutral;
    public CustomRoleConfiguration Configuration => new(this)
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
        return gameOverReason == (GameOverReason)NewModEndReasons.SpecialAgentWin;
    }
}
