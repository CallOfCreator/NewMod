using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GeneralEvents;

public interface IGeneralEvent
{
    /// <summary>
    /// Gets the event title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the event description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the event icon.
    /// </summary>
    LoadableAsset<Sprite> Icon { get; }

    /// <summary>
    /// Gets the HUD accent color.
    /// </summary>
    Color AccentColor { get; }

    /// <summary>
    /// Gets the event selection weight.
    /// </summary>
    int OccurrenceChance { get; }

    /// <summary>
    /// Gets the event duration in seconds.
    /// </summary>
    float Duration { get; }

    /// <summary>
    /// Called on all clients when the event starts.
    /// </summary>
    void OnEventStart();

    /// <summary>
    /// Called on all clients when the event ends.
    /// </summary>
    void OnEventEnd();

    /// <summary>
    /// Called each frame while the event HUD is active.
    /// </summary>
    void OnHudUpdate()
    {
    }

    /// <summary>
    /// Return false to skip this event. Defaults to true.
    /// </summary>
    bool CanOccur()
    {
        return true;
    }
}
