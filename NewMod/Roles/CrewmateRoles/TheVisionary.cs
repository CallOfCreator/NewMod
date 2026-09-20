using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Utilities;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class TheVisionary : CrewmateRole, INewModRole
{
    public RoleOptionsGroup RoleOptionGroup { get; } = RoleOptionsGroup.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.TheVisionary");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.TheVisionary.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.TheVisionary.TabDescription");
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

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var options = OptionGroupSingleton<VisionaryOptions>.Instance;
        return INewModRole.GetRoleTabText(this).Append(string.Format(MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.TheVisionary.Tab.Details"), RoleColor.ToTextColor(), options.MaxScreenshots, options.CaptureDelay));
    }
}