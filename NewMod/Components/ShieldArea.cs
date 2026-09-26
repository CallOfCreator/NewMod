using System;
using NewMod.Utilities;
using System.Collections.Generic;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class ShieldArea(IntPtr ptr) : MonoBehaviour(ptr)
{
    public static readonly List<ShieldArea> _active = new();
    public AreaBubble bubble;
    public byte ownerId;
    public float radius;
    public float expiresAt;

    public void Init(byte owner, float range, float duration)
    {
        ownerId = owner;
        radius = range;
        expiresAt = Time.time + duration;
        _active.Add(this);
        transform.position += new Vector3(0f, 0f, -1f);

        var visual = Utils.CreateSphere("AegisBubble", transform.position, radius, new Color(0.23f, 0.65f, 1f), duration);
        bubble = visual.GetComponent<AreaBubble>();
    }

    public void Update()
    {
        if (Time.time >= expiresAt || MeetingHud.Instance)
            Destroy(gameObject);
    }

    public bool Contains(Vector2 position)
    {
        return Time.time < expiresAt && Vector2.Distance(position, transform.position) <= radius;
    }

    public void OnDestroy()
    {
        _active.Remove(this);
        if (bubble)
            bubble.Break();
    }
}