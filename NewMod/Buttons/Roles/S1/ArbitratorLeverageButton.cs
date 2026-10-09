using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class ArbitratorLeverageButton : CustomActionButton<PlayerControl>, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    public override string Name => "Leverage";
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<ArbitratorOptions>.Instance.LeverageCooldown);
    public override float Distance => OptionGroupSingleton<ArbitratorOptions>.Instance.LeverageRange;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.LeverageButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is ArbitratorRole;
    }

    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(false, Distance, predicate: player => !player.inVent && ArbitratorRole.LastVotes.ContainsKey(player.PlayerId));
    }

    public override bool CanUse()
    {
        return base.CanUse() && ArbitratorRole.LastVotes.ContainsKey(PlayerControl.LocalPlayer.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(Color.yellow));
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.HasModifier<OverclockedModifier>())
            OverclockedModifier.RpcRequestPulse(PlayerControl.LocalPlayer);
        var sameVote = ArbitratorRole.LastVotes[PlayerControl.LocalPlayer.PlayerId] == ArbitratorRole.LastVotes[Target.PlayerId];
        Coroutines.Start(CoroutinesHelper.CoNotify(sameVote ? $"{Target.Data.PlayerName} chose the same vote as you last meeting." : $"{Target.Data.PlayerName} chose a different vote last meeting."));
    }
}