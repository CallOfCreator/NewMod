using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class StrikeButton : CustomActionButton
{
    public override string Name => "Strike";
    public override float Cooldown => OptionGroupSingleton<PulseBladeOptions>.Instance.StrikeCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<PulseBladeOptions>.Instance.MaxStrikeUses;
    public override float EffectDuration => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.StrikeButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PulseBlade;
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;

        var target = PlayerControl.AllPlayerControls.ToArray().Where(p => p != player && !p.Data.IsDead && !p.Data.Disconnected && !p.inVent).OrderBy(p => Vector2.Distance(player.GetTruePosition(), p.GetTruePosition())).FirstOrDefault(p => Vector2.Distance(player.GetTruePosition(), p.GetTruePosition()) <= OptionGroupSingleton<PulseBladeOptions>.Instance.StrikeRange);

        if (target)
            RpcPulseStrike(player, target);
    }

    [MethodRpc((uint)CustomRPC.Dash)]
    public static void RpcPulseStrike(PlayerControl source, PlayerControl target)
    {
        Coroutines.Start(DoPulseStrike(source, target));
    }

    public static IEnumerator DoPulseStrike(PlayerControl killer, PlayerControl target)
    {
        var originalSpeed = killer.MyPhysics.Speed;
        var dashSpeed = OptionGroupSingleton<PulseBladeOptions>.Instance.DashSpeed;

        killer.moveable = false;
        killer.MyPhysics.inputHandler.enabled = false;
        killer.MyPhysics.Speed = dashSpeed;

        while (Vector2.Distance(killer.GetTruePosition(), target.GetTruePosition()) > 0.1f)
        {
            var direction = target.GetTruePosition() - killer.GetTruePosition();
            killer.MyPhysics.SetNormalizedVelocity(direction.normalized);

            if (killer.MyPhysics.TrueSpeed * Time.fixedDeltaTime >= direction.magnitude)
                break;

            yield return new WaitForFixedUpdate();
        }

        killer.MyPhysics.SetNormalizedVelocity(Vector2.zero);
        killer.MyPhysics.Speed = originalSpeed;
        killer.MyPhysics.inputHandler.enabled = true;
        killer.moveable = true;

        SoundManager.Instance.PlaySound(NewModAsset.StrikeSound.LoadAsset(), false);

        killer.RpcCustomMurder(target, true, false, true, false, false, false);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source.Data.Role is not PulseBlade)
            return;

        Utils.RegisterStrikeKill(evt.Source, evt.Target);

        if (evt.Source.AmOwner)
        {
            var notification = Helpers.CreateAndShowNotification($"Perfect kill {evt.Target.Data.PlayerName} eliminated", new Color(1f, 0.25f, 0.25f), spr: NewModAsset.StrikeIcon.LoadAsset());

            notification.Text.SetOutlineThickness(0.3f);
        }

        if (evt.DeadBody)
            Coroutines.Start(CoHideBody(evt.DeadBody));
    }

    private static IEnumerator CoHideBody(DeadBody body)
    {
        body.gameObject.SetActive(false);

        yield return new WaitForSeconds(OptionGroupSingleton<PulseBladeOptions>.Instance.HideBodyDuration);

        if (body)
            body.gameObject.SetActive(true);
    }
}