using System;
using System.Collections;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class WitnessTrapArea(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public byte _ownerId;
        public float _radius;
        public float _freezeSeconds;
        public static float _duration;
        public bool _consumed;
        public AudioClip _enterClip;
        public AudioClip _heartbeatClip;
        public void Init(byte ownerId, float radius, float freeze, float duration)
        {
            _ownerId = ownerId;
            _radius = radius;
            _freezeSeconds = freeze;
            _duration = duration;
            _enterClip = NewModAsset.FearSound.LoadAsset();
            _heartbeatClip = NewModAsset.HeartbeatSound.LoadAsset();
        }

        public void Update()
        {
            if (_consumed)
            {
                _duration -= Time.deltaTime;
                if (_duration <= 0f) Destroy(gameObject);
                return;
            }

            var center = (Vector2)transform.position;

            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (!p || p.PlayerId == _ownerId || p.Data == null || p.Data.IsDead || p.Data.Disconnected) continue;

                if (Vector2.Distance(p.GetTruePosition(), center) <= _radius)
                {
                    _consumed = true;
                    _duration = _freezeSeconds + 0.1f;

                    if (p.AmOwner)
                    {
                        if (Constants.ShouldPlaySfx() && _enterClip && !SoundManager.Instance.SoundIsPlaying(_enterClip))
                            SoundManager.Instance.PlaySound(_enterClip, false, 1f);

                        if (p.MyPhysics.Velocity.sqrMagnitude > 0.0001f && !SoundManager.Instance.SoundIsPlaying(_heartbeatClip))
                            SoundManager.Instance.PlaySound(_heartbeatClip, false, 1f);

                        Coroutines.Start(Freeze(p, _freezeSeconds));
                    }
                    break;
                }
            }

            _duration -= Time.deltaTime;
            if (_duration <= 0f) Destroy(gameObject);
        }

        public static IEnumerator Freeze(PlayerControl p, float seconds)
        {
            p.MyPhysics.inputHandler.enabled = false;
            p.moveable = false;

            if (p.AmOwner)
            {
                Coroutines.Start(Utils.CoShakeCamera(Camera.main.GetComponent<FollowerCamera>(), 0.5f));
                var notif = Helpers.CreateAndShowNotification("You have entered the Intimation Protocol Area. You are frozen!", Color.cyan, spr: NewModAsset.Freeze.LoadAsset());
                notif.Text.SetOutlineThickness(0.36f);
            }

            var t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (p)
            {
                p.moveable = true;
                p.MyPhysics.inputHandler.enabled = true;
            }
        }
    }
}
