using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1;

[MiraIgnore]
public class Voidwalker : ImpostorRole, INewModRole
{
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Voidwalker");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Voidwalker.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Voidwalker.TabDescription");

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            OptionsScreenshot = MiraAssets.Empty,
            Icon = NewModAsset.VoidwalkerIcon,
            CanGetKilled = true,
            UseVanillaKillButton = true,
            CanUseVent = true,
            TasksCountForProgress = false,
            CanUseSabotage = true,
            MaxRoleCount = 1,
            RoleHintType = RoleHintType.RoleTab,
        };

    public Color RoleColor => new(0.3f, 0f, 0.5f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Rift;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var options = OptionGroupSingleton<VoidwalkerOptions>.Instance;

        tabText.AppendLine();
        tabText.AppendLine(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Voidwalker.Tab.Timing"), options.VoidTime, options.EnterVoidCooldown));

        tabText.AppendLine(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Voidwalker.Tab.PhasedRules"));

        tabText.AppendLine(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Voidwalker.Tab.ReturnRules"));

        return tabText;
    }
}
