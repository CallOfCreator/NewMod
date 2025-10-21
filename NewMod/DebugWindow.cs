using UnityEngine;
using System;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using NewMod.Utilities;
using NewMod.Modifiers;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using Il2CppInterop.Runtime.Attributes;
using AmongUs.GameOptions;
using NewMod.Components.ScreenEffects;
using Reactor.Utilities;

namespace NewMod
{
    [RegisterInIl2Cpp]
    public class DebugWindow(nint ptr) : MonoBehaviour(ptr)
    {
        [HideFromIl2Cpp] public bool EnableDebugger { get; set; } = false;
        public float Zoom = 3f;
        public const float ZoomMin = 3f;
        public const float ZoomMax = 15f;
        public bool ScrollZoomWhileOpen = true;
        public static DebugWindow Instance;
        public int tab;

        //ShadowFlux
        public float noiseScale = 2f;
        public float fluxSpeed = 0.3f;
        public float edgeWidth = 0.25f;
        public float threshold = 0.55f;
        public float opacity = 0.75f;
        public float darkness = 0.8f;

        //DistorationWave
        public float amplitude = 0.05f;
        public float frequency = 5f;
        public float distoSpeed = 1f;
        public float radius = 0.25f;
        public float falloff = 1f;

        public void ApplyZoom(float size)
        {
            size = Mathf.Clamp(size, ZoomMin, ZoomMax);
            if (Camera.main) Camera.main.orthographicSize = size;
            foreach (var cam in Camera.allCameras) if (cam) cam.orthographicSize = size;
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            if (HudManager.Instance && HudManager.Instance.ShadowQuad)
                HudManager.Instance.ShadowQuad.gameObject.SetActive(size <= 3f);
        }

        public static bool AllowDebug() => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
        public readonly DragWindow DebuggingWindow = new(new Rect(10, 10, 0, 0), "NewMod Debug Window", () =>
        {
            bool allow = AllowDebug();

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(Instance.tab == 0, "Game", GUI.skin.button)) Instance.tab = 0;
            if (GUILayout.Toggle(Instance.tab == 1, "Effects", GUI.skin.button)) Instance.tab = 1;
            GUILayout.EndHorizontal();

            if (Instance.tab == 0)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Camera Zoom");
                var newZoom = GUILayout.HorizontalSlider(Instance.Zoom, ZoomMin, ZoomMax, GUILayout.Width(220f));
                if (!Mathf.Approximately(newZoom, Instance.Zoom) && allow)
                {
                    Instance.Zoom = newZoom;
                    Instance.ApplyZoom(Instance.Zoom);
                }
                GUILayout.Label($"Size: {Instance.Zoom:0.00}");
                GUILayout.EndVertical();

                GUILayout.Space(6);

                if (GUILayout.Button("Become Necromancer") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<NecromancerRole>(), false);
                if (GUILayout.Button("Become DoubleAgent") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<DoubleAgent>(), false);
                if (GUILayout.Button("Become EnergyThief") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<EnergyThief>(), false);
                if (GUILayout.Button("Become SpecialAgent") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<SpecialAgent>(), false);
                if (GUILayout.Button("Increase Uses by 3") && allow) foreach (var b in CustomButtonManager.Buttons) b.SetUses(3);
                if (GUILayout.Button("Random Vote") && allow && MeetingHud.Instance)
                {
                    var p = Utils.GetRandomPlayer(x => !x.Data.IsDead && !x.Data.Disconnected);
                    MeetingHud.Instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, p.PlayerId);
                }
                if (GUILayout.Button("End Meeting") && allow && MeetingHud.Instance) MeetingHud.Instance.Close();
                if (GUILayout.Button("Apply Glitch Effect") && allow) Camera.main.gameObject.AddComponent<GlitchEffect>();
                if (GUILayout.Button("Apply Earthquake Effect") && allow) Camera.main.gameObject.AddComponent<EarthquakeEffect>();
                if (GUILayout.Button("Apply PulseHue Effect") && allow) Camera.main.gameObject.AddComponent<SlowPulseHueEffect>();
                if (GUILayout.Button("Apply DistortionWave Effect") && allow) Camera.main.gameObject.AddComponent<DistorationWaveEffect>();
                if (GUILayout.Button("Apply ShadowFlux Effect") && allow) Camera.main.gameObject.AddComponent<ShadowFluxEffect>();
                if (GUILayout.Button("Reset Effects") && allow) Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 1f));
            }

            if (Instance.tab == 1)
            {
                var cam = Camera.main;

                if (cam.gameObject.TryGetComponent(out ShadowFluxEffect flux) && flux._mat)
                {
                    GUILayout.Label("ShadowFlux");
                    Instance.noiseScale = Slider("NoiseScale", Instance.noiseScale, 0f, 5f);
                    Instance.fluxSpeed = Slider("Speed", Instance.fluxSpeed, 0f, 3f);
                    Instance.edgeWidth = Slider("EdgeWidth", Instance.edgeWidth, 0f, 1f);
                    Instance.threshold = Slider("Threshold", Instance.threshold, 0f, 1f);
                    Instance.opacity = Slider("Opacity", Instance.opacity, 0f, 1f);
                    Instance.darkness = Slider("Darkness", Instance.darkness, 0f, 1f);

                    var m = flux._mat;
                    m.SetFloat("_NoiseScale", Instance.noiseScale);
                    m.SetFloat("_Speed", Instance.fluxSpeed);
                    m.SetFloat("_EdgeWidth", Instance.edgeWidth);
                    m.SetFloat("_Threshold", Instance.threshold);
                    m.SetFloat("_Opacity", Instance.opacity);
                    m.SetFloat("_Darkness", Instance.darkness);
                }

                GUILayout.Space(8);

                if (cam.gameObject.TryGetComponent(out DistorationWaveEffect disto) && disto._mat)
                {
                    GUILayout.Label("DistortionWave");
                    Instance.amplitude = Slider("Amplitude", Instance.amplitude, 0f, 1f);
                    Instance.frequency = Slider("Frequency", Instance.frequency, 0f, 10f);
                    Instance.distoSpeed = Slider("Speed", Instance.distoSpeed, 0f, 5f);
                    Instance.radius = Slider("Radius", Instance.radius, 0f, 1f);
                    Instance.falloff = Slider("Falloff", Instance.falloff, 0f, 5f);

                    var m = disto._mat;
                    m.SetFloat("_Amplitude", Instance.amplitude);
                    m.SetFloat("_Frequency", Instance.frequency);
                    m.SetFloat("_Speed", Instance.distoSpeed);
                    m.SetFloat("_Radius", Instance.radius);
                    m.SetFloat("_Falloff", Instance.falloff);
                }
            }
        });

        public static float Slider(string name, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(100f));
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(200f));
            GUILayout.EndHorizontal();
            return value;
        }

        public void OnGUI()
        {
            if (EnableDebugger) DebuggingWindow.OnGUI();
        }

        public void Start()
        {
            Instance = this;
            if (Camera.main) Zoom = Mathf.Clamp(Camera.main.orthographicSize, ZoomMin, ZoomMax);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3)) EnableDebugger = !EnableDebugger;
            if (EnableDebugger && ScrollZoomWhileOpen && AllowDebug())
            {
                float wheel = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(wheel) > 0.0001f)
                {
                    var factor = wheel > 0 ? 1f / 1.25f : 1.25f;
                    Zoom = Mathf.Clamp(Zoom * factor, ZoomMin, ZoomMax);
                    ApplyZoom(Zoom);
                }
            }
        }
    }
}
