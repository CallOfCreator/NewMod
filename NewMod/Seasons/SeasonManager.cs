using System;
using System.Collections.Generic;
using System.Linq;

namespace NewMod.Seasons
{
    public static class SeasonManager
    {
        private static uint _nextTypeId = 0;
        private static readonly Dictionary<uint, Type> TypeIdMap = new();
        private static readonly Dictionary<Type, uint> TypeToIdMap = new();
        private static readonly List<ISeason> ActiveSeasons = new()
        {
           new S1()
        };

        public static IReadOnlyList<ISeason> CurrentActiveSeasons =>
            [.. ActiveSeasons.Where(s =>
                AmongUsDateTime.UtcNow >= s.SeasonStartDate.ToUniversalTime() &&
                AmongUsDateTime.UtcNow <= s.SeasonEndDate.ToUniversalTime())];

        private static uint GenerateNextTypeId()
        {
            _nextTypeId++;
            return _nextTypeId;
        }

        public static void InitializeSeasons(MainMenuManager menuManager)
        {
            foreach (var season in CurrentActiveSeasons)
            {
                season.HandleMainMenu(menuManager);
                RegisterSeasonContent(season);
                NewMod.Instance.Log.LogMessage($"Registered {season.Name}");
            }
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
