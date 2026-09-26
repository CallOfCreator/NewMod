using NewMod.Roles.ImpostorRoles.S1;
using MiraAPI.Translation;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using NewMod.Utilities;
using DeadwireRole = NewMod.Roles.ImpostorRoles.S1.Deadwire;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class OverrideButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override string Name => "Override";
    public override float InitialCooldown => 0f;
    public override float Cooldown => OptionGroupSingleton<DeadwireOptions>.Instance.OverrideCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.OverrideButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is DeadwireRole;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || !DeadwireRole.Records.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var record))
            return false;
        if (DeadwireRole.GetResponse(record.Category) is not (Deadwire.Response.TrackActor or Deadwire.Response.JamActor))
            return true;
        var actor = Utils.PlayerById(record.ActorId);
        return actor && !actor.Data.IsDead && !actor.Data.Disconnected;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        OverrideName(DeadwireRole.Records.TryGetValue(playerControl.PlayerId, out var record) ? MiraLocaleManager.Get($"NewMod.Deadwire.Reward.{DeadwireRole.GetResponse(record.Category)}") : Name);
    }

    protected override void OnClick()
    {
        DeadwireRole.RpcRequestOverride(PlayerControl.LocalPlayer);
    }
}