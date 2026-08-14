using System;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using NewMod;
using NewMod.Options.Roles;
using Reactor.Utilities.Attributes;
using UnityEngine;

[RegisterInIl2Cpp]
public class SlashTray(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer SlashTrayRend;
    public Vector2 _dir;
    public float _speed;
    public int _kills;
    public PlayerControl Owner { get; set; }

    public void Awake()
    {
        SlashTrayRend = transform.Find("Background").GetComponent<SpriteRenderer>();
        var rb = transform.gameObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.simulated = true;
    }

    public void Update()
    {
        transform.position += (Vector3)(_dir * _speed * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        NewMod.NewMod.Instance.Log.LogMessage($"Hit {other.name}");
        var pc = other.GetComponentInParent<PlayerControl>();
        if (pc == null) return;
        if (pc == Owner) return;
        if (pc.Data.IsDead) return;

        PlayerControl.LocalPlayer.RpcCustomMurder(pc, teleportMurderer: false);

        _kills++;
        if (_kills >= (int)OptionGroupSingleton<EdgeveilOptions>.Instance.PlayersToKill) Destroy(gameObject);
    }

    public static SlashTray CreateTray()
    {
        var gameObject = Instantiate(NewModAsset.SlashTray.LoadAsset(), HudManager.Instance.transform);
        var tray = gameObject.AddComponent<SlashTray>();
        return tray;
    }

    public void SetMotion(Vector2 dir, float speed)
    {
        _dir = dir.normalized;
        _speed = speed;
    }
}