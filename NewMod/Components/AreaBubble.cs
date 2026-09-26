using NewMod.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class AreaBubble(nint ptr) : MonoBehaviour(ptr)
{
    public Mesh mesh;
    public float expiresAt;
    public float breakStartedAt;
    public float breakDuration = 0.5f;
    public bool breaking;
    public Vector3[] vertices;
    public Color[] colors;
    public int[] triangles;
    public Vector3[] fragments;
    public Color[] fragmentColors;

    public void Init(float radius, Color color, float opacity = 0.04f)
    {
        vertices = new Vector3[25 * 49];
        colors = new Color[vertices.Length];
        triangles = new int[24 * 48 * 6];
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
                colors[index] = Color.Lerp(new Color(color.r, color.g, color.b, opacity + Mathf.Pow(distance, 10f) * 0.5f), new Color(0.85f, 0.97f, 1f, 0.6f), highlight);
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
        if (!ShipStatus.Instance || (breaking && (MeetingHud.Instance || ExileController.Instance)))
        {
            Destroy(gameObject);
            return;
        }

        if (!breaking)
        {
            if (Time.time >= expiresAt)
                Break();
            return;
        }

        var progress = Mathf.Clamp01((Time.time - breakStartedAt) / breakDuration);
        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        var fill = Mathf.Clamp01(progress / 0.25f);
        var spread = Mathf.Clamp01((progress - 0.25f) / 0.75f);
        for (var triangle = 0; triangle < triangles.Length; triangle += 3)
        {
            var center = (vertices[triangles[triangle]] + vertices[triangles[triangle + 1]] + vertices[triangles[triangle + 2]]) / 3f;
            var offset = center * (spread * 0.35f);
            for (var corner = 0; corner < 3; corner++)
            {
                var index = triangle + corner;
                var vertex = triangles[index];
                fragments[index] = center + (vertices[vertex] - center) * (1f - spread * 0.8f) + offset;
                var color = Color.Lerp(colors[vertex], Color.white, fill * 0.3f);
                color.a = Mathf.Lerp(colors[vertex].a, 0.65f, fill) * (1f - spread);
                fragmentColors[index] = color;
            }
        }

        mesh.vertices = fragments;
        mesh.colors = fragmentColors;
        mesh.RecalculateBounds();
    }

    public void Break()
    {
        if (breaking)
            return;

        if (!ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance)
        {
            Destroy(gameObject);
            return;
        }

        breaking = true;
        breakStartedAt = Time.time;
        fragments = new Vector3[triangles.Length];
        fragmentColors = new Color[triangles.Length];
        var indices = new int[triangles.Length];
        for (var i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
            fragments[i] = vertices[triangles[i]];
            fragmentColors[i] = colors[triangles[i]];
        }

        mesh.MarkDynamic();
        mesh.Clear();
        mesh.vertices = fragments;
        mesh.colors = fragmentColors;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
    }

    public void OnDestroy()
    {
        Destroy(mesh);
    }
}