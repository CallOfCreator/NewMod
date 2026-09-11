using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using NewMod.Components.ScreenEffects;
using NewMod.Options;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class ScrDesyncEffectGE : IGeneralEvent
{
    public string Title => "Screen Desynchronization";
    public string Description => "REALITY HAS FALLEN OUT OF SYNC!";
    public LoadableAsset<Sprite> Icon => NewModAsset.ScrDesyncIcon;
    public Color AccentColor => new(1f, 0.08f, 0.72f);
    public int OccurrenceChance => (int)OptionGroupSingleton<GEOptions>.Instance.ScreenDesynchronizationFrequency.Value;
    public float Duration => 24f;

    public void OnEventStart()
    {
        Camera.main.AddScreenEffect<ScrDesyncEffect>().duration = Duration;
    }

    public void OnEventEnd()
    {
        Camera.main.GetScreenEffect<ScrDesyncEffect>()?.Remove();
    }
}