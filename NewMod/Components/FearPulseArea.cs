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

    public static readonly Dictionary<byte, int> ActivePulseCounts = [];
    public static readonly Dictionary<byte, float> OriginalSpeeds = [];

    public AreaBubble bubble;
    public byte ownerId;

    public AudioClip _enterClip;
    public AudioClip _heartbeatClip;
    public byte _affectedPlayerId = byte.MaxValue;
    public float _duration;
    public float _elapsed;
    public float _radius;
    public bool _affectingLocalPlayer;
    public bool _restored;
    public float _speedMultiplier;

    public void Update()
    {
        if (_restored)
            return;

        _elapsed += Time.deltaTime;

        if (_elapsed >= _duration || MeetingHud.Instance || ExileController.Instance)
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

                var speedNotification = Helpers.CreateAndShowNotification("Intimidated: leave the area to restore your speed.", Color.red, spr: NewModAsset.SpeedDebuff.LoadAsset());
                speedNotification.Text.SetOutlineThickness(0.36f);

                if (Constants.ShouldPlaySfx())
                    SoundManager.Instance.PlaySound(_enterClip, false);
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
        if (bubble)
            bubble.Break();
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

        SoundManager.Instance.StopSound(_enterClip);
        SoundManager.Instance.StopSound(_heartbeatClip);
        Helpers.CreateAndShowNotification("Your speed is restored.", new Color(0.8f, 1f, 0.8f));
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

        if (localPlayer && OriginalSpeeds.Remove(localPlayer.PlayerId, out var originalSpeed)) localPlayer.MyPhysics.Speed = originalSpeed;

        ActivePulseCounts.Clear();
        OriginalSpeeds.Clear();
        AffectedPlayers.Clear();
    }
}