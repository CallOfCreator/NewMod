using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using NewMod.Components.ScreenEffects;
using NewMod.Options.Roles.BeaconOptions;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;
using BC = NewMod.Roles.CrewmateRoles.Beacon;

namespace NewMod.Patches.Roles.Beacon
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
    public static class BeaconShowMapPatch
    {
        private static readonly List<SpriteRenderer> _markers = new();
        private static bool _armedPulseThisOpen;

        public static void Prefix(MapBehaviour __instance, MapOptions opts)
        {
            _armedPulseThisOpen = false;

            if (PlayerControl.LocalPlayer.Data.Role is not BC) return;
            if (__instance.IsOpen || opts.Mode != MapOptions.Modes.Normal) return;

            var settings = OptionGroupSingleton<BeaconOptions>.Instance;

            if (BC.charges <= 0 || Time.time < BC.cooldownUntil) return;

            BC.charges--;
            BC.pulseUntil = Time.time + settings.PulseDuration;
            BC.cooldownUntil = Time.time + settings.PulseCooldown;

            opts.Mode = MapOptions.Modes.CountOverlay;
            opts.ShowLivePlayerPosition = true;
            opts.IncludeDeadBodies = settings.IncludeDeadBodies;
            opts.AllowMovementWhileMapOpen = true;

            _armedPulseThisOpen = true;
        }

        public static void Postfix(MapBehaviour __instance, MapOptions opts)
        {
            if (!_armedPulseThisOpen) return;
            if (!__instance || !__instance.IsOpen) { _armedPulseThisOpen = false; return; }

            ClearMarkers();

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc) continue;

                var marker = Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);
                marker.name = $"BeaconMarker_{pc.PlayerId}";
                marker.enabled = true;

                pc.SetPlayerMaterialColors(marker);

                _markers.Add(marker);
            }

            Coroutines.Start(CoUpdateMarkers(__instance));
            _armedPulseThisOpen = false;

            Helpers.CreateAndShowNotification(
                $"Beacon pulse active ({BC.charges}/{OptionGroupSingleton<BeaconOptions>.Instance.MaxCharges} left)",
                new Color(0.75f, 0.65f, 1f),
                spr: NewModAsset.RadarIcon.LoadAsset());
        }
        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        public static class BeaconOverlayTintKeeper
        {
            static void Postfix(MapCountOverlay __instance)
            {
                if (PlayerControl.LocalPlayer.Data.Role is not BC) return;
                if (Time.time >= BC.pulseUntil)
                {
                    var effect = Camera.main.GetComponent<DistorationWaveEffect>();
                    Object.Destroy(effect); ;
                }

                var map = MapBehaviour.Instance;
                if (!map || !map.IsOpen) return;
                if (!__instance || !__instance.isActiveAndEnabled) return;

                if (__instance.BackgroundColor)
                {
                    __instance.BackgroundColor.SetColor(new Color(0.60f, 0.20f, 0.80f, 1f));
                }
            }
        }

        public static IEnumerator CoUpdateMarkers(MapBehaviour map)
        {
            while (Time.time < BC.pulseUntil && map && map.IsOpen && ShipStatus.Instance)
            {
                var players = PlayerControl.AllPlayerControls.ToArray();
                for (int i = 0; i < players.Length && i < _markers.Count; i++)
                {
                    var pc = players[i];
                    var mrk = _markers[i];
                    if (!pc || !mrk) continue;

                    Vector3 v = pc.transform.position;
                    v /= ShipStatus.Instance.MapScale;
                    v.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                    v.z = -1f;
                    mrk.transform.localPosition = v;
                }
                yield return null;
            }

            ClearMarkers();
            if (map && map.IsOpen)
                map.Show(new MapOptions { Mode = MapOptions.Modes.Normal });
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
        public static class BeaconCloseMapPatch
        {
            public static void Postfix() => ClearMarkers();
        }
        public static void ClearMarkers()
        {
            foreach (var r in _markers) if (r) Object.Destroy(r.gameObject);
            _markers.Clear();
        }
    }
}
