using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using static NewMod.Options.Roles.AegisOptions;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class ShieldArea(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public byte ownerId;
        public float radius;
        public float duration;
        float _t;
        public static readonly List<ShieldArea> _active = new();
        public static IEnumerable<ShieldArea> AreasAt(Vector2 pos)
            => _active.Where(a => a && a.Contains(pos));

        public static IEnumerable<ShieldArea> AreasOwnedBy(byte id)
            => _active.Where(a => a && a.ownerId == id);

        public static AegisMode Mode
            => OptionGroupSingleton<AegisOptions>.Instance.Behavior;

        public void Init(byte ownerId, float radius, float duration)
        {
            this.ownerId = ownerId;
            this.radius = Mathf.Max(0.1f, radius);
            this.duration = Mathf.Max(0.1f, duration);

            var lp = PlayerControl.LocalPlayer;
            bool shouldSee = false;

            if (lp.PlayerId == ownerId)
            {
                shouldSee = OptionGroupSingleton<AegisOptions>.Instance.Visibility != WardVisibilityMode.AllPlayers
                                || true;
            }
            if (!shouldSee)
            {
                switch (OptionGroupSingleton<AegisOptions>.Instance.Visibility)
                {
                    case WardVisibilityMode.AllPlayers:
                        shouldSee = true;
                        break;
                    case WardVisibilityMode.TeamOnly:
                        var role = lp.Data.Role;
                        var isCrew = role && role.TeamType == RoleTeamTypes.Crewmate;

                        if (isCrew && CustomRoleManager.GetCustomRoleBehaviour(role.Role, out var customRole) && customRole != null)
                        {
                            isCrew = customRole.Team == ModdedRoleTeams.Crewmate;
                        }
                        shouldSee = isCrew;
                        break;

                    case WardVisibilityMode.OwnerOnly:
                        break;
                }
            }
            if (shouldSee)
            {
                Utils.CreateCircle(
                    "AegisShieldVisual",
                    (Vector2)transform.position,
                    this.radius,
                    new Color(0.227f, 0.651f, 1f, 0.35f),
                    this.duration
                );
            }
        }

        public void Awake()
        {
            if (!_active.Contains(this)) _active.Add(this);
        }

        public void OnDestroy()
        {
            _active.Remove(this);
        }

        public void Update()
        {
            _t += Time.deltaTime;
            if (_t >= duration)
            {
                Destroy(gameObject);
            }
        }

        public bool Contains(Vector2 worldPos)
        {
            var center = (Vector2)transform.position;
            return Vector2.Distance(worldPos, center) <= radius;
        }
        public static bool IsInsideAny(Vector2 pos) => _active.Any(a => a && a.Contains(pos));

        public static bool IsInsideOthersWard(PlayerControl player)
        {
            if (!player) return false;
            var pos = player.GetTruePosition();
            return _active.Any(a => a && a.ownerId != player.PlayerId && a.Contains(pos));
        }
        public static bool IsInsideOthersWardAt(Vector2 pos, byte sourceId)
        {
            return _active.Any(a => a && a.ownerId != sourceId && a.Contains(pos));
        }
    }
}
