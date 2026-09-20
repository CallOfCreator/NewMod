using MiraAPI.Translation;
using MiraAPI.Roles;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class Edgeveil : ImpostorRole, INewModRole
{
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Edgeveil");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Edgeveil.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Edgeveil.TabDescription");
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