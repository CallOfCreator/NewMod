using Il2CppSystem;
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
        IReadOnlyList<System.Type> GetSeasonRoleTypes();
        IReadOnlyList<System.Type> GetSeasonModifierTypes();
        IReadOnlyList<System.Type> GetSeasonGamemodeTypes();
    }
}
