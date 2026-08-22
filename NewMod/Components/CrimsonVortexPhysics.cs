using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using NewMod.GeneralEvents.Season1;
using NewMod.Achievements;
using NewMod.Options;
using NewMod.Seasons;
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
    private Vector2 _lastSafePosition;
    private bool _lastSafePositionReady;

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
            if (!CrismonVortexGE.Active && _physics.AmOwner && !_player.Data.IsDead && _player.CanMove && !_player.inVent)
            {
                _lastSafePosition = _player.GetTruePosition();
                _lastSafePositionReady = true;
            }

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

        var killRadius = Mathf.Clamp(options.CrimsonRadius * 0.12f, 0.45f, 0.9f);
        var escapeRadius = options.CrimsonRadius * 0.75f;
        var progressPerPress = options.CrimsonEscapeDifficulty switch
        {
            GEOptions.EscapeDifficulty.Easy => 0.12f,
            GEOptions.EscapeDifficulty.Standard => 0.09f,
            GEOptions.EscapeDifficulty.Hard => 0.07f,
            _ => 0.09f
        };
        var decayDelay = options.CrimsonEscapeDifficulty switch
        {
            GEOptions.EscapeDifficulty.Easy => 0.25f,
            GEOptions.EscapeDifficulty.Standard => 0.15f,
            GEOptions.EscapeDifficulty.Hard => 0.1f,
            _ => 0.15f
        };
        var decayRate = options.CrimsonEscapeDifficulty switch
        {
            GEOptions.EscapeDifficulty.Easy => 0.35f,
            GEOptions.EscapeDifficulty.Standard => 0.55f,
            GEOptions.EscapeDifficulty.Hard => 0.75f,
            _ => 0.55f
        };

        if (_escapedThisEntry || !_player.CanMove || _player.inVent || distance > escapeRadius || distance <= killRadius)
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
            _escapeProgress = Mathf.Clamp01(_escapeProgress + progressPerPress);

            _lastPressTime = Time.time;

            if (CrimsonVortexEscapeHud.Instance)
                CrimsonVortexEscapeHud.Instance.Pulse();
        }
        else if (Time.time - _lastPressTime > decayDelay)
        {
            _escapeProgress = Mathf.Max(0f, _escapeProgress - decayRate * Time.deltaTime);
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
        var killRadius = Mathf.Clamp(options.CrimsonRadius * 0.12f, 0.45f, 0.9f);

        if (distance > killRadius)
            _deathRequested = false;

        if (distance >= options.CrimsonRadius)
        {
            if (_physics.AmOwner && _player.CanMove && !_player.inVent)
            {
                _lastSafePosition = _player.GetTruePosition();
                _lastSafePositionReady = true;
            }

            _escaping = false;
            _escapedThisEntry = false;

            if (_collisionDisabled)
            {
                _player.Collider.enabled = true;
                _collisionDisabled = false;
            }
        }

        if (AmongUsClient.Instance.AmHost && !_deathRequested && !_escaping && !_player.inVent && distance <= killRadius)
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
        var intensity = options.CrimsonIntensity switch
        {
            GEOptions.VortexIntensity.Gentle => 0.75f,
            GEOptions.VortexIntensity.Standard => 1f,
            GEOptions.VortexIntensity.Brutal => 1.3f,
            _ => 1f
        };
        var pull = Mathf.Lerp(0.55f, 4.25f, pullProgress) * intensity;
        var orbit = 2.25f * intensity * (1f - pullProgress);

        var currentDirection = Vector2.zero;

        if (!_player.isDummy)
        {
            currentDirection = _physics.GetVelocity() / Mathf.Max(_physics.TrueSpeed, 0.01f);
        }

        var direction = currentDirection + inward * pull + tangent * orbit;

        _writingVelocity = true;

        _physics.SetNormalizedVelocity(Vector2.ClampMagnitude(direction, 2.1f * intensity));

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
        var killRadius = Mathf.Clamp(options.CrimsonRadius * 0.12f, 0.45f, 0.9f);

        return distance < options.CrimsonRadius && distance > killRadius;
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

        var escapePosition = _lastSafePositionReady ? _lastSafePosition : ShipStatus.Instance.InitialSpawnCenter;
        _player.NetTransform.RpcSnapTo(escapePosition);
        if (SeasonManager.AvailableAchievementTabTypes.Contains(typeof(PreseasonAchievementsTab)))
        {
            PreseasonAchievementsTab.EventHorizonDenied.Unlock();
        }

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
        var killRadius = Mathf.Clamp(options.CrimsonRadius * 0.12f, 0.45f, 0.9f);

        return !_escapedThisEntry && distance <= options.CrimsonRadius * 0.75f && distance > killRadius;
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
