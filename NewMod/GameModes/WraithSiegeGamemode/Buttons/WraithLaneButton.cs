using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.GameModes.WraithSiegeGamemode.Options;
using UnityEngine;

namespace NewMod.GameModes.WraithSiegeGamemode.Buttons;

[MiraIgnore]
public abstract class WraithLaneButton : CustomActionButton
{
    protected abstract WraithLane Lane { get; }

    public override float Cooldown => OptionGroupSingleton<WraithSiegeOptions>.Instance.SummonCooldown;
    public override int MaxUses => 0;
    public override float EffectDuration => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.CallWraith;

    public override bool Enabled(RoleBehaviour role)
    {
        return AmongUsClient.Instance.IsGameStarted && CustomGameModeManager.ActiveMode is WraithSiege mode && !mode.Ended && role.IsImpostor;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || CustomGameModeManager.ActiveMode is not WraithSiege mode)
            return false;

        var player = PlayerControl.LocalPlayer;
        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        return !player.Data.IsDead && mode.NpcPool > 0 && mode.WraithEnergy >= options.SummonCost && mode.GetActiveNpcCount(player.PlayerId) < (int)options.MaxActiveNpcsPerWraith;
    }

    protected override void OnClick()
    {
        WraithSiege.RpcRequestSummon(PlayerControl.LocalPlayer, (byte)Lane);
    }
}

[MiraIgnore]
public sealed class TopWraithLaneButton : WraithLaneButton
{
    public override string Name => "TOP";
    protected override WraithLane Lane => WraithLane.Top;
}

[MiraIgnore]
public sealed class MidWraithLaneButton : WraithLaneButton
{
    public override string Name => "MID";
    protected override WraithLane Lane => WraithLane.Mid;
}

[MiraIgnore]
public sealed class BottomWraithLaneButton : WraithLaneButton
{
    public override string Name => "BOTTOM";
    protected override WraithLane Lane => WraithLane.Bottom;
}