using AmongUs.GameOptions;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class Prankster : CrewmateRole, ICustomRole
{
    public string RoleName => "Prankster";
    public string RoleDescription => "Place deadly fake bodies.";
    public string RoleLongDescription => "Place fake bodies that kill anyone who reports them.\nGet the required number of fake-body reports to win.";
    public Color RoleColor => new(1f, 0.55f, 0f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 30,
            OptionsScreenshot = MiraAssets.Empty,
            Icon = MiraAssets.Empty,
            AffectedByLightOnAirship = false,
            KillButtonOutlineColor = RoleColor,
            RoleHintType = RoleHintType.RoleTab,
            GhostRole = RoleTypes.CrewmateGhost,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            CanUseSabotage = false,
            TasksCountForProgress = false,
            HideSettings = false,
            CanModifyChance = true
        };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<PranksterGameOver>();
    }
}