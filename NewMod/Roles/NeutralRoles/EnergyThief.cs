using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Components;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.Roles.NeutralRoles;

public sealed class EnergyThief : CrewmateRole, INewModRole
{
    public const int TypedEnergyGain = 30;
    public const int RawEnergyGain = 10;

    public static readonly Dictionary<byte, int> Energy = [];
    public static readonly Dictionary<byte, List<EnergyCategory>> Categories = [];
    public static readonly Dictionary<byte, byte> TetherTargets = [];
    public static readonly Dictionary<byte, float> TetherExpiresAt = [];
    public static readonly Dictionary<byte, HashSet<byte>> HarvestedTargets = [];
    public static readonly Dictionary<byte, EnergyCategory> BrownoutCategories = [];
    public static readonly Dictionary<byte, float> BrownoutExpiresAt = [];

    // Inspired by: https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Utilities/HackerUtilities.cs#L15
    public static readonly Dictionary<ShipStatus.MapType, Vector3[]> MapNodePositions = new()
    {
        [ShipStatus.MapType.Ship] =
        [
            new Vector3(-3.9285f, 5.6983f, 0.0057f),
            new Vector3(12.1729f, -6.5887f, -0.0066f),
            new Vector3(-19.7123f, -6.8006f, -0.0068f),
            new Vector3(-12.3633f, -14.6075f, -0.0146f)
        ],
        [ShipStatus.MapType.Pb] =
        [
            new Vector3(3.5599f, -7.584f, -0.0076f),
            new Vector3(22.1169f, -25.0981f, -0.0251f),
            new Vector3(37.3687f, -21.9697f, -0.022f),
            new Vector3(40.6573f, -7.9562f, -0.008f)
        ],
        [ShipStatus.MapType.Hq] =
        [
            new Vector3(11.5355f, 10.3573f, 0.0104f),
            new Vector3(-3.063f, 3.8147f, 0.0038f),
            new Vector3(16.6542f, 25.3223f, 0.0253f),
            new Vector3(19.5728f, 17.4778f, 0.0175f)
        ],
        [ShipStatus.MapType.Fungle] =
        [
            new Vector3(-22.4063f, -1.6647f, -0.0017f),
            new Vector3(-11.0019f, 12.6502f, 0.0127f),
            new Vector3(24.3133f, 14.628f, 0.0146f),
            new Vector3(7.6678f, -9.9008f, -0.0099f)
        ],
        [(ShipStatus.MapType)6] = []
    };

    public static readonly Vector3[] AirshipPositions =
    [
        new(-5.0792f, 10.9539f, 0.011f),
        new(16.856f, 14.7769f, 0.0148f),
        new(37.3283f, -3.7612f, -0.0038f),
        new(19.8862f, -3.9247f, -0.0039f),
        new(-13.1688f, -14.4867f, -0.0145f),
        new(-14.2747f, -4.8171f, -0.0048f),
        new(1.4743f, -2.5041f, -0.0025f)
    ];

    public static byte NodeOwnerId { get; private set; } = byte.MaxValue;
    public static int NodePositionIndex { get; private set; } = -1;
    public static bool BreachActive { get; private set; }
    public static float BreachEndsAt { get; private set; }

    private static bool _breachResolved;

    public string RoleName => "Energy Thief";
    public string RoleDescription => "Every power leaves something to steal.";
    public string RoleLongDescription => "Siphon players to capture energy from their abilities. Gather the required energy and categories, then hold a Power Node during Grid Breach to win alone.";
    public Color RoleColor => new(0.78f, 0.18f, 0.92f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Rift;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 35,
            CanModifyChance = true,
            AffectedByLightOnAirship = false,
            CanGetKilled = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = NewModAsset.EnergyThiefIcon,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        var playerId = PlayerControl.LocalPlayer.PlayerId;
        Energy.TryGetValue(playerId, out var energy);
        Categories.TryGetValue(playerId, out var categories);
        categories ??= [];

        var options = OptionGroupSingleton<EnergyThiefOptions>.Instance;
        text.AppendLine($"<size=65%>Energy: <color=#D96BFF>{energy}</color>/{(int)options.EnergyRequired}</size>");
        text.AppendLine($"<size=65%>Resonance: <color=#75E6FF>{categories.Count}</color>/{(int)options.CategoriesRequired} categories</size>");

        if (categories.Count > 0)
            text.AppendLine($"<size=60%><color=#BEBEBE>{string.Join(" • ", categories)}</color></size>");

        if (BreachActive && NodeOwnerId == playerId)
            text.AppendLine($"<size=65%><color=#FF5CEA>GRID BREACH: {Mathf.Max(0f, BreachEndsAt - Time.time):F1}s</color></size>");
        else if (IsReady(playerId))
            text.AppendLine("<size=65%><color=#75E6A5>Power Node located. Reach it and breach the grid.</color></size>");
        else if (TetherTargets.TryGetValue(playerId, out var targetId))
            text.AppendLine($"<size=65%><color=#FFD166>Siphoning {Utils.PlayerById(targetId).Data.PlayerName}...</color></size>");

        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<EnergyThiefGameOver>();
    }

    public static bool IsReady(byte playerId)
    {
        Energy.TryGetValue(playerId, out var energy);
        Categories.TryGetValue(playerId, out var categories);
        var options = OptionGroupSingleton<EnergyThiefOptions>.Instance;
        return energy >= options.EnergyRequired && categories is { Count: var count } && count >= options.CategoriesRequired;
    }

    public static bool IsBrownedOut(byte playerId, EnergyCategory category)
    {
        if (!BrownoutCategories.TryGetValue(playerId, out var storedCategory) || storedCategory != category || !BrownoutExpiresAt.TryGetValue(playerId, out var expiresAt))
            return false;

        if (Time.time < expiresAt)
            return true;

        BrownoutCategories.Remove(playerId);
        BrownoutExpiresAt.Remove(playerId);
        return false;
    }

    public static void ReportAbilityUse(EnergyCategory category)
    {
        var player = PlayerControl.LocalPlayer;
        if (player && TetherTargets.ContainsValue(player.PlayerId))
            RpcRequestCapture(player, (byte)category, false);
    }

    public static void ReportRawAction(PlayerControl player)
    {
        if (player.AmOwner && TetherTargets.ContainsValue(player.PlayerId))
            RpcRequestCapture(player, byte.MaxValue, true);
    }

    public static Vector3[] CurrentMapNodePositions => ShipStatus.Instance is AirshipStatus ? AirshipPositions : MapNodePositions[ShipStatus.Instance.Type];

    public static Vector3 GetNodePosition()
    {
        return NodePositionIndex >= 0 && NodePositionIndex < CurrentMapNodePositions.Length ? CurrentMapNodePositions[NodePositionIndex] : Vector3.zero;
    }

    public static void ResetState()
    {
        Energy.Clear();
        Categories.Clear();
        TetherTargets.Clear();
        TetherExpiresAt.Clear();
        HarvestedTargets.Clear();
        BrownoutCategories.Clear();
        BrownoutExpiresAt.Clear();
        NodeOwnerId = byte.MaxValue;
        NodePositionIndex = -1;
        BreachActive = false;
        BreachEndsAt = 0f;
        _breachResolved = false;
        EnergyPowerNode.DestroyCurrent();
    }

    public static void HostFixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in TetherTargets.ToArray())
        {
            var thief = Utils.PlayerById(pair.Key);
            var target = Utils.PlayerById(pair.Value);
            var expired = !TetherExpiresAt.TryGetValue(pair.Key, out var expiresAt) || Time.time >= expiresAt;
            var invalid = !thief || !target || thief.Data.IsDead || target.Data.IsDead || thief.Data.Disconnected || target.Data.Disconnected;

            if (invalid || MeetingHud.Instance || ExileController.Instance)
            {
                RpcConfirmCancelSiphon(PlayerControl.LocalPlayer, pair.Key);
                continue;
            }

            if (expired)
                RpcConfirmCapture(PlayerControl.LocalPlayer, pair.Key, pair.Value, byte.MaxValue, true);
        }

        if (!BreachActive || _breachResolved)
            return;

        var owner = Utils.PlayerById(NodeOwnerId);
        var interrupted = !owner || owner.Data.IsDead || owner.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || Vector2.Distance(owner.GetTruePosition(), GetNodePosition()) > OptionGroupSingleton<EnergyThiefOptions>.Instance.GridBreachRadius;

        if (interrupted)
        {
            _breachResolved = true;
            RpcResolveBreach(PlayerControl.LocalPlayer, NodeOwnerId, false);
        }
        else if (Time.time >= BreachEndsAt)
        {
            _breachResolved = true;
            RpcResolveBreach(PlayerControl.LocalPlayer, NodeOwnerId, true);
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro || (Application.platform == RuntimePlatform.Android && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay))
            return;

        ResetState();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var thiefId in TetherTargets.Keys.ToArray())
            RpcConfirmCancelSiphon(PlayerControl.LocalPlayer, thiefId);

        HarvestedTargets.Clear();
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is EnergyThief)
            return;

        var playerId = evt.Player.PlayerId;
        Energy.Remove(playerId);
        Categories.Remove(playerId);
        TetherTargets.Remove(playerId);
        TetherExpiresAt.Remove(playerId);
        HarvestedTargets.Remove(playerId);
        BrownoutCategories.Remove(playerId);
        BrownoutExpiresAt.Remove(playerId);
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        ReportRawAction(evt.Player);
    }

    [RegisterEvent(int.MaxValue)]
    public static void OnEnterVent(EnterVentEvent evt)
    {
        if (!evt.IsCancelled)
            ReportRawAction(evt.Player);
    }

    [RegisterEvent(int.MaxValue)]
    public static void OnReportBody(ReportBodyEvent evt)
    {
        if (!evt.IsCancelled)
            ReportRawAction(evt.Reporter);
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefRequestSiphon)]
    public static void RpcRequestSiphon(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not EnergyThief || source.Data.IsDead || target.Data.IsDead || target.Data.Disconnected || source == target || IsReady(source.PlayerId) || TetherTargets.ContainsKey(source.PlayerId))
            return;

        var options = OptionGroupSingleton<EnergyThiefOptions>.Instance;
        if (Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > options.SiphonRange)
            return;

        if (HarvestedTargets.TryGetValue(source.PlayerId, out var harvested) && harvested.Contains(target.PlayerId))
            return;

        RpcConfirmSiphon(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId, options.SiphonDuration);
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefConfirmSiphon, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmSiphon(PlayerControl host, byte thiefId, byte targetId, float duration)
    {
        if (!host.IsHost() || TetherTargets.ContainsKey(thiefId))
            return;

        TetherTargets[thiefId] = targetId;
        TetherExpiresAt[thiefId] = Time.time + duration;

        if (PlayerControl.LocalPlayer.PlayerId == thiefId)
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#D96BFF>Siphon attached:</color> {Utils.PlayerById(targetId).Data.PlayerName}"));
        else if (PlayerControl.LocalPlayer.PlayerId == targetId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#D96BFF>Energy interference detected.</color> Using an ability will cause a brownout."));
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefRequestCapture)]
    public static void RpcRequestCapture(PlayerControl source, byte categoryId, bool raw)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.IsDead || source.Data.Disconnected)
            return;

        foreach (var pair in TetherTargets)
        {
            if (pair.Value != source.PlayerId)
                continue;

            var thief = Utils.PlayerById(pair.Key);
            if (!thief || thief.Data.Role is not EnergyThief || thief.Data.IsDead || thief.Data.Disconnected || !TetherExpiresAt.TryGetValue(pair.Key, out var expiresAt) || Time.time >= expiresAt)
                return;

            if (!raw && categoryId >= (byte)EnergyCategory.Protection + 1)
                return;

            RpcConfirmCapture(PlayerControl.LocalPlayer, pair.Key, source.PlayerId, categoryId, raw);
            return;
        }
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefConfirmCapture, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmCapture(PlayerControl host, byte thiefId, byte targetId, byte categoryId, bool raw)
    {
        if (!host.IsHost() || !TetherTargets.TryGetValue(thiefId, out var storedTarget) || storedTarget != targetId)
            return;

        Energy.TryGetValue(thiefId, out var energy);
        Energy[thiefId] = Mathf.Min((int)OptionGroupSingleton<EnergyThiefOptions>.Instance.EnergyRequired, energy + (raw ? RawEnergyGain : TypedEnergyGain));
        var category = (EnergyCategory)categoryId;
        Sprite categoryIcon = null;

        if (!raw)
        {
            if (!Categories.TryGetValue(thiefId, out var categories))
                Categories[thiefId] = categories = [];

            if (!categories.Contains(category))
                categories.Add(category);

            BrownoutCategories[targetId] = category;
            BrownoutExpiresAt[targetId] = Time.time + OptionGroupSingleton<EnergyThiefOptions>.Instance.BrownoutDuration;
            categoryIcon = (category switch
            {
                EnergyCategory.Intelligence => NewModAsset.IntelligenceEnergyIcon,
                EnergyCategory.Mobility => NewModAsset.MobilityEnergyIcon,
                EnergyCategory.Control => NewModAsset.ControlEnergyIcon,
                EnergyCategory.Protection => NewModAsset.ProtectionEnergyIcon,
                _ => NewModAsset.AggressionEnergyIcon
            }).LoadAsset();
        }

        TetherTargets.Remove(thiefId);
        TetherExpiresAt.Remove(thiefId);

        if (!HarvestedTargets.TryGetValue(thiefId, out var harvested))
            HarvestedTargets[thiefId] = harvested = [];

        harvested.Add(targetId);

        if (PlayerControl.LocalPlayer.PlayerId == thiefId)
        {
            if (raw)
                Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#D96BFF>Captured:</color> Raw Energy +{RawEnergyGain}"));
            else
                Helpers.CreateAndShowNotification($"Captured {category} Energy +{TypedEnergyGain}", new Color(0.85f, 0.42f, 1f), spr: categoryIcon);
        }

        if (PlayerControl.LocalPlayer.PlayerId == targetId && !raw)
            Helpers.CreateAndShowNotification($"{category} abilities disrupted", new Color(0.85f, 0.42f, 1f), spr: categoryIcon);

        if (AmongUsClient.Instance.AmHost && IsReady(thiefId) && NodePositionIndex < 0)
        {
            var positions = CurrentMapNodePositions;

            if (positions.Length > 0)
                RpcSelectNode(PlayerControl.LocalPlayer, thiefId, Random.Range(0, positions.Length));
        }
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefRequestCancelSiphon)]
    public static void RpcRequestCancelSiphon(PlayerControl source)
    {
        if (AmongUsClient.Instance.AmHost && source.Data.Role is EnergyThief && TetherTargets.ContainsKey(source.PlayerId))
            RpcConfirmCancelSiphon(PlayerControl.LocalPlayer, source.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefConfirmCancelSiphon, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmCancelSiphon(PlayerControl host, byte thiefId)
    {
        if (!host.IsHost())
            return;

        TetherTargets.Remove(thiefId);
        TetherExpiresAt.Remove(thiefId);
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefSelectNode, LocalHandling = RpcLocalHandling.After)]
    public static void RpcSelectNode(PlayerControl host, byte ownerId, int positionIndex)
    {
        if (!host.IsHost() || positionIndex < 0 || positionIndex >= CurrentMapNodePositions.Length)
            return;

        NodeOwnerId = ownerId;
        NodePositionIndex = positionIndex;
        EnergyPowerNode.Create();

        if (PlayerControl.LocalPlayer.PlayerId == ownerId)
        {
            var room = ShipStatus.Instance.AllRooms.FirstOrDefault(candidate => candidate.roomArea && candidate.roomArea.OverlapPoint(GetNodePosition()));
            var location = room ? DestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId) : "Unknown";
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#75E6A5>Power Node located:</color> {location}"));
        }
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefRequestBreach)]
    public static void RpcRequestBreach(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not EnergyThief || source.Data.IsDead || source.PlayerId != NodeOwnerId || BreachActive || !IsReady(source.PlayerId))
            return;

        var options = OptionGroupSingleton<EnergyThiefOptions>.Instance;
        if (Vector2.Distance(source.GetTruePosition(), GetNodePosition()) > options.GridBreachRadius)
            return;

        RpcConfirmBreach(PlayerControl.LocalPlayer, source.PlayerId, options.GridBreachDuration);
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefConfirmBreach, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmBreach(PlayerControl host, byte ownerId, float duration)
    {
        if (!host.IsHost() || ownerId != NodeOwnerId || BreachActive)
            return;

        BreachActive = true;
        BreachEndsAt = Time.time + duration;
        _breachResolved = false;

        var room = ShipStatus.Instance.AllRooms.FirstOrDefault(candidate => candidate.roomArea && candidate.roomArea.OverlapPoint(GetNodePosition()));
        var location = room ? DestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId) : "Unknown";
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF5CEA>ENERGY SURGE DETECTED:</color> {location}"));
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefRequestGround)]
    public static void RpcRequestGround(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || !BreachActive || _breachResolved || source.Data.IsDead || source.Data.Disconnected || source.PlayerId == NodeOwnerId)
            return;

        if (Vector2.Distance(source.GetTruePosition(), GetNodePosition()) > OptionGroupSingleton<EnergyThiefOptions>.Instance.GridBreachRadius)
            return;

        _breachResolved = true;
        RpcResolveBreach(PlayerControl.LocalPlayer, NodeOwnerId, false);
    }

    [MethodRpc((uint)CustomRPC.EnergyThiefResolveBreach, LocalHandling = RpcLocalHandling.After)]
    public static void RpcResolveBreach(PlayerControl host, byte ownerId, bool succeeded)
    {
        if (!host.IsHost() || ownerId != NodeOwnerId)
            return;

        BreachActive = false;
        BreachEndsAt = 0f;

        if (succeeded)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                var winner = Utils.PlayerById(ownerId);
                CustomGameOver.Trigger<EnergyThiefGameOver>([winner.Data]);
            }

            return;
        }

        Energy.TryGetValue(ownerId, out var energy);
        Energy[ownerId] = Mathf.Max(0, energy - (int)OptionGroupSingleton<EnergyThiefOptions>.Instance.EnergyLostOnInterruption);

        if (Categories.TryGetValue(ownerId, out var categories) && categories.Count > 0)
            categories.RemoveAt(0);

        NodeOwnerId = byte.MaxValue;
        NodePositionIndex = -1;
        EnergyPowerNode.DestroyCurrent();

        if (PlayerControl.LocalPlayer.PlayerId == ownerId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF5CEA>Grid Breach interrupted.</color> Oldest resonance lost."));
    }
}
