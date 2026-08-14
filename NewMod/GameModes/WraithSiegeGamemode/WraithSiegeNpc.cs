using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using NewMod.GameModes.WraithSiegeGamemode.Options;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.GameModes.WraithSiegeGamemode;

[RegisterInIl2Cpp]
public sealed class WraithSiegeNpc(IntPtr ptr) : MonoBehaviour(ptr)
{
    public int NpcId;
    public PlayerControl Owner;
    public PlayerControl Visual;
    public Rigidbody2D Body;
    public PlayerAnimations Animations;

    public Vector2[] Route;
    public int WaypointIndex;

    public bool Active;
    public bool Delivering;
    public bool AtDeliverySlot;

    private Vector2 _deliveryPoint;

    [HideFromIl2Cpp]
    public void Initialize(int npcId, PlayerControl owner, WraithLane lane, Vector2[] route)
    {
        NpcId = npcId;
        Owner = owner;
        Route = route;
        WaypointIndex = 1;

        Visual = Instantiate(AmongUsClient.Instance.PlayerPrefab);
        Visual.transform.position = new Vector3(route[0].x, route[0].y, owner.transform.position.z);

        Visual.notRealPlayer = true;
        Visual.enabled = false;
        Visual.NetTransform.enabled = false;
        Visual.Collider.enabled = false;
        Visual.MyPhysics.enabled = false;

        PlayerControl.AllPlayerControls.Remove(Visual);

        Body = Visual.MyPhysics.body;
        Animations = Visual.MyPhysics.Animations;
        Body.isKinematic = false;

        Visual.cosmetics.enabled = true;
        Visual.cosmetics.Visible = true;
        Visual.cosmetics.SetName($"{lane} Wraith");
        Visual.cosmetics.ToggleName(true);
        Visual.cosmetics.SetNamePosition(new Vector3(0f, 0.8f, -0.5f));
        Visual.cosmetics.ToggleHat(false);
        Visual.cosmetics.TogglePet(false);
        Visual.cosmetics.ToggleVisor(false);

        var bodySprite = Visual.cosmetics.currentBodySprite;
        bodySprite.Visible = true;

        PlayerMaterial.SetColors(owner.Data.DefaultOutfit.ColorId, bodySprite.BodySprite);

        var noShadow = Visual.gameObject.AddComponent<NoShadowBehaviour>();
        noShadow.rend = bodySprite.BodySprite;
        noShadow.hitOverride = Visual.Collider;

        Active = true;

        Coroutines.Start(CoMove());
    }

    [HideFromIl2Cpp]
    public void BeginDelivery(Vector2 point)
    {
        Delivering = true;
        AtDeliverySlot = false;
        _deliveryPoint = point;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoMove()
    {
        while (Active)
        {
            if (Delivering)
            {
                var delta = _deliveryPoint - (Vector2)Visual.transform.position;

                if (delta.sqrMagnitude <= 0.01f)
                {
                    Body.velocity = Vector2.zero;
                    UpdateAnimation(Vector2.zero);
                    AtDeliverySlot = true;
                }
                else
                {
                    Move(delta);
                }

                yield return new WaitForFixedUpdate();
                continue;
            }

            if (WaypointIndex < Route.Length)
            {
                var delta = Route[WaypointIndex] - (Vector2)Visual.transform.position;

                if (delta.sqrMagnitude <= 0.025f)
                {
                    WaypointIndex++;
                    continue;
                }

                Move(delta);

                yield return new WaitForFixedUpdate();
                continue;
            }

            Body.velocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);

            if (AmongUsClient.Instance.AmHost)
                WraithSiege.Instance.StartDelivery(this);

            yield return new WaitForFixedUpdate();
        }
    }

    [HideFromIl2Cpp]
    private void Move(Vector2 direction)
    {
        var velocity = direction.normalized * OptionGroupSingleton<WraithSiegeOptions>.Instance.NpcSpeed;

        Body.velocity = velocity;
        UpdateAnimation(velocity);
    }

    [HideFromIl2Cpp]
    private void UpdateAnimation(Vector2 velocity)
    {
        var moving = velocity.sqrMagnitude >= 0.01f;

        if (velocity.x < -0.01f)
            Visual.cosmetics.SetFlipXWithoutPet(true);
        else if (velocity.x > 0.01f)
            Visual.cosmetics.SetFlipXWithoutPet(false);

        if (moving)
        {
            if (!Animations.IsPlayingRunAnimation())
                Animations.PlayRunAnimation();

            if (Visual.cosmetics.HasSkinLoaded() && !Visual.cosmetics.IsSkinPlayingRunAnim())
                Visual.cosmetics.AnimateSkinRun();
        }
        else if (Animations.IsPlayingRunAnimation() || !Animations.IsPlayingSomeAnimation())
        {
            Animations.PlayIdleAnimation();

            if (Visual.cosmetics.HasSkinLoaded())
                Visual.cosmetics.AnimateSkinIdle();
        }

        var position = Visual.transform.position;
        position.z = position.y / 1000f;
        Visual.transform.position = position;
    }

    [HideFromIl2Cpp]
    public void Dispose()
    {
        if (!Active)
            return;

        Active = false;

        if (Body)
            Body.velocity = Vector2.zero;

        if (Visual)
            Destroy(Visual.gameObject);

        Destroy(gameObject);
    }
}