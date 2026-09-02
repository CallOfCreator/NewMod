using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public sealed class FeignDeathButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Protection;
    public override string Name => "Feign Death";
    public override float InitialCooldown => 0f;
    public override float Cooldown => OptionGroupSingleton<RevenantOptions>.Instance.FeignDeathCooldown;
    public override int MaxUses => 1;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadBodySprite;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Revenant;
    }

    public override bool CanUse()
    {
        return base.CanUse() && (!Revenant.Phases.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var phase) || phase == Revenant.Phase.Ready);
    }

    protected override void OnClick()
    {
        Revenant.RpcRequestFeign(PlayerControl.LocalPlayer);
    }
}