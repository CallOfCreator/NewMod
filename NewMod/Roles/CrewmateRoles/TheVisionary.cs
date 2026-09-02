using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class TheVisionary : CrewmateRole, ICustomRole
{
    public RoleOptionsGroup RoleOptionGroup { get; } = RoleOptionsGroup.Crewmate;
    public string RoleName => "The Visionary";
    public string RoleDescription => "Capture and review screenshots as evidence.";
    public string RoleLongDescription => "Take a limited screenshot of the current scene.\nReview saved shots to compare players and locations.";
    public Color RoleColor => new(0.75f, 0.5f, 1.0f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            DefaultRoleCount = 1,
            DefaultChance = 50,
            MaxRoleCount = 1,
            AffectedByLightOnAirship = true,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = true,
            Icon = MiraAssets.Empty,
            OptionsScreenshot = MiraAssets.Empty,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };
}
