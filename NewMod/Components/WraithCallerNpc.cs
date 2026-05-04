using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using NewMod.Options.Roles.WraithCallerOptions;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class WraithCallerNpc(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public PlayerControl Owner { get; set; }
        public PlayerControl Target { get; set; }
        public PlayerControl Visual { get; set; }
        public Rigidbody2D body;
        public PlayerAnimations animations;

        public LightSource ownerLight;
        public bool isActive;

        [HideFromIl2Cpp]

        // Inspired by: https://github.com/NuclearPowered/Reactor/blob/e27a79249ea706318f3c06f3dc56a5c42d65b1cf/Reactor.Debugger/Window/Tabs/GameTab.cs#L70
        public void Initialize(PlayerControl owner, PlayerControl target, Vector2 start)
        {
            Owner = owner;
            Target = target;

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
            Visual.cosmetics.ToggleHat(false);
            Visual.cosmetics.TogglePet(false);
            Visual.cosmetics.ToggleVisor(false);

            var color = Owner.PlayerId % Palette.PlayerColors.Length;
            var bodySprite = Visual.cosmetics.currentBodySprite;

            bodySprite.Visible = true;
            PlayerMaterial.SetColors(color, bodySprite.BodySprite);

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
                SoundManager.Instance.PlaySound(NewModAsset.HeartbeatSound.LoadAsset(), false, 1f);
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
                var targetPos = Target.GetTruePosition();
                var delta = targetPos - npcPos;

                var slowDown = Mathf.Clamp(delta.magnitude * 2f, 0.05f, 1f);
                var velocity = delta.normalized * speed * slowDown;

                body.velocity = velocity;

                UpdateWalkAnimation(velocity);

                if (AmongUsClient.Instance.AmHost && delta.magnitude <= 0.15f)
                {
                    body.velocity = Vector2.zero;
                    UpdateWalkAnimation(Vector2.zero);

                    Owner.RpcCustomMurder(Target, true, teleportMurderer: false);

                    if (Target.AmOwner)
                    {
                        CoroutinesHelper.CoNotify("<color=#FF4D4D><b>Oops!</b> The <i>Wraith NPC</i> got you...");
                    }

                    WraithCallerUtilities.AddKillNPC(Owner.PlayerId);
                    break;
                }

                yield return null;
            }

            body.velocity = Vector2.zero;
            UpdateWalkAnimation(Vector2.zero);
            Dispose();
        }
        [HideFromIl2Cpp]
        public void UpdateWalkAnimation(Vector2 velocity)
        {
            bool moving = velocity.sqrMagnitude >= 0.01f;

            if (velocity.x < -0.01f)
                Visual.cosmetics.SetFlipXWithoutPet(true);
            else if (velocity.x > 0.01f)
                Visual.cosmetics.SetFlipXWithoutPet(false);

            if (moving)
            {
                if (!animations.IsPlayingRunAnimation())
                {
                    animations.PlayRunAnimation();
                }
                if (Visual.cosmetics.HasSkinLoaded() && !Visual.cosmetics.IsSkinPlayingRunAnim())
                {
                    Visual.cosmetics.AnimateSkinRun();
                }
            }
            else
            {
                if (animations.IsPlayingRunAnimation() || !animations.IsPlayingSomeAnimation())
                {
                    animations.PlayIdleAnimation();

                    if (Visual.cosmetics.HasSkinLoaded())
                    {
                        Visual.cosmetics.AnimateSkinIdle();
                    }
                }
            }
            var pos = Visual.transform.position;
            pos.z = pos.y / 1000f;
            Visual.transform.position = pos;
        }
        [HideFromIl2Cpp]
        public void Dispose()
        {
            if (!isActive) return;

            isActive = false;

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
}