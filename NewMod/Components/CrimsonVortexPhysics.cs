using MiraAPI.GameOptions;
using MiraAPI.Networking;
using NewMod.GeneralEvents.Season1;
using NewMod.Achievements;
using NewMod.Options;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class CrismonVortexPhysics(nint ptr) : MonoBehaviour(ptr)
{
    private PlayerPhysics _physics;
    private PlayerControl _player;

    private bool _collisionDisabled;
    private bool _writingVelocity;
    private bool _escaping;
    private bool _escapedThisEntry;
    private bool _deathRequested;

    private float _escapeProgress;
    private float _lastPressTime;

    public void Awake()
    {
        _physics = GetComponent<PlayerPhysics>();
        _player = GetComponent<PlayerControl>();
        _lastPressTime = float.NegativeInfinity;
    }

    public void Update()
    {
        if (!_physics.AmOwner)
            return;

        if (!CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance || _player.Data.IsDead)
        {
            _escapeProgress = 0f;
            _lastPressTime = float.NegativeInfinity;

            if (CrimsonVortexEscapeHud.Instance)
                CrimsonVortexEscapeHud.Instance.ForceHide();

            return;
        }

        if (_escaping)
            return;

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var distance = Vector2.Distance(_player.GetTruePosition(), CrismonVortexGE.VortexPosition);

        var escapeRadius = options.CrimsonRadius * options.CrimsonEscapeZone / 100f;

        if (_escapedThisEntry || !_player.CanMove || _player.inVent || distance > escapeRadius || distance <= options.CrimsonKillRadius)
        {
            _escapeProgress = 0f;
            _lastPressTime = float.NegativeInfinity;

            if (CrimsonVortexEscapeHud.Instance)
                CrimsonVortexEscapeHud.Instance.Hide();

            return;
        }

        var inputBlocked = HudManager.Instance.Chat.IsOpenOrOpening || Minigame.Instance || MapBehaviour.Instance && MapBehaviour.Instance.gameObject.activeSelf || MatchInfoGuide.Instance && MatchInfoGuide.Instance.IsActive;

        if (!inputBlocked && Input.GetKeyDown(KeyCode.Space))
        {
            _escapeProgress = Mathf.Clamp01(_escapeProgress + options.CrimsonEscapeProgressPerPress / 100f);

            _lastPressTime = Time.time;

            if (CrimsonVortexEscapeHud.Instance)
                CrimsonVortexEscapeHud.Instance.Pulse();
        }
        else if (Time.time - _lastPressTime > options.CrimsonEscapeDecayDelay)
        {
            _escapeProgress = Mathf.Max(0f, _escapeProgress - options.CrimsonEscapeDecayRate / 100f * Time.deltaTime);
        }

        if (CrimsonVortexEscapeHud.Instance)
        {
            CrimsonVortexEscapeHud.Instance.SetProgress(_escapeProgress);
        }

        if (_escapeProgress >= 1f)
            CrismonVortexGE.RpcEscapeVortex(_player);
    }

    public void FixedUpdate()
    {
        if (!CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance || _player.Data.IsDead)
        {
            _escaping = false;
            _escapedThisEntry = false;
            _deathRequested = false;
            _escapeProgress = 0f;
            _lastPressTime = float.NegativeInfinity;

            if (_collisionDisabled)
            {
                _player.Collider.enabled = true;
                _collisionDisabled = false;
            }

            return;
        }

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var distance = Vector2.Distance(_player.GetTruePosition(), CrismonVortexGE.VortexPosition);

        if (distance > options.CrimsonKillRadius)
            _deathRequested = false;

        if (distance >= options.CrimsonRadius)
        {
            _escaping = false;
            _escapedThisEntry = false;

            if (_collisionDisabled)
            {
                _player.Collider.enabled = true;
                _collisionDisabled = false;
            }
        }

        if (AmongUsClient.Instance.AmHost && !_deathRequested && !_escaping && !_player.inVent && distance <= options.CrimsonKillRadius)
        {
            _deathRequested = true;

            _player.RpcAdvancedCustomMurder(_player, MeetingCheck.OutsideMeeting, isIndirect: true, ignoreDefense: true, resetKillTimer: false, teleportMurderer: false, showKillAnim: false, playKillSound: false);

            return;
        }

        if (_player.isDummy && AmongUsClient.Instance.AmHost)
        {
            ApplyVortex();
        }
        else if (_physics.AmOwner && !_escaping && !_player.CanMove && _collisionDisabled)
        {
            _player.Collider.enabled = true;
            _collisionDisabled = false;
        }
    }

    public void ApplyVortex()
    {
        if (_writingVelocity || !CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance || _player.Data.IsDead)
        {
            return;
        }

        if (_escaping)
            return;

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var fromCenter = _player.GetTruePosition() - CrismonVortexGE.VortexPosition;

        var distance = fromCenter.magnitude;

        if (_player.inVent)
        {
            if (_collisionDisabled)
            {
                _player.Collider.enabled = true;
                _collisionDisabled = false;
            }

            if (_physics.AmOwner && Vent.currentVent)
                _physics.RpcBootFromVent(Vent.currentVent.Id);

            return;
        }

        if (distance >= options.CrimsonRadius)
        {
            if (_collisionDisabled)
            {
                _player.Collider.enabled = true;
                _collisionDisabled = false;
            }

            return;
        }

        if (!_player.CanMove)
        {
            if (_collisionDisabled)
            {
                _player.Collider.enabled = true;
                _collisionDisabled = false;
            }

            return;
        }

        if (!_collisionDisabled && _player.Collider.enabled)
        {
            _player.Collider.enabled = false;
            _collisionDisabled = true;
        }

        if (distance <= 0.001f)
            return;

        var outward = fromCenter / distance;
        var inward = -outward;
        var tangent = new Vector2(-outward.y, outward.x);

        var proximity = Mathf.Clamp01(1f - distance / options.CrimsonRadius);

        var pullProgress = proximity * proximity;

        var pull = Mathf.Lerp(options.CrimsonEdgePull, options.CrimsonPullStrength, pullProgress);

        var orbit = options.CrimsonOrbitStrength * (1f - pullProgress);

        var currentDirection = Vector2.zero;

        if (!_player.isDummy)
        {
            currentDirection = _physics.GetVelocity() / Mathf.Max(_physics.TrueSpeed, 0.01f);
        }

        var direction = currentDirection + inward * pull + tangent * orbit;

        _writingVelocity = true;

        _physics.SetNormalizedVelocity(Vector2.ClampMagnitude(direction, options.CrimsonMaxSpeed));

        _writingVelocity = false;
    }

    public bool CanAcceptEscape()
    {
        if (!CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance || _player.Data.IsDead || !_player.CanMove || _player.inVent || _escaping || _escapedThisEntry)
        {
            return false;
        }

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var distance = Vector2.Distance(_player.GetTruePosition(), CrismonVortexGE.VortexPosition);

        return distance < options.CrimsonRadius && distance > options.CrimsonKillRadius;
    }

    public void BeginEscape()
    {
        if (_escaping || _escapedThisEntry)
            return;

        _escaping = true;
        _escapedThisEntry = true;
        _escapeProgress = 0f;
        _lastPressTime = float.NegativeInfinity;

        if (!_physics.AmOwner)
            return;

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var escapePosition = Vector2.zero;
        var closestDistance = float.MaxValue;

        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room.RoomId == SystemTypes.Hallway || !room.roomArea)
            {
                continue;
            }

            var roomPosition = (Vector2)room.roomArea.bounds.center;

            if (Vector2.Distance(roomPosition, CrismonVortexGE.VortexPosition) <= options.CrimsonRadius + options.CrimsonEscapeSafeDistance)
            {
                continue;
            }

            var distance = Vector2.Distance(_player.GetTruePosition(), roomPosition);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            escapePosition = roomPosition;
        }

        _player.NetTransform.RpcSnapTo(escapePosition);
        NewModAchievementsTab.EventHorizonDenied.Unlock();

        if (_collisionDisabled)
        {
            _player.Collider.enabled = true;
            _collisionDisabled = false;
        }

        if (CrimsonVortexEscapeHud.Instance)
            CrimsonVortexEscapeHud.Instance.ShowSuccess();
    }

    public bool ShouldConsumeSpace()
    {
        if (!CrismonVortexGE.Active || !CrismonVortexGE.PositionReady || MeetingHud.Instance || ExileController.Instance || _player.Data.IsDead)
        {
            return false;
        }

        if (_escaping)
            return true;

        var options = OptionGroupSingleton<GEOptions>.Instance;
        var distance = Vector2.Distance(_player.GetTruePosition(), CrismonVortexGE.VortexPosition);

        return !_escapedThisEntry && distance <= options.CrimsonRadius * options.CrimsonEscapeZone / 100f && distance > options.CrimsonKillRadius;
    }

    public void OnDestroy()
    {
        if (_collisionDisabled)
            _player.Collider.enabled = true;

        if (_physics.AmOwner && CrimsonVortexEscapeHud.Instance)
        {
            CrimsonVortexEscapeHud.Instance.ForceHide();
        }
    }
}