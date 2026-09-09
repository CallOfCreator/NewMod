using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using NewMod.Options;
using NewMod.Roles.NeutralRoles.S1;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NewMod.GeneralEvents;

public static class GeneralEventManager
{
    private static uint _nextTypeId;
    private static uint _issuedSequence;
    private static uint _lastSequence;
    private static uint _activeSequence;

    private static readonly Dictionary<uint, Type> TypeIdMap = [];
    private static readonly Dictionary<Type, uint> TypeToIdMap = [];
    private static readonly List<IGeneralEvent> Registered = [];

    private static GeneralEventHud _hud;
    private static IEnumerator _cycleRoutine;
    private static IEnumerator _eventRoutine;

    public static IGeneralEvent CurrentEvent { get; private set; }
    public static bool CycleRunning => _cycleRoutine != null;
    public static int RegisteredCount => Registered.Count;
    public static IReadOnlyList<IGeneralEvent> RegisteredEvents => Registered;

    public static void RegisterEvent<T>() where T : IGeneralEvent, new()
    {
        RegisterEvent(typeof(T));
    }

    public static void RegisterEvent(Type type)
    {
        if (TypeToIdMap.ContainsKey(type))
            return;

        var id = ++_nextTypeId;

        TypeToIdMap[type] = id;
        TypeIdMap[id] = type;
        Registered.Add((IGeneralEvent)Activator.CreateInstance(type));
    }

    public static void StartCycle()
    {
        if (!AmongUsClient.Instance.AmHost || _cycleRoutine != null || !OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents || GameManager.Instance.IsHideAndSeek() || (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.IsClassic() == false))
            return;

        _cycleRoutine = Coroutines.Start(CoCycle());
    }

    public static void StopCycle()
    {
        if (_cycleRoutine == null)
            return;

        Coroutines.Stop(_cycleRoutine);
        _cycleRoutine = null;
    }

    public static void Reset()
    {
        StopCycle();

        if (_eventRoutine != null)
        {
            Coroutines.Stop(_eventRoutine);
            _eventRoutine = null;
        }

        if (CurrentEvent != null)
        {
            CurrentEvent.OnEventEnd();
            CurrentEvent = null;
        }

        if (_hud)
            Object.Destroy(_hud.gameObject);

        _hud = null;

        _issuedSequence = 0;
        _lastSequence = 0;
        _activeSequence = 0;
    }

    public static void ForceEvent<T>() where T : IGeneralEvent
    {
        ForceEvent(typeof(T));
    }

    public static void ForceEvent(Type type)
    {
        if (!AmongUsClient.Instance.AmHost || !OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents || GameManager.Instance.IsHideAndSeek() || (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.IsClassic() == false))
            return;

        if (!TypeToIdMap.TryGetValue(type, out var id))
            return;

        _issuedSequence++;

        RpcStartGeneralEvent(PlayerControl.LocalPlayer, id, _issuedSequence);
    }

    public static void ForceEnd()
    {
        if (!AmongUsClient.Instance.AmHost || CurrentEvent == null)
            return;

        RpcEndGeneralEvent(PlayerControl.LocalPlayer, _activeSequence);
    }

    public static IEnumerator CoCycle()
    {
        while (GameManager.Instance && !GameManager.Instance.GameHasStarted)
            yield return null;

        while (GameManager.Instance && GameManager.Instance.GameHasStarted)
        {
            var options = OptionGroupSingleton<GEOptions>.Instance;

            if (!options.EnableGeneralEvents || GameManager.Instance.IsHideAndSeek() || (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.IsClassic() == false))
            {
                _cycleRoutine = null;
                yield break;
            }

            var minimum = options.MinimumInterval.Value;
            var maximum = Mathf.Max(minimum, options.MaximumInterval.Value);

            yield return new WaitForSeconds(Random.Range(minimum, maximum));

            if (!GameManager.Instance || !GameManager.Instance.GameHasStarted)
                break;

            if (!options.EnableGeneralEvents || GameManager.Instance.IsHideAndSeek() || (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.IsClassic() == false))
            {
                _cycleRoutine = null;
                yield break;
            }

            if (MeetingHud.Instance || ExileController.Instance || CurrentEvent != null) continue;

            if (Random.Range(0f, 100f) >= options.EventTriggerChance.Value)
                continue;

            var candidate = PickEvent();

            if (candidate == null)
                continue;

            _issuedSequence++;

            RpcStartGeneralEvent(PlayerControl.LocalPlayer, TypeToIdMap[candidate.GetType()], _issuedSequence);
        }

        _cycleRoutine = null;
    }

    public static IGeneralEvent PickEvent()
    {
        var eligible = Registered.Where(ge => ge.OccurrenceChance > 0 && ge.CanOccur()).ToArray();

        if (eligible.Length == 0)
            return null;

        var totalWeight = eligible.Sum(ge => ge.OccurrenceChance);
        var roll = Random.Range(0, totalWeight);

        foreach (var ge in eligible)
        {
            roll -= ge.OccurrenceChance;
            if (roll < 0)
                return ge;
        }

        return eligible[^1];
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        if (_hud)
            _hud.gameObject.SetActive(false);
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        if (_hud && CurrentEvent != null)
            _hud.gameObject.SetActive(true);
    }

    [MethodRpc((uint)CustomRPC.StartGeneralEvent, LocalHandling = RpcLocalHandling.After)]
    public static void RpcStartGeneralEvent(PlayerControl source, uint eventTypeId, uint sequence)
    {
        if (!source.IsHost() || !OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents || GameManager.Instance.IsHideAndSeek() || (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.IsClassic() == false))
            return;

        if (sequence <= _lastSequence)
            return;

        if (!TypeIdMap.TryGetValue(eventTypeId, out var type))
            return;

        _lastSequence = sequence;

        if (CurrentEvent != null)
            EndEvent(_activeSequence);

        StartEvent((IGeneralEvent)Activator.CreateInstance(type), sequence);
    }

    [MethodRpc((uint)CustomRPC.EndGeneralEvent, LocalHandling = RpcLocalHandling.After)]
    public static void RpcEndGeneralEvent(PlayerControl source, uint sequence)
    {
        if (!source.IsHost())
            return;

        EndEvent(sequence);
    }

    public static void StartEvent(IGeneralEvent ge, uint sequence)
    {
        CurrentEvent = ge;
        _activeSequence = sequence;

        ge.OnEventStart();

        _hud = GeneralEventHud.Create();
        _hud.Show(ge);

        if (MeetingHud.Instance || ExileController.Instance)
            _hud.gameObject.SetActive(false);

        SoundManager.Instance.PlaySound(NewModAsset.GEEnterSound.LoadAsset(), false);

        if (!AmongUsClient.Instance.AmHost)
            return;

        if (_eventRoutine != null)
            Coroutines.Stop(_eventRoutine);

        _eventRoutine = Coroutines.Start(CoEventTimer(sequence, ge.Duration));
    }

    public static void EndEvent(uint sequence)
    {
        if (CurrentEvent == null || sequence != _activeSequence) return;

        if (_eventRoutine != null)
        {
            Coroutines.Stop(_eventRoutine);
            _eventRoutine = null;
        }

        var ge = CurrentEvent;

        CurrentEvent = null;
        _activeSequence = 0;

        ge.OnEventEnd();
        if (_hud)
            _hud.Hide();

        _hud = null;

        if (GameManager.Instance && GameManager.Instance.GameHasStarted)
            SoundManager.Instance.PlaySound(NewModAsset.GEExitSound.LoadAsset(), false);
    }

    public static IEnumerator CoEventTimer(uint sequence, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (CurrentEvent == null || _activeSequence != sequence || !AmongUsClient.Instance.AmHost) yield break;

        _eventRoutine = null;

        RpcEndGeneralEvent(PlayerControl.LocalPlayer, sequence);
    }
}