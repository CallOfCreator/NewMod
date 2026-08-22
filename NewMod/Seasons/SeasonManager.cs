using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.GameModes;
using MiraAPI.PluginLoading;
using NewMod.GeneralEvents;
using DateTime = Il2CppSystem.DateTime;

namespace NewMod.Seasons;

public enum SeasonState
{
    Upcoming,
    Active,
    Ended
}

public static class SeasonManager
{
    private static readonly HashSet<Type> RegisteredSeasonContent = [];

    public static readonly List<ISeason> ActiveSeasons =
    [
        new Preseason(),
        new S1()
    ];

    public static IReadOnlyList<ISeason> StartedSeasons =>
        NewMod.ForceEnableAllSeasons?.Value == true
            ? [.. ActiveSeasons]
            :
            [
                .. ActiveSeasons.Where(season => EffectiveNow >= season.SeasonStartDate.ToUniversalTime())
            ];

    public static IReadOnlyList<ISeason> CurrentActiveSeasons =>
        NewMod.ForceEnableAllSeasons?.Value == true
            ? [.. ActiveSeasons]
            :
            [
                .. ActiveSeasons.Where(season => EffectiveNow >= season.SeasonStartDate.ToUniversalTime() && EffectiveNow <= season.SeasonEndDate.ToUniversalTime())
            ];

    public static IReadOnlyList<ISeason> ContentSeasons =>
        NewMod.ForceEnableAllSeasons?.Value == true
            ? [.. ActiveSeasons]
            :
            [
                .. ActiveSeasons.Where(season => EffectiveNow >= season.SeasonStartDate.ToUniversalTime() && (!season.ContentExpires || EffectiveNow <= season.SeasonEndDate.ToUniversalTime()))
            ];
    public static IReadOnlyList<Type> AvailableAchievementTabTypes =>
    [
        .. ContentSeasons
            .SelectMany(season => season.GetSeasonAchievementTabTypes())
            .Distinct()
    ];

    private static DateTime EffectiveNow => AmongUsDateTime.UtcNow;

    public static SeasonState GetState(ISeason season)
    {
        if (NewMod.ForceEnableAllSeasons?.Value == true)
            return SeasonState.Active;

        var now = EffectiveNow;

        if (now < season.SeasonStartDate.ToUniversalTime())
            return SeasonState.Upcoming;

        if (now > season.SeasonEndDate.ToUniversalTime())
            return SeasonState.Ended;

        return SeasonState.Active;
    }

    public static void InitializeSeasons(MainMenuManager menuManager)
    {
        foreach (var season in ContentSeasons)
            RegisterSeasonGeneralEvents(season);

        foreach (var season in CurrentActiveSeasons)
            season.HandleMainMenu(menuManager);
    }

    private static void RegisterSeasonGeneralEvents(ISeason season)
    {
        foreach (var type in season.GetSeasonGETypes())
            GeneralEventManager.RegisterEvent(type);
    }

    public static void InjectSeasonContent()
    {
        var pluginInfo = MiraPluginManager.GetPluginByGuid(NewMod.Id);

        if (pluginInfo == null)
            return;

        var managerType = typeof(MiraPluginManager);

        var instance = managerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(null);

        var roleQueueProperty = managerType.GetProperty("QueuedRoleRegistrations", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (roleQueueProperty?.GetValue(instance) is not Dictionary<MiraPluginInfo, List<Type>> roleQueue)
        {
            return;
        }

        if (!roleQueue.TryGetValue(pluginInfo, out var roles))
        {
            roles = [];
            roleQueue[pluginInfo] = roles;
        }

        var registerModifier = GetPrivateMethod("RegisterModifier", typeof(Type), typeof(MiraPluginInfo));

        var registerOptions = GetPrivateMethod("RegisterOptions", typeof(Type), typeof(MiraPluginInfo));

        var registerButton = GetPrivateMethod("RegisterButton", typeof(Type), typeof(MiraPluginInfo));

        var registerGameMode = typeof(CustomGameModeManager).GetMethod("RegisterGameMode", BindingFlags.Static | BindingFlags.NonPublic, null, [
            typeof(Type),
            typeof(MiraPluginInfo)
        ], null);

        foreach (var season in ContentSeasons)
        {
            foreach (var type in season.GetSeasonRoleTypes())
            {
                if (roles.Contains(type))
                    continue;

                roles.Add(type);

                foreach (var method in AccessTools.GetDeclaredMethods(type))
                {
                    var attribute = method.GetCustomAttribute<RegisterEventAttribute>();

                    if (attribute == null || !method.IsStatic)
                        continue;

                    var parameters = method.GetParameters();

                    if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(MiraEvent)))
                    {
                        continue;
                    }

                    MiraEventManager.RegisterEventHandler(parameters[0].ParameterType, method, attribute.Priority);
                }
            }

            foreach (var type in season.GetSeasonModifierTypes())
            {
                if (registerModifier == null || !RegisteredSeasonContent.Add(type))
                {
                    continue;
                }

                registerModifier.Invoke(null, [type, pluginInfo]);
            }

            foreach (var type in season.GetSeasonOptionTypes())
            {
                if (registerOptions == null || !RegisteredSeasonContent.Add(type))
                {
                    continue;
                }

                registerOptions.Invoke(null, [type, pluginInfo]);
            }

            foreach (var type in season.GetSeasonButtonTypes())
            {
                if (registerButton == null || !RegisteredSeasonContent.Add(type))
                {
                    continue;
                }

                registerButton.Invoke(null, [type, pluginInfo]);
            }

            foreach (var type in season.GetSeasonGamemodeTypes())
            {
                if (registerGameMode == null || !RegisteredSeasonContent.Add(type))
                {
                    continue;
                }

                registerGameMode.Invoke(null, [type, pluginInfo]);
            }
        }
    }

    private static MethodInfo GetPrivateMethod(string name, params Type[] parameterTypes)
    {
        return typeof(MiraPluginManager).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic, null, parameterTypes, null);
    }
}