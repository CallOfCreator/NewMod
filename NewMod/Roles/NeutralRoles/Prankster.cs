using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;
public class Prankster : CrewmateRole, ICustomRole
{
    public string RoleName => "Prankster";
    public string RoleDescription => "Set up fake bodies to trick others. When reported, each fake body triggers a funny or deadly surprise for the reporter";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => new Color(1f, 0.55f, 0f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;
    public CustomRoleConfiguration Configuration => new(this)
    {
       MaxRoleCount = 3,
       DefaultRoleCount = 1,
       DefaultChance = 30,
       OptionsScreenshot = MiraAssets.Empty,
       Icon = MiraAssets.Empty,
       AffectedByLightOnAirship = false,
       KillButtonOutlineColor = RoleColor,
       RoleHintType = RoleHintType.RoleTab,
       GhostRole = AmongUs.GameOptions.RoleTypes.CrewmateGhost,
       CanGetKilled = true,
       UseVanillaKillButton = false,
       CanUseVent = false,
       CanUseSabotage = false,
       TasksCountForProgress = false,
       HideSettings = false,
       CanModifyChance = true,
    };

}