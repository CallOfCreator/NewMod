using System.Collections.Generic;
using UnityEngine;
using NewMod.Components;
using NewMod.Options.Roles.AegisOptions;
using MiraAPI.GameOptions;
using Reactor.Utilities.Extensions;
using Reactor.Utilities;

namespace NewMod.Utilities
{
    public static class AegisUtilities
    {
        public static readonly HashSet<byte> ActiveOwners = new();
        public static bool HasActiveShield()
        {
            var lp = PlayerControl.LocalPlayer;
            return lp && ActiveOwners.Contains(lp.PlayerId);
        }
        public static void ActivateShield(PlayerControl owner, Vector2 position)
        {
            if (!owner) return;

            var opts = OptionGroupSingleton<AegisOptions>.Instance;

            var go = new GameObject("AegisShieldArea").DontDestroy();
            go.transform.position = position;

            var area = go.AddComponent<ShieldArea>();
            area.Init(owner.PlayerId, opts.Radius, opts.DurationSeconds);

            ActiveOwners.Add(owner.PlayerId);
            Coroutines.Start(CoCleanupOwner(owner.PlayerId, opts.DurationSeconds));
        }

        static System.Collections.IEnumerator CoCleanupOwner(byte ownerId, float duration)
        {
            yield return new WaitForSeconds(duration);
            ActiveOwners.Remove(ownerId);
        }
    }
}
