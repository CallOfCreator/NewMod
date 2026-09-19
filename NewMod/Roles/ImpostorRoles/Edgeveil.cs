using MiraAPI.Roles;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class Edgeveil : ImpostorRole, INewModRole
{
    public string RoleName => "Edgeveil";
    public string RoleDescription => "Launch a short lethal Arc in front of you.";
    public string RoleLongDescription => "Stand still to charge, then launch a slash in the direction you face.\nOpponents can dodge it or take cover behind a wall. Misses still use the cooldown.";
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