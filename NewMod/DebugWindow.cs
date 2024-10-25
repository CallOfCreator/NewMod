using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Buttons;
using NewMod.Modifiers;
using NewMod.Roles;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using UnityEngine;

namespace NewMod;

[RegisterInIl2Cpp]
public class DebugWindow(nint ptr) : MonoBehaviour(ptr)
{
    [HideFromIl2Cpp]
    public bool EnableDebugger {get; set;} = false; 
    public readonly DragWindow DebuggingWindow = new(new Rect(10, 10, 0, 0), "NewMod Debug Window", () =>
    {
         bool isFreeplay  = AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
         
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
              CustomButtonSingleton<NecromancerButton>.Instance.IncreaseUses(3);
            }
            else if (player.Data.Role is EnergyThief)
            {
               CustomButtonSingleton<DrainButton>.Instance.IncreaseUses(3);
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
       EnableDebugger = !EnableDebugger;
    }
}