using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class FearPulseArea(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public byte ownerId;
        float _radius, _duration, _speedMul, _t;
        readonly Dictionary<byte, float> _origSpeed = new();
        readonly HashSet<byte> _insideNow = new();
        public static readonly HashSet<byte> AffectedPlayers = new();
        public static readonly HashSet<byte> _speedNotifShown = new();
        public static readonly HashSet<byte> _visionNotifShown = new();
        public AudioClip _enterClip;
        public AudioClip _heartbeatClip;
        public bool _pulsingHb;

        public void Init(byte ownerId, float radius, float duration, float speedMul)
        {
            this.ownerId = ownerId;
            _radius = radius;
            _duration = duration;
            _speedMul = Mathf.Max(0f, 1f - (speedMul / 100f));
            _enterClip = NewModAsset.FearSound.LoadAsset();
            _heartbeatClip = NewModAsset.HeartbeatSound.LoadAsset();
        }

        public void Update()
        {
            _t += Time.deltaTime;
            if (_t > _duration)
            {
                RestoreAll();
                Destroy(gameObject);
                return;
            }

            var center = (Vector2)transform.position;

            var nearby = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => !p.Data.IsDead && !p.Data.Disconnected && p.PlayerId != ownerId)
                .Where(p => Vector2.Distance(p.GetTruePosition(), center) <= _radius)
                .ToList();

            _insideNow.Clear();

            foreach (var p in nearby)
            {
                _insideNow.Add(p.PlayerId);

                if (!_origSpeed.ContainsKey(p.PlayerId))
                {
                    _origSpeed[p.PlayerId] = p.MyPhysics.Speed;
                    p.MyPhysics.Speed = _origSpeed[p.PlayerId] * _speedMul;

                    if (p.AmOwner && !_speedNotifShown.Contains(p.PlayerId))
                    {
                        _speedNotifShown.Add(p.PlayerId);
                        var notif = Helpers.CreateAndShowNotification(
                            "You have entered the Fear Pulse Area. Your speed is reduced!",
                            Color.red,
                            spr: NewModAsset.SpeedDebuff.LoadAsset()
                        );
                        notif.Text.SetOutlineThickness(0.36f);
                    }
                }

                if (p.AmOwner)
                {
                    if (!AffectedPlayers.Contains(p.PlayerId))
                    {
                        AffectedPlayers.Add(p.PlayerId);
                        p.lightSource.lightChild.SetActive(false);
                    }

                    if (!p.Data.IsDead && Constants.ShouldPlaySfx() && !SoundManager.Instance.SoundIsPlaying(_enterClip))
                        SoundManager.Instance.PlaySound(_enterClip, false, 1f);

                    if (!_visionNotifShown.Contains(p.PlayerId))
                    {
                        _visionNotifShown.Add(p.PlayerId);
                        var notif = Helpers.CreateAndShowNotification(
                             "You have entered the Fear Pulse Area. Your vision is reduced!",
                             new Color(1f, 0.8f, 0.2f),
                             spr: NewModAsset.VisionDebuff.LoadAsset()
                         );
                        notif.Text.SetOutlineThickness(0.36f);
                    }
                    Coroutines.Start(Utils.CoShakeCamera(Camera.main.GetComponent<FollowerCamera>(), 0.5f));
                }

                if (p.MyPhysics.Velocity.sqrMagnitude > 0.0001f)
                {
                    _pulsingHb = true;
                    if (!p.Data.IsDead && Constants.ShouldPlaySfx() && !SoundManager.Instance.SoundIsPlaying(_heartbeatClip))
                        SoundManager.Instance.PlaySound(_heartbeatClip, false, 1f);
                    _pulsingHb = false;
                }
            }

            if (_origSpeed.Count > 0)
            {
                var toRestore = _origSpeed.Keys.Where(id => !_insideNow.Contains(id)).ToList();
                foreach (var id in toRestore)
                {
                    var p = Utils.PlayerById(id);
                    if (p) p.MyPhysics.Speed = _origSpeed[id];
                    _origSpeed.Remove(id);

                    if (p.AmOwner)
                    {
                        AffectedPlayers.Remove(p.PlayerId);
                        p.lightSource.lightChild.SetActive(true);
                        _speedNotifShown.Remove(p.PlayerId);
                        _visionNotifShown.Remove(p.PlayerId);
                        Helpers.CreateAndShowNotification("Your vision is restored.", new Color(0.8f, 1f, 0.8f));
                    }
                }
            }
        }
        public void RestoreAll()
        {
            foreach (var kv in _origSpeed)
            {
                var p = Utils.PlayerById(kv.Key);
                if (p) p.MyPhysics.Speed = kv.Value;
            }
            _origSpeed.Clear();
            AffectedPlayers.Clear();

            var lp = PlayerControl.LocalPlayer;

            if (AffectedPlayers.Contains(lp.PlayerId))
                AffectedPlayers.Remove(lp.PlayerId);

            lp.lightSource.lightChild.SetActive(true);

            _speedNotifShown.Remove(lp.PlayerId);
            _visionNotifShown.Remove(lp.PlayerId);
        }
    }
}
