using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.EnergyThiefOptions;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using NewMod.Utilities;

namespace NewMod.Buttons;
public class DrainButton : CustomActionButton<PlayerControl>
{
    public override string Name => "DRAIN";
    public override float Cooldown => OptionGroupSingleton<EnergyThiefOptions>.Instance.DrainCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<EnergyThiefOptions>.Instance.DrainMaxUses;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override float EffectDuration => 0f;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;
    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, false, p => !p.Data.IsDead && !p.Data.Disconnected);
    }
    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.magenta));
    }
    public override bool IsTargetValid(PlayerControl? target)
    {
        return true;
    }
    public override bool Enabled(RoleBehaviour role)
    {
        return role is EnergyThief;
    }
    protected override void OnClick()
    {
        PendingEffectManager.AddPendingEffect(Target);

        Utils.RecordDrainCount(PlayerControl.LocalPlayer);

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage($"The Drain effect will be applied to {Target.Data.PlayerName} after the meeting ends.");
        }
        Utils.waitingPlayers.Add(PlayerControl.LocalPlayer);
    }

    public override bool CanUse()
    {
        if (Utils.waitingPlayers.Contains(PlayerControl.LocalPlayer))
        {
            return false;
        }
        return base.CanUse();
    }
}