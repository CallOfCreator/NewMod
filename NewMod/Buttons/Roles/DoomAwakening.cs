using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public sealed class DoomAwakening : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Aggression;
    public override string Name => "";
    public override float Cooldown => 0f;
    public override int MaxUses => 1;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DoomAwakeningButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Revenant;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Revenant.Phases.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var phase) && phase == Revenant.Phase.Revived;
    }

    protected override void OnClick()
    {
        Revenant.RpcRequestDoom(PlayerControl.LocalPlayer);
    }

    public static IEnumerator CoDoom(PlayerControl player, float expiresAt)
    {
        if (!player || !player.AmOwner)
            yield break;

        var options = OptionGroupSingleton<RevenantOptions>.Instance;
        var physics = player.MyPhysics;
        var originalSpeed = physics.Speed;
        var overlay = HudManager.Instance.FullScreen;
        var originalColor = overlay.color;
        var overlayWasActive = overlay.gameObject.activeSelf;
        var sound = NewModAsset.DoomAwakeningSound.LoadAsset();
        var interval = new WaitForSeconds(0.25f);

        physics.Speed *= 1f + options.DoomSpeedBonus / 100f;
        overlay.color = new Color(0.55f, 0f, 0.08f, 0.24f);
        overlay.gameObject.SetActive(true);
        SoundManager.Instance.PlaySound(sound, true);

        while (player && !player.Data.IsDead && !player.Data.Disconnected && AmongUsClient.Instance.IsGameStarted && !MeetingHud.Instance && !ExileController.Instance && Time.time < expiresAt && Revenant.Phases.TryGetValue(player.PlayerId, out var phase) && phase == Revenant.Phase.DoomActive)
        {
            if (physics.GetVelocity().sqrMagnitude > 0.01f)
            {
                var ghost = new GameObject("RevenantTrail");
                var renderer = ghost.AddComponent<SpriteRenderer>();
                var body = player.cosmetics.normalBodySprite.BodySprite;

                renderer.sprite = body.sprite;
                renderer.flipX = body.flipX;
                renderer.flipY = body.flipY;
                renderer.sharedMaterial = body.sharedMaterial;
                renderer.sortingLayerID = body.sortingLayerID;
                renderer.sortingOrder = body.sortingOrder + 1;
                PlayerMaterial.SetColors(player.Data.DefaultOutfit.ColorId, renderer);
                ghost.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
                ghost.transform.localScale = body.transform.lossyScale;
                Coroutines.Start(Utils.FadeAndDestroy(ghost, 0.8f));
            }

            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (!target || target.Data.IsDead || target.Data.Disconnected || target.inVent || target.Data.Role.IsImpostor)
                    continue;

                if (Vector2.Distance(player.GetTruePosition(), target.GetTruePosition()) > options.DoomContactRadius)
                    continue;

                Revenant.RpcRequestDoomContact(player, target);
                break;
            }

            yield return interval;
        }

        if (physics)
            physics.Speed = originalSpeed;
        if (overlay)
        {
            overlay.color = originalColor;
            overlay.gameObject.SetActive(overlayWasActive);
        }

        SoundManager.Instance.StopSound(sound);
        if (player && !player.Data.IsDead && AmongUsClient.Instance.IsGameStarted)
            SoundManager.Instance.PlaySoundImmediate(NewModAsset.DoomAwakeningEndSound.LoadAsset(), false, 1f, 1f, null);
    }
}