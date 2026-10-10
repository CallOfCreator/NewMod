using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class ManifestButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override string Name => "Manifest";
    public override float Cooldown => 5f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Collector;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Collector.VictoryArmed.Contains(PlayerControl.LocalPlayer.PlayerId) || !Collector.Inventories.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var inventory))
            return false;

        return Collector.CanManifest(inventory);
    }

    protected override void OnClick()
    {
        Collector.RpcRequestManifest(PlayerControl.LocalPlayer);
    }
}
