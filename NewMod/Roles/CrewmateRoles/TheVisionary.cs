using MiraAPI.Roles;
using UnityEngine;
using MiraAPI.Utilities.Assets;

namespace NewMod.Roles.CrewmateRoles;

public class TheVisionary : CrewmateRole, ICustomRole
{
    public string RoleName => "TheVisionary";
    public string RoleDescription => $"Take photos during the game";
    public string RoleLongDescription => "Capture key moments during the game by taking photos to gather evidence";
    public Color RoleColor => new(0.75f, 0.5f, 1.0f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        DefaultRoleCount = 2,
        DefaultChance = 50,
        MaxRoleCount = 3,
        AffectedByLightOnAirship = true,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = true,
        Icon = MiraAssets.Empty,
        OptionsScreenshot  = MiraAssets.Empty,
        CanModifyChance = true,
        RoleHintType = RoleHintType.RoleTab
    };
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == (GameOverReason)NewModEndReasons.TheVisionaryWin;
    }
}