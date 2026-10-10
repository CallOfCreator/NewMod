using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using NewMod.Options.Roles;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class PulseBlade : ImpostorRole, INewModRole
{
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.PulseBlade");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.PulseBlade.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.PulseBlade.TabDescription");
    public Color RoleColor => new(1f, 0.25f, 0.25f);
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
            Icon = NewModAsset.StrikeIcon,
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var options = OptionGroupSingleton<PulseBladeOptions>.Instance;
        tabText.AppendLine(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.PulseBlade.Tab.Timing"), options.ChargeDuration, options.RecoveryDuration));
        tabText.AppendLine(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.PulseBlade.Tab.DashRules"));
        return tabText;
    }
}
