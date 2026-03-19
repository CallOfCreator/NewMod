using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class NecromancerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Necromancer";
    public string RoleDescription => "You can revive dead players who weren't killed by you";
    public string RoleLongDescription => "As the Necromancer, you possess a unique and powerful ability: the power to bring one dead player back to life. However,\nyou can only revive someone who wasn't killed by you";
    public Color RoleColor => Palette.AcceptedGreen.FindAlternateColor();
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = MiraAssets.Empty,
        OptionsScreenshot = NewModAsset.Banner,
        MaxRoleCount = 3,
    };
    public TeamIntroConfiguration TeamConfiguration => new()
    {
        IntroTeamDescription = RoleDescription,
        IntroTeamColor = RoleColor
    };
    public override bool DidWin(GameOverReason reason)
    {
        if (reason == (GameOverReason)NewModEndReasons.TyrantWin ||
            reason == (GameOverReason)NewModEndReasons.ShadeWin ||
            reason == (GameOverReason)NewModEndReasons.WraithCallerWin ||
            reason == (GameOverReason)NewModEndReasons.SpecialAgentWin ||
            reason == (GameOverReason)NewModEndReasons.PranksterWin ||
            reason == (GameOverReason)NewModEndReasons.EnergyThiefWin ||
            reason == (GameOverReason)NewModEndReasons.InjectorWin ||
            reason == (GameOverReason)NewModEndReasons.DoubleAgentWin)
        {
            return false;
        }

        return base.DidWin(reason);
    }
}
