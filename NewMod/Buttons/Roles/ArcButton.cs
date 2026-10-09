using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class ArcButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Aggression;

    public override string Name => "Arc";

    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<EdgeveilOptions>.Instance.SlashCooldown);

    public override float EffectDuration => OptionGroupSingleton<EdgeveilOptions>.Instance.ChargeDuration;

    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public override LoadableAsset<Sprite> Sprite => NewModAsset.Slash;

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        OverrideName(EffectActive ? "Charging" : Name);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;

        var flipLeft = player.cosmetics.currentBodySprite.BodySprite.flipX;
        var dir = flipLeft ? Vector2.left : Vector2.right;

        RpcArc(player, dir);
    }

    [MethodRpc((uint)CustomRPC.EdgeveilArc)]
    public static void RpcArc(PlayerControl source, Vector2 direction)
    {
        if (source.Data.Role is not Edgeveil || source.Data.IsDead || source.inVent || MeetingHud.Instance)
            return;
        OverclockedModifier.Pulse(source);
        Coroutines.Start(CoArc(source, direction.normalized));
    }

    public static IEnumerator CoArc(PlayerControl source, Vector2 direction)
    {
        var options = OptionGroupSingleton<EdgeveilOptions>.Instance;
        if (source.AmOwner)
        {
            source.moveable = false;
            source.MyPhysics.body.velocity = Vector2.zero;
        }

        var end = Time.time + options.ChargeDuration;
        while (source && !source.Data.IsDead && !MeetingHud.Instance && Time.time < end)
        {
            source.cosmetics.currentBodySprite.BodySprite.UpdateOutline(Color.red);
            yield return null;
        }

        if (!source)
            yield break;
        source.cosmetics.currentBodySprite.BodySprite.UpdateOutline(null);
        if (source.AmOwner && !MeetingHud.Instance && !ExileController.Instance)
            source.moveable = true;
        if (source.Data.IsDead || MeetingHud.Instance || ExileController.Instance)
            yield break;

        var tray = SlashTray.CreateTray();
        var position = source.GetTruePosition();
        tray.transform.SetPositionAndRotation(new Vector3(position.x, position.y, source.transform.position.z), Quaternion.identity);
        var scale = tray.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction.x < 0f ? -1f : 1f);
        tray.transform.localScale = scale;
        tray.Owner = source;
        tray.SetMotion(direction, options.SlashSpeed);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Edgeveil;
    }
}