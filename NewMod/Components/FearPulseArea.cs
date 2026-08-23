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

        private float _radius;
        private float _duration;
        private float _speedMultiplier;
        private float _elapsed;
        private bool _restored;

        private readonly Dictionary<byte, float> _originalSpeeds = new();
        private readonly HashSet<byte> _insideNow = new();

        public static readonly HashSet<byte> AffectedPlayers = new();
        public static readonly HashSet<byte> _speedNotifShown = new();
        public static readonly HashSet<byte> _visionNotifShown = new();

        private AudioClip _enterClip;
        private AudioClip _heartbeatClip;

        public void Init(byte ownerId, float radius, float duration, float speedMul)
        {
            this.ownerId = ownerId;
            _radius = radius;
            _duration = duration;
            _speedMultiplier = Mathf.Max(0f, 1f - speedMul / 100f);
            _enterClip = NewModAsset.FearSound.LoadAsset();
            _heartbeatClip = NewModAsset.HeartbeatSound.LoadAsset();
        }

        public void Update()
        {
            if (_restored)
                return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                RestoreAll();
                Destroy(gameObject);
                return;
            }

            var localPlayer = PlayerControl.LocalPlayer;
            if (!localPlayer || localPlayer.Data == null || localPlayer.PlayerId == ownerId)
                return;

            _insideNow.Clear();

            var inside = !localPlayer.Data.IsDead && !localPlayer.Data.Disconnected && Vector2.Distance(localPlayer.GetTruePosition(), transform.position) <= _radius;

            if (inside)
            {
                _insideNow.Add(localPlayer.PlayerId);

                if (!_originalSpeeds.ContainsKey(localPlayer.PlayerId))
                {
                    _originalSpeeds[localPlayer.PlayerId] = localPlayer.MyPhysics.Speed;
                    localPlayer.MyPhysics.Speed *= _speedMultiplier;
                    AffectedPlayers.Add(localPlayer.PlayerId);

                    if (_speedNotifShown.Add(localPlayer.PlayerId))
                    {
                        var notification = Helpers.CreateAndShowNotification("You have entered the Fear Pulse Area. Your speed is reduced!", Color.red, spr: NewModAsset.SpeedDebuff.LoadAsset());
                        notification.Text.SetOutlineThickness(0.36f);
                    }

                    if (_visionNotifShown.Add(localPlayer.PlayerId))
                    {
                        var notification = Helpers.CreateAndShowNotification("You have entered the Fear Pulse Area. Your vision is reduced!", new Color(1f, 0.8f, 0.2f), spr: NewModAsset.VisionDebuff.LoadAsset());
                        notification.Text.SetOutlineThickness(0.36f);
                    }

                    if (localPlayer.lightSource && localPlayer.lightSource.lightChild)
                        localPlayer.lightSource.lightChild.SetActive(false);

                    if (Constants.ShouldPlaySfx())
                        SoundManager.Instance.PlaySound(_enterClip, false, 1f);

                    var camera = Camera.main.GetComponent<FollowerCamera>();
                    if (camera)
                        Coroutines.Start(Utils.CoShakeCamera(camera, 0.5f));
                }

                if (localPlayer.MyPhysics.Velocity.sqrMagnitude > 0.0001f && Constants.ShouldPlaySfx() && !SoundManager.Instance.SoundIsPlaying(_heartbeatClip))
                {
                    SoundManager.Instance.PlaySound(_heartbeatClip, false, 1f);
                }
            }

            foreach (var playerId in _originalSpeeds.Keys.Where(id => !_insideNow.Contains(id)).ToArray())
                RestorePlayer(playerId);
        }

        public void RestorePlayer(byte playerId)
        {
            if (_originalSpeeds.Remove(playerId, out var originalSpeed))
            {
                var player = Utils.PlayerById(playerId);
                if (player && player.MyPhysics)
                {
                    player.MyPhysics.Speed = originalSpeed;

                    if (player.AmOwner)
                    {
                        if (player.lightSource && player.lightSource.lightChild)
                            player.lightSource.lightChild.SetActive(true);

                        SoundManager.Instance.StopSound(_enterClip);
                        SoundManager.Instance.StopSound(_heartbeatClip);

                        Helpers.CreateAndShowNotification("Your speed and vision are restored.", new Color(0.8f, 1f, 0.8f));
                    }
                }
            }

            AffectedPlayers.Remove(playerId);
            _speedNotifShown.Remove(playerId);
            _visionNotifShown.Remove(playerId);
        }

        public void RestoreAll()
        {
            if (_restored)
                return;

            _restored = true;

            foreach (var playerId in _originalSpeeds.Keys.ToArray())
                RestorePlayer(playerId);

            _insideNow.Clear();
        }

        public void OnDestroy()
        {
            RestoreAll();
        }
    }
}