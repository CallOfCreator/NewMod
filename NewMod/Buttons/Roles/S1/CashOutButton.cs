using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class CashOutButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Aggression;
    public override string Name => "Cash Out";
    public override float Cooldown => OptionGroupSingleton<BountyOptions>.Instance.CashOutCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Bounty;
    }

    public override bool CanUse()
    {
        var bountyId = PlayerControl.LocalPlayer.PlayerId;
        if (!base.CanUse() || !Bounty.Contracts.TryGetValue(bountyId, out var contract) || contract.Phase != RoleLogic.BountyPhase.Collection)
            return false;

        var target = Utils.PlayerById(contract.TargetId);
        return !target.Data.IsDead && !target.Data.Disconnected && Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), target.GetTruePosition()) <= OptionGroupSingleton<BountyOptions>.Instance.CashOutRange;
    }

    protected override void OnClick()
    {
        Bounty.RpcRequestCashOut(PlayerControl.LocalPlayer);
    }
}
