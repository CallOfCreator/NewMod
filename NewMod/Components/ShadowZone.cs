using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using NewMod.Components.ScreenEffects;
using NewMod.Options.Roles.ShadeOptions;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class ShadowZone(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public byte shadeId;
        public float radius = 2.5f;
        public float duration = 20f;
        public float _t;
        public bool _on;
        public static List<ShadowZone> zones = new();

        public void Awake()
        {
            if (!zones.Contains(this)) zones.Add(this);
        }

        public void OnDestroy()
        {
            zones.Remove(this);
        }

        public bool Contains(Vector2 pos)
        {
            return Vector2.Distance(pos, (Vector2)transform.position) <= radius;
        }

        public void Update()
        {
            _t += Time.deltaTime;
            if (_t >= duration)
            {
                Destroy(gameObject);
                return;
            }

            var lp = PlayerControl.LocalPlayer;
            if (!lp || lp.PlayerId == shadeId) return;

            bool inside = Contains(lp.GetTruePosition());
            var cam = Camera.main;
            var fx = cam.GetComponent<ShadowCrawlEffect>();

            var mode = OptionGroupSingleton<ShadeOptions>.Instance.Behavior;

            if (inside && !_on)
            {
                if (cam && fx == null) cam.gameObject.AddComponent<ShadowCrawlEffect>();

                if (mode is ShadeOptions.ShadowMode.Invisible or ShadeOptions.ShadowMode.Both)
                {
                    lp.SetInvisibility(true);
                }

                if (mode is ShadeOptions.ShadowMode.KillEnabled or ShadeOptions.ShadowMode.Both)
                {
                    lp.Data.Role.CanUseKillButton = true;
                }

                _on = true;
            }
            else if (!inside && _on)
            {
                if (fx) Destroy(fx);

                lp.SetInvisibility(false);
                lp.Data.Role.CanUseKillButton = false;

                _on = false;
            }
        }

        public static ShadowZone Create(byte id, Vector2 pos, float r, float dur)
        {
            var go = new GameObject("ShadowZone");
            var z = go.AddComponent<ShadowZone>();
            z.shadeId = id;
            z.radius = r;
            z.duration = dur;
            go.transform.position = pos;
            return z;
        }
        public static bool IsInsideAny(Vector2 pos)
        {
            return zones.Any(a => a && a.Contains(pos));
        }
    }
}
