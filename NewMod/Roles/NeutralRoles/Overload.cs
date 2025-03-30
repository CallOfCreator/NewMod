using System;
using System.Reflection;
using System.Linq;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events;
using MiraAPI.Hud;
using MiraAPI.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace NewMod.Roles.NeutralRoles;
public class OverloadRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Overload";
    public string RoleDescription => "Absorb, Consume, Devour, Overload.";
    public string RoleLongDescription => "You are the Overload, an impostor who thrives on the abilities of the fallen. Each ejected player fuels your chaos, granting you their power";
    public Color RoleColor => new Color(0.6f, 0.1f, 0.3f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;
    public static int AbsorbedAbilityCount = 0;
    public CustomRoleConfiguration Configuration => new(this)
    {
        AffectedByLightOnAirship = false,
        CanGetKilled = true,
        UseVanillaKillButton = true,
        CanUseVent = true,
        CanUseSabotage = false,
        TasksCountForProgress = false,
        ShowInFreeplay = true,
        HideSettings = false,
        OptionsScreenshot = null,
        Icon = null,
    };
}
