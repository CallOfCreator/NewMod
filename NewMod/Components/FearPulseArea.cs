using System;
using System.Collections.Generic;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class FearPulseArea(IntPtr ptr) : MonoBehaviour(ptr)
{
    public static readonly HashSet<byte> AffectedPlayers = [];

    private static readonly Dictionary<byte, int> ActivePulseCounts = [];
    private static readonly Dictionary<byte, float> OriginalSpeeds = [];

    public byte ownerId;

    private AudioClip _enterClip;
    private AudioClip _heartbeatClip;
    private byte _affectedPlayerId = byte.MaxValue;
    private float _duration;
    private float _elapsed;
    private float _radius;
    private bool _affectingLocalPlayer;
    private bool _restored;
    private float _speedMultiplier;

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

        var inside = !localPlayer.Data.IsDead && !localPlayer.Data.Disconnected && Vector2.Distance(localPlayer.GetTruePosition(), transform.position) <= _radius;

        if (inside && !_affectingLocalPlayer)
        {
            _affectingLocalPlayer = true;
            _affectedPlayerId = localPlayer.PlayerId;

            ActivePulseCounts.TryGetValue(localPlayer.PlayerId, out var activePulses);
            ActivePulseCounts[localPlayer.PlayerId] = activePulses + 1;

            if (activePulses == 0)
            {
                OriginalSpeeds[localPlayer.PlayerId] = localPlayer.MyPhysics.Speed;
                localPlayer.MyPhysics.Speed *= _speedMultiplier;
                AffectedPlayers.Add(localPlayer.PlayerId);

                var speedNotification = Helpers.CreateAndShowNotification("You have entered the Fear Pulse Area. Your speed is reduced!", Color.red, spr: NewModAsset.SpeedDebuff.LoadAsset());
                speedNotification.Text.SetOutlineThickness(0.36f);

                var visionNotification = Helpers.CreateAndShowNotification("You have entered the Fear Pulse Area. Your vision is reduced!", new Color(1f, 0.8f, 0.2f), spr: NewModAsset.VisionDebuff.LoadAsset());
                visionNotification.Text.SetOutlineThickness(0.36f);

                if (localPlayer.lightSource && localPlayer.lightSource.lightChild)
                    localPlayer.lightSource.lightChild.SetActive(false);

                if (Constants.ShouldPlaySfx())
                    SoundManager.Instance.PlaySound(_enterClip, false);

                var camera = Camera.main.GetComponent<FollowerCamera>();

                if (camera)
                    Coroutines.Start(Utils.CoShakeCamera(camera, 0.5f));
            }
        }
        else if (!inside && _affectingLocalPlayer)
        {
            RestorePlayer(_affectedPlayerId);
        }

        if (inside && localPlayer.MyPhysics.Velocity.sqrMagnitude > 0.0001f && Constants.ShouldPlaySfx() && !SoundManager.Instance.SoundIsPlaying(_heartbeatClip))
            SoundManager.Instance.PlaySound(_heartbeatClip, false);
    }

    public void OnDestroy()
    {
        RestoreAll();
    }

    public void Init(byte ownerId, float radius, float duration, float speedMul)
    {
        this.ownerId = ownerId;
        _radius = radius;
        _duration = duration;
        _speedMultiplier = Mathf.Max(0f, 1f - speedMul / 100f);
        _enterClip = NewModAsset.FearSound.LoadAsset();
        _heartbeatClip = NewModAsset.HeartbeatSound.LoadAsset();
    }

    public void RestorePlayer(byte playerId)
    {
        if (!_affectingLocalPlayer || playerId != _affectedPlayerId)
            return;

        _affectingLocalPlayer = false;
        _affectedPlayerId = byte.MaxValue;

        if (!ActivePulseCounts.TryGetValue(playerId, out var activePulses))
            return;

        if (activePulses > 1)
        {
            ActivePulseCounts[playerId] = activePulses - 1;
            return;
        }

        ActivePulseCounts.Remove(playerId);
        AffectedPlayers.Remove(playerId);

        if (!OriginalSpeeds.Remove(playerId, out var originalSpeed))
            return;

        var player = Utils.PlayerById(playerId);

        if (!player || !player.MyPhysics)
            return;

        player.MyPhysics.Speed = originalSpeed;

        if (!player.AmOwner)
            return;

        if (player.lightSource && player.lightSource.lightChild)
            player.lightSource.lightChild.SetActive(true);

        SoundManager.Instance.StopSound(_enterClip);
        SoundManager.Instance.StopSound(_heartbeatClip);
        Helpers.CreateAndShowNotification("Your speed and vision are restored.", new Color(0.8f, 1f, 0.8f));
    }

    public void RestoreAll()
    {
        if (_restored)
            return;

        _restored = true;

        if (_affectingLocalPlayer)
            RestorePlayer(_affectedPlayerId);
    }

    public static void ResetState()
    {
        var localPlayer = PlayerControl.LocalPlayer;

        if (localPlayer && OriginalSpeeds.Remove(localPlayer.PlayerId, out var originalSpeed))
        {
            localPlayer.MyPhysics.Speed = originalSpeed;

            if (localPlayer.lightSource && localPlayer.lightSource.lightChild)
                localPlayer.lightSource.lightChild.SetActive(true);
        }

        ActivePulseCounts.Clear();
        OriginalSpeeds.Clear();
        AffectedPlayers.Clear();
    }
}