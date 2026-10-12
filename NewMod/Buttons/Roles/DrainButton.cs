using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public sealed class DrainButton : CustomActionButton
{
    private PlayerControl _target;
    private bool _breachMode;

    public override string Name => "Siphon";
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<EnergyThiefOptions>.Instance.SiphonCooldown);
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.Siphon;

    public override bool Enabled(RoleBehaviour role)
    {
        return AbilityExchangeGE.HasRoleAbility<EnergyThief>(role, PlayerControl.LocalPlayer.PlayerId);
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        base.SetActive(visible, role);
        if (visible && Enabled(role))
            return;

        if (_target)
            _target.cosmetics.SetOutline(false, new Nullable<Color>(Color.magenta));
        _target = null;
        if (_breachMode)
        {
            _breachMode = false;
            OverrideName(Name);
        }
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || EnergyThief.BreachActive)
            return false;

        var player = PlayerControl.LocalPlayer;
        var ready = EnergyThief.IsReady(player.PlayerId);

        if (ready != _breachMode)
        {
            _breachMode = ready;
            OverrideName(ready ? "GRID BREACH" : "Siphon");
        }

        if (ready)
        {
            if (_target)
            {
                _target.cosmetics.SetOutline(false, new Nullable<Color>(Color.magenta));
                _target = null;
            }

            return EnergyThief.NodePositionIndex >= 0 && Vector2.Distance(player.GetTruePosition(), EnergyThief.GetNodePosition()) <= OptionGroupSingleton<EnergyThiefOptions>.Instance.GridBreachRadius;
        }

        if (EnergyThief.TetherTargets.ContainsKey(player.PlayerId))
            return false;

        var previous = _target;
        _target = player.GetClosestPlayer(true, OptionGroupSingleton<EnergyThiefOptions>.Instance.SiphonRange, false, false, candidate => !candidate.Data.IsDead && !candidate.Data.Disconnected && (!EnergyThief.HarvestedTargets.TryGetValue(player.PlayerId, out var harvested) || !harvested.Contains(candidate.PlayerId)));

        if (previous && previous != _target)
            previous.cosmetics.SetOutline(false, new Nullable<Color>(Color.magenta));

        if (_target)
            _target.cosmetics.SetOutline(true, new Nullable<Color>(Color.magenta));

        return _target;
    }

    protected override void OnClick()
    {
        if (_breachMode)
        {
            EnergyThief.RpcRequestBreach(PlayerControl.LocalPlayer);
            return;
        }

        _target.cosmetics.SetOutline(false, new Nullable<Color>(Color.magenta));
        EnergyThief.RpcRequestSiphon(PlayerControl.LocalPlayer, _target);
        _target = null;
    }
}
