// Inspired by ID-based registration from https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI/Modifiers/ModifierManager.cs#L38
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
            // TODO: Add all available seasons here
        };

        public static IReadOnlyList<ISeason> CurrentActiveSeasons =>
            ActiveSeasons.Where(s =>
                DateTime.UtcNow >= s.SeasonStartDate.ToUniversalTime() &&
                DateTime.UtcNow <= s.SeasonEndDate.ToUniversalTime())
                .ToList();

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
            }
        }
        private static void RegisterSeasonContent(ISeason season)
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
