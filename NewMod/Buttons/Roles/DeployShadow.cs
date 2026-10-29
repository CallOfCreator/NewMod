using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Components;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class DeployShadow : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override string Name => "Deploy Shadow";
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<ShadeOptions>.Instance.Cooldown);
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DeployShadow;

    public override bool Enabled(RoleBehaviour role)
    {
        return AbilityExchangeGE.HasRoleAbility<Shade>(role, PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;

        var position = player.GetTruePosition();

        var radius = OptionGroupSingleton<ShadeOptions>.Instance.Radius;
        var duration = OptionGroupSingleton<ShadeOptions>.Instance.Duration;

        ShadowZone.RpcDeployZone(player, position, radius, duration);
    }
}
