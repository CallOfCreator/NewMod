using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.PluginLoading;
using NewMod.GeneralEvents;

namespace NewMod.Seasons
{
    public enum SeasonState
    {
        Upcoming,
        Active,
        Ended
    }

    public static class SeasonManager
    {
        public static readonly List<ISeason> ActiveSeasons = new()
        {
            new S1()
        };

        public static IReadOnlyList<ISeason> StartedSeasons =>
        [
            .. ActiveSeasons.Where(s => EffectiveNow >= s.SeasonStartDate.ToUniversalTime())
        ];

        public static IReadOnlyList<ISeason> CurrentActiveSeasons =>
        [
            .. ActiveSeasons.Where(s =>
                EffectiveNow >= s.SeasonStartDate.ToUniversalTime() &&
                EffectiveNow <= s.SeasonEndDate.ToUniversalTime())
        ];

        static Il2CppSystem.DateTime EffectiveNow =>
            NewMod.ForceEnableAllSeasons?.Value == true
                ? Il2CppSystem.DateTime.MaxValue
                : AmongUsDateTime.UtcNow;

        public static SeasonState GetState(ISeason season)
        {
            var now = EffectiveNow;

            if (now < season.SeasonStartDate.ToUniversalTime())
                return SeasonState.Upcoming;

            if (now > season.SeasonEndDate.ToUniversalTime())
                return SeasonState.Ended;

            return SeasonState.Active;
        }

        public static void InitializeSeasons(MainMenuManager menuManager)
        {
            foreach (var season in StartedSeasons)
                RegisterSeasonGeneralEvents(season);

            foreach (var season in CurrentActiveSeasons)
                season.HandleMainMenu(menuManager);
        }

        static void RegisterSeasonGeneralEvents(ISeason season)
        {
            foreach (var type in season.GetSeasonGETypes())
                GeneralEventManager.RegisterEvent(type);
        }

        public static void InjectSeasonContent()
        {
            var pluginInfo = MiraPluginManager.GetPluginByGuid(NewMod.Id);
            if (pluginInfo == null)
            {
                NewMod.Instance.Log.LogError("[SeasonManager] Could not find NewMod plugin info");
                return;
            }

            var queueProp = typeof(MiraPluginManager).GetProperty(
                "QueuedRoleRegistrations",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var instance = typeof(MiraPluginManager).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(null);

            if (queueProp?.GetValue(instance) is not Dictionary<MiraPluginInfo, List<Type>> roleQueue)
            {
                NewMod.Instance.Log.LogError("[SeasonManager] Failed to access QueuedRoleRegistrations");
                return;
            }

            if (!roleQueue.TryGetValue(pluginInfo, out var roles))
            {
                roles = [];
                roleQueue[pluginInfo] = roles;
                NewMod.Instance.Log.LogWarning("[SeasonManager] NewMod had no role queue, created one manually");
            }

            var fnModifier = GetPrivateMethod("RegisterModifier", typeof(Type), typeof(MiraPluginInfo));
            var fnOptions = GetPrivateMethod("RegisterOptions", typeof(Type), typeof(MiraPluginInfo));
            var fnButton = GetPrivateMethod("RegisterButton", typeof(Type), typeof(MiraPluginInfo));

            int cRoles = 0;
            int cMods = 0;
            int cOpts = 0;
            int cBtns = 0;
            int cEvts = 0;

            foreach (var season in StartedSeasons)
            {
                foreach (var type in season.GetSeasonRoleTypes())
                {
                    if (!roles.Contains(type))
                    {
                        roles.Add(type);
                        cRoles++;
                    }

                    foreach (var method in AccessTools.GetDeclaredMethods(type))
                    {
                        var attr = method.GetCustomAttribute<RegisterEventAttribute>();
                        if (attr == null || !method.IsStatic)
                            continue;

                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(MiraEvent)))
                            continue;

                        MiraEventManager.RegisterEventHandler(parameters[0].ParameterType, method, attr.Priority);
                        cEvts++;
                    }
                }

                foreach (var type in season.GetSeasonModifierTypes())
                {
                    if (fnModifier == null)
                        continue;

                    fnModifier.Invoke(null, [type, pluginInfo]);
                    cMods++;
                }

                foreach (var type in season.GetSeasonOptionTypes())
                {
                    if (fnOptions == null)
                        continue;

                    fnOptions.Invoke(null, [type, pluginInfo]);
                    cOpts++;
                }

                foreach (var type in season.GetSeasonButtonTypes())
                {
                    if (fnButton == null)
                        continue;

                    fnButton.Invoke(null, [type, pluginInfo]);
                    cBtns++;
                }
            }

            NewMod.Instance.Log.LogMessage(
                $"[SeasonManager] Injected content: roles={cRoles}, mods={cMods}, opts={cOpts}, btns={cBtns}, events={cEvts}");
        }

        static MethodInfo GetPrivateMethod(string name, params Type[] paramTypes)
        {
            var method = typeof(MiraPluginManager).GetMethod(
                name,
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                paramTypes,
                null);

            if (method == null)
                NewMod.Instance.Log.LogWarning($"[SeasonManager] MiraPluginManager.{name} not found, content may not register");

            return method;
        }
    }
}