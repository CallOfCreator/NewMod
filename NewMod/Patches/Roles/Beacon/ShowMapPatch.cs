using System.Collections;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using NewMod.Networking;
using NewMod.Options.Roles;
using Reactor.Networking.Rpc;
using UnityEngine;
using BC = NewMod.Roles.CrewmateRoles.Beacon;

namespace NewMod.Patches.Roles.Beacon;

public static class BeaconShowMapPatch
{
    public static IEnumerator ShowSnapshot()
    {
        HudManager.Instance.InitMap();
        var map = MapBehaviour.Instance;
        map.Show(new MapOptions { Mode = MapOptions.Modes.Normal, AllowMovementWhileMapOpen = true });
        if (!map.IsOpen)
            yield break;

        var options = OptionGroupSingleton<BeaconOptions>.Instance;
        if (PlayerControl.LocalPlayer.HasModifier<OverclockedModifier>())
            OverclockedModifier.RpcRequestPulse(PlayerControl.LocalPlayer);
        BC.charges--;
        BC.pulseUntil = Time.time + options.PulseDuration;
        Rpc<BeaconPulseRpc>.Instance.Send(new BeaconPulseRpc.Data(0.6f));
        var markers = new List<SpriteRenderer>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.IsDead || player.Data.Disconnected || player.inVent || player == PlayerControl.LocalPlayer)
                continue;

            var marker = Object.Instantiate(map.HerePoint, map.HerePoint.transform.parent);
            marker.enabled = true;
            PlayerMaterial.SetColors(new Color(0.75f, 0.65f, 1f), marker);
            var position = (Vector3)player.GetTruePosition() / ShipStatus.Instance.MapScale;
            position.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
            position.z = -1f;
            marker.transform.localPosition = position;
            markers.Add(marker);
        }

        while (map && map.IsOpen && Time.time < BC.pulseUntil && !PlayerControl.LocalPlayer.Data.IsDead && !MeetingHud.Instance && !Utilities.Utils.IsActive(SystemTypes.Comms))
            yield return null;

        foreach (var marker in markers)
        {
            if (marker)
            {
                Object.Destroy(marker.material);
                Object.Destroy(marker.gameObject);
            }
        }
    }
}
