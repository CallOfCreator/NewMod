using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using DeadwireRole = NewMod.Roles.ImpostorRoles.S1.Deadwire;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class DeadlockButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    private PlayerControl _target;

    public override string Name => "Deadlock";
    public override float InitialCooldown => 0f;
    public override float Cooldown => OptionGroupSingleton<DeadwireOptions>.Instance.DeadlockCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<DeadwireOptions>.Instance.DeadlockUses;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadlockButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is DeadwireRole;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || DeadwireRole.Records.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
            return false;

        _target = null;
        var distance = OptionGroupSingleton<DeadwireOptions>.Instance.DeadlockRange;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == PlayerControl.LocalPlayer || player.Data.IsDead || player.Data.Disconnected || player.Data.Role is DeadwireRole)
                continue;

            var candidateDistance = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), player.GetTruePosition());
            if (candidateDistance >= distance)
                continue;

            distance = candidateDistance;
            _target = player;
        }

        return _target;
    }

    protected override void OnClick()
    {
        DeadwireRole.RpcRequestDeadlock(PlayerControl.LocalPlayer, _target);
    }
}