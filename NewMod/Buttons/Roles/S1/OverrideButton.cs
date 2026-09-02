using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using DeadwireRole = NewMod.Roles.ImpostorRoles.S1.Deadwire;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class OverrideButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override string Name => "Override";
    public override float InitialCooldown => 0f;
    public override float Cooldown => OptionGroupSingleton<DeadwireOptions>.Instance.OverrideCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.OverrideButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is DeadwireRole;
    }

    public override bool CanUse()
    {
        return base.CanUse() && DeadwireRole.Records.ContainsKey(PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        DeadwireRole.RpcRequestOverride(PlayerControl.LocalPlayer);
    }
}