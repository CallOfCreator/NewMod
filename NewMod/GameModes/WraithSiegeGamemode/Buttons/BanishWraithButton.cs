using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.GameModes.WraithSiegeGamemode.Options;
using UnityEngine;

namespace NewMod.GameModes.WraithSiegeGamemode.Buttons;

[MiraIgnore]
public sealed class BanishWraithButton : CustomActionButton
{
    public override string Name => "BANISH";
    public override float Cooldown => OptionGroupSingleton<WraithSiegeOptions>.Instance.BanishCooldown;
    public override int MaxUses => 0;
    public override float EffectDuration => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.WraithSiegeBanish;

    public override bool Enabled(RoleBehaviour role)
    {
        return AmongUsClient.Instance.IsGameStarted && CustomGameModeManager.ActiveMode is WraithSiege mode && !mode.Ended && !role.IsImpostor;
    }

    public override bool CanUse()
    {
        return base.CanUse() && CustomGameModeManager.ActiveMode is WraithSiege mode && !PlayerControl.LocalPlayer.Data.IsDead && mode.GetClosestNpc(PlayerControl.LocalPlayer);
    }

    protected override void OnClick()
    {
        var npc = WraithSiege.Instance.GetClosestNpc(PlayerControl.LocalPlayer);

        if (npc)
            WraithSiege.RpcRequestBanish(PlayerControl.LocalPlayer, npc.NpcId);
    }
}