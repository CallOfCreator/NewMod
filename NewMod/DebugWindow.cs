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

        //CrismonVortex
        public float CrismonOpacity = 0.78f;
        public float tiling = 1.15f;
        public float detailTiling = 2.35f;
        public float detailStrength = 0.45f;
        public float brightness = 1.2f;
        public float contrast = 1.65f;
        public Vector2 scrollDir = new Vector2(1f, 0f);
        public float scrollSpeed = 0.12f;
        public float distortStrength = 0.055f;
        public float distortSpeed = 0.35f;
        public float distortScale = 1.85f;
        public float swirlStrength = 0.95f;
        public float swirlSpeed = 0.55f;
        public float detailSwirlStrength = 1.35f;
        public float detailSwirlSpeed = -0.75f;
        public Vector2 center = new Vector2(0.5f, 0.5f);
        public float crismonRadius = 0.95f;
        public float edgeSoftness = 0.26f;
        public float coreRadius = 0.28f;
        public float coreSoftness = 0.16f;
        public float coreIntensity = 1.25f;
        public float pulseStrength = 0.16f;
        public float pulseSpeed = 0.9f;
        public float frontWidth = 0.72f;
        public float frontSoftness = 0.2f;
        public float frontTravel = 0.48f;
        public float leadingEdgeWidth = 0.055f;
        public float leadingEdgeStrength = 1.15f;
        public bool useCircular = false;

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
                if (GUILayout.Button("Apply CrismonVortex Effect") && allow) Camera.main.gameObject.AddComponent<CrismonVortexEffect>();
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
                if (cam.gameObject.TryGetComponent(out CrismonVortexEffect vortex) && vortex._mat)
                {
                    GUILayout.Label("CrismonVortex");
                    Instance.CrismonOpacity = Slider("Opacity", Instance.CrismonOpacity, 0f, 1f);
                    Instance.tiling = Slider("Tiling", Instance.tiling, 0.1f, 4f);
                    Instance.detailTiling = Slider("DetailTiling", Instance.detailTiling, 0.1f, 6f);
                    Instance.detailStrength = Slider("DetailStrength", Instance.detailStrength, 0f, 1f);
                    Instance.brightness = Slider("Brightness", Instance.brightness, 0f, 3f);
                    Instance.contrast = Slider("Contrast", Instance.contrast, 0.1f, 5f);
                    Instance.scrollSpeed = Slider("ScrollSpeed", Instance.scrollSpeed, 0f, 1f);
                    Instance.distortStrength = Slider("DistortStrength", Instance.distortStrength, 0f, 0.2f);
                    Instance.distortSpeed = Slider("DistortSpeed", Instance.distortSpeed, 0f, 2f);
                    Instance.distortScale = Slider("DistortScale", Instance.distortScale, 0.1f, 5f);
                    Instance.swirlStrength = Slider("SwirlStrength", Instance.swirlStrength, 0f, 3f);
                    Instance.swirlSpeed = Slider("SwirlSpeed", Instance.swirlSpeed, -2f, 2f);
                    Instance.detailSwirlStrength = Slider("DetailSwirlStrength", Instance.detailSwirlStrength, 0f, 3f);
                    Instance.detailSwirlSpeed = Slider("DetailSwirlSpeed", Instance.detailSwirlSpeed, -2f, 2f);
                    Instance.crismonRadius = Slider("Radius", Instance.crismonRadius, 0f, 2f);
                    Instance.edgeSoftness = Slider("EdgeSoftness", Instance.edgeSoftness, 0f, 1f);
                    Instance.coreRadius = Slider("CoreRadius", Instance.coreRadius, 0f, 1f);
                    Instance.coreSoftness = Slider("CoreSoftness", Instance.coreSoftness, 0.001f, 1f);
                    Instance.coreIntensity = Slider("CoreIntensity", Instance.coreIntensity, 0f, 3f);
                    Instance.pulseStrength = Slider("PulseStrength", Instance.pulseStrength, 0f, 0.5f);
                    Instance.pulseSpeed = Slider("PulseSpeed", Instance.pulseSpeed, 0f, 5f);
                    Instance.frontWidth = Slider("FrontWidth", Instance.frontWidth, 0.05f, 2f);
                    Instance.frontSoftness = Slider("FrontSoftness", Instance.frontSoftness, 0.001f, 1f);
                    Instance.frontTravel = Slider("FrontTravel", Instance.frontTravel, 0f, 2f);
                    Instance.leadingEdgeWidth = Slider("LeadEdgeWidth", Instance.leadingEdgeWidth, 0.001f, 0.5f);
                    Instance.leadingEdgeStrength = Slider("LeadEdgeStrength", Instance.leadingEdgeStrength, 0f, 3f);

                    Instance.useCircular = GUILayout.Toggle(Instance.useCircular, "Use Circular");

                    var m = vortex._mat;
                    m.SetFloat("_Opacity", Instance.CrismonOpacity);
                    m.SetFloat("_Tiling", Instance.tiling);
                    m.SetFloat("_DetailTiling", Instance.detailTiling);
                    m.SetFloat("_DetailStrength", Instance.detailStrength);
                    m.SetFloat("_Brightness", Instance.brightness);
                    m.SetFloat("_Contrast", Instance.contrast);
                    m.SetVector("_ScrollDir", new Vector4(Instance.scrollDir.x, Instance.scrollDir.y, 0f, 0f));
                    m.SetFloat("_ScrollSpeed", Instance.scrollSpeed);
                    m.SetFloat("_DistortStrength", Instance.distortStrength);
                    m.SetFloat("_DistortSpeed", Instance.distortSpeed);
                    m.SetFloat("_DistortScale", Instance.distortScale);
                    m.SetFloat("_SwirlStrength", Instance.swirlStrength);
                    m.SetFloat("_SwirlSpeed", Instance.swirlSpeed);
                    m.SetFloat("_DetailSwirlStrength", Instance.detailSwirlStrength);
                    m.SetFloat("_DetailSwirlSpeed", Instance.detailSwirlSpeed);
                    m.SetVector("_Center", new Vector4(Instance.center.x, Instance.center.y, 0f, 0f));
                    m.SetFloat("_Radius", Instance.crismonRadius);
                    m.SetFloat("_EdgeSoftness", Instance.edgeSoftness);
                    m.SetFloat("_CoreRadius", Instance.coreRadius);
                    m.SetFloat("_CoreSoftness", Instance.coreSoftness);
                    m.SetFloat("_CoreIntensity", Instance.coreIntensity);
                    m.SetFloat("_PulseStrength", Instance.pulseStrength);
                    m.SetFloat("_PulseSpeed", Instance.pulseSpeed);
                    m.SetFloat("_FrontWidth", Instance.frontWidth);
                    m.SetFloat("_FrontSoftness", Instance.frontSoftness);
                    m.SetFloat("_FrontTravel", Instance.frontTravel);
                    m.SetFloat("_LeadingEdgeWidth", Instance.leadingEdgeWidth);
                    m.SetFloat("_LeadingEdgeStrength", Instance.leadingEdgeStrength);
                    m.SetFloat("_UseCircular", Instance.useCircular ? 1f : 0f);
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
