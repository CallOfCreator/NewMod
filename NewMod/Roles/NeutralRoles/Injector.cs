using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using NewMod.Options.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class InjectorRole : ImpostorRole, INewModRole
{
    public TeamIntroConfiguration TeamConfiguration => new() { IntroTeamDescription = RoleDescription, IntroTeamColor = RoleColor };

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole.TabDescription");
    public Color RoleColor => new(0.9f, 0.3f, 0.1f);
    public NewModFaction Faction => NewModFaction.Entropy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            Icon = NewModAsset.InjectIcon,
            MaxRoleCount = 1,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = false,
            DefaultChance = 35,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab,
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        text.AppendLine();
        var options = OptionGroupSingleton<InjectorOptions>.Instance;
        text.AppendLine(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole.Tab.Progress"), InjectorUtilities.SampleCount(PlayerControl.LocalPlayer.PlayerId), options.RequiredInjectCount, options.ObservationDuration, options.CollectionWindow, options.SubmissionDuration));
        return text;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<InjectorGameOver>();
    }

    public enum SerumType
    {
        Adrenaline,
        Sedative,
    }
}
