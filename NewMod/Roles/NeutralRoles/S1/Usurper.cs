using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Buttons.Roles.S1;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public class Usurper : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, (byte TargetId, CrownPhase Phase)> States = [];
    public static readonly Dictionary<byte, Vector2> CrownPositions = [];

    public static readonly Dictionary<byte, GameObject> CrownObjects = [];

    public static readonly Dictionary<byte, (byte Player, float Started)> Pickup = [];

    public static readonly Dictionary<byte, float> HoldRemaining = [];
    public static readonly HashSet<byte> MeetingEligible = [];

    public static byte ExiledPlayerId = byte.MaxValue;
    public static Vector2 ExilePosition;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.TabDescription");
    public Color RoleColor => new(0.72f, 0.34f, 0.16f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 30,
            CanModifyChance = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = MiraAssets.Empty
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        if (!States.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var state))
            return text;

        if (state.Phase == CrownPhase.Claimed)
            text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.Tab.Claimed"), Utils.PlayerById(state.TargetId).Data.PlayerName));
        else if (state.Phase == CrownPhase.Available)
            text.AppendLine(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.Tab.CrownWaiting"));
        else if (state.Phase == CrownPhase.Held)
        {
            var remaining = HoldRemaining.GetValueOrDefault(PlayerControl.LocalPlayer.PlayerId);
            text.AppendLine(remaining > 0f
                ? string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.Tab.Hold"), Mathf.CeilToInt(remaining))
                : MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.Tab.SurvivalGoal"));
        }

        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<UsurperGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            Reset();

            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.Data.Role is Usurper)
                    States[player.PlayerId] = (byte.MaxValue, CrownPhase.Unclaimed);

            return;
        }

        if (!AmongUsClient.Instance.AmHost || ExiledPlayerId == byte.MaxValue)
            return;

        MarkClaimedDeath(ExiledPlayerId, ExilePosition);
        ExiledPlayerId = byte.MaxValue;
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is Usurper)
        {
            States[evt.Player.PlayerId] = (byte.MaxValue, CrownPhase.Unclaimed);
            return;
        }

        var playerId = evt.Player.PlayerId;
        HoldRemaining.Remove(playerId);
        MeetingEligible.Remove(playerId);
        States.Remove(playerId);
        Pickup.Remove(playerId);
        CrownPositions.Remove(playerId);
        if (CrownObjects.Remove(playerId, out var crown))
            Destroy(crown);
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        Reset();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (States.TryGetValue(evt.Target.PlayerId, out var state) && state.Phase == CrownPhase.Held)
        {
            States[evt.Target.PlayerId] = (byte.MaxValue, CrownPhase.Unclaimed);
            HoldRemaining.Remove(evt.Target.PlayerId);
            MeetingEligible.Remove(evt.Target.PlayerId);
        }

        if (!AmongUsClient.Instance.AmHost)
            return;

        var position = evt.DeadBody ? (Vector2)evt.DeadBody.transform.position : evt.Target.GetTruePosition();
        MarkClaimedDeath(evt.Target.PlayerId, position);
    }

    [RegisterEvent]
    public static void OnMeetingResolved(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro || !GameManager.Instance.ShouldCheckForGameEnd || !AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in States)
        {
            var usurper = Utils.PlayerById(pair.Key);
            if (!MeetingEligible.Contains(pair.Key) || pair.Value.Phase != CrownPhase.Held || !usurper || usurper.Data.Role is not Usurper || usurper.Data.IsDead || usurper.Data.Disconnected)
                continue;

            CustomGameOver.Trigger<UsurperGameOver>([usurper.Data]);
            return;
        }
    }

    [RegisterEvent]
    public static void OnPlayerLeave(PlayerLeaveEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || evt.ClientData.Character == null)
            return;

        var targetId = evt.ClientData.Character.PlayerId;
        foreach (var pair in States)
            if (pair.Value.Phase == CrownPhase.Claimed && pair.Value.TargetId == targetId)
                RpcRefundClaim(PlayerControl.LocalPlayer, pair.Key, targetId);
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        Pickup.Clear();
        PrepareMeeting();
    }

    public static void HostFixedUpdate()
    {
        var options = OptionGroupSingleton<UsurperOptions>.Instance;
        foreach (var pair in CrownPositions)
        {
            PlayerControl candidate = null;
            var contested = false;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player.Data.Disconnected || player.inVent || Vector2.Distance(player.GetTruePosition(), pair.Value) > options.CrownPickupRange || PhysicsHelpers.AnythingBetween(player.GetTruePosition(), pair.Value, Constants.ShipAndObjectsMask, false)) continue;
                if (candidate)
                {
                    contested = true;
                    break;
                }

                candidate = player;
            }

            if (!candidate || contested || !candidate.CanMove || candidate.MyPhysics.Velocity.sqrMagnitude > 0.01f)
            {
                Pickup.Remove(pair.Key);
                continue;
            }

            if (!Pickup.TryGetValue(pair.Key, out var pickup) || pickup.Player != candidate.PlayerId)
            {
                Pickup[pair.Key] = (candidate.PlayerId, Time.time);
            }
            else if (Time.time - pickup.Started >= Mathf.Max(3f, options.PickupDuration))
            {
                if (candidate.PlayerId == pair.Key) RpcTakeCrown(PlayerControl.LocalPlayer, pair.Key);
                else RpcSecureCrown(PlayerControl.LocalPlayer, pair.Key);
                break;
            }
        }
    }

    public static void MarkClaimedDeath(byte targetId, Vector2 position)
    {
        foreach (var pair in States)
            if (pair.Value.Phase == CrownPhase.Claimed && pair.Value.TargetId == targetId)
                RpcSpawnCrown(PlayerControl.LocalPlayer, pair.Key, targetId, position.x, position.y);
    }

    [MethodRpc((uint)CustomRPC.UsurperRequestClaim)]
    public static void RpcRequestClaim(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Usurper || source.Data.IsDead || target.Data.IsDead || target.Data.Disconnected || !States.TryGetValue(source.PlayerId, out var state) || state.Phase != CrownPhase.Unclaimed || Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > OptionGroupSingleton<UsurperOptions>.Instance.ClaimRange)
            return;

        RpcConfirmClaim(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.UsurperConfirmClaim, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmClaim(PlayerControl source, byte usurperId, byte targetId)
    {
        if (source.IsHost())
            Claim(usurperId, targetId);
    }

    [MethodRpc((uint)CustomRPC.UsurperSpawnCrown, LocalHandling = RpcLocalHandling.After)]
    public static void RpcSpawnCrown(PlayerControl source, byte usurperId, byte targetId, float x, float y)
    {
        if (!source.IsHost() || !MakeCrownAvailable(usurperId, targetId))
            return;

        var position = new Vector2(x, y);
        CrownPositions[usurperId] = position;

        var crown = new GameObject($"UsurperCrown_{usurperId}");
        crown.transform.SetParent(ShipStatus.Instance.transform, true);
        crown.transform.position = new Vector3(x, y, -1f);

        var renderer = crown.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.Crown.LoadAsset();
        var size = renderer.sprite.bounds.size;
        crown.transform.localScale = Vector3.one * (0.6f / Mathf.Max(size.x, size.y));
        CrownObjects[usurperId] = crown;
        Coroutines.Start(CoroutinesHelper.CoNotify($"A crown has appeared.\nStand beside it alone for {Mathf.Max(3f, OptionGroupSingleton<UsurperOptions>.Instance.PickupDuration):0.#} seconds\nto secure it."));
    }

    [MethodRpc((uint)CustomRPC.UsurperTakeCrown, LocalHandling = RpcLocalHandling.After)]
    public static void RpcTakeCrown(PlayerControl source, byte usurperId)
    {
        if (!source.IsHost() || !TakeCrown(usurperId))
            return;

        Pickup.Remove(usurperId);
        CrownPositions.Remove(usurperId);
        var holder = Utils.PlayerById(usurperId);
        var crown = CrownObjects[usurperId];
        crown.transform.SetParent(holder.transform, false);
        HoldRemaining[usurperId] = Mathf.Max(20f, OptionGroupSingleton<UsurperOptions>.Instance.HoldDuration);
        MeetingEligible.Remove(usurperId);
        Coroutines.Start(CoHoldCrown(holder, crown));
        var color = ((Usurper)holder.Data.Role).RoleColor.ToTextColor();
        var message = holder.AmOwner
            ? string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.Notice.YourCrown"), color, HoldRemaining[usurperId])
            : string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Usurper.Notice.Claimed"), color, holder.Data.PlayerName);
        Coroutines.Start(CoroutinesHelper.CoNotify(message));
    }

    [MethodRpc((uint)CustomRPC.UsurperRefundClaim, LocalHandling = RpcLocalHandling.After)]
    public static void RpcRefundClaim(PlayerControl source, byte usurperId, byte targetId)
    {
        if (!source.IsHost() || !RefundClaim(usurperId, targetId))
            return;

        var usurper = Utils.PlayerById(usurperId);
        if (usurper.AmOwner)
        {
            CustomButtonSingleton<ClaimButton>.Instance.SetUses(1);
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#D8844D>Claim refunded:</color> your target disconnected."));
        }
    }

    [MethodRpc((uint)CustomRPC.UsurperSecureCrown)]
    public static void RpcSecureCrown(PlayerControl source, byte usurperId)
    {
        if (!source.IsHost() || !CrownPositions.Remove(usurperId)) return;
        Pickup.Remove(usurperId);
        Destroy(CrownObjects[usurperId]);
        CrownObjects.Remove(usurperId);
        States[usurperId] = (byte.MaxValue, CrownPhase.Unclaimed);
        if (PlayerControl.LocalPlayer.PlayerId == usurperId)
            CustomButtonSingleton<ClaimButton>.Instance.SetUses(1);
        Coroutines.Start(CoroutinesHelper.CoNotify("The unclaimed crown was secured.\nThe Usurper must choose a new claim."));
    }

    public static void Reset()
    {
        foreach (var crown in CrownObjects.Values)
            Destroy(crown);

        Pickup.Clear();
        HoldRemaining.Clear();
        MeetingEligible.Clear();
        States.Clear();
        CrownPositions.Clear();
        CrownObjects.Clear();
        ExiledPlayerId = byte.MaxValue;
        ExilePosition = Vector2.zero;
    }

    public enum CrownPhase : byte
    {
        Unclaimed,
        Claimed,
        Available,
        Held
    }
    public static bool Claim(byte ownerId, byte targetId)
    {
        if (States[ownerId].Phase != CrownPhase.Unclaimed)
            return false;
        States[ownerId] = (targetId, CrownPhase.Claimed);
        return true;
    }

    public static bool MakeCrownAvailable(byte ownerId, byte targetId)
    {
        var state = States[ownerId];
        if (state.Phase != CrownPhase.Claimed || state.TargetId != targetId)
            return false;
        States[ownerId] = (targetId, CrownPhase.Available);
        return true;
    }

    public static bool RefundClaim(byte ownerId, byte targetId)
    {
        var state = States[ownerId];
        if (state.Phase != CrownPhase.Claimed || state.TargetId != targetId)
            return false;
        States[ownerId] = (byte.MaxValue, CrownPhase.Unclaimed);
        return true;
    }

    public static bool TakeCrown(byte ownerId)
    {
        var state = States[ownerId];
        if (state.Phase != CrownPhase.Available)
            return false;
        States[ownerId] = (state.TargetId, CrownPhase.Held);
        return true;
    }

    public static void AdvanceHold(byte ownerId, float elapsed, bool meetingActive)
    {
        if (!meetingActive && HoldRemaining.TryGetValue(ownerId, out var remaining))
            HoldRemaining[ownerId] = System.Math.Max(0f, remaining - elapsed);
    }

    public static void PrepareMeeting()
    {
        MeetingEligible.Clear();
        foreach (var pair in HoldRemaining)
            if (pair.Value <= 0f && States.TryGetValue(pair.Key, out var state) && state.Phase == CrownPhase.Held)
                MeetingEligible.Add(pair.Key);
    }

    public static IEnumerator CoHoldCrown(PlayerControl holder, GameObject crown)
    {
        var ownerId = holder.PlayerId;
        var renderer = crown.GetComponent<SpriteRenderer>();
        var nameRenderer = holder.cosmetics.nameText.GetComponent<MeshRenderer>();
        var colorNameRenderer = holder.cosmetics.colorBlindText.GetComponent<MeshRenderer>();
        while (holder && !holder.Data.IsDead && !holder.Data.Disconnected && holder.Data.Role is Usurper
            && States.TryGetValue(ownerId, out var held) && held.Phase == CrownPhase.Held
            && CrownObjects.TryGetValue(ownerId, out var current) && current == crown)
        {
            var meetingActive = MeetingHud.Instance || ExileController.Instance;
            AdvanceHold(ownerId, Time.deltaTime, meetingActive);
            var local = PlayerControl.LocalPlayer;
            var body = holder.cosmetics.currentBodySprite.BodySprite;
            var position = holder.transform.position + new Vector3(0f, 0.95f, -0.1f);
            var crownHalfHeight = renderer.bounds.extents.y;
            if (nameRenderer.enabled && nameRenderer.gameObject.activeInHierarchy)
                position.y = Mathf.Max(position.y, nameRenderer.bounds.max.y + crownHalfHeight + 0.03f);
            if (colorNameRenderer.enabled && colorNameRenderer.gameObject.activeInHierarchy)
                position.y = Mathf.Max(position.y, colorNameRenderer.bounds.max.y + crownHalfHeight + 0.03f);
            crown.transform.position = position;
            renderer.enabled = !meetingActive && holder.Visible && !holder.inVent && body.enabled && body.color.a > 0f
                && (holder == local || (Vector2.Distance(local.GetTruePosition(), holder.GetTruePosition()) <= ShipStatus.Instance.CalculateLightRadius(local.Data)
                    && !PhysicsHelpers.AnythingBetween(local.GetTruePosition(), holder.GetTruePosition(), Constants.ShipAndObjectsMask, false)));
            renderer.color = new Color(1f, 1f, 1f, body.color.a);
            yield return null;
        }

        if (CrownObjects.TryGetValue(ownerId, out var active) && active == crown)
        {
            CrownObjects.Remove(ownerId);
            HoldRemaining.Remove(ownerId);
            MeetingEligible.Remove(ownerId);
            if (States.TryGetValue(ownerId, out var state) && state.Phase == CrownPhase.Held)
                States[ownerId] = (byte.MaxValue, CrownPhase.Unclaimed);
        }
        if (crown)
            Destroy(crown);
    }

}