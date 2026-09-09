using MiraAPI.Roles;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class Edgeveil : ImpostorRole, INewModRole
{
    public string RoleName => "Edgeveil";
    public string RoleDescription => "Launch a short lethal Arc in front of you.";
    public string RoleLongDescription => "Fire a moving slash in the direction you face.\nIt stops at its range or after hitting the configured victim limit.";
    public Color RoleColor => new(0.90f, 0.20f, 0.35f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Apex;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            MaxRoleCount = 1,
            Icon = NewModAsset.SlashIcon
        };
}