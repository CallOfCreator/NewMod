using MiraAPI.GameModes;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GameModes.WraithSiegeGamemode.Buttons;

[MiraIgnore]
public sealed class WraithSiegeReviveButton : CustomActionButton
{
    public override string Name => "REVIVE";
    public override float Cooldown => 0.5f;
    public override int MaxUses => 0;
    public override float EffectDuration => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.NecromancerButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return AmongUsClient.Instance.IsGameStarted && CustomGameModeManager.ActiveMode is WraithSiege mode && !mode.Ended && !role.IsImpostor;
    }

    public override bool CanUse()
    {
        return base.CanUse() && CustomGameModeManager.ActiveMode is WraithSiege mode && !PlayerControl.LocalPlayer.Data.IsDead && mode.Tickets > 0 && mode.GetClosestReviverBody(PlayerControl.LocalPlayer);
    }

    protected override void OnClick()
    {
        var body = WraithSiege.Instance.GetClosestReviverBody(PlayerControl.LocalPlayer);

        if (body)
            WraithSiege.RpcRequestRevive(PlayerControl.LocalPlayer, body.ParentId);
    }
}