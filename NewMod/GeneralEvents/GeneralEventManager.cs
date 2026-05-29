using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.GeneralEvents
{
    public static class GeneralEventManager
    {
        private static uint _nextTypeId = 0;
        private static readonly Dictionary<uint, Type> TypeIdMap = new();
        private static readonly Dictionary<Type, uint> TypeToIdMap = new();
        private static readonly List<IGeneralEvent> _registered = new();

        public static IGeneralEvent CurrentEvent { get; private set; }
        private static GeneralEventHud _hud;
        public static IEnumerator _cycleRoutine;

        private const float MinInterval = 20f;
        private const float MaxInterval = 30f;

        public static uint GenerateNextTypeId()
        {
            _nextTypeId++;
            return _nextTypeId;
        }

        public static void RegisterEvent<T>() where T : IGeneralEvent, new()
        {
            RegisterEvent(typeof(T));
        }

        public static void RegisterEvent(Type type)
        {
            if (TypeToIdMap.ContainsKey(type))
                return;

            var id = GenerateNextTypeId();
            TypeToIdMap[type] = id;
            TypeIdMap[id] = type;

            var instance = (IGeneralEvent)Activator.CreateInstance(type);
            _registered.Add(instance);

            NewMod.Instance.Log.LogMessage($"[GE] Registered '{instance.Title}' (id={id}, chance={instance.OccurrenceChance}%)");
        }

        public static void StartCycle()
        {
            if (!AmongUsClient.Instance.AmHost)
                return;

            if (_cycleRoutine != null)
                return;

            _cycleRoutine = Coroutines.Start(CoCycle());
            NewMod.Instance.Log.LogMessage("[GE] Event cycle started.");
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
            
            CurrentEvent?.OnEventEnd();
            CurrentEvent = null;

            if (_hud)
                UnityEngine.Object.Destroy(_hud.gameObject);

            _hud = null;
        }

        public static IEnumerator CoCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(MinInterval, MaxInterval));

                if (MeetingHud.Instance || ExileController.Instance || CurrentEvent != null)
                    continue;

                var candidate = PickEvent();

                if (candidate == null)
                    continue;

                RpcStartGeneralEvent(PlayerControl.LocalPlayer, TypeToIdMap[candidate.GetType()]);
            }
        }

        public static IGeneralEvent PickEvent()
        {
            var eligible = _registered
                .Where(e => e.CanOccur() && Helpers.CheckChance(e.OccurrenceChance))
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            return eligible.Count > 0 ? eligible[0] : null;
        }

        [MethodRpc((uint)CustomRPC.StartGeneralEvent)]
        public static void RpcStartGeneralEvent(PlayerControl source, uint eventTypeId)
        {
            if (!TypeIdMap.TryGetValue(eventTypeId, out var type))
            {
                NewMod.Instance.Log.LogWarning($"[GE] Unknown event type id: {eventTypeId}");
                return;
            }

            StartEvent((IGeneralEvent)Activator.CreateInstance(type));
        }

        [MethodRpc((uint)CustomRPC.EndGeneralEvent)]
        public static void RpcEndGeneralEvent(PlayerControl source)
        {
            EndEvent();
        }

        public static void StartEvent(IGeneralEvent ge)
        {
            if (CurrentEvent != null)
            {
                NewMod.Instance.Log.LogMessage("[GE] Start blocked because another GE is already active.");
                return;
            }

            CurrentEvent = ge;

            NewMod.Instance.Log.LogMessage($"[GE] Starting '{ge.Title}' ({ge.Duration}s)");

            ge.OnEventStart();

            _hud = GeneralEventHud.Create();
            _hud.Show(ge);

            Coroutines.Start(CoEventTimer(ge));
        }

        public static void EndEvent()
        {
            if (CurrentEvent == null)
                return;

            NewMod.Instance.Log.LogMessage($"[GE] Ending '{CurrentEvent.Title}'");

            CurrentEvent.OnEventEnd();
            CurrentEvent = null;

            if (_hud)
                _hud.Hide();

            _hud = null;
        }

        public static IEnumerator CoEventTimer(IGeneralEvent ge)
        {
            yield return new WaitForSeconds(ge.Duration);

            if (CurrentEvent != ge)
                yield break;

            if (AmongUsClient.Instance.AmHost)
                RpcEndGeneralEvent(PlayerControl.LocalPlayer);
        }
    }
}