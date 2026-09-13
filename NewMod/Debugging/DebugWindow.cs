using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using NewMod.Debugging.Tabs;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using UnityEngine;

namespace NewMod.Debugging;

[RegisterInIl2Cpp]
public class DebugWindow(nint ptr) : MonoBehaviour(ptr)
{
    public int tapCount;
    public float lastTapTime;
    public float touchStartTime;
    public Vector2 touchStart;
    public Vector2 lastTapPosition;
    public int _activeTabIdx;
    public readonly int _windowId = Window.NextWindowId();
    public Rect _windowRect = new(20, 20, 100, 100);

    public static DebugWindow Instance { get; private set; }

    [HideFromIl2Cpp] public bool Enabled { get; set; }

    [HideFromIl2Cpp] public string Title { get; set; } = "NewMod Debug Window";

    [HideFromIl2Cpp] public List<IDebugTab> Tabs { get; set; } = new();

    public const KeyCode ToggleKey = KeyCode.F8;
    public const float ZoomMin = 2.5f;
    public const float ZoomMax = 15f;
    public const float ZoomDefault = 3f;

    public bool CameraControlsOpen => Enabled && _activeTabIdx < Tabs.Count && Tabs[_activeTabIdx].GetType() == typeof(MatchTab);

    public float Zoom { get; private set; } = ZoomDefault;

    public void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        HandleTouchToggle();
        if (Input.GetKeyDown(ToggleKey))
            Enabled = !Enabled;

        if (CameraControlsOpen) return;

        var scroll = Input.mouseScrollDelta.y;

        if (scroll != 0f)
            ApplyZoom(Zoom - scroll * 0.5f);
    }

    public void HandleTouchToggle()
    {
        if (Application.platform != RuntimePlatform.Android || !DestroyableSingleton<TutorialManager>.InstanceExists)
        {
            tapCount = 0;
            return;
        }

        if (Time.unscaledTime - lastTapTime > 0.4f) tapCount = 0;
        if (Input.touchCount != 1)
        {
            if (Input.touchCount > 1) tapCount = 0;
            return;
        }

        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            touchStart = touch.position;
            touchStartTime = Time.unscaledTime;
        }

        if (touch.phase != TouchPhase.Ended) return;
        if (Time.unscaledTime - touchStartTime > 0.3f || Vector2.Distance(touch.position, touchStart) > 40f)
        {
            tapCount = 0;
            return;
        }

        if (tapCount > 0 && Vector2.Distance(touch.position, lastTapPosition) > 80f) tapCount = 0;
        lastTapTime = Time.unscaledTime;
        lastTapPosition = touch.position;
        if (++tapCount < 3) return;
        tapCount = 0;
        Enabled = !Enabled;
    }

    public void OnGUI()
    {
        if (!Enabled || !Tabs.Any()) return;

        if (Event.current.type == EventType.Layout) _windowRect.height = _windowRect.width = 20;

        _windowRect = GUILayout.Window(_windowId, _windowRect, (Action<int>)(_ => DrawWindow()), Title);

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) Input.ResetInputAxes();
    }

    public void DrawWindow()
    {
        if (Tabs.Count == 0) return;

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
        GUILayout.BeginHorizontal(GUIStyle.none);

        for (var i = 0; i < Tabs.Count; i++)
        {
            var currentTab = Tabs[i];

            if (!currentTab.ShouldShow)
            {
                if (_activeTabIdx == i) _activeTabIdx = (_activeTabIdx + 1) % Tabs.Count;

                continue;
            }

            if (GUILayout.Toggle(_activeTabIdx == i, currentTab.Name, new GUIStyle(GUI.skin.button))) _activeTabIdx = i;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        Tabs[_activeTabIdx].BuildGUI();
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