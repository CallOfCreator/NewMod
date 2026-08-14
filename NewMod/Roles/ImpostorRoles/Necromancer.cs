using System.Collections.Generic;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class NecromancerRole : ImpostorRole, ICustomRole
{
    public static Dictionary<byte, byte> RevivedPlayers = new();

    public TeamIntroConfiguration TeamConfiguration => new()
    {
        IntroTeamDescription = RoleDescription,
        IntroTeamColor = RoleColor
    };

    public string RoleName => "Necromancer";
    public string RoleDescription => "You can revive dead players who weren't killed by you";
    public string RoleLongDescription => "As the Necromancer, you possess a unique and powerful ability: the power to bring one dead player back to life. However,\nyou can only revive someone who wasn't killed by you";
    public Color RoleColor => Palette.AcceptedGreen.FindAlternateColor();
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = NewModAsset.ReviveIcon,
        OptionsScreenshot = NewModAsset.Banner,
        MaxRoleCount = 3
    };
}