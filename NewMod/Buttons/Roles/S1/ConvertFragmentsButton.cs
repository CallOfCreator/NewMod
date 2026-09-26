using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class ConvertFragmentsButton : CustomActionButton
{
    public override string Name => "Convert";
    public override float Cooldown => 1f;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.LeverageButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Collector;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !Collector.VictoryArmed.Contains(PlayerControl.LocalPlayer.PlayerId) && Collector.Inventories.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var inventory) && Collector.CanConvert(inventory, (int)OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost);
    }

    protected override void OnClick()
    {
        Collector.RpcRequestConvert(PlayerControl.LocalPlayer);
    }
}