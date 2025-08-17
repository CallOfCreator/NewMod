using System;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public sealed class SuppressionDomeArea(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public byte _ownerId;
        public float _radius, _duration, _t;
        public bool _inside;
        public AudioClip _enterClip;
        public AudioClip _heartbeatClip;

        public void Init(byte ownerId, float radius, float duration)
        {
            _ownerId = ownerId;
            _radius = radius;
            _duration = duration;
            _inside = false;
            _enterClip = NewModAsset.FearSound.LoadAsset();
            _heartbeatClip = NewModAsset.HeartbeatSound.LoadAsset();
        }

        public void Update()
        {
            _t += Time.deltaTime;
            if (_t > _duration)
            {
                Destroy(gameObject);
                return;
            }
            var lp = PlayerControl.LocalPlayer;
            if (!lp || lp.Data == null) return;
            if (lp.PlayerId == _ownerId) return;

            bool nowInside = Vector2.Distance(lp.GetTruePosition(), (Vector2)transform.position) <= _radius;
            if (nowInside == _inside) return;

            _inside = nowInside;
            var hud = HudManager.Instance;
            hud.SetHudActive(lp, lp.Data.Role, !_inside);

            if (_inside)
            {
                Coroutines.Start(Utils.CoShakeCamera(Camera.main.GetComponent<FollowerCamera>(), 0.5f));

                if (!lp.Data.IsDead && Constants.ShouldPlaySfx() && !SoundManager.Instance.SoundIsPlaying(_enterClip))
                    SoundManager.Instance.PlaySound(_enterClip, false, 1f);

                var notif = Helpers.CreateAndShowNotification(
                    "You’ve stepped into the Suppression Dome! All your abilities are LOCKED",
                    Color.red
                );
                notif.Text.SetOutlineThickness(0.36f);

                if (lp.MyPhysics.Velocity.sqrMagnitude > 0.0001f && !SoundManager.Instance.SoundIsPlaying(_heartbeatClip))
                    SoundManager.Instance.PlaySound(_heartbeatClip, false, 1f);
            }
        }
        public void OnDestroy()
        {
            if (!_inside) return; 
            var lp = PlayerControl.LocalPlayer;
            if (!lp || lp.Data == null) return;
            HudManager.Instance.SetHudActive(lp, lp.Data.Role, true);
            _inside = false;
        }
    }
}
