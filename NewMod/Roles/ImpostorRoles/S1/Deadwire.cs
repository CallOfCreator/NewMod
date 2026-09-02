using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.RoleLogic;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1;

[MiraIgnore]
public sealed class Deadwire : ImpostorRole, INewModRole
{
    public static readonly Dictionary<byte, DeadwireRecord> Records = [];
    public static readonly Dictionary<byte, float> JammedUntil = [];
    public static readonly Dictionary<byte, float> BarrierUntil = [];

    private static readonly Dictionary<byte, byte> MarkedPlayers = [];
    private static readonly Dictionary<byte, float> MarkExpiresAt = [];

    public string RoleName => "Deadwire";
    public string RoleDescription => "Record an ability category and turn it into your own countermeasure.";
    public string RoleLongDescription => "Deadlock a player and capture the category of their next ability. Override converts it into a native Deadwire response instead of replaying the original ability.";
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
            Icon = MiraAssets.Empty
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        text.AppendLine(Records.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var record)
            ? $"<size=65%><color=#FF6B72>Captured:</color> {record.Category}</size>"
            : "<size=65%><color=#9B9B9B>No category recorded.</color></size>");
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

        Records.Clear();
        MarkedPlayers.Clear();
        MarkExpiresAt.Clear();
        JammedUntil.Clear();
        BarrierUntil.Clear();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
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

        if (!BarrierUntil.TryGetValue(evt.Target.PlayerId, out var expiresAt))
            return;

        BarrierUntil.Remove(evt.Target.PlayerId);
        if (Time.time < expiresAt)
            evt.Cancel();
    }

    [MethodRpc((uint)CustomRPC.DeadwireRequestDeadlock)]
    public static void RpcRequestDeadlock(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Deadwire || source.Data.IsDead || target.Data.IsDead || target.Data.Disconnected || Records.ContainsKey(source.PlayerId))
            return;

        var options = OptionGroupSingleton<DeadwireOptions>.Instance;
        if (Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) <= options.DeadlockRange)
            RpcConfirmDeadlock(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId, options.DeadlockDuration);
    }

    [MethodRpc((uint)CustomRPC.DeadwireConfirmDeadlock, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmDeadlock(PlayerControl host, byte deadwireId, byte targetId, float duration)
    {
        if (!host.IsHost() || Records.ContainsKey(deadwireId))
            return;

        MarkedPlayers[deadwireId] = targetId;
        MarkExpiresAt[deadwireId] = Time.time + duration;
    }

    [MethodRpc((uint)CustomRPC.DeadwireRequestCapture)]
    public static void RpcRequestCapture(PlayerControl actor, byte categoryId)
    {
        if (!AmongUsClient.Instance.AmHost || actor.Data.IsDead || categoryId > (byte)EnergyCategory.Protection)
            return;

        foreach (var pair in MarkedPlayers)
        {
            if (pair.Value != actor.PlayerId || Records.ContainsKey(pair.Key) || Time.time > MarkExpiresAt[pair.Key])
                continue;

            RpcConfirmCapture(PlayerControl.LocalPlayer, pair.Key, actor.PlayerId, categoryId);
            return;
        }
    }

    [MethodRpc((uint)CustomRPC.DeadwireConfirmCapture, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmCapture(PlayerControl host, byte deadwireId, byte actorId, byte categoryId)
    {
        if (!host.IsHost() || Records.ContainsKey(deadwireId) || !MarkedPlayers.TryGetValue(deadwireId, out var markedId) || markedId != actorId || Time.time > MarkExpiresAt[deadwireId])
            return;

        var category = (EnergyCategory)categoryId;
        Records[deadwireId] = new DeadwireRecord(actorId, category);
        MarkedPlayers.Remove(deadwireId);
        MarkExpiresAt.Remove(deadwireId);

        if (PlayerControl.LocalPlayer.PlayerId == deadwireId)
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF6B72>Captured:</color> {category}"));
    }

    [MethodRpc((uint)CustomRPC.DeadwireRequestOverride)]
    public static void RpcRequestOverride(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Deadwire || source.Data.IsDead || !Records.TryGetValue(source.PlayerId, out var record))
            return;

        RpcConfirmOverride(PlayerControl.LocalPlayer, source.PlayerId, record.ActorId, (byte)record.Category);
    }

    [MethodRpc((uint)CustomRPC.DeadwireConfirmOverride, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmOverride(PlayerControl host, byte deadwireId, byte actorId, byte categoryId)
    {
        if (!host.IsHost() || !Records.Remove(deadwireId))
            return;

        var deadwire = Utils.PlayerById(deadwireId);
        var actor = Utils.PlayerById(actorId);
        var record = new DeadwireRecord(actorId, (EnergyCategory)categoryId);
        var options = OptionGroupSingleton<DeadwireOptions>.Instance;

        switch (record.Response)
        {
            case DeadwireResponse.RefreshKill:
                if (deadwire.AmOwner)
                    deadwire.SetKillTimer(0f);
                break;
            case DeadwireResponse.TrackActor:
                if (deadwire.AmOwner)
                    Coroutines.Start(CoTrackActor(deadwire, actor, options.TrackingDuration));
                break;
            case DeadwireResponse.Blink:
                if (deadwire.AmOwner)
                    Blink(deadwire, options.BlinkDistance);
                break;
            case DeadwireResponse.JamActor:
                JammedUntil[actorId] = Time.time + options.JamDuration;
                break;
            case DeadwireResponse.Barrier:
                BarrierUntil[deadwireId] = Time.time + options.BarrierDuration;
                break;
        }
    }

    private static IEnumerator CoTrackActor(PlayerControl deadwire, PlayerControl actor, float duration)
    {
        deadwire.StartPlayerTracking(actor, actor.Data.DefaultOutfit.ColorId);
        yield return new WaitForSeconds(duration);
        deadwire.CancelPlayerTracking();
    }

    private static void Blink(PlayerControl player, float distance)
    {
        var velocity = player.MyPhysics.body.velocity;
        var direction = velocity.sqrMagnitude > 0.01f ? velocity.normalized : player.cosmetics.currentBodySprite.BodySprite.flipX ? Vector2.left : Vector2.right;
        var hit = Physics2D.Raycast(player.GetTruePosition(), direction, distance, Constants.ShipAndObjectsMask);
        if (hit.collider)
            distance = Mathf.Max(0f, hit.distance - 0.35f);

        player.NetTransform.RpcSnapTo(player.GetTruePosition() + direction * distance);
    }
}

