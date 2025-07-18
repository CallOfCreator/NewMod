using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class InjectorRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Injector";
    public string RoleDescription => "Inject other players with serums that alter their abilities";
    public string RoleLongDescription => "You hold unstable serums. Inject. Distort. Dominate";
    public Color RoleColor => new(0.9f, 0.3f, 0.1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = MiraAssets.Empty,
        OptionsScreenshot = NewModAsset.Banner,
        MaxRoleCount = 1,
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        DefaultChance = 50,
        DefaultRoleCount = 1,
        CanModifyChance = true,
        RoleHintType = RoleHintType.RoleTab
    };
    public TeamIntroConfiguration TeamConfiguration => new()
    {
        IntroTeamDescription = RoleDescription,
        IntroTeamColor = RoleColor
    };
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == (GameOverReason)NewModEndReasons.InjectorWin;
    }
}
