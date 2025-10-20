using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using NewMod.Components.ScreenEffects;
using NewMod.Options.Roles.ShadeOptions;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class ShadowZone(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public byte shadeId;
        public float radius;
        public float duration;
        private float timer;
        private bool active;
        public static readonly List<ShadowZone> zones = new();

        public void Awake()
        {
            if (!zones.Contains(this))
                zones.Add(this);
        }

        public void OnDestroy()
        {
            zones.Remove(this);
        }

        private bool Contains(Vector2 pos)
        {
            return Vector2.Distance(pos, (Vector2)transform.position) <= radius;
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 0f));
                active = false;
                Destroy(gameObject);
                return;
            }

            var lp = PlayerControl.LocalPlayer;
            var hud = HudManager.Instance;
            var killButton = hud.KillButton;

            bool inside = Contains(lp.GetTruePosition());
            var mode = OptionGroupSingleton<ShadeOptions>.Instance.Behavior;
            var cam = Camera.main;

            if (inside && !active)
            {
                cam.gameObject.AddComponent<ShadowFluxEffect>();
                if (lp.PlayerId == shadeId && lp.Data.Role is Shade)
                {
                    if (mode is ShadeOptions.ShadowMode.Invisible or ShadeOptions.ShadowMode.Both)
                    {
                        lp.cosmetics.SetPhantomRoleAlpha(0);
                        lp.cosmetics.nameText.gameObject.SetActive(false);
                    }

                    killButton.gameObject.SetActive(true);
                    killButton.currentTarget = null;
                }
                active = true;
            }

            if (inside && active && lp.PlayerId == shadeId && lp.Data.Role is Shade)
            {
                if (mode is ShadeOptions.ShadowMode.KillEnabled or ShadeOptions.ShadowMode.Both)
                {
                    var list = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    lp.Data.Role.GetPlayersInAbilityRangeSorted(list);
                    var players = list.ToArray().Where(p => p.PlayerId != lp.PlayerId && !p.Data.IsDead).ToList();
                    var closest = players.Count > 0 ? players[0] : null;

                    if (killButton.currentTarget && killButton.currentTarget != closest)
                        killButton.currentTarget.ToggleHighlight(false, RoleTeamTypes.Impostor);

                    killButton.currentTarget = closest;
                    if (closest != null)
                        closest.ToggleHighlight(true, RoleTeamTypes.Impostor);
                }
            }
            else if (!inside && active)
            {
                Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(cam, 0f));
                if (lp.PlayerId == shadeId)
                {
                    lp.cosmetics.SetPhantomRoleAlpha(1);
                    lp.cosmetics.nameText.gameObject.SetActive(true);

                    if (killButton.currentTarget)
                    {
                        killButton.currentTarget.ToggleHighlight(false, RoleTeamTypes.Impostor);
                        killButton.currentTarget = null;
                    }
                    killButton.gameObject.SetActive(false);
                }
                active = false;
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

        [MethodRpc((uint)CustomRPC.DeployZone)]
        public static void RpcDeployZone(PlayerControl source, Vector2 pos, float radius, float duration)
        {
            Create(source.PlayerId, pos, radius, duration);
        }

        public static bool IsInsideAny(Vector2 pos)
        {
            return zones.Any(z => z && z.Contains(pos));
        }
    }
}
