using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class ArbitratorLeverageButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    private PlayerControl _target;

    public override string Name => "Leverage";
    public override float Cooldown => OptionGroupSingleton<ArbitratorOptions>.Instance.LeverageCooldown;

    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.LeverageButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is ArbitratorRole;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || MeetingHud.Instance || !ArbitratorRole.LastVotes.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
        {
            _target = null;
            return false;
        }

        _target = null;

        var position = PlayerControl.LocalPlayer.GetTruePosition();
        var range = OptionGroupSingleton<ArbitratorOptions>.Instance.LeverageRange;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == PlayerControl.LocalPlayer || player.Data.IsDead || player.Data.Disconnected || !ArbitratorRole.LastVotes.ContainsKey(player.PlayerId)) continue;

            var distance = Vector2.Distance(position, player.GetTruePosition());

            if (distance > range)
                continue;

            range = distance;
            _target = player;
        }

        return _target;
    }

    protected override void OnClick()
    {
        if (!_target)
            return;

        var ownVote = ArbitratorRole.LastVotes[PlayerControl.LocalPlayer.PlayerId];

        var targetVote = ArbitratorRole.LastVotes[_target.PlayerId];

        var votedWith = ownVote == targetVote;

        var result = votedWith ? "<color=#66D17A>WITH</color>" : "<color=#FF6868>AGAINST</color>";

        Coroutines.Start(CoroutinesHelper.CoNotify($"{_target.Data.PlayerName} voted {result} you last meeting."));
    }
}