using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using MiraAPI;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using Reactor;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using HarmonyLib;
using NewMod.Options;
using NewMod.Utilities;
using NewMod.Roles.ImpostorRoles;
using MiraAPI.Events.Vanilla.Gameplay;
using NewMod.Roles.NeutralRoles;
using MiraAPI.Roles;
using System;
using MiraAPI.Hud;
using UnityEngine.Events;
using NewMod.Options.Roles.OverloadOptions;
using MiraAPI.Events;
using NewMod.Patches.Compatibility;
using NewMod.Buttons.Overload;

namespace NewMod;

[BepInPlugin(Id, "NewMod", ModVersion)]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(ModCompatibility.LaunchpadReloaded_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(Reactor.Networking.ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
public partial class NewMod : BasePlugin, IMiraPlugin
{
   public const string Id = "com.callofcreator.newmod";
   public const string ModVersion = "1.2.4";
   public Harmony Harmony { get; } = new Harmony(Id);
   public static BasePlugin Instance;
   public static Minigame minigame;
   public const string SupportedAmongUsVersion = "2025.6.10";
   public static ConfigEntry<bool> ShouldEnableBepInExConsole { get; set; }
   public ConfigFile GetConfigFile() => Config;
   public string OptionsTitleText => "NewMod";
   public override void Load()
   {
      Instance = this;
      AddComponent<DebugWindow>();
      ReactorCredits.Register<NewMod>(ReactorCredits.AlwaysShow);
      Harmony.PatchAll();
      CheckVersionCompatibility();
      NewModEventHandler.RegisterEventsLogs();

      if (ModCompatibility.IsLaunchpadLoaded())
      {
         Harmony.PatchAll(typeof(LaunchpadCompatibility));
         Harmony.PatchAll(typeof(LaunchpadHackTextPatch));
      }
      ShouldEnableBepInExConsole = Config.Bind("NewMod", "Console", false, "Whether to enable BepInEx Console for debugging");
      if (!ShouldEnableBepInExConsole.Value) ConsoleManager.DetachConsole();
      Instance.Log.LogMessage($"Loaded Successfully NewMod v{ModVersion} With MiraAPI Version : {MiraApiPlugin.Version} with ID : {MiraApiPlugin.Id}");
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
         }
      }
      if (Input.GetKeyDown(KeyCode.F3) && PlayerControl.LocalPlayer.Data.Role is NecromancerRole && OptionGroupSingleton<GeneralOption>.Instance.EnableTeleportation)
      {
         var deadBodies = Helpers.GetNearestDeadBodies(PlayerControl.LocalPlayer.GetTruePosition(), 20f, Helpers.CreateFilter(Constants.NotShipMask));
         if (deadBodies != null && deadBodies.Count > 0)
         {
            var randomIndex = UnityEngine.Random.Range(0, deadBodies.Count);
            var randomBodyPosition = deadBodies[randomIndex].transform.position;
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(randomBodyPosition);
         }
         else
         {
            CoroutinesHelper.CoNotify("<b><color=#FF0000>No dead bodies nearby to teleport to.</color></b>");
         }
      }
   }

   [RegisterEvent]
   public static void OnBeforeMurder(BeforeMurderEvent evt)
   {
      if (evt.Target != OverloadRole.chosenPrey) return;

      //TODO: Use the newest MiraAPI roles for button mapping
      if (evt.Target.Data.Role is ICustomRole customRole && Utils.RoleToButtonsMap.TryGetValue(customRole.GetType(), out var buttonsType))
      {
         OverloadRole.CachedButtons = CustomButtonManager.Buttons
               .Where(b => buttonsType.Contains(b.GetType()))
               .ToList();
      }
   }
   [RegisterEvent]
   public static void OnAfterMurder(AfterMurderEvent evt)
   {
      var source = evt.Source;
      var target = evt.Target;
      Utils.RecordOnKill(source, target);

      if (target != OverloadRole.chosenPrey) return;

      foreach (var pc in PlayerControl.AllPlayerControls.ToArray().Where(p => p.AmOwner && p.Data.Role is OverloadRole))
      {
         if (target.Data.Role is ICustomRole customRole && Utils.RoleToButtonsMap.TryGetValue(customRole.GetType(), out var buttonsType))
         {
            foreach (var button in OverloadRole.CachedButtons)
            {
               CustomButtonSingleton<OverloadButton>.Instance.Absorb(button);
               Debug.Log($"[Overload] Successfully absorbed ability: {button.Name}");
            }
         }
         else if (target.Data.Role is not ICustomRole)
         {
            var btn = Object.Instantiate(
                HudManager.Instance.AbilityButton,
                HudManager.Instance.AbilityButton.transform.parent);
            btn.SetFromSettings(target.Data.Role.Ability);
            var pb = btn.GetComponent<PassiveButton>();
            pb.OnClick.RemoveAllListeners();
            pb.OnClick.AddListener((UnityAction)target.Data.Role.UseAbility);
         }
      }
      OverloadRole.CachedButtons.Clear();
      OverloadRole.AbsorbedAbilityCount++;
      OverloadRole.chosenPrey = null;
      Coroutines.Start(CoroutinesHelper.CoNotify($"<color=green>Charge {OverloadRole.AbsorbedAbilityCount}/{OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge}</color>"));

      if (OverloadRole.AbsorbedAbilityCount >= OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge)
      {
         Coroutines.Start(CoroutinesHelper.CoNotify("<color=#00FF7F>Objective completed: Final Ability unlocked!</color>"));
      }
      else
      {
         Coroutines.Start(OverloadRole.CoShowMenu(1f));
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
