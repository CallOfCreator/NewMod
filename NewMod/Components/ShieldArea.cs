using System;
using System.Collections.Generic;
using NewMod.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class ShieldArea(IntPtr ptr) : MonoBehaviour(ptr)
{
    public static readonly List<ShieldArea> _active = new();
    public byte ownerId;
    public float radius;
    public float expiresAt;
    public Mesh mesh;

    public void Init(byte owner, float range, float duration)
    {
        ownerId = owner;
        radius = range;
        expiresAt = Time.time + duration;
        _active.Add(this);
        transform.position += new Vector3(0f, 0f, -1f);

        var vertices = new Vector3[25 * 49];
        var colors = new Color[vertices.Length];
        var triangles = new int[24 * 48 * 6];
        var light = new Vector3(-0.4f, 0.65f, -0.65f).normalized;
        for (var ring = 0; ring <= 24; ring++)
        {
            var angle = ring * Mathf.PI / 48f;
            var distance = Mathf.Sin(angle);
            var depth = Mathf.Cos(angle);
            for (var segment = 0; segment <= 48; segment++)
            {
                var azimuth = segment * Mathf.PI / 24f;
                var normal = new Vector3(distance * Mathf.Cos(azimuth), distance * Mathf.Sin(azimuth), -depth);
                var index = ring * 49 + segment;
                vertices[index] = new Vector3(normal.x, normal.y, normal.z * 0.45f) * radius;
                var highlight = Mathf.Pow(Mathf.Max(0f, Vector3.Dot(normal, light)), 32f);
                colors[index] = Color.Lerp(new Color(0.23f, 0.65f, 1f, 0.04f + Mathf.Pow(distance, 10f) * 0.5f), new Color(0.85f, 0.97f, 1f, 0.6f), highlight);
                if (ring == 24 || segment == 48)
                    continue;
                var triangle = (ring * 48 + segment) * 6;
                triangles[triangle] = index;
                triangles[triangle + 1] = index + 49;
                triangles[triangle + 2] = index + 1;
                triangles[triangle + 3] = index + 1;
                triangles[triangle + 4] = index + 49;
                triangles[triangle + 5] = index + 50;
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Utils.GetCircleMat();
        renderer.sortingOrder = 2;
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
        Destroy(mesh);
    }
}