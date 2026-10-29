using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using NewMod;
using NewMod.Options.Roles;
using Reactor.Utilities.Attributes;
using UnityEngine;

[RegisterInIl2Cpp]
public class SlashTray(nint ptr) : MonoBehaviour(ptr)
{
    public Vector2 Direction;
    public float Speed;
    public Rigidbody2D Body;
    public float Distance;
    public readonly HashSet<byte> HitPlayers = [];
    public PlayerControl Owner { get; set; }

    public void Awake()
    {
        Body = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        Body.isKinematic = true;
        Body.simulated = true;
    }

    public void FixedUpdate()
    {
        if (!Owner || Owner.Data.IsDead || MeetingHud.Instance || ExileController.Instance || Distance >= OptionGroupSingleton<EdgeveilOptions>.Instance.SlashRange)
        {
            Destroy(gameObject);
            return;
        }

        var step = Mathf.Min(Speed * Time.fixedDeltaTime, OptionGroupSingleton<EdgeveilOptions>.Instance.SlashRange - Distance);
        if (PhysicsHelpers.AnyNonTriggersBetween(Body.position, Direction, step, Constants.ShipAndObjectsMask))
        {
            Destroy(gameObject);
            return;
        }

        Body.MovePosition(Body.position + Direction * step);
        Distance += step;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!Owner || HitPlayers.Count >= (int)OptionGroupSingleton<EdgeveilOptions>.Instance.PlayersToKill)
            return;
        var player = other.GetComponentInParent<PlayerControl>();
        if (!player || player == Owner || player.Data.IsDead || player.Data.Disconnected || player.inVent || player.Data.Role.IsImpostor || PhysicsHelpers.AnythingBetween(transform.position, player.GetTruePosition(), Constants.ShipAndObjectsMask, false) || !HitPlayers.Add(player.PlayerId))
            return;

        if (Owner.AmOwner)
            Owner.RpcCustomMurder(player, MeetingCheck.OutsideMeeting, teleportMurderer: false);

        if (HitPlayers.Count >= (int)OptionGroupSingleton<EdgeveilOptions>.Instance.PlayersToKill) Destroy(gameObject);
    }

    public static SlashTray CreateTray()
    {
        var gameObject = Instantiate(NewModAsset.SlashTray.LoadAsset(), ShipStatus.Instance.transform);
        return gameObject.GetComponent<SlashTray>() ?? gameObject.AddComponent<SlashTray>();
    }

    public void SetMotion(Vector2 dir, float speed)
    {
        Direction = dir.normalized;
        Speed = speed;
    }
}
