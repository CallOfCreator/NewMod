using System.Linq;
using AchievementsAPI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Buttons.Roles;
using NewMod.Cosmetics;
using NewMod.Components;
using NewMod.Debugging;
using NewMod.Options;
using NewMod.Options.Roles;
using NewMod.Patches.Compatibility;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.UI;
using NewMod.Utilities;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using ReactUI.Plugin;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace NewMod;

[BepInPlugin(Id, "NewMod", ModVersion)]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(CorsacPluginId, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LaunchpadReloadedId, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(AchievementsAPIPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
public class NewMod : BasePlugin, IMiraPlugin
{
    public const string Id = "com.callofcreator.newmod";
    public const string ModVersion = "1.3.0";
    
    public const string CorsacPluginId = "CorsacCosmetics";
    public const string LaunchpadReloadedId = "dev.xtracube.launchpad";
    
    public const string NewModBackendAPI = "";
    
    public static BasePlugin Instance;
    public static Minigame Minigame;
    public static Harmony Harmony { get; } = new(Id);
    public static ConfigEntry<bool> ShouldEnableBepInExConsole { get; set; }
    public static ConfigEntry<bool> ForceEnableAllSeasons { get; set; }
    public static bool ShouldReactUIDebug { get; set; } = false;

    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public string OptionsTitleText => "NewMod";

    public override void Load()
    {
        Instance = this;

        if (Application.platform != RuntimePlatform.Android)
        {
            if (ShouldReactUIDebug)
            {
                ReactUIBootstrap.Initialize();
                NewModDebugStyles.Register();
                NewModDebugPanel.Mount();
                ReactUIBehaviour.OnUpdate += NewModDebugPanel.Tick;
                AddComponent<DebugWindow>();
            }
            else
            {
                DebugMode.Initialize(this);
            }
        }

        ReactorCredits.Register("NewMod", ModVersion + " ALPHA Build 2", true, ReactorCredits.AlwaysShow);
        Harmony.PatchAll();

        NewModEventHandler.RegisterEventsLogs();
        
        ModCompatibility.Initialize();

        ShouldEnableBepInExConsole = Config.Bind("NewMod", "Console", true, "Whether to enable BepInEx Console for debugging");
        if (!ShouldEnableBepInExConsole.Value) ConsoleManager.DetachConsole();

        ForceEnableAllSeasons = Config.Bind("NewMod", "ForceEnableAllSeasons", false, "Force all seasons as started");

        var bundle = NewModAsset.Bundle;
        var assetNames = bundle.GetAllAssetNames();

        Message($"AssetBundle '{bundle.name}' contains {assetNames.Length} assets");

        foreach (var name in assetNames) Message($"{name}");
        Message($"Loaded Successfully NewMod v{ModVersion} ALPHA With MiraAPI Version : {MiraApiPlugin.Version}");
    }

    public static void InitializeKeyBinds()
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || MeetingHud.Instance || ExileController.Instance)
            return;

        if (Input.GetKeyDown(KeyCode.F2) && PlayerControl.LocalPlayer.Data.IsDead && OptionGroupSingleton<GeneralOption>.Instance.AllowCams)
        {
            var sys = Utils.FindSurveillanceConsole();
            var mainCam = Camera.main;
            if (mainCam == null) return;

            Minigame = Object.Instantiate(sys.MinigamePrefab, mainCam.transform, false);
            Minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            Minigame.Begin(null);
        }

        if (Input.GetKeyDown(KeyCode.F3) && PlayerControl.LocalPlayer.Data.Role is NecromancerRole)
        {
            var deadBodies = Helpers.GetNearestDeadBodies(PlayerControl.LocalPlayer.GetTruePosition(), 20f, Helpers.CreateFilter(Constants.NotShipMask));
            if (deadBodies.Count > 0)
            {
                var randomIndex = Random.Range(0, deadBodies.Count);
                var randomBodyPosition = deadBodies[randomIndex].transform.position;
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(randomBodyPosition);
            }
            else
            {
                CoroutinesHelper.CoNotify("<b><color=#FF0000>No dead bodies nearby to teleport to.</color></b>");
            }
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public class KeyboardJoystickUpdatePatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            return Application.platform != RuntimePlatform.Android;
        }

        public static void Postfix(KeyboardJoystick __instance)
        {
            InitializeKeyBinds();
        }
    }

    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
    public static class SetTaskTextPatch
    {
        public static void Postfix(TaskPanelBehaviour __instance, [HarmonyArgument(0)] string str)
        {
            if (__instance != HudManager.Instance.TaskPanel || !PlayerControl.LocalPlayer.Data.IsDead)
                return;

            __instance.taskText.text += "\n" + (OptionGroupSingleton<GeneralOption>.Instance.AllowCams ? "<color=blue>Remote cameras enabled: press F2</color>" : "<color=red>Remote cameras disabled</color>");
        }
    }
}