using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using NewMod.Options.Roles.S1;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles.S1;

public class VerifierRole : CrewmateRole, INewModRole
{
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.S1.VerifierRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.S1.VerifierRole.IntroBlurb");

    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.S1.VerifierRole.TabDescription");

    public Color RoleColor => new Color32(88, 232, 190, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = true,
            Icon = NewModAsset.VerifyIcon,
            MaxRoleCount = 1,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab,
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var radius = OptionGroupSingleton<VerifierOptions>.Instance.NearBodyRadius;
        return tabText.Append(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.S1.VerifierRole.Tab.Details"), RoleColor.ToTextColor(), radius));
    }
}
