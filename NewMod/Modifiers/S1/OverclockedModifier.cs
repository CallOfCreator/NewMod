using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Options;
using NewMod.Options.Modifiers;
using NewMod.Options.Roles;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class OverclockedModifier : GameModifier
{
    public float NextLocalPulse;

    public override string ModifierName => "Overclocked";

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.OverclockedAmount;
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<ModifiersOptions>.Instance.OverclockedChance;
    }

    public override string GetDescription()
    {
        var options = OptionGroupSingleton<OverclockedModifierOptions>.Instance;
        return $"Wait {(1f - options.CooldownMultiplier) * 100f:0}% less between uses of your role abilities.\nEach use shows a glowing bubble at your position for {options.PulseDuration:0.##}s, visible to nearby players";
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return SupportsRole(role);
    }

    public override void OnMeetingStart()
    {
        NextLocalPulse = 0f;
    }

    public static bool SupportsRole(RoleBehaviour role)
    {
        return role is Aegis or Beacon or DoubleAgent or Specialist or TheVisionary or WardenRole or Edgeveil or PulseBlade or Deadwire or MirrorBladeRole or Voidwalker or EnergyThief or InjectorRole or Prankster or Shade or Tyrant or WraithCaller or ArbitratorRole or Collector or Nomad or Usurper || role is NecromancerRole && OptionGroupSingleton<NecromancerOption>.Instance.AbilityUses > 1;
    }

    public static float GetCooldown(PlayerControl player, float cooldown)
    {
        if (!player || !player.HasModifier<OverclockedModifier>() || !SupportsRole(player.Data.Role))
            return cooldown;

        return cooldown * OptionGroupSingleton<OverclockedModifierOptions>.Instance.CooldownMultiplier;
    }

    public static void Pulse(PlayerControl player, bool enteringVoid = false)
    {
        if ((!AmongUsClient.Instance.AmHost && !AmongUsClient.Instance.AmLocalHost) || !player || player.Data.IsDead || player.Data.Disconnected || player.inVent || MeetingHud.Instance || ExileController.Instance || !player.Visible || player.cosmetics.currentBodySprite.BodySprite.color.a <= 0f || !SupportsRole(player.Data.Role) || !player.HasModifier<OverclockedModifier>())
            return;

        RpcPulse(PlayerControl.LocalPlayer, player.PlayerId, player.GetTruePosition(), enteringVoid);
    }

    [MethodRpc((uint)CustomRPC.OverclockedRequestPulse)]
    public static void RpcRequestPulse(PlayerControl source)
    {
        if ((!AmongUsClient.Instance.AmHost && !AmongUsClient.Instance.AmLocalHost) || source.Data.IsDead || source.Data.Disconnected || source.inVent || MeetingHud.Instance || ExileController.Instance)
            return;

        var modifier = source.GetModifier<OverclockedModifier>();
        if (modifier == null || Time.time < modifier.NextLocalPulse)
            return;

        var cooldown = source.Data.Role switch
        {
            Beacon => (float)OptionGroupSingleton<BeaconOptions>.Instance.PulseCooldown,
            Specialist => 8f,
            ArbitratorRole => (float)OptionGroupSingleton<ArbitratorOptions>.Instance.LeverageCooldown,
            _ => 0f
        };
        if (cooldown <= 0f)
            return;

        modifier.NextLocalPulse = Time.time + GetCooldown(source, cooldown);
        Pulse(source);
    }

    [MethodRpc((uint)CustomRPC.OverclockedPulse, LocalHandling = RpcLocalHandling.After)]
    public static void RpcPulse(PlayerControl source, byte playerId, Vector2 position, bool enteringVoid)
    {
        if (!source.IsHost())
            return;

        var player = Utils.PlayerById(playerId);
        if (!player || player.inVent || (!enteringVoid && (!player.Visible || player.cosmetics.currentBodySprite.BodySprite.color.a <= 0f)))
            return;

        Coroutines.Start(CoPulse(position));
    }

    public static IEnumerator CoPulse(Vector2 position)
    {
        var duration = OptionGroupSingleton<OverclockedModifierOptions>.Instance.PulseDuration;
        var sphere = Utils.CreateSphere("OverclockedPulse", new Vector3(position.x, position.y, -0.1f), 0.8f, new Color(0.65f, 0.9f, 1f), duration);
        var bubble = sphere.GetComponent<AreaBubble>();
        bubble.breakDuration = Mathf.Min(0.5f, duration * 0.5f);
        bubble.expiresAt -= bubble.breakDuration;
        var renderer = sphere.GetComponent<MeshRenderer>();

        var startedAt = Time.time;
        while (sphere && ShipStatus.Instance && !MeetingHud.Instance && !ExileController.Instance && Time.time - startedAt < duration)
        {
            var progress = (Time.time - startedAt) / duration;
            var local = PlayerControl.LocalPlayer;
            renderer.enabled = local && Vector2.Distance(local.GetTruePosition(), position) <= Mathf.Min(3f, ShipStatus.Instance.CalculateLightRadius(local.Data)) && !PhysicsHelpers.AnythingBetween(local.GetTruePosition(), position, Constants.ShipAndObjectsMask, false);
            sphere.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1f, progress);
            yield return null;
        }

        if (sphere) Object.Destroy(sphere);
    }
}