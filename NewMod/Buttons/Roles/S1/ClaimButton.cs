using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.RoleLogic;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class ClaimButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    private PlayerControl _target;

    public override string Name => "Claim";
    public override float Cooldown => OptionGroupSingleton<UsurperOptions>.Instance.ClaimCooldown;
    public override int MaxUses => 1;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Usurper;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || !Usurper.States.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var state) || state.Phase != UsurperCrownPhase.Unclaimed)
            return false;

        _target = null;
        var range = OptionGroupSingleton<UsurperOptions>.Instance.ClaimRange;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == PlayerControl.LocalPlayer || player.Data.IsDead || player.Data.Disconnected || player.Data.Role is Usurper)
                continue;

            var distance = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), player.GetTruePosition());
            if (distance >= range)
                continue;

            range = distance;
            _target = player;
        }

        return _target;
    }

    protected override void OnClick()
    {
        Usurper.RpcRequestClaim(PlayerControl.LocalPlayer, _target);
    }
}
