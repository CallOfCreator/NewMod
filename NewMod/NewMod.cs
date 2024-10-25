using System.Linq;
using MiraAPI;
using MiraAPI.Networking;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using Reactor;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using NewMod.Options;
using NewMod.Utilities;
using HarmonyLib;

namespace NewMod;

[BepInPlugin(Id, "NewMod", ModVersion)]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(Reactor.Networking.ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
public partial class NewMod : BasePlugin, IMiraPlugin
{
    public const string Id = "com.callofcreator.newmod";
    public const string ModVersion = "1.0.0";
    public Harmony Harmony { get; } = new Harmony(Id);
    public static BasePlugin Instance;
    public static Minigame minigame;
    public const string SupportedAmongUsVersion = "2024.9.4";
    public static ConfigEntry<bool> ShouldEnableBepInExConsole {get; set;}
    public ConfigFile GetConfigFile() => Config;
    public string OptionsTitleText => "NewMod"; 
    public override void Load()
    {
        Instance = this;
        AddComponent<DebugWindow>();
        ReactorCredits.Register<NewMod>(ReactorCredits.AlwaysShow);
        Harmony.PatchAll();
        CheckVersionCompatibility();
        ShouldEnableBepInExConsole = Config.Bind("NewMod", "Console", false, "Whether to enable BepInEx Console for debugging");
        Instance.Log.LogMessage($"Loaded Successfully NewMod With MiraAPI Version : {MiraApiPlugin.Version} with ID : {MiraApiPlugin.Id}");
        if (!ShouldEnableBepInExConsole.Value) ConsoleManager.DetachConsole(); 
    }
    public static void CheckVersionCompatibility()
    {
       if (Application.version != SupportedAmongUsVersion)
       {
          Instance.Log.LogError($"Detected unsupported Among Us version. Current version: {Application.version}, Supported version: {SupportedAmongUsVersion}");
       }
    }

   [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public class KeyboardJoystickUpdatePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            InitializeKeyBinds();
        }
    }
    public static void InitializeKeyBinds()
    {
         if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

           if (Input.GetKeyDown(KeyCode.F2) && PlayerControl.LocalPlayer.Data.Role.Role is AmongUs.GameOptions.RoleTypes.Crewmate && OptionGroupSingleton<GeneralOption>.Instance.CanOpenCams)
           {
              var cam = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.name.Contains("Surv"));
              if (Camera.main is not null || cam != null)
              {
                 minigame = Object.Instantiate(cam.MinigamePrefab, Camera.main.transform, false);
                 minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
                 minigame.Begin(null);
                 Instance.Log.LogDebug("Open Cams");
              }   
           }
           if (Input.GetKeyDown(KeyCode.F4) && PlayerControl.LocalPlayer.Data.Role is not CrewmateRole && OptionGroupSingleton<GeneralOption>.Instance.EnableTeleportation)
           {
              var rand = Utils.GetRandomPlayer();
              if (rand != null)
              {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(rand.GetTruePosition());
              }
           }
    }
     
    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcCustomMurder))]
    public static class CustomMurderPatch
    {
     public static void Postfix(PlayerControl target,
        bool didSucceed,
        bool resetKillTimer,
        bool createDeadBody,
        bool teleportMurderer,
        bool showKillAnim,
        bool playKillSound)
      {
         Utils.RecordOnKill(PlayerControl.LocalPlayer, target);
      }
   }
   
    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
    public static class SetTaskTextPatch
    {
      public static void Postfix(TaskPanelBehaviour __instance, [HarmonyArgument(0)] string str)
      {
         if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started && PlayerControl.LocalPlayer.Data.Role.Role is AmongUs.GameOptions.RoleTypes.Crewmate)
         {
            __instance.taskText.text += "\n" + (OptionGroupSingleton<GeneralOption>.Instance.CanOpenCams ? "<color=blue>Press F2 For Open Cams</color>" : "<color=red>You cannot open cams because the host has disabled this setting</color>");
         }
      }
    }
}