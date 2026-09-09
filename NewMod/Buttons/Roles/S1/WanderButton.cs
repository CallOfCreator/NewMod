using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class WanderButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Mobility;
    public override string Name => "Wander";
    public override float Cooldown => OptionGroupSingleton<NomadOptions>.Instance.WanderCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.WanderButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Nomad;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Nomad.CanWander.Contains(PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        Nomad.RpcRequestWander(PlayerControl.LocalPlayer);
    }
}