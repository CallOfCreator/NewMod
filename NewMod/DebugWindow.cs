using Il2CppInterop.Runtime.Attributes;
using NewMod.UI;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod;

[RegisterInIl2Cpp]
public sealed class DebugWindow(nint ptr) : MonoBehaviour(ptr)
{
    public const float ZoomMin = 2.5f;
    public const float ZoomMax = 15f;
    public const KeyCode ToggleKey = KeyCode.F8;

    public static DebugWindow Instance { get; private set; }

    public float Zoom { get; private set; } = 3f;

    public void Start()
    {
        Instance = this;

        if (Camera.main)
            Zoom = Mathf.Clamp(Camera.main.orthographicSize, ZoomMin, ZoomMax);
    }

    public void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
            NewModDebugPanel.SetOpen(!NewModDebugPanel.IsOpen);

        if (!NewModDebugPanel.CameraControlsOpen)
            return;

        var scroll = Input.mouseScrollDelta.y;

        if (scroll != 0f)
            ApplyZoom(Zoom - scroll * 0.5f);
    }

    [HideFromIl2Cpp]
    public void ApplyZoom(float size)
    {
        Zoom = Mathf.Clamp(size, ZoomMin, ZoomMax);

        foreach (var camera in Camera.allCameras)
            camera.orthographicSize = Zoom;

        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);

        HudManager.Instance.ShadowQuad.gameObject.SetActive(Zoom <= 3f);
    }
}