using System;
using System.Collections.Generic;
using System.Linq;
using NewMod.GeneralEvents;

namespace NewMod.Seasons
{
    public static class SeasonManager
    {
        public static uint _nextTypeId = 0;
        public static readonly Dictionary<uint, Type> TypeIdMap = new();
        public static readonly Dictionary<Type, uint> TypeToIdMap = new();
        public static readonly List<ISeason> ActiveSeasons = new()
        {
            new S1()
        };

        public static IReadOnlyList<ISeason> CurrentActiveSeasons =>
            [.. ActiveSeasons.Where(s =>
                AmongUsDateTime.UtcNow >= s.SeasonStartDate.ToUniversalTime() &&
                AmongUsDateTime.UtcNow <= s.SeasonEndDate.ToUniversalTime())];

        public static IReadOnlyList<ISeason> StartedSeasons =>
            [.. ActiveSeasons.Where(s =>
                AmongUsDateTime.UtcNow >= s.SeasonStartDate.ToUniversalTime())];

        static uint GenerateNextTypeId()
        {
            _nextTypeId++;
            return _nextTypeId;
        }

        public static void InitializeSeasons(MainMenuManager menuManager)
        {
            foreach (var season in StartedSeasons)
            {
                RegisterSeasonContent(season);
                RegisterSeasonGeneralEvents(season);
                NewMod.Instance.Log.LogMessage($"Registered {season.Name}");
            }

            foreach (var season in CurrentActiveSeasons)
                season.HandleMainMenu(menuManager);
        }

        static void RegisterSeasonGeneralEvents(ISeason season)
        {
            foreach (var type in season.GetSeasonGETypes())
                GeneralEventManager.RegisterEvent(type);
        }

        public static void RegisterSeasonContent(ISeason season)
        {
            RegisterContent(season.GetSeasonRoleTypes());
            RegisterContent(season.GetSeasonModifierTypes());
            RegisterContent(season.GetSeasonGamemodeTypes());
        }

        public static void RegisterContent(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                if (TypeToIdMap.ContainsKey(type))
                    continue;

                var id = GenerateNextTypeId();
                TypeToIdMap[type] = id;
                TypeIdMap[id] = type;
            }
        }
    }
}