using HarmonyLib;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class NMOverlay(nint ptr) : MonoBehaviour(ptr)
{
    public TextMeshPro Label;
    public float StartedAt;
    public int StartedFrame;
    public float FramesPerSecond;
    public float WarningUntil;
    public bool Has;

    public void Awake()
    {
        var labelObject = new GameObject("NewModOverlay") { layer = LayerMask.NameToLayer("UI") };
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 2.22f, -20f);

        Label = labelObject.AddComponent<TextMeshPro>();
        Label.font = HudManager.Instance.TaskPanel.taskText.font;
        Label.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        Label.fontSize = HudManager.Instance.TaskPanel.taskText.fontSize;
        Label.alignment = TextAlignmentOptions.Top;
        Label.enableWordWrapping = false;
        Label.rectTransform.sizeDelta = new Vector2(8f, 0.7f);
        StartedAt = Time.unscaledTime;
        StartedFrame = Time.frameCount;
    }

    public void Update()
    {
        var client = AmongUsClient.Instance;
        var visible = client && PlayerControl.LocalPlayer;
        Label.gameObject.SetActive(visible);
        if (!visible)
        {
            StartedAt = Time.unscaledTime;
            StartedFrame = Time.frameCount;
            Has = false;
            WarningUntil = 0f;
            return;
        }

        var now = Time.unscaledTime;
        var elapsed = now - StartedAt;
        if (elapsed is >= 0.5f and > 0f)
        {
            var fps = (Time.frameCount - StartedFrame) / elapsed;
            if (Has && fps < FramesPerSecond * (1f - 0.25f))
                WarningUntil = now + 1.5f;
            FramesPerSecond = fps;
            Has = true;
            StartedAt = now;
            StartedFrame = Time.frameCount;
        }

        var fpsColor = FramesPerSecond < 30f ? "#FF6060" : FramesPerSecond < 60f || now < WarningUntil ? "#FFD45C" : "#75E6A5";
        Label.text = Has ? $"FPS: <color={fpsColor}><b>{Mathf.RoundToInt(FramesPerSecond)}</b></color>" : "FPS: ...";

        if (!client.IsGameStarted && client.NetworkMode != NetworkModes.FreePlay)
        {
            var local = client.NetworkMode == NetworkModes.LocalGame;
            var region = local ? "Local" : ServerManager.Instance.CurrentRegion.Name;
            var regionColor = local || client.Ping <= 60f ? "#75E6A5" : client.Ping <= 200f ? "#FFD45C" : "#FF6060";
            Label.text += $"  |  REGION: <color={regionColor}><b>{region}</b></color>";
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class NMOverlayHudPatch
{
    [HarmonyPostfix]
    public static void Postfix(HudManager __instance)
    {
        if (!__instance.GetComponent<NMOverlay>())
            __instance.gameObject.AddComponent<NMOverlay>();
    }
}