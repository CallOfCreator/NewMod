using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class Revenant : ImpostorRole, INewModRole
{
    public enum Phase : byte
    {
        Ready,
        Feigning,
        Revived,
        DoomActive,
        DoomSpent,
        PermanentlyDead
    }

    public static readonly Dictionary<byte, Phase> Phases = [];
    public static readonly Dictionary<byte, float> PhaseExpiresAt = [];
    public static readonly Dictionary<byte, DeadBody> Bodies = [];
    public static readonly Dictionary<byte, byte> PendingDoomTargets = [];

    public string RoleName => "Revenant";
    public string RoleDescription => "Feign death, revive, then kill by contact.";
    public string RoleLongDescription => "Feign death once. If your body remains unreported, return with Doom Awakening and attack the first eligible player you touch.";
    public Color RoleColor => new(0.36f, 0.08f, 0.52f);
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
        if (Phases.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var phase))
            text.AppendLine($"<size=65%><color=#B77ADB>State:</color> {phase}</size>");
        return text;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro || (Application.platform == RuntimePlatform.Android && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay))
            return;

        Phases.Clear();
        PhaseExpiresAt.Clear();
        Bodies.Clear();
        PendingDoomTargets.Clear();

        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.Role is Revenant)
                Phases[player.PlayerId] = Phase.Ready;
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        var playerId = evt.Player.PlayerId;
        if (evt.Player.Data.Role is Revenant)
        {
            Phases[playerId] = Phase.Ready;
            PhaseExpiresAt.Remove(playerId);
            Bodies.Remove(playerId);
            PendingDoomTargets.Remove(playerId);
            return;
        }

        Phases.Remove(playerId);
        PhaseExpiresAt.Remove(playerId);
        Bodies.Remove(playerId);
        PendingDoomTargets.Remove(playerId);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!PendingDoomTargets.TryGetValue(evt.Source.PlayerId, out var targetId) || targetId != evt.Target.PlayerId || !Phases.TryGetValue(evt.Source.PlayerId, out var doomPhase) || doomPhase != Phase.DoomActive)
            return;

        PendingDoomTargets.Remove(evt.Source.PlayerId);
        if (AmongUsClient.Instance.AmHost)
            RpcFinishDoom(PlayerControl.LocalPlayer, evt.Source.PlayerId);
    }

    [RegisterEvent]
    public static void OnReportBody(ReportBodyEvent evt)
    {
        if (evt.Target != null && Phases.TryGetValue(evt.Target.PlayerId, out var phase) && phase == Phase.Feigning)
            RpcRequestFeignReport(evt.Reporter, evt.Target.PlayerId);
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!Phases.TryGetValue(player.PlayerId, out var phase))
                continue;

            if (phase == Phase.Feigning)
            {
                if (AmongUsClient.Instance.AmHost)
                    RpcConfirmFeignReport(PlayerControl.LocalPlayer, player.PlayerId);
            }
            else if (phase == Phase.DoomActive)
            {
                Phases[player.PlayerId] = Phase.DoomSpent;
            }
        }

        PhaseExpiresAt.Clear();
        PendingDoomTargets.Clear();
    }

    [MethodRpc((uint)CustomRPC.RevenantRequestFeign)]
    public static void RpcRequestFeign(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Revenant || source.Data.IsDead)
            return;

        if (!Phases.TryGetValue(source.PlayerId, out var phase))
        {
            phase = Phase.Ready;
            Phases[source.PlayerId] = phase;
        }

        if (phase != Phase.Ready)
            return;

        var duration = OptionGroupSingleton<RevenantOptions>.Instance.FeignDuration;
        RpcConfirmFeign(PlayerControl.LocalPlayer, source.PlayerId, duration);
        Coroutines.Start(CoResolveFeign(source.PlayerId));
    }

    [MethodRpc((uint)CustomRPC.RevenantConfirmFeign, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmFeign(PlayerControl host, byte playerId, float duration)
    {
        if (!host.IsHost() || (Phases.TryGetValue(playerId, out var phase) && phase != Phase.Ready))
            return;

        Phases[playerId] = Phase.Feigning;
        PhaseExpiresAt[playerId] = Time.time + duration;

        var player = Utils.PlayerById(playerId);
        var body = Instantiate(GameManager.Instance.GetDeadBody(player.Data.Role));
        body.name = "RevenantFeignBody";
        body.ParentId = playerId;
        body.transform.position = player.GetTruePosition();

        foreach (var renderer in body.bodyRenderers)
            player.SetPlayerMaterialColors(renderer);

        Bodies[playerId] = body;
        player.Visible = false;
        player.moveable = false;
        player.MyPhysics.inputHandler.enabled = false;
        player.MyPhysics.body.velocity = Vector2.zero;
        player.Collider.enabled = false;

        if (player.AmOwner)
        {
            HudManager.Instance.SetHudActive(player, player.Data.Role, false);
            SoundManager.Instance.PlaySound(NewModAsset.FeignDeathSound.LoadAsset(), false);
        }
    }

    public static IEnumerator CoResolveFeign(byte playerId)
    {
        while (Phases.TryGetValue(playerId, out var phase) && phase == Phase.Feigning && PhaseExpiresAt.TryGetValue(playerId, out var expiresAt) && Time.time < expiresAt)
            yield return null;

        if (!Phases.TryGetValue(playerId, out var currentPhase) || currentPhase != Phase.Feigning || !Bodies.TryGetValue(playerId, out var body))
            yield break;

        RpcConfirmRevive(PlayerControl.LocalPlayer, playerId, body.transform.position.x, body.transform.position.y);
    }

    [MethodRpc((uint)CustomRPC.RevenantRequestReport)]
    public static void RpcRequestFeignReport(PlayerControl reporter, byte playerId)
    {
        if (AmongUsClient.Instance.AmHost && Phases.TryGetValue(playerId, out var phase) && phase == Phase.Feigning)
            RpcConfirmFeignReport(PlayerControl.LocalPlayer, playerId);
    }

    [MethodRpc((uint)CustomRPC.RevenantConfirmReport, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmFeignReport(PlayerControl host, byte playerId)
    {
        if (!host.IsHost() || !Phases.TryGetValue(playerId, out var phase) || phase != Phase.Feigning)
            return;

        Phases[playerId] = Phase.PermanentlyDead;
        PhaseExpiresAt.Remove(playerId);

        if (Bodies.Remove(playerId, out var body))
            Destroy(body.gameObject);

        var player = Utils.PlayerById(playerId);
        player.Visible = true;
        player.moveable = true;
        player.MyPhysics.inputHandler.enabled = true;
        player.MyPhysics.body.velocity = Vector2.zero;
        player.Collider.enabled = true;

        if (AmongUsClient.Instance.AmHost && !player.Data.IsDead)
            player.RpcCustomMurder(player, true, false, false, false, false, false);
    }

    [MethodRpc((uint)CustomRPC.RevenantConfirmRevive, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmRevive(PlayerControl host, byte playerId, float x, float y)
    {
        if (!host.IsHost() || !Phases.TryGetValue(playerId, out var phase) || phase != Phase.Feigning)
            return;

        Phases[playerId] = Phase.Revived;
        PhaseExpiresAt.Remove(playerId);

        var player = Utils.PlayerById(playerId);
        player.Visible = true;
        player.moveable = true;
        player.NetTransform.SnapTo(new Vector2(x, y));
        player.MyPhysics.body.velocity = Vector2.zero;
        player.MyPhysics.inputHandler.enabled = true;
        player.Collider.enabled = true;

        if (Bodies.Remove(playerId, out var body))
            Destroy(body.gameObject);

        if (player.AmOwner)
            HudManager.Instance.SetHudActive(player, player.Data.Role, true);
    }

    [MethodRpc((uint)CustomRPC.RevenantRequestDoom)]
    public static void RpcRequestDoom(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Revenant || source.Data.IsDead || !Phases.TryGetValue(source.PlayerId, out var phase) || phase != Phase.Revived)
            return;

        var duration = OptionGroupSingleton<RevenantOptions>.Instance.DoomDuration;
        RpcConfirmDoom(PlayerControl.LocalPlayer, source.PlayerId, duration);
        Coroutines.Start(CoExpireDoom(source.PlayerId));
    }

    [MethodRpc((uint)CustomRPC.RevenantConfirmDoom, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmDoom(PlayerControl host, byte playerId, float duration)
    {
        if (!host.IsHost() || !Phases.TryGetValue(playerId, out var phase) || phase != Phase.Revived)
            return;

        Phases[playerId] = Phase.DoomActive;
        PhaseExpiresAt[playerId] = Time.time + duration;

        var player = Utils.PlayerById(playerId);
        if (player.AmOwner)
            Coroutines.Start(Buttons.Roles.DoomAwakening.CoDoom(player, PhaseExpiresAt[playerId]));
    }

    private static IEnumerator CoExpireDoom(byte playerId)
    {
        while (Phases.TryGetValue(playerId, out var phase) && phase == Phase.DoomActive && PhaseExpiresAt.TryGetValue(playerId, out var expiresAt) && Time.time < expiresAt)
            yield return null;

        if (Phases.TryGetValue(playerId, out var currentPhase) && currentPhase == Phase.DoomActive)
            RpcFinishDoom(PlayerControl.LocalPlayer, playerId);
    }

    [MethodRpc((uint)CustomRPC.RevenantRequestContact)]
    public static void RpcRequestDoomContact(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !AmongUsClient.Instance.IsGameStarted || MeetingHud.Instance || ExileController.Instance)
            return;

        if (!source || !source.Data || source.Data.Role is not Revenant || source.Data.IsDead || source.Data.Disconnected || source.inVent)
            return;

        if (!target || target == source || !target.Data || !target.Data.Role || target.Data.IsDead || target.Data.Disconnected || target.inVent || target.Data.Role.IsImpostor)
            return;

        if (!Phases.TryGetValue(source.PlayerId, out var phase) || phase != Phase.DoomActive || PendingDoomTargets.ContainsKey(source.PlayerId) || !PhaseExpiresAt.TryGetValue(source.PlayerId, out var expiresAt) || Time.time >= expiresAt)
            return;

        if (Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > OptionGroupSingleton<RevenantOptions>.Instance.DoomContactRadius)
            return;

        PendingDoomTargets[source.PlayerId] = target.PlayerId;
        source.RpcCustomMurder(target, MeetingCheck.OutsideMeeting, resetKillTimer: false, teleportMurderer: false);
        if (!target.Data.IsDead)
            RpcFinishDoom(PlayerControl.LocalPlayer, source.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.RevenantFinishDoom, LocalHandling = RpcLocalHandling.After)]
    public static void RpcFinishDoom(PlayerControl host, byte playerId)
    {
        if (!host.IsHost())
            return;

        Phases[playerId] = Phase.DoomSpent;
        PhaseExpiresAt.Remove(playerId);
        PendingDoomTargets.Remove(playerId);
    }
}
