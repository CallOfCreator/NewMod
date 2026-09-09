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
[BepInDependency(ModCompatibility.LaunchpadReloaded_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(AchievementsAPIPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
public class NewMod : BasePlugin, IMiraPlugin
{
    public const string Id = "com.callofcreator.newmod";
    public const string CorsacPluginId = "CorsacCosmetics";
    public const string ModVersion = "1.3.0";
    public const string NewModBackendAPI = "";
    public static BasePlugin Instance;
    public static Minigame minigame;
    public Harmony Harmony { get; } = new(Id);
    public static ConfigEntry<bool> ShouldEnableBepInExConsole { get; set; }
    public static ConfigEntry<bool> ForceEnableAllSeasons { get; set; }
    public static bool CorsacCosmeticsEnabled => Application.platform != RuntimePlatform.Android && IL2CPPChainloader.Instance.Plugins.ContainsKey(CorsacPluginId);

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
            ReactUIBootstrap.Initialize();
            NewModDebugStyles.Register();
            NewModDebugPanel.Mount();
            ReactUIBehaviour.OnUpdate += NewModDebugPanel.Tick;
            AddComponent<DebugWindow>();
        }

        ReactorCredits.Register("NewMod", ModVersion + " ALPHA Build 2", true, ReactorCredits.AlwaysShow);
        Harmony.PatchAll();

        if (CorsacCosmeticsEnabled)
            CorsacCosmeticsIntegration.Initialize(Harmony);

        NewModEventHandler.RegisterEventsLogs();

        if (ModCompatibility.IsLaunchpadLoaded())
        {
            Harmony.PatchAll(typeof(LaunchpadCompatibility));
            Harmony.PatchAll(typeof(LaunchpadHackTextPatch));
        }

        ShouldEnableBepInExConsole = Config.Bind("NewMod", "Console", true, "Whether to enable BepInEx Console for debugging");
        if (!ShouldEnableBepInExConsole.Value) ConsoleManager.DetachConsole();

        ForceEnableAllSeasons = Config.Bind("NewMod", "ForceEnableAllSeasons", false, "Force all seasons as started");

        var bundle = NewModAsset.Bundle;
        var assetNames = bundle.GetAllAssetNames();

        Instance.Log.LogMessage($"AssetBundle '{bundle.name}' contains {assetNames.Length} assets");

        foreach (var name in assetNames) Instance.Log.LogMessage($"{name}");
        Instance.Log.LogMessage($"Loaded Successfully NewMod v{ModVersion} ALPHA With MiraAPI Version : {MiraApiPlugin.Version}");
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

            var minigame = Object.Instantiate(sys.MinigamePrefab, mainCam.transform, false);
            minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            minigame.Begin(null);
        }

        if (Input.GetKeyDown(KeyCode.F3) && PlayerControl.LocalPlayer.Data.Role is NecromancerRole)
        {
            var deadBodies = Helpers.GetNearestDeadBodies(PlayerControl.LocalPlayer.GetTruePosition(), 20f, Helpers.CreateFilter(Constants.NotShipMask));
            if (deadBodies != null && deadBodies.Count > 0)
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

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (!evt.Source.AmOwner || evt.Source.Data.Role is not OverloadRole || evt.Target != OverloadRole.chosenPrey) return;

        //TODO: Use the newest MiraAPI roles for button mapping
        if (evt.Target.Data.Role is ICustomRole customRole && Utils.RoleToButtonsMap.TryGetValue(customRole.GetType(), out var buttonsType))
        {
            OverloadRole.CachedButtons = [.. CustomButtonManager.Buttons.Where(b => buttonsType.Contains(b.GetType()))];
            Instance.Log.LogMessage($"CachedButton: {buttonsType.GetType().Name}");
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var source = evt.Source;
        var target = evt.Target;
        Utils.RecordOnKill(source, target);

        if (!source.AmOwner || source.Data.Role is not OverloadRole || target != OverloadRole.chosenPrey) return;

        foreach (var pc in PlayerControl.AllPlayerControls.ToArray().Where(p => p.AmOwner && p.Data.Role is OverloadRole))
            if (target.Data.Role is ICustomRole customRole)
            {
                foreach (var button in OverloadRole.CachedButtons)
                {
                    CustomButtonSingleton<OverloadButton>.Instance.Absorb(button);
                    Debug.Log($"[Overload] Successfully absorbed ability: {button.Name}");
                }
            }
            else if (target.Data.Role is not ICustomRole)
            {
                var btn = Object.Instantiate(HudManager.Instance.AbilityButton, HudManager.Instance.AbilityButton.transform.parent);
                btn.SetFromSettings(target.Data.Role.Ability);
                var pb = btn.GetComponent<PassiveButton>();
                pb.OnClick.RemoveAllListeners();
                pb.OnClick.AddListener((UnityAction)target.Data.Role.UseAbility);
            }

        OverloadRole.CachedButtons.Clear();
        OverloadRole.AbsorbedAbilityCount++;
        OverloadRole.chosenPrey = null;
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=green>Charge {OverloadRole.AbsorbedAbilityCount}/{OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge}</color>"));

        if (OverloadRole.AbsorbedAbilityCount >= OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#00FF7F>Objective completed: Final Ability unlocked!</color>"));
        else
            Coroutines.Start(OverloadRole.CoShowMenu(1f));
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