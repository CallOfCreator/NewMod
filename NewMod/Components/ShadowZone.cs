using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using NewMod.Components.ScreenEffects;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class ShadowZone(nint ptr) : MonoBehaviour(ptr)
{
    public static readonly List<ShadowZone> zones = [];
    public byte shadeId;
    public float radius;
    public float duration;
    public float timer;
    public float activationDelay;
    public bool concealed;
    public PlayerControl owner;

    public void Update()
    {
        timer += Time.deltaTime;
        if (!owner || owner.Data.IsDead || owner.Data.Disconnected || owner.Data.Role is not Shade || timer >= duration + activationDelay || MeetingHud.Instance || ExileController.Instance)
        {
            Destroy(gameObject);
            return;
        }

        var hide = Contains(owner.GetTruePosition());
        if (hide != concealed)
        {
            concealed = hide;
            owner.cosmetics.SetPhantomRoleAlpha(hide ? owner.AmOwner ? 0.35f : 0f : 1f);
            owner.cosmetics.nameText.gameObject.SetActive(!hide);
        }

        var local = PlayerControl.LocalPlayer;
        var camera = Camera.main;
        if (zones.Any(zone => zone && zone.Contains(local.GetTruePosition())))
        {
            if (camera.GetScreenEffect<ShadowFluxEffect>() == null) camera.AddScreenEffect<ShadowFluxEffect>();
        }
        else
        {
            camera.GetScreenEffect<ShadowFluxEffect>()?.Remove();
        }

        if (!owner.AmOwner) return;
        var killButton = HudManager.Instance.KillButton;
        var canKill = Contains(owner.GetTruePosition());
        killButton.gameObject.SetActive(canKill);
        PlayerControl target = null;
        if (canKill)
        {
            var players = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            owner.Data.Role.GetPlayersInAbilityRangeSorted(players);
            target = players.ToArray().FirstOrDefault(player => player != owner && !player.Data.IsDead && !player.Data.Disconnected && !player.inVent && Contains(player.GetTruePosition()) && !PhysicsHelpers.AnythingBetween(owner.GetTruePosition(), player.GetTruePosition(), Constants.ShipAndObjectsMask, false));
        }

        killButton.SetTarget(target);
    }

    public void OnDestroy()
    {
        zones.Remove(this);
        if (owner && concealed)
        {
            owner.cosmetics.SetPhantomRoleAlpha(1f);
            owner.cosmetics.nameText.gameObject.SetActive(true);
        }

        if (owner && owner.AmOwner && HudManager.Instance)
        {
            HudManager.Instance.KillButton.SetTarget(null);
            HudManager.Instance.KillButton.Hide();
        }

        if (Camera.main && PlayerControl.LocalPlayer && !IsInsideAny(PlayerControl.LocalPlayer.GetTruePosition()))
            Camera.main.GetScreenEffect<ShadowFluxEffect>()?.Remove();
    }

    public bool Contains(Vector2 position)
    {
        return timer >= activationDelay && timer < activationDelay + duration && Vector2.Distance(position, transform.position) <= radius;
    }

    public static ShadowZone Create(byte id, Vector2 position, float radius, float duration)
    {
        foreach (var previous in zones.Where(zone => zone && zone.shadeId == id).ToArray())
            Destroy(previous.gameObject);
        var zone = new GameObject("ShadowZone").AddComponent<ShadowZone>();
        zone.shadeId = id;
        zone.owner = Utils.PlayerById(id);
        zone.radius = radius;
        zone.duration = duration;
        zone.activationDelay = OptionGroupSingleton<ShadeOptions>.Instance.ActivationDelay;
        zone.transform.position = position;
        zones.Add(zone);
        var bubble = Utils.CreateSphere("ShadowBoundary", new Vector3(position.x, position.y, -1f), radius, new Color(0.65f, 0.3f, 0.95f), zone.activationDelay + duration, true);
        bubble.transform.SetParent(zone.transform, true);
        return zone;
    }

    [MethodRpc((uint)CustomRPC.DeployZone)]
    public static void RpcDeployZone(PlayerControl source, Vector2 position, float radius, float duration)
    {
        if (source.Data.Role is Shade && !source.Data.IsDead && !source.Data.Disconnected && !MeetingHud.Instance)
            Create(source.PlayerId, position, radius, duration);
    }

    public static bool IsInsideAny(Vector2 position)
    {
        return zones.Any(zone => zone && zone.Contains(position));
    }
}