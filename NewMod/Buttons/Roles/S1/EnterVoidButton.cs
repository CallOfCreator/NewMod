using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles.S1;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class EnterVoid : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Mobility;
    public override string Name => "Enter Void";
    public override float Cooldown => OptionGroupSingleton<VoidwalkerOptions>.Instance.EnterVoidCooldown;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.EnterVoid;

    public override bool IsEffectCancellable()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Voidwalker;
    }

    public override float EffectDuration => OptionGroupSingleton<VoidwalkerOptions>.Instance.VoidTime;

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<InVoid>();
        OverrideName("Exit Void");
    }

    public override void OnEffectEnd()
    {
        PlayerControl.LocalPlayer.RpcRemoveModifier<InVoid>();
        OverrideName("Enter Void");
    }
}