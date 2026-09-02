using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public sealed class EnergyGroundButton : CustomActionButton
{
    public override string Name => "Ground";
    public override float Cooldown => 0f;
    public override float EffectDuration => OptionGroupSingleton<EnergyThiefOptions>.Instance.GroundDuration;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.GroundAbilityButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return true;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;

        if (!player)
            return false;

        return EnergyThief.BreachActive && player.PlayerId != EnergyThief.NodeOwnerId && Vector2.Distance(player.GetTruePosition(), EnergyThief.GetNodePosition()) <= OptionGroupSingleton<EnergyThiefOptions>.Instance.GridBreachRadius && base.CanUse();
    }

    protected override void OnClick()
    {
        //indeed nothing
    }

    public override void OnEffectEnd()
    {
        if (CanUse())
            EnergyThief.RpcRequestGround(PlayerControl.LocalPlayer);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        var visible = EnergyThief.BreachActive && playerControl.PlayerId != EnergyThief.NodeOwnerId;
        Button.ToggleVisible(visible);

        var holdingInRange = visible && playerControl.moveable && Vector2.Distance(playerControl.GetTruePosition(), EnergyThief.GetNodePosition()) <= OptionGroupSingleton<EnergyThiefOptions>.Instance.GridBreachRadius;

        if (EffectActive && !holdingInRange)
        {
            EffectActive = false;
            Timer = 0f;
        }
    }
}