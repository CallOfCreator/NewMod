using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewMod.Seasons
{
    public interface ISeason
    {
        string Name { get; }
        DateTime SeasonStartDate { get; }
        DateTime SeasonEndDate { get; }
        Color SeasonMainColor { get; }
        void HandleMainMenu(MainMenuManager menuManager);
        IReadOnlyList<Type> GetSeasonRoleTypes();
        IReadOnlyList<Type> GetSeasonModifierTypes();
        IReadOnlyList<Type> GetSeasonGamemodeTypes();
    }
}
