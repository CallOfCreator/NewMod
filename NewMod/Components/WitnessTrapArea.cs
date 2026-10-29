using System;
using System.Collections;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class WitnessTrapArea(IntPtr ptr) : MonoBehaviour(ptr)
{
    public float _duration;
    public byte _ownerId;
    public float _radius;
    public float _freezeSeconds;
    public bool _consumed;
    public AudioClip _enterClip;
    public AudioClip _heartbeatClip;

    public void Update()
    {
        if (_consumed)
        {
            _duration -= Time.deltaTime;
            if (_duration <= 0f) Destroy(gameObject);
            return;
        }

        var center = (Vector2)transform.position;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player || player.PlayerId == _ownerId || player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;

            if (Vector2.Distance(player.GetTruePosition(), center) <= _radius)
            {
                _consumed = true;
                _duration = _freezeSeconds + 0.1f;

                if (player.AmOwner)
                {
                    if (Constants.ShouldPlaySfx() && _enterClip && !SoundManager.Instance.SoundIsPlaying(_enterClip))
                        SoundManager.Instance.PlaySound(_enterClip, false);

                    if (player.MyPhysics.Velocity.sqrMagnitude > 0.0001f && !SoundManager.Instance.SoundIsPlaying(_heartbeatClip))
                        SoundManager.Instance.PlaySound(_heartbeatClip, false);

                    Coroutines.Start(Freeze(player, _freezeSeconds));
                }

                break;
            }
        }

        _duration -= Time.deltaTime;
        if (_duration <= 0f) Destroy(gameObject);
    }

    public void Init(byte ownerId, float radius, float freeze, float duration)
    {
        _ownerId = ownerId;
        _radius = radius;
        _freezeSeconds = freeze;
        _duration = duration;
        _enterClip = NewModAsset.FearSound.LoadAsset();
        _heartbeatClip = NewModAsset.HeartbeatSound.LoadAsset();
    }

    public static IEnumerator Freeze(PlayerControl p, float seconds)
    {
        p.MyPhysics.inputHandler.enabled = false;
        p.moveable = false;

        if (p.AmOwner)
        {
            Coroutines.Start(Utils.CoShakeCamera(Camera.main.GetComponent<FollowerCamera>(), 0.5f));
            var notification = Helpers.CreateAndShowNotification("You have entered the Intimation Protocol Area. You are frozen!", Color.cyan, spr: NewModAsset.Freeze.LoadAsset());
            notification.Text.SetOutlineThickness(0.36f);
        }

        var elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (p)
        {
            p.moveable = true;
            p.MyPhysics.inputHandler.enabled = true;
        }
    }
}
