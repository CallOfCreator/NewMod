using UnityEngine;
using System.Linq;
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

      public void ApplyZoom(float size)
      {
         size = Mathf.Clamp(size, ZoomMin, ZoomMax);
         if (Camera.main) Camera.main.orthographicSize = size;
         foreach (var cam in Camera.allCameras) if (cam) cam.orthographicSize = size;
         ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
         if (HudManager.Instance && HudManager.Instance.ShadowQuad)
         {
            bool zoomingOut = size > 3f;
            HudManager.Instance.ShadowQuad.gameObject.SetActive(!zoomingOut);
         }
      }

      private static bool AllowDebug()
      {
         return AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
      }

      public readonly DragWindow DebuggingWindow = new(new Rect(10, 10, 0, 0), "NewMod Debug Window", () =>
      {
         bool allow = AllowDebug();

         GUILayout.BeginVertical(GUI.skin.box);
         GUILayout.Label("Camera Zoom");
         var newZoom = GUILayout.HorizontalSlider(Instance.Zoom, ZoomMin, ZoomMax, GUILayout.Width(220f));
         GUILayout.BeginHorizontal();
         if (GUILayout.Button("-", GUILayout.Width(28f)) && allow) newZoom = Mathf.Clamp(Instance.Zoom / 1.25f, ZoomMin, ZoomMax);
         if (GUILayout.Button("+", GUILayout.Width(28f)) && allow) newZoom = Mathf.Clamp(Instance.Zoom * 1.25f, ZoomMin, ZoomMax);
         if (GUILayout.Button("Reset", GUILayout.Width(64f)) && allow) newZoom = 3f;
         Instance.ScrollZoomWhileOpen = GUILayout.Toggle(Instance.ScrollZoomWhileOpen, "Scroll-wheel zoom");
         GUILayout.EndHorizontal();
         if (!Mathf.Approximately(newZoom, Instance.Zoom) && allow)
         {
            Instance.Zoom = newZoom;
            Instance.ApplyZoom(Instance.Zoom);
         }
         GUILayout.Label($"Size: {Instance.Zoom:0.00}");
         GUILayout.EndVertical();

         GUILayout.Space(6);

         if (GUILayout.Button("Become Explosive Modifier") && allow) PlayerControl.LocalPlayer.RpcAddModifier<ExplosiveModifier>();
         if (GUILayout.Button("Remove Explosive Modifier") && allow) PlayerControl.LocalPlayer.RpcRemoveModifier<ExplosiveModifier>();
         if (GUILayout.Button("Become Necromancer") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<NecromancerRole>(), false);
         if (GUILayout.Button("Become DoubleAgent") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<DoubleAgent>(), false);
         if (GUILayout.Button("Become EnergyThief") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<EnergyThief>(), false);
         if (GUILayout.Button("Become SpecialAgent") && allow) PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<SpecialAgent>(), false);
         if (GUILayout.Button("Force Start Game") && allow) if (GameOptionsManager.Instance.CurrentGameOptions.NumImpostors is not 1) AmongUsClient.Instance.StartGame();
         if (GUILayout.Button("Increases Uses by 3") && allow) foreach (var button in CustomButtonManager.Buttons) button.SetUses(3);
         if (GUILayout.Button("Randomly Cast a Vote") && allow && MeetingHud.Instance)
         {
            var randPlayer = Utils.GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);
            MeetingHud.Instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, randPlayer.PlayerId);
         }
         if (GUILayout.Button("End Meeting") && allow && MeetingHud.Instance)
         {
            MeetingHud.Instance.Close();
         }
         if (GUILayout.Button("Apply Glitch Effect to Main Camera") && allow)
         {
            Camera.main.gameObject.AddComponent<GlitchEffect>();
         }
         if (GUILayout.Button("Apply Earthquake Effect to Main Camera") && allow)
         {
            Camera.main.gameObject.AddComponent<EarthquakeEffect>();
         }
         if (GUILayout.Button("Apply Slow Hue Pulse Effect to Main Camera") && allow)
         {
            Camera.main.gameObject.AddComponent<SlowPulseHueEffect>();
         }
         if (GUILayout.Button("Reset Camera Effects") && allow)
         {
            Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 1f));
         }
         if (GUILayout.Button("Show Toast") && LobbyBehaviour.Instance)
         {
            var toast = Toast.CreateToast();
            toast.ShowToast(string.Empty, "NewMod v1.2.6", Color.red, 5f);
         }
         /*if (GUILayout.Button("Spawn General NPC") && allow)
         {
            var npc = new GameObject("GeneralNPC").AddComponent<GeneralNPC>();
            npc.Initialize(PlayerControl.LocalPlayer);
         }*/
      });

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
