using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GeneralEvents;

public interface IGeneralEvent
{
    /// <summary>The name shown on the GE HUD title.</summary>
    string Title { get; }

    /// <summary>The warning text shown on the GE HUD description.</summary>
    string Description { get; }

    /// <summary>The icon sprite shown in the GE HUD logo slot.</summary>
    LoadableAsset<Sprite> Icon { get; }

    /// <summary>
    ///     Accent color used for the HUD
    /// </summary>
    Color AccentColor { get; }

    /// <summary>
    ///     Percentage chance (0–100) this event is selected during a random roll
    /// </summary>
    int OccurrenceChance { get; }

    /// <summary>How long (in seconds) the event runs before ending.</summary>
    float Duration { get; }

    /// <summary>
    ///     Called on all clients when the event starts.
    /// </summary>
    void OnEventStart();

    /// <summary>
    ///     Called on all clients when the event ends.
    /// </summary>
    void OnEventEnd();

    /// <summary>
    ///     Called once per rendered frame while this event is active.
    ///     Use this for local presentation state that must be reapplied after HUD updates.
    /// </summary>
    void Tick()
    {
    }

    /// <summary>
    ///     Optional extra condition checked before this event can fire.
    ///     Return false to skip it (e.g. if a required role isn't in the game).
    ///     Defaults to true.
    /// </summary>
    bool CanOccur()
    {
        return true;
    }
}