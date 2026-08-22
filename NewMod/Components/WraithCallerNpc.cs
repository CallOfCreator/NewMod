using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class WraithCallerNpc(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public PlayerControl Target;
    public PlayerControl Visual;
    public PlayerControl ReflectedBy;
    public Rigidbody2D body;
    public PlayerAnimations animations;
    public LightSource ownerLight;
    public int NpcId;
    public bool isActive;
    public bool Reflected;

    [HideFromIl2Cpp]
    public void Initialize(PlayerControl owner, PlayerControl target, Vector2 start, int npcId)
    {
        Owner = owner;
        Target = target;
        NpcId = npcId;

        Visual = Instantiate(AmongUsClient.Instance.PlayerPrefab);
        Visual.transform.position = new Vector3(start.x, start.y, Owner.transform.position.z);
        Visual.notRealPlayer = true;
        Visual.enabled = false;
        Visual.NetTransform.enabled = false;
        Visual.Collider.enabled = false;
        Visual.MyPhysics.enabled = false;

        PlayerControl.AllPlayerControls.Remove(Visual);

        body = Visual.MyPhysics.body;
        animations = Visual.MyPhysics.Animations;
        body.isKinematic = false;

        Visual.cosmetics.enabled = true;
        Visual.cosmetics.Visible = true;
        Visual.cosmetics.SetName("Wraith NPC");
        Visual.cosmetics.ToggleName(true);
        Visual.cosmetics.SetNamePosition(new Vector3(0f, 0.8f, -0.5f));
        Visual.cosmetics.ToggleHat(false);
        Visual.cosmetics.TogglePet(false);
        Visual.cosmetics.ToggleVisor(false);

        var color = Random.Range(0, Palette.PlayerColors.Length);
        var bodySprite = Visual.cosmetics.currentBodySprite;
        bodySprite.Visible = true;
        Visual.cosmetics.SetBodyColor(color);
        Visual.cosmetics.colorBlindText.text = "NPC";

        var noShadow = Visual.gameObject.AddComponent<NoShadowBehaviour>();
        noShadow.rend = bodySprite.BodySprite;
        noShadow.hitOverride = Visual.Collider;

        isActive = true;
        Coroutines.Start(CoMove());

        if (Owner.AmOwner && OptionGroupSingleton<WraithCallerOptions>.Instance.ShouldSwitchCamToNPC)
        {
            Camera.main.GetComponent<FollowerCamera>().SetTarget(Visual);
            ownerLight = Owner.lightSource;
            ownerLight.transform.SetParent(Visual.transform, false);
            ownerLight.transform.localPosition = Visual.Collider.offset;
        }

        if (Target.AmOwner)
            SoundManager.Instance.PlaySound(NewModAsset.HeartbeatSound.LoadAsset(), false);
    }

    [HideFromIl2Cpp]
    public void RedirectToOwner(PlayerControl mirror)
    {
        if (Reflected)
            return;

        Reflected = true;
        ReflectedBy = mirror;
        Target = Owner;
        Visual.cosmetics.SetName("Reflected Wraith");

        if (Owner.AmOwner)
            SoundManager.Instance.PlaySound(NewModAsset.HeartbeatSound.LoadAsset(), false);
    }

    [HideFromIl2Cpp]
    public IEnumerator CoMove()
    {
        var speed = OptionGroupSingleton<WraithCallerOptions>.Instance.NPCSpeed;

        while (isActive && !MeetingHud.Instance)
        {
            if (Target.Data.IsDead || Target.Data.Disconnected)
                break;

            var npcPos = (Vector2)Visual.transform.position;
            var delta = Target.GetTruePosition() - npcPos;

            if (AmongUsClient.Instance.AmHost && delta.sqrMagnitude <= 0.01f)
            {
                body.velocity = Vector2.zero;
                UpdateWalkAnimation(Vector2.zero);

                if (!Reflected && Target.Data.Role is MirrorBladeRole && MirrorBladeRole.ArmedReflections.Remove(Target.PlayerId))
                {
                    var mirror = Target;
                    RedirectToOwner(mirror);
                    MirrorBladeRole.RpcReflectionTriggered(mirror, Owner.PlayerId, NpcId);
                    yield return new WaitForSeconds(0.15f);
                    continue;
                }

                var victim = Target;

                if (Reflected)
                    ReflectedBy.RpcCustomMurder(victim, true, false, true, false, false, false);
                else
                    Owner.RpcCustomMurder(victim, true, false, teleportMurderer: false);

                yield return null;

                if (!Reflected && victim.Data.IsDead)
                    WraithCallerUtilities.AddKillNPC(Owner.PlayerId);

                break;
            }

            var velocity = delta.normalized * speed;
            body.velocity = velocity;
            UpdateWalkAnimation(velocity);

            yield return new WaitForFixedUpdate();
        }

        body.velocity = Vector2.zero;
        UpdateWalkAnimation(Vector2.zero);
        Dispose();
    }

    [HideFromIl2Cpp]
    public void UpdateWalkAnimation(Vector2 velocity)
    {
        var moving = velocity.sqrMagnitude >= 0.01f;

        if (velocity.x < -0.01f)
            Visual.cosmetics.SetFlipXWithoutPet(true);
        else if (velocity.x > 0.01f)
            Visual.cosmetics.SetFlipXWithoutPet(false);

        if (moving)
        {
            if (!animations.IsPlayingRunAnimation())
                animations.PlayRunAnimation();

            if (Visual.cosmetics.HasSkinLoaded() && !Visual.cosmetics.IsSkinPlayingRunAnim())
                Visual.cosmetics.AnimateSkinRun();
        }
        else if (animations.IsPlayingRunAnimation() || !animations.IsPlayingSomeAnimation())
        {
            animations.PlayIdleAnimation();

            if (Visual.cosmetics.HasSkinLoaded())
                Visual.cosmetics.AnimateSkinIdle();
        }

        var pos = Visual.transform.position;
        pos.z = pos.y / 1000f;
        Visual.transform.position = pos;
    }

    [HideFromIl2Cpp]
    public void Dispose()
    {
        if (!isActive)
            return;

        isActive = false;
        WraithCallerUtilities.ActiveNpcs.Remove(((uint)Owner.PlayerId << 16) | (uint)NpcId);

        if (Owner.AmOwner && OptionGroupSingleton<WraithCallerOptions>.Instance.ShouldSwitchCamToNPC)
        {
            Camera.main.GetComponent<FollowerCamera>().SetTarget(Owner);
            ownerLight.transform.SetParent(Owner.transform, false);
            ownerLight.transform.localPosition = Owner.Collider.offset;
        }

        if (body)
            body.velocity = Vector2.zero;

        Destroy(Visual.gameObject);
        Destroy(gameObject);
    }
}
