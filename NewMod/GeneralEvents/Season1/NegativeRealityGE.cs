using MiraAPI.Utilities.Assets;
using NewMod.Components.ScreenEffects;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class NegativeRealityGE : IGeneralEvent
{
    public string Title => "Negative Reality";
    public string Description => "REALITY INVERSION IMMINENT!";
    public LoadableAsset<Sprite> Icon => NewModAsset.NegativeRealityIcon;
    public Color AccentColor => new(0.55f, 0.2f, 1.0f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<Options.GEOptions>.Instance.NegativeRealityFrequency.Value;
    public float Duration => 30f;

    public void OnEventStart()
    {
        Camera.main.AddScreenEffect<NegativeRealityEffect>();
    }

    public void OnEventEnd()
    {
        Camera.main.GetScreenEffect<NegativeRealityEffect>()?.Remove();
    }
}