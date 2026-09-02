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
public sealed class HarvestButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    private uint _fragmentId;

    public override string Name => "Harvest";
    public override float Cooldown => OptionGroupSingleton<CollectorOptions>.Instance.HarvestCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Collector;
    }

    public override bool CanUse()
    {
        if (!base.CanUse())
            return false;

        var range = OptionGroupSingleton<CollectorOptions>.Instance.HarvestRange;
        foreach (var pair in Collector.Fragments)
        {
            var distance = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), pair.Value.Position);
            if (distance >= range)
                continue;

            range = distance;
            _fragmentId = pair.Key;
        }

        return range < OptionGroupSingleton<CollectorOptions>.Instance.HarvestRange;
    }

    protected override void OnClick()
    {
        Collector.RpcRequestHarvest(PlayerControl.LocalPlayer, _fragmentId);
    }
}