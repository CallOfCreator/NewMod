using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class EnergyThief : CrewmateRole, ICustomRole
{  
    public string RoleName => "EnergyThief";
    public string RoleDescription => "Drains energy from others, making them weak";
    public string RoleLongDescription => $"The Energy Thief can drain energy from Crewmates or Impostors, weakening them and gaining temporary buffs\nDrain 3 players to win.";
    public Color RoleColor => Color.magenta.GetAlternateColor();
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;
    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 5,
        AffectedByLightOnAirship = false,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        Icon = MiraAssets.Empty,
        OptionsScreenshot  = MiraAssets.Empty,
        DefaultChance = 50,
        DefaultRoleCount = 2, 
        CanModifyChance = true,
        RoleHintType = RoleHintType.RoleTab
    };
    public override bool DidWin(GameOverReason gameOverReason)
    {
       return gameOverReason == (GameOverReason)NewModEndReasons.EnergyThiefWin;
    }
}