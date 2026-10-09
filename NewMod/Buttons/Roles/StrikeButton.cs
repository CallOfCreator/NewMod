using NewMod.Modifiers.S1;
using System.Collections;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class StrikeButton : CustomActionButton<PlayerControl>, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Aggression;
    public override string Name => "Strike";
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<PulseBladeOptions>.Instance.StrikeCooldown);
    public override float EffectDuration => OptionGroupSingleton<PulseBladeOptions>.Instance.ChargeDuration;
    public override float Distance => OptionGroupSingleton<PulseBladeOptions>.Instance.StrikeRange;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.StrikeButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PulseBlade;
    }

    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(false, Distance, predicate: player => !player.inVent);
    }

    public override void SetOutline(bool active)
    {
        if (Target)
            Target.cosmetics.SetOutline(active && !EffectActive, new Il2CppSystem.Nullable<Color>(Color.red));
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        OverrideName(EffectActive ? "Charging" : Name);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        RpcPulseStrike(player, (Target.GetTruePosition() - player.GetTruePosition()).normalized);
    }

    [MethodRpc((uint)CustomRPC.Dash)]
    public static void RpcPulseStrike(PlayerControl source, Vector2 direction)
    {
        if (source.Data.Role is not PulseBlade || source.Data.IsDead || source.inVent || MeetingHud.Instance || ExileController.Instance)
            return;

        OverclockedModifier.Pulse(source);
        Coroutines.Start(DoPulseStrike(source, direction.normalized));
    }

    public static IEnumerator DoPulseStrike(PlayerControl player, Vector2 direction)
    {
        var options = OptionGroupSingleton<PulseBladeOptions>.Instance;
        if (player.AmOwner)
        {
            player.moveable = false;
            player.MyPhysics.body.velocity = Vector2.zero;
        }

        var end = Time.time + options.ChargeDuration;
        while (player && !player.Data.IsDead && !MeetingHud.Instance && Time.time < end)
        {
            player.cosmetics.currentBodySprite.BodySprite.UpdateOutline(Color.red);
            yield return null;
        }

        if (!player)
            yield break;
        player.cosmetics.currentBodySprite.BodySprite.UpdateOutline(null);
        if (!player.Data.IsDead && !MeetingHud.Instance && !ExileController.Instance && Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), player.GetTruePosition()) <= options.StrikeRange && !PhysicsHelpers.AnyNonTriggersBetween(player.GetTruePosition(), direction, options.DashSpeed * Time.fixedDeltaTime + 0.2f, Constants.ShipAndObjectsMask))
            SoundManager.Instance.PlaySound(NewModAsset.StrikeSound.LoadAsset(), false);
        if (!player.AmOwner)
            yield break;

        var remaining = options.StrikeRange;
        while (!player.Data.IsDead && !MeetingHud.Instance && !ExileController.Instance && remaining > 0f)
        {
            var step = Mathf.Min(options.DashSpeed * Time.fixedDeltaTime, remaining);
            var position = player.GetTruePosition();
            if (PhysicsHelpers.AnyNonTriggersBetween(position, direction, step + 0.2f, Constants.ShipAndObjectsMask))
                break;

            player.MyPhysics.body.velocity = direction * (step / Time.fixedDeltaTime);
            remaining -= step;
            var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p != player && !p.Data.IsDead && !p.Data.Disconnected && !p.inVent && !p.Data.Role.IsImpostor && Vector2.Distance(position, p.GetTruePosition()) <= 0.4f && !PhysicsHelpers.AnythingBetween(position, p.GetTruePosition(), Constants.ShipAndObjectsMask, false));
            if (target)
            {
                player.RpcCustomMurder(target, MeetingCheck.OutsideMeeting, resetKillTimer: false, teleportMurderer: false, showKillAnim: false);
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        player.MyPhysics.body.velocity = Vector2.zero;
        end = Time.time + options.RecoveryDuration;
        while (!player.Data.IsDead && !MeetingHud.Instance && !ExileController.Instance && Time.time < end)
            yield return null;
        if (!MeetingHud.Instance && !ExileController.Instance)
            player.moveable = true;
    }
}