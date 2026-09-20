using System.Collections.Generic;
using MiraAPI.Translation;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class NecromancerRole : ImpostorRole, ICustomRole
{
    public static Dictionary<byte, byte> RevivedPlayers = new();

    public TeamIntroConfiguration TeamConfiguration => new() { IntroTeamDescription = RoleDescription, IntroTeamColor = RoleColor };

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.NecromancerRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.NecromancerRole.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.NecromancerRole.TabDescription");
    public Color RoleColor => Palette.AcceptedGreen.FindAlternateColor();
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Impostor;

    public CustomRoleConfiguration Configuration => new(this) { Icon = NewModAsset.ReviveIcon, OptionsScreenshot = NewModAsset.Banner, MaxRoleCount = 1 };
}