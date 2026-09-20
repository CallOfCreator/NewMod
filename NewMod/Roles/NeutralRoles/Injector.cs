using MiraAPI.Translation;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class InjectorRole : ImpostorRole, ICustomRole
{
    public TeamIntroConfiguration TeamConfiguration => new() { IntroTeamDescription = RoleDescription, IntroTeamColor = RoleColor };

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.InjectorRole.TabDescription");
    public Color RoleColor => new(0.9f, 0.3f, 0.1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            Icon = NewModAsset.InjectIcon,
            OptionsScreenshot = NewModAsset.Banner,
            MaxRoleCount = 1,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = false,
            DefaultChance = 35,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<InjectorGameOver>();
    }
}