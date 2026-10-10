using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1;

[MiraIgnore]
public class Deadwire : ImpostorRole, INewModRole
{
    public static readonly Dictionary<byte, (byte ActorId, EnergyCategory Category)> Records = [];
    public static readonly Dictionary<byte, float> JammedUntil = [];
    public static readonly Dictionary<byte, float> BarrierUntil = [];

    public static readonly Dictionary<byte, byte> MarkedPlayers = [];
    public static readonly Dictionary<byte, float> MarkExpiresAt = [];

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire.TabDescription");
    public Color RoleColor => new(0.92f, 0.12f, 0.2f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Apex;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 35,
            CanModifyChance = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = true,
            TasksCountForProgress = false,
            Icon = MiraAssets.Empty,
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        text.AppendLine(Records.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var record) ? string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire.Tab.Captured"), MiraLocaleManager.Get($"NewMod.Deadwire.Reward.{GetResponse(record.Category)}")) : MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire.Tab.NoCategoryRecorded"));
        text.AppendLine(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire.Tab.OffensiveRewards"));
        text.AppendLine(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.Deadwire.Tab.UtilityRewards"));
        return text;
    }

    public static bool IsJammed(byte playerId)
    {
        if (!JammedUntil.TryGetValue(playerId, out var expiresAt))
            return false;

        if (Time.time < expiresAt)
            return true;

        JammedUntil.Remove(playerId);
        return false;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        ResetState();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        ResetState();
    }

    public static void ResetState()
    {
        Records.Clear();
        MarkedPlayers.Clear();
        MarkExpiresAt.Clear();
        JammedUntil.Clear();
        BarrierUntil.Clear();
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (IsJammed(evt.Source.PlayerId))
        {
            evt.Cancel();
            return;
        }

        if (!BarrierUntil.Remove(evt.Target.PlayerId, out var expiresAt))
            return;

        if (Time.time < expiresAt)
        {
            evt.Cancel();
            if (evt.Target.AmOwner || evt.Source.AmOwner)
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF6B72>The shield blocked the attack.</color>"));
        }
    }

    [MethodRpc((uint)CustomRPC.DeadwireRequestDeadlock)]
    public static void RpcRequestDeadlock(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || MeetingHud.Instance || ExileController.Instance || source.inVent || source.Data.Role is not Deadwire || source.Data.IsDead || target.Data.IsDead || target.Data.Disconnected || Records.ContainsKey(source.PlayerId))
            return;

        var options = OptionGroupSingleton<DeadwireOptions>.Instance;
        if (source != target && !target.inVent && !target.Data.Role.IsImpostor && Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) <= options.DeadlockRange && !PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false))
            RpcConfirmDeadlock(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId, options.DeadlockDuration);
    }

    [MethodRpc((uint)CustomRPC.DeadwireConfirmDeadlock, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmDeadlock(PlayerControl source, byte deadwireId, byte targetId, float duration)
    {
        if (!source.IsHost() || Records.ContainsKey(deadwireId))
            return;

        OverclockedModifier.Pulse(Utils.PlayerById(deadwireId));
        MarkedPlayers[deadwireId] = targetId;
        MarkExpiresAt[deadwireId] = Time.time + duration;
    }

    [MethodRpc((uint)CustomRPC.DeadwireRequestCapture)]
    public static void RpcRequestCapture(PlayerControl source, byte categoryId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.IsDead || categoryId > (byte)EnergyCategory.Protection)
            return;

        foreach (var pair in MarkedPlayers)
        {
            if (pair.Value != source.PlayerId || Records.ContainsKey(pair.Key) || Time.time > MarkExpiresAt[pair.Key])
                continue;

            RpcConfirmCapture(PlayerControl.LocalPlayer, pair.Key, source.PlayerId, categoryId);
            return;
        }
    }

    [MethodRpc((uint)CustomRPC.DeadwireConfirmCapture, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmCapture(PlayerControl source, byte deadwireId, byte actorId, byte categoryId)
    {
        if (!source.IsHost() || Records.ContainsKey(deadwireId) || !MarkedPlayers.TryGetValue(deadwireId, out var markedId) || markedId != actorId || Time.time > MarkExpiresAt[deadwireId])
            return;

        var category = (EnergyCategory)categoryId;
        Records[deadwireId] = (actorId, category);
        MarkedPlayers.Remove(deadwireId);
        MarkExpiresAt.Remove(deadwireId);

        if (PlayerControl.LocalPlayer.PlayerId == deadwireId)
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF6B72>Captured:</color> {MiraLocaleManager.Get($"NewMod.Deadwire.Reward.{GetResponse(Records[deadwireId].Category)}")}"));
        if (PlayerControl.LocalPlayer.PlayerId == actorId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF6B72>Deadwire recorded your ability.</color> Your ability still works normally."));
    }

    [MethodRpc((uint)CustomRPC.DeadwireRequestOverride)]
    public static void RpcRequestOverride(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || MeetingHud.Instance || ExileController.Instance || source.inVent || source.Data.Role is not Deadwire || source.Data.IsDead || !Records.TryGetValue(source.PlayerId, out var record))
            return;

        if (GetResponse(record.Category) is Response.TrackActor or Response.JamActor)
        {
            var actor = Utils.PlayerById(record.ActorId);
            if (!actor || actor.Data.IsDead || actor.Data.Disconnected)
                return;
        }

        RpcConfirmOverride(PlayerControl.LocalPlayer, source.PlayerId, record.ActorId, (byte)record.Category);
    }

    [MethodRpc((uint)CustomRPC.DeadwireConfirmOverride, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmOverride(PlayerControl source, byte deadwireId, byte actorId, byte categoryId)
    {
        if (!source.IsHost() || !Records.Remove(deadwireId))
            return;

        var deadwire = Utils.PlayerById(deadwireId);
        OverclockedModifier.Pulse(deadwire);
        var actor = Utils.PlayerById(actorId);
        var record = (ActorId: actorId, Category: (EnergyCategory)categoryId);
        var options = OptionGroupSingleton<DeadwireOptions>.Instance;

        switch (GetResponse(record.Category))
        {
            case Response.ReduceKillCooldown:
                if (deadwire.AmOwner)
                    deadwire.SetKillTimer(ReduceCooldown(deadwire.killTimer, options.KillCooldownReduction, options.MinimumKillDelay));
                break;
            case Response.TrackActor:
                if (deadwire.AmOwner)
                    Coroutines.Start(CoTrackActor(deadwire, actor, options.TrackingDuration));
                break;
            case Response.Blink:
                if (deadwire.AmOwner)
                    Blink(deadwire, options.BlinkDistance);
                break;
            case Response.JamActor:
                JammedUntil[actorId] = Time.time + options.JamDuration;
                if (actor.AmOwner)
                    Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF6B72>Abilities jammed for {options.JamDuration:0.#}s.</color>"));
                break;
            case Response.Barrier:
                BarrierUntil[deadwireId] = Time.time + options.BarrierDuration;
                break;
        }
    }

    public static IEnumerator CoTrackActor(PlayerControl deadwire, PlayerControl actor, float duration)
    {
        deadwire.StartPlayerTracking(actor, actor.Data.DefaultOutfit.ColorId);
        yield return new WaitForSeconds(duration);
        deadwire.CancelPlayerTracking();
    }

    public static void Blink(PlayerControl player, float distance)
    {
        var velocity = player.MyPhysics.body.velocity;
        Vector2 direction;
        if (velocity.sqrMagnitude > 0.01f)
            direction = velocity.normalized;
        else
            direction = player.cosmetics.currentBodySprite.BodySprite.flipX ? Vector2.left : Vector2.right;
        var hit = Physics2D.Raycast(player.GetTruePosition(), direction, distance, Constants.ShipAndObjectsMask);
        if (hit.collider)
            distance = Mathf.Max(0f, hit.distance - 0.35f);

        player.NetTransform.RpcSnapTo(player.GetTruePosition() + direction * distance);
    }

    public enum Response : byte
    {
        ReduceKillCooldown,
        TrackActor,
        Blink,
        JamActor,
        Barrier,
    }

    public static float ReduceCooldown(float remaining, float reduction, float minimum)
    {
        return System.Math.Min(remaining, System.Math.Max(minimum, remaining - reduction));
    }

    public static Response GetResponse(EnergyCategory category)
    {
        return category switch
        {
            EnergyCategory.Aggression => Response.ReduceKillCooldown,
            EnergyCategory.Intelligence => Response.TrackActor,
            EnergyCategory.Mobility => Response.Blink,
            EnergyCategory.Control => Response.JamActor,
            _ => Response.Barrier,
        };
    }
}
