using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using NewMod.Components.ScreenEffects;
using NewMod.Components.ScreenEffects.Effects;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers.S1;
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
    public AreaBubble bubble;
    public byte shadeId;
    public float radius;
    public float duration;
    public float timer;
    public float activationDelay;
    public bool concealed;
    public PlayerControl owner;
    public readonly Il2CppSystem.Collections.Generic.List<PlayerControl> PlayersInRange = new();

    public void Update()
    {
        timer += Time.deltaTime;
        if (!owner || owner.Data.IsDead || owner.Data.Disconnected || !AbilityExchangeGE.HasRoleAbility<Shade>(owner) || timer >= duration + activationDelay || MeetingHud.Instance || ExileController.Instance)
        {
            Destroy(gameObject);
            return;
        }

        var hide = Contains(owner.GetTruePosition());
        if (hide != concealed)
        {
            concealed = hide;
            var alpha = 1f;
            if (hide)
                alpha = owner.AmOwner ? 0.35f : 0f;
            owner.cosmetics.SetPhantomRoleAlpha(alpha);
            owner.cosmetics.nameText.gameObject.SetActive(!hide);
        }

        var local = PlayerControl.LocalPlayer;
        var camera = Camera.main;
        if (IsInsideAny(local.GetTruePosition()))
        {
            camera.AddScreenEffect<ShadowFluxEffect>();
        }
        else
        {
            camera.GetScreenEffect<ShadowFluxEffect>()?.Remove();
        }

        if (!owner.AmOwner) return;
        var killButton = HudManager.Instance.KillButton;
        if (!owner.Data.Role.CanUseKillButton)
        {
            owner.killTimer = Mathf.Max(0f, owner.killTimer - Time.deltaTime);
            killButton.SetCoolDown(owner.killTimer, GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
        }
        var canKill = Contains(owner.GetTruePosition());
        killButton.gameObject.SetActive(canKill);
        PlayerControl target = null;
        if (canKill)
        {
            owner.Data.Role.GetPlayersInAbilityRangeSorted(PlayersInRange);
            foreach (var player in PlayersInRange)
            {
                if (player == owner || player.Data.IsDead || player.Data.Disconnected || player.inVent || !Contains(player.GetTruePosition()) || PhysicsHelpers.AnythingBetween(owner.GetTruePosition(), player.GetTruePosition(), Constants.ShipAndObjectsMask, false))
                    continue;

                target = player;
                break;
            }
        }

        killButton.SetTarget(target);
    }

    public void OnDestroy()
    {
        zones.Remove(this);
        if (bubble)
            bubble.Break();
        if (owner && concealed)
        {
            owner.cosmetics.SetPhantomRoleAlpha(1f);
            owner.cosmetics.nameText.gameObject.SetActive(true);
        }

        if (owner && owner.AmOwner && HudManager.Instance)
        {
            HudManager.Instance.KillButton.SetTarget(null);
            if (owner.Data.Role is Shade)
                HudManager.Instance.KillButton.Hide();
            else
                HudManager.Instance.SetHudActive(owner, owner.Data.Role, !owner.Data.IsDead && !MeetingHud.Instance && !ExileController.Instance);
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
        zone.bubble = bubble.GetComponent<AreaBubble>();
        return zone;
    }

    [MethodRpc((uint)CustomRPC.DeployZone)]
    public static void RpcDeployZone(PlayerControl source, Vector2 position, float radius, float duration)
    {
        if (AbilityExchangeGE.HasRoleAbility<Shade>(source) && !source.Data.IsDead && !source.Data.Disconnected && !MeetingHud.Instance)
        {
            OverclockedModifier.Pulse(source);
            Create(source.PlayerId, position, radius, duration);
        }
    }

    public static bool IsInsideAny(Vector2 position)
    {
        foreach (var zone in zones)
        {
            if (zone && zone.Contains(position))
                return true;
        }

        return false;
    }
}
