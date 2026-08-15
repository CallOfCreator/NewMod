using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameEnd;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.GameModes.WraithSiegeGamemode.Options;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace NewMod.GameModes.WraithSiegeGamemode;

[MiraIgnore]
public sealed class WraithSiege : AbstractGameMode
{
    public static readonly Color WraithColor = new Color32(155, 108, 255, 255);
    public static readonly Color ReviverColor = new Color32(88, 232, 190, 255);

    private readonly Dictionary<int, WraithSiegeNpc> _activeNpcs = [];
    private readonly Dictionary<byte, float> _burnTimers = [];
    private readonly Dictionary<int, int> _deliverySlots = [];
    private readonly Dictionary<byte, float> _summonCooldowns = [];
    private readonly Dictionary<byte, float> _nextSummonAt = [];
    private readonly HashSet<byte> _wraithIds = [];

    private TextMeshPro _energyText;
    private TextMeshPro _hudText;
    private TextMeshPro _warningText;

    private SpriteRenderer _ticketIcon;
    private ArrowBehaviour _flagArrow;

    private GameObject _flagZone;
    private GameObject _flagObject;

    private int _deliveries;
    private int _nextNpcId;
    private int _lastCountdownSecond = -1;

    private float _remainingTime;
    private float _syncTimer;
    private float _localBurnTime;

    private bool _initialized;
    private bool _lastStandPending;
    private bool _finalAlert;
    private bool _wraithsWon;
    private bool _layoutReady;

    private byte _layoutIndex;

    private Vector2 _flagPosition;
    private Vector2 _wraithSpawn;
    private Vector2 _reviverSpawn;

    public float WraithEnergy { get; private set; }

    public int NpcPool { get; private set; }
    public int Tickets { get; private set; }
    public bool Ended { get; private set; }

    public static WraithSiege Instance => (WraithSiege)CustomGameModeManager.ActiveMode;

    public override string Name => "Wraith Siege";

    public override string Description => "Wraiths send spirits to the Flag. Revivers defend with shared lives.";

    public override LoadableAsset<Sprite> Icon => NewModAsset.WraithSiegeFlag;
    public override Color Color => WraithColor;

    public override bool ShowNormalGameSettings => false;
    public override bool ShowNormalRoleSettings => false;
    public override bool ShowGameModeIntroCutscene => true;

    public override float DefaultImpostorKillCooldown => OptionGroupSingleton<WraithSiegeOptions>.Instance.WraithKillCooldown;

    private int AliveRevivers
    {
        get
        {
            var count = 0;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!IsWraith(player) && !player.Data.IsDead && !player.Data.Disconnected)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private int ConnectedRevivers
    {
        get
        {
            var count = 0;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!IsWraith(player) && !player.Data.Disconnected)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private int ConnectedWraiths
    {
        get
        {
            var count = 0;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (IsWraith(player) && !player.Data.Disconnected)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int GetActiveNpcCount(byte ownerId)
    {
        return _activeNpcs.Values.Count(npc => npc && npc.Active && npc.Owner && npc.Owner.PlayerId == ownerId);
    }

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;

        if (!AmongUsClient.Instance.AmHost)
            return;

        var players = GameData.Instance.AllPlayers.ToArray().Where(player => player != null && player.Object && !player.Disconnected).ToList();

        var requested = (int)OptionGroupSingleton<WraithSiegeOptions>.Instance.WraithPlayers;

        var wraithCount = Mathf.Clamp(requested, 1, Mathf.Max(1, players.Count - 1));

        for (var i = 0; i < wraithCount; i++)
        {
            var index = HashRandom.FastNext(players.Count);
            var player = players[index];

            players.RemoveAt(index);
            player.Object.RpcSetRole(RoleTypes.Impostor);
        }

        foreach (var player in players)
            player.Object.RpcSetRole(RoleTypes.Crewmate);
    }

    public override void Initialize()
    {
        Coroutines.Start(CoInitializeRound());
    }

    private IEnumerator CoInitializeRound()
    {
        yield return new WaitForEndOfFrame();

        Reset();

        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        _remainingTime = options.RoundTime;
        Tickets = (int)options.Tickets;
        NpcPool = (int)options.NpcPool;
        WraithEnergy = options.MaxEnergy;

        if (AmongUsClient.Instance.AmHost)
        {
            _layoutIndex = WraithSiegeMapData.PickRandomLayout();
            WraithSiegeMapData.SetLayout(_layoutIndex);
            _layoutReady = true;

            RpcSyncState(PlayerControl.LocalPlayer, _remainingTime, Tickets, _deliveries, NpcPool, WraithEnergy, _layoutIndex);
        }
        else
        {
            while (!_layoutReady)
                yield return null;
        }

        _flagPosition = WraithSiegeMapData.FlagPoint;
        _wraithSpawn = WraithSiegeMapData.WraithSpawn;
        _reviverSpawn = WraithSiegeMapData.ReviverSpawn;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            player.ClearTasks();

            if (player.Data.Role.IsImpostor)
                _wraithIds.Add(player.PlayerId);
        }

        ShipStatus.Instance.BreakEmergencyButton();

        var hud = HudManager.Instance;

        hud.SetAlertOverlay(false);

        _hudText = Helpers.CreateTextLabel("WraithSiegeHud", hud.transform, AspectPosition.EdgeAlignments.Top, new Vector3(0f, 0.2f, -20f), 1.55f, TextAlignmentOptions.TopRight);

        _warningText = Helpers.CreateTextLabel("WraithSiegeWarning", hud.transform, AspectPosition.EdgeAlignments.Top, new Vector3(0f, 0.8f, -20f), 1.7f, TextAlignmentOptions.Center);

        _energyText = Helpers.CreateTextLabel("WraithSiegeEnergy", hud.transform, AspectPosition.EdgeAlignments.Bottom, new Vector3(0f, 1.05f, -20f), 1.45f, TextAlignmentOptions.Center);

        _energyText.fontStyle = FontStyles.Bold;
        _energyText.gameObject.SetActive(IsWraith(PlayerControl.LocalPlayer));

        _warningText.fontStyle = FontStyles.Bold;

        var ticketObject = new GameObject("WraithSiegeTicketIcon") { layer = 5 };

        ticketObject.transform.SetParent(hud.transform, false);
        ticketObject.transform.localScale = Vector3.one * 0.3f;

        var ticketPosition = ticketObject.AddComponent<AspectPosition>();

        ticketPosition.Alignment = AspectPosition.EdgeAlignments.Top;
        ticketPosition.DistanceFromEdge = new Vector3(2.2f, 0.75f, -20f);

        ticketPosition.AdjustPosition();

        _ticketIcon = ticketObject.AddComponent<SpriteRenderer>();
        _ticketIcon.sprite = NewModAsset.WraithSiegeTicket.LoadAsset();
        _ticketIcon.sortingOrder = 100;

        _flagZone = Utils.CreateCircle("WraithSiegeFlagZone", _flagPosition, options.FlagRadius, new Color32(155, 108, 255, 90), options.RoundTime + 10f);

        _flagObject = new GameObject("WraithSiegeFlag");

        _flagObject.transform.SetParent(ShipStatus.Instance.transform, true);

        _flagObject.transform.position = new Vector3(_flagPosition.x, _flagPosition.y, -5f);

        _flagObject.transform.localScale = Vector3.one * 0.8f;

        var flagRenderer = _flagObject.AddComponent<SpriteRenderer>();

        flagRenderer.sprite = NewModAsset.WraithSiegeFlag.LoadAsset();

        flagRenderer.sortingOrder = 100;

        var arrowObject = new GameObject("WraithSiegeFlagArrow") { layer = 5 };

        var arrowRenderer = arrowObject.AddComponent<SpriteRenderer>();

        arrowRenderer.sprite = NewModAsset.Arrow.LoadAsset();
        arrowRenderer.color = IsWraith(PlayerControl.LocalPlayer) ? WraithColor : ReviverColor;

        _flagArrow = arrowObject.AddComponent<ArrowBehaviour>();
        _flagArrow.target = _flagPosition;
        _flagArrow.alwaysMaxSize = true;
        _flagArrow.MaxScale = 0.75f;

        var localPlayer = PlayerControl.LocalPlayer;

        var spawn = IsWraith(localPlayer) ? WraithSiegeMapData.GetWraithPlayerSpawn(localPlayer.PlayerId) : WraithSiegeMapData.GetReviverPlayerSpawn(localPlayer.PlayerId);

        localPlayer.NetTransform.Halt();
        localPlayer.NetTransform.RpcSnapTo(spawn);

        hud.PlayerCam.SnapToTarget();

        _initialized = true;

        _ticketIcon.gameObject.SetActive(!IsWraith(localPlayer));

        hud.SetHudActive(true);
        yield return new WaitForSeconds(1.25f);

        Coroutines.Start(CoroutinesHelper.CoNotify(IsWraith(localPlayer) ? "<color=#9B6CFF><b>WRAITH TEAM</b></color>\nSummon Wraiths, choose a lane and breach the Flag." : "<color=#58E8BE><b>REVIVER TEAM</b></color>\nDefend the Flag and preserve your Tickets."));
    }

    public override void HudUpdate(HudManager instance)
    {
        if (!_initialized || Ended)
            return;

        var deltaTime = Time.deltaTime;
        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        _remainingTime = Mathf.Max(0f, _remainingTime - deltaTime);

        WraithEnergy = Mathf.Min(options.MaxEnergy, WraithEnergy + options.EnergyRegen * deltaTime);

        foreach (var playerId in _wraithIds)
        {
            if (!_summonCooldowns.TryGetValue(playerId, out var cooldown) || cooldown <= 0f)
            {
                continue;
            }

            _summonCooldowns[playerId] = Mathf.Max(0f, cooldown - deltaTime);
        }

        UpdateLocalBurn(deltaTime);
        UpdateFinalCountdown();
        UpdateHud();

        if (!AmongUsClient.Instance.AmHost)
            return;

        UpdateFlagBurn(deltaTime);

        _syncTimer -= deltaTime;

        if (_syncTimer <= 0f)
        {
            _syncTimer = 0.5f;

            RpcSyncState(PlayerControl.LocalPlayer, _remainingTime, Tickets, _deliveries, NpcPool, WraithEnergy, _layoutIndex);
        }

        EvaluateGameEnd();
    }

    private void UpdateFlagBurn(float deltaTime)
    {
        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        foreach (var playerId in _wraithIds)
        {
            var player = GameData.Instance.GetPlayerById(playerId)?.Object;

            if (!player || player.Data.IsDead || player.Data.Disconnected)
            {
                _burnTimers.Remove(playerId);
                continue;
            }

            if (Vector2.Distance(player.GetTruePosition(), _flagPosition) > options.FlagRadius)
            {
                _burnTimers.Remove(playerId);
                continue;
            }

            _burnTimers.TryGetValue(playerId, out var burnTime);

            burnTime += deltaTime;
            _burnTimers[playerId] = burnTime;

            if (burnTime < options.BurnTime)
                continue;

            _burnTimers.Remove(playerId);

            player.RpcCustomMurder(player, didSucceed: true, resetKillTimer: false, createDeadBody: true, teleportMurderer: false, showKillAnim: false, playKillSound: false);
        }
    }

    private void UpdateLocalBurn(float deltaTime)
    {
        var player = PlayerControl.LocalPlayer;
        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        if (!IsWraith(player) || player.Data.IsDead || Vector2.Distance(player.GetTruePosition(), _flagPosition) > options.FlagRadius)
        {
            _localBurnTime = 0f;
            return;
        }

        _localBurnTime = Mathf.Min(options.BurnTime, _localBurnTime + deltaTime);
    }

    private void UpdateFinalCountdown()
    {
        if (_remainingTime > 10f || _remainingTime <= 0f)
        {
            return;
        }

        var hns = GameManagerCreator.Instance.HideAndSeekManagerPrefab;

        if (!_finalAlert)
        {
            _finalAlert = true;

            HudManager.Instance.SetAlertOverlay(true);

            SoundManager.Instance.PlaySound(hns.FinalHideAlertSFX, false);

            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF4D4D><b>FINAL 10 SECONDS</b></color>"));
        }

        var second = Mathf.CeilToInt(_remainingTime);

        if (second == _lastCountdownSecond)
            return;

        _lastCountdownSecond = second;

        var ratio = _remainingTime / 10f;

        SoundManager.Instance.PlaySoundImmediate(hns.FinalHideCountdownSFX, false, 1f, 1.5f - ratio / 2f);
    }

    private void UpdateHud()
    {
        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        var seconds = Mathf.CeilToInt(_remainingTime);

        var time = $"{seconds / 60:00}:{seconds % 60:00}";

        var localPlayer = PlayerControl.LocalPlayer;

        _ticketIcon.gameObject.SetActive(!IsWraith(localPlayer));

        if (IsWraith(localPlayer))
        {
            var percentage = Mathf.Clamp01(WraithEnergy / options.MaxEnergy);

            var filled = Mathf.RoundToInt(percentage * 16f);

            _energyText.text = "<color=#9B6CFF><b>TEAM WRAITH ENERGY</b></color>\n" + $"<color=#9B6CFF>{new string('█', filled)}</color>" + $"<color=#39323F>{new string('█', 16 - filled)}</color>\n" + $"<size=70%>{Mathf.CeilToInt(WraithEnergy)}/" + $"{Mathf.CeilToInt(options.MaxEnergy)}  •  " + $"Summon: {Mathf.CeilToInt(options.SummonCost)}</size>";

            var summonCooldown = _summonCooldowns.TryGetValue(localPlayer.PlayerId, out var cooldown) ? cooldown : 0f;

            var activeOwned = GetActiveNpcCount(localPlayer.PlayerId);

            var maxActive = (int)options.MaxActiveNpcsPerWraith;

            _energyText.gameObject.SetActive(true);

            _hudText.text = $"<b><color=#9B6CFF>WRAITHS</color></b>  {time}\n" + $"NPC Pool  <b>{NpcPool}</b>  |  " + $"Yours  <b>{activeOwned}/{maxActive}</b>\n" + $"Deliveries  <b>{_deliveries}/" + $"{(int)options.RequiredDeliveries}</b>" + (summonCooldown > 0f ? $"\n<size=70%>Summon ready in {summonCooldown:0.0}s</size>" : "");

            if (_localBurnTime > 0f)
            {
                var burn = Mathf.Clamp01(_localBurnTime / options.BurnTime);

                _warningText.text = "<color=#FF4D4D><b>THE FLAG IS BURNING YOU</b>\n" + $"{Mathf.RoundToInt(burn * 100f)}%</color>";

                return;
            }

            if (_remainingTime <= 10f && _remainingTime > 0f)
            {
                var needed = Mathf.Max(0, (int)options.RequiredDeliveries - _deliveries);

                _warningText.text = "<color=#FF4D4D><b>FINAL PUSH</b></color>\n" + $"<color=#FFFFFF>{needed} " + $"{(needed == 1 ? "DELIVERY" : "DELIVERIES")} NEEDED  •  " + $"{Mathf.CeilToInt(_remainingTime)}s</color>";

                return;
            }

            _warningText.text = "";
            return;
        }

        _energyText.gameObject.SetActive(false);

        _hudText.text = $"<b><color=#58E8BE>REVIVERS</color></b>  {time}\n" + $"Tickets  <b>{Tickets}</b>  |  " + $"Alive  <b>{AliveRevivers}/{ConnectedRevivers}</b>\n" + $"Deliveries  <b>{_deliveries}/" + $"{(int)options.RequiredDeliveries}</b>\n" + $"Wraiths Remaining  <b>{NpcPool + _activeNpcs.Count}</b>";

        if (localPlayer.Data.IsDead)
        {
            _warningText.text = Tickets <= 0 ? "<color=#FF4D4D><b>NO TICKETS REMAIN</b></color>" : _lastStandPending ? "<color=#FFD166><b>LAST STAND REVIVE...</b></color>" : "<color=#FFD166><b>WAITING FOR A REVIVER...</b></color>";

            return;
        }

        if (_remainingTime <= 10f && _remainingTime > 0f)
        {
            _warningText.text = "<color=#58E8BE><b>HOLD THE LINE</b></color>\n" + $"<color=#FFFFFF>STOP THE FINAL PUSH  •  " + $"{Mathf.CeilToInt(_remainingTime)}s</color>";

            return;
        }

        _warningText.text = "";
    }

    public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
    {
        base.OnPlayerDeath(player, assignGhostRole);

        if (!AmongUsClient.Instance.AmHost || player.Data.Disconnected || Ended)
        {
            return;
        }

        if (IsWraith(player))
        {
            Coroutines.Start(CoRespawnWraith(player.PlayerId));

            return;
        }

        if (AliveRevivers == 0 && Tickets > 0 && !_lastStandPending)
        {
            Coroutines.Start(CoLastStandRevive());
        }
    }

    public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = false;

        if (AmongUsClient.Instance.AmHost)
            EvaluateGameEnd();
    }

    public override List<NetworkedPlayerInfo> CalculateWinners()
    {
        return GameData.Instance.AllPlayers.ToArray().Where(player => !player.Disconnected && _wraithIds.Contains(player.PlayerId) == _wraithsWon).ToList();
    }

    private void EvaluateGameEnd()
    {
        if (Ended)
            return;

        var required = (int)OptionGroupSingleton<WraithSiegeOptions>.Instance.RequiredDeliveries;

        if (_deliveries >= required || ConnectedRevivers == 0)
        {
            FinishGame(true);
            return;
        }

        if (Tickets <= 0 && AliveRevivers == 0 && !_lastStandPending)
        {
            FinishGame(true);
            return;
        }

        if (ConnectedWraiths == 0 || _remainingTime <= 0f || (NpcPool <= 0 && _activeNpcs.Count == 0))
        {
            FinishGame(false);
        }
    }

    private void FinishGame(bool wraithsWon)
    {
        if (Ended)
            return;

        Ended = true;
        _wraithsWon = wraithsWon;

        HudManager.Instance.SetAlertOverlay(false);

        var winners = CalculateWinners();

        if (wraithsWon)
        {
            CustomGameOver.Trigger<WraithSiegeWraithGameOver>(winners);
        }
        else
        {
            CustomGameOver.Trigger<WraithSiegeReviverGameOver>(winners);
        }
    }

    private IEnumerator CoRespawnWraith(byte playerId)
    {
        yield return new WaitForSeconds(OptionGroupSingleton<WraithSiegeOptions>.Instance.WraithRespawnTime);

        if (Ended)
            yield break;

        var player = GameData.Instance.GetPlayerById(playerId)?.Object;

        if (!player || !player.Data.IsDead || player.Data.Disconnected)
        {
            yield break;
        }

        Utils.HandleRevive(PlayerControl.LocalPlayer, playerId, RoleTypes.Impostor, _wraithSpawn.x, _wraithSpawn.y);
    }

    private IEnumerator CoLastStandRevive()
    {
        _lastStandPending = true;

        yield return new WaitForSeconds(2f);

        if (Ended)
        {
            _lastStandPending = false;
            yield break;
        }

        var player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(candidate => !IsWraith(candidate) && candidate.Data.IsDead && !candidate.Data.Disconnected);

        if (player && AliveRevivers == 0 && Tickets > 0)
        {
            Tickets--;

            RpcSyncState(PlayerControl.LocalPlayer, _remainingTime, Tickets, _deliveries, NpcPool, WraithEnergy, _layoutIndex);

            Utils.HandleRevive(PlayerControl.LocalPlayer, player.PlayerId, RoleTypes.Crewmate, _reviverSpawn.x, _reviverSpawn.y);

            yield return new WaitForSeconds(0.3f);
        }

        _lastStandPending = false;
    }

    public bool StartDelivery(WraithSiegeNpc npc)
    {
        if (!AmongUsClient.Instance.AmHost || !npc.Active || _deliverySlots.ContainsKey(npc.NpcId))
        {
            return false;
        }

        var capacity = (int)OptionGroupSingleton<WraithSiegeOptions>.Instance.FlagCapacity;

        if (_deliverySlots.Count >= capacity)
            return false;

        var slot = 0;

        while (_deliverySlots.ContainsValue(slot))
            slot++;

        _deliverySlots[npc.NpcId] = slot;

        RpcBeginDelivery(PlayerControl.LocalPlayer, npc.NpcId, slot);

        Coroutines.Start(CoDelivery(npc.NpcId));

        return true;
    }

    private IEnumerator CoDelivery(int npcId)
    {
        while (!Ended && _activeNpcs.TryGetValue(npcId, out var npc) && npc.Active && !npc.AtDeliverySlot)
        {
            yield return null;
        }

        if (Ended || !_activeNpcs.ContainsKey(npcId))
        {
            yield break;
        }

        var time = OptionGroupSingleton<WraithSiegeOptions>.Instance.DeliveryTime;

        while (time > 0f)
        {
            if (Ended || !_activeNpcs.TryGetValue(npcId, out var npc) || !npc.Active)
            {
                yield break;
            }

            time -= Time.deltaTime;
            yield return null;
        }

        RpcResolveNpc(PlayerControl.LocalPlayer, npcId, true);
    }

    public WraithSiegeNpc GetClosestNpc(PlayerControl player)
    {
        var range = OptionGroupSingleton<WraithSiegeOptions>.Instance.BanishRange;

        var bestDistance = range * range;

        WraithSiegeNpc closest = null;

        foreach (var npc in _activeNpcs.Values)
        {
            if (!npc || !npc.Active)
            {
                continue;
            }

            var distance = ((Vector2)npc.Visual.transform.position - player.GetTruePosition()).sqrMagnitude;

            if (distance > bestDistance)
                continue;

            bestDistance = distance;
            closest = npc;
        }

        return closest;
    }

    public DeadBody GetClosestReviverBody(PlayerControl player)
    {
        var range = OptionGroupSingleton<WraithSiegeOptions>.Instance.ReviveRange;

        var bestDistance = range * range;

        DeadBody closest = null;

        var bodies = Helpers.GetNearestDeadBodies(player.GetTruePosition(), range, Helpers.CreateFilter(Constants.NotShipMask));

        foreach (var body in bodies)
        {
            var target = GameData.Instance.GetPlayerById(body.ParentId)?.Object;

            if (!target || IsWraith(target) || !target.Data.IsDead || target.Data.Disconnected)
            {
                continue;
            }

            var distance = ((Vector2)body.transform.position - player.GetTruePosition()).sqrMagnitude;

            if (distance > bestDistance)
                continue;

            bestDistance = distance;
            closest = body;
        }

        return closest;
    }

    public bool IsWraith(PlayerControl player)
    {
        return _wraithIds.Contains(player.PlayerId);
    }

    public Vector2 GetDeliveryPoint(int slot)
    {
        var capacity = Mathf.Max(1, (int)OptionGroupSingleton<WraithSiegeOptions>.Instance.FlagCapacity);
        var angle = slot * Mathf.PI * 2f / capacity;
        var desired = _flagPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.4f;

        return WraithSiegeMapData.GetSafeNearby(desired, _flagPosition);
    }

    private void Reset()
    {
        foreach (var npc in _activeNpcs.Values.ToArray())
        {
            if (npc)
                npc.Dispose();
        }

        _activeNpcs.Clear();
        _deliverySlots.Clear();
        _wraithIds.Clear();
        _burnTimers.Clear();
        _summonCooldowns.Clear();
        _nextSummonAt.Clear();

        WraithEnergy = 0f;

        if (_flagZone)
            Object.Destroy(_flagZone);

        if (_flagObject)
            Object.Destroy(_flagObject);

        if (_flagArrow)
            Object.Destroy(_flagArrow.gameObject);

        if (_hudText)
            Object.Destroy(_hudText.gameObject);

        if (_warningText)
            Object.Destroy(_warningText.gameObject);

        if (_ticketIcon)
            Object.Destroy(_ticketIcon.gameObject);

        if (_energyText)
            Object.Destroy(_energyText.gameObject);

        _flagZone = null;
        _flagObject = null;
        _flagArrow = null;

        _hudText = null;
        _warningText = null;
        _energyText = null;

        _ticketIcon = null;

        _flagPosition = default;
        _wraithSpawn = default;
        _reviverSpawn = default;

        _deliveries = 0;
        _nextNpcId = 0;

        _remainingTime = 0f;
        _syncTimer = 0f;
        _localBurnTime = 0f;
        _layoutIndex = 0;
        _layoutReady = false;

        _lastCountdownSecond = -1;

        NpcPool = 0;
        Tickets = 0;

        _initialized = false;
        _lastStandPending = false;
        _finalAlert = false;
        _wraithsWon = false;
        Ended = false;
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeSyncState)]
    public static void RpcSyncState(PlayerControl source, float time, int tickets, int deliveries, int npcPool, float energy, byte layoutIndex)
    {
        if (!source.IsHost() || CustomGameModeManager.ActiveMode is not WraithSiege mode)
        {
            return;
        }

        mode._remainingTime = Mathf.Max(0f, time);
        mode.Tickets = tickets;
        mode._deliveries = deliveries;
        mode.NpcPool = npcPool;
        mode.WraithEnergy = Mathf.Clamp(energy, 0f, OptionGroupSingleton<WraithSiegeOptions>.Instance.MaxEnergy);
        mode._layoutIndex = layoutIndex;
        WraithSiegeMapData.SetLayout(layoutIndex);
        mode._layoutReady = true;
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeRequestSummon)]
    public static void RpcRequestSummon(PlayerControl source, byte laneValue)
    {
        if (!AmongUsClient.Instance.AmHost || CustomGameModeManager.ActiveMode is not WraithSiege mode || mode.Ended || !mode.IsWraith(source) || source.Data.IsDead || source.Data.Disconnected || laneValue > (byte)WraithLane.Bottom)
        {
            return;
        }

        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        if (mode._nextSummonAt.TryGetValue(source.PlayerId, out var nextSummon) && Time.time < nextSummon)
        {
            return;
        }

        if (mode.GetActiveNpcCount(source.PlayerId) >= (int)options.MaxActiveNpcsPerWraith)
        {
            return;
        }

        if (mode.NpcPool <= 0 || mode.WraithEnergy < options.SummonCost)
        {
            return;
        }

        mode._nextSummonAt[source.PlayerId] = Time.time + options.SummonCooldown;

        mode.WraithEnergy = Mathf.Max(0f, mode.WraithEnergy - options.SummonCost);

        mode.NpcPool--;

        var npcId = ++mode._nextNpcId;

        RpcSpawnNpc(PlayerControl.LocalPlayer, source.PlayerId, npcId, laneValue, mode.WraithEnergy, mode.NpcPool);
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeSpawnNpc)]
    public static void RpcSpawnNpc(PlayerControl source, byte ownerId, int npcId, byte laneValue, float energy, int npcPool)
    {
        if (!source.IsHost() || CustomGameModeManager.ActiveMode is not WraithSiege mode || mode._activeNpcs.ContainsKey(npcId))
        {
            return;
        }

        var owner = GameData.Instance.GetPlayerById(ownerId)?.Object;

        if (!owner)
            return;

        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        mode.WraithEnergy = Mathf.Clamp(energy, 0f, options.MaxEnergy);

        mode.NpcPool = npcPool;

        mode._summonCooldowns[ownerId] = options.SummonCooldown;

        var lane = (WraithLane)laneValue;

        var route = WraithSiegeMapData.ResolveLane(lane, options.FlagRadius);

        var holder = new GameObject($"WraithSiegeNpc_{npcId}");

        var npc = holder.AddComponent<WraithSiegeNpc>();

        mode._activeNpcs[npcId] = npc;

        npc.Initialize(npcId, owner, lane, route);

        if (owner.AmOwner)
        {
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#9B6CFF>{lane} Wraith deployed.</color>"));
        }
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeBeginDelivery)]
    public static void RpcBeginDelivery(PlayerControl source, int npcId, int slot)
    {
        if (!source.IsHost() || CustomGameModeManager.ActiveMode is not WraithSiege mode || !mode._activeNpcs.TryGetValue(npcId, out var npc))
        {
            return;
        }

        mode._deliverySlots[npcId] = slot;

        npc.BeginDelivery(mode.GetDeliveryPoint(slot));
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeResolveNpc)]
    public static void RpcResolveNpc(PlayerControl source, int npcId, bool delivered)
    {
        if (!source.IsHost() || CustomGameModeManager.ActiveMode is not WraithSiege mode || !mode._activeNpcs.TryGetValue(npcId, out var npc))
        {
            return;
        }

        mode._deliverySlots.Remove(npcId);
        mode._activeNpcs.Remove(npcId);

        npc.Dispose();

        if (!delivered)
            return;

        mode._deliveries++;

        var required = (int)OptionGroupSingleton<WraithSiegeOptions>.Instance.RequiredDeliveries;

        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#9B6CFF><b>WRAITH DELIVERED</b></color> " + $"{mode._deliveries}/{required}"));
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeRequestBanish)]
    public static void RpcRequestBanish(PlayerControl source, int npcId)
    {
        if (!AmongUsClient.Instance.AmHost || CustomGameModeManager.ActiveMode is not WraithSiege mode || mode.Ended || mode.IsWraith(source) || source.Data.IsDead || source.Data.Disconnected || !mode._activeNpcs.TryGetValue(npcId, out var npc))
        {
            return;
        }

        var range = OptionGroupSingleton<WraithSiegeOptions>.Instance.BanishRange;

        if (Vector2.Distance(source.GetTruePosition(), npc.Visual.transform.position) > range)
        {
            return;
        }

        RpcResolveNpc(PlayerControl.LocalPlayer, npcId, false);
    }

    [MethodRpc((uint)CustomRPC.WraithSiegeRequestRevive)]
    public static void RpcRequestRevive(PlayerControl source, byte targetId)
    {
        if (!AmongUsClient.Instance.AmHost || CustomGameModeManager.ActiveMode is not WraithSiege mode || mode.Ended || mode.IsWraith(source) || source.Data.IsDead || source.Data.Disconnected || mode.Tickets <= 0)
        {
            return;
        }

        var target = GameData.Instance.GetPlayerById(targetId)?.Object;

        if (!target || mode.IsWraith(target) || !target.Data.IsDead || target.Data.Disconnected)
            return;

        var body = Helpers.GetBodyById(targetId);

        if (!body)
            return;

        var range = OptionGroupSingleton<WraithSiegeOptions>.Instance.ReviveRange;

        if (Vector2.Distance(source.GetTruePosition(), body.transform.position) > range)
            return;

        mode.Tickets--;

        RpcSyncState(PlayerControl.LocalPlayer, mode._remainingTime, mode.Tickets, mode._deliveries, mode.NpcPool, mode.WraithEnergy, mode._layoutIndex);

        Utils.HandleRevive(PlayerControl.LocalPlayer, targetId, RoleTypes.Crewmate, body.transform.position.x, body.transform.position.y);
    }

    public override IEnumerator IntroCutscene(IntroCutscene intro)
    {
        var isWraith = PlayerControl.LocalPlayer.Data.Role.IsImpostor;

        var color = isWraith ? WraithColor : ReviverColor;

        var options = OptionGroupSingleton<WraithSiegeOptions>.Instance;

        var team = GameData.Instance.AllPlayers.ToArray().Where(player => !player.Disconnected && player.Role.IsImpostor == isWraith).OrderBy(player => player.PlayerId == PlayerControl.LocalPlayer.PlayerId ? 0 : 1).ToArray();

        SoundManager.Instance.PlaySound(intro.IntroStinger, false, 1f, null);

        intro.LogPlayerRoleData();

        intro.HideAndSeekPanels.SetActive(false);
        intro.CrewmateRules.SetActive(false);
        intro.ImpostorRules.SetActive(false);
        intro.ImpostorName.gameObject.SetActive(false);
        intro.ImpostorTitle.gameObject.SetActive(false);
        intro.YouAreText.gameObject.SetActive(false);
        intro.RoleText.gameObject.SetActive(false);
        intro.RoleBlurbText.gameObject.SetActive(false);

        intro.FrontMost.color = Color.clear;

        intro.Foreground.material.SetFloat("_Rad", intro.ForegroundRadius.max);

        intro.TeamTitle.gameObject.SetActive(true);
        intro.ImpostorText.gameObject.SetActive(true);
        intro.BackgroundBar.enabled = true;

        intro.TeamTitle.text = isWraith ? "WRAITHS" : "REVIVERS";

        intro.TeamTitle.color = color;
        intro.TeamTitle.fontSize = 4.5f;

        intro.ImpostorText.text = isWraith ? "SUMMON WRAITHS  •  PUSH THE LANES  •  BREACH THE FLAG\n" + $"<size=70%>Reach {Mathf.RoundToInt(options.RequiredDeliveries)} " + "deliveries before time runs out</size>" : "DEFEND THE FLAG  •  BANISH WRAITHS  •  REVIVE YOUR TEAM\n" + $"<size=70%>{Mathf.RoundToInt(options.Tickets)} shared tickets</size>";

        intro.ImpostorText.color = Color.white;
        intro.ImpostorText.fontSize = 1.65f;

        intro.BackgroundBar.material.SetColor(ShaderID.Color, color);

        yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();

        var maxDepth = Mathf.CeilToInt(7.5f);

        for (var i = 0; i < team.Length; i++)
        {
            var display = intro.CreatePlayer(i, isWraith ? 1 : maxDepth, team[i], isWraith);

            display.ToggleName(false);
        }

        var iconObject = new GameObject(isWraith ? "WraithSiegeIntroWraith" : "WraithSiegeIntroTicket");

        iconObject.transform.SetParent(intro.transform, false);

        iconObject.transform.localPosition = new Vector3(-3.25f, 1.5f, -25f);

        var baseScale = Vector3.one * (isWraith ? 0.55f : 0.4f);

        iconObject.transform.localScale = baseScale;

        var icon = iconObject.AddComponent<SpriteRenderer>();

        icon.sprite = (isWraith ? NewModAsset.WraithSiegeWraith : NewModAsset.WraithSiegeTicket).LoadAsset();

        icon.sortingOrder = 100;

        var timer = 0f;

        while (timer < 6f)
        {
            timer += Time.unscaledDeltaTime;

            iconObject.transform.localScale = baseScale * (1f + Mathf.Sin(timer * 4f) * 0.04f);

            yield return null;
        }

        intro.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        ShipStatus.Instance.StartSFX();
        Object.Destroy(intro.gameObject);
    }

    public override bool CanReport(DeadBody body) => false;

    public override bool CanUseMapConsole(MapConsole console) => false;

    public override bool CanUseSystemConsole(SystemConsole console) => false;

    public override bool CanUseTasks(Console console) => false;

    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;

    public override bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo) => false;
}