using System.Collections.Generic;
using Il2CppSystem;
using UnityEngine;
using Type = System.Type;

namespace NewMod.Seasons;

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
    IReadOnlyList<Type> GetSeasonOptionTypes();
    IReadOnlyList<Type> GetSeasonButtonTypes();
    IReadOnlyList<Type> GetSeasonGETypes();
}