using System.Collections.Generic;
using UnityEngine;

namespace NewMod.Seasons
{
    public interface ISeason
    {
        string Name { get; }
        Il2CppSystem.DateTime SeasonStartDate { get; }
        Il2CppSystem.DateTime SeasonEndDate { get; }
        Color SeasonMainColor { get; }

        void HandleMainMenu(MainMenuManager menuManager);

        IReadOnlyList<System.Type> GetSeasonRoleTypes();
        IReadOnlyList<System.Type> GetSeasonModifierTypes();
        IReadOnlyList<System.Type> GetSeasonGamemodeTypes();
        IReadOnlyList<System.Type> GetSeasonOptionTypes();
        IReadOnlyList<System.Type> GetSeasonButtonTypes();
        IReadOnlyList<System.Type> GetSeasonGETypes();
    }
}