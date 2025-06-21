using AmongUs.GameOptions;
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
using NewMod.Buttons.EnergyThief;
using NewMod.Buttons.SpecialAgent;
using NewMod.Buttons.Visionary;
using NewMod.Buttons.Prankster;
using NewMod.Buttons.Necromancer;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using UnityEngine;
using UnityEngine.Events;
using Il2CppInterop.Runtime.Attributes;

namespace NewMod
{
   [RegisterInIl2Cpp]
   public class DebugWindow(nint ptr) : MonoBehaviour(ptr)
   {
      [HideFromIl2Cpp]
      public bool EnableDebugger { get; set; } = false;
      public readonly DragWindow DebuggingWindow = new(new Rect(10, 10, 0, 0), "NewMod Debug Window", () =>
      {
         bool isFreeplay = AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;

         if (GUILayout.Button("Become Explosive Modifier"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.RpcAddModifier<ExplosiveModifier>();
         }
         if (GUILayout.Button("Remove Explosive Modifier"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.RpcRemoveModifier<ExplosiveModifier>();
         }
         if (GUILayout.Button("Disable Collider"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.Collider.enabled = false;
         }
         if (GUILayout.Button("Enable Collider"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.Collider.enabled = true;
         }
         if (GUILayout.Button("Become Necromancer"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<NecromancerRole>(), false);
         }
         if (GUILayout.Button("Become DoubleAgent"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<DoubleAgent>(), false);
         }
         if (GUILayout.Button("Become EnergyThief"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<EnergyThief>(), false);
         }
         if (GUILayout.Button("Become SpecialAgent"))
         {
            if (!isFreeplay) return;
            PlayerControl.LocalPlayer.RpcSetRole((RoleTypes)RoleId.Get<SpecialAgent>(), false);
         }
         if (GUILayout.Button("Force Start Game"))
         {
            if (GameOptionsManager.Instance.CurrentGameOptions.NumImpostors is 1) return;
            AmongUsClient.Instance.StartGame();
         }
         if (GUILayout.Button("Increases Uses by 3"))
         {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role is NecromancerRole)
            {
               CustomButtonSingleton<ReviveButton>.Instance.IncreaseUses(3);
            }
            else if (player.Data.Role is EnergyThief)
            {
               CustomButtonSingleton<DrainButton>.Instance.IncreaseUses(3);
            }
            else if (player.Data.Role is SpecialAgent)
            {
               CustomButtonSingleton<AssignButton>.Instance.IncreaseUses(3);
            }
            else if (player.Data.Role is Prankster)
            {
               CustomButtonSingleton<FakeBodyButton>.Instance.IncreaseUses(3);
            }
            else
            {
               CustomButtonSingleton<CaptureButton>.Instance.IncreaseUses(3);
               CustomButtonSingleton<ShowScreenshotButton>.Instance.IncreaseUses(3);
            }
         }
         if (GUILayout.Button("Randomly Cast a Vote"))
         {
            if (!MeetingHud.Instance) return;
            var randPlayer = Utils.GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);
            MeetingHud.Instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, randPlayer.PlayerId);
         }
         GUILayout.Space(4);

         GUILayout.Label("Overload button tests", GUI.skin.box);

         if (GUILayout.Button("Test Overload Finale"))
         {
            OverloadRole.UnlockFinalAbility();
         }
         if (GUILayout.Button("Test Absorb"))
         {
            var prey = Utils.GetRandomPlayer(p =>
                  !p.Data.IsDead &&
                  !p.Data.Disconnected &&
                  p.PlayerId != PlayerControl.LocalPlayer.PlayerId);
            if (prey != null)
            {
               if (prey.Data.Role is ICustomRole customRole)
               {
                  Debug.Log("[Overload] Absorbing ability from custom role...");
               }
               else if (prey.Data.Role.Ability != null)
               {
                  var btn = Instantiate(
                        HudManager.Instance.AbilityButton,
                        HudManager.Instance.AbilityButton.transform.parent);
                  btn.SetFromSettings(prey.Data.Role.Ability);
                  var pb = btn.GetComponent<PassiveButton>();
                  pb.OnClick.RemoveAllListeners();
                  pb.OnClick.AddListener((UnityAction)prey.Data.Role.UseAbility);
               }
            }
         }
      });
      public void OnGUI()
      {
         if (EnableDebugger) DebuggingWindow.OnGUI();
      }
      public void Update()
      {
         if (Input.GetKey(KeyCode.F3))
         {
            EnableDebugger = !EnableDebugger;
         }
      }
   }
}
