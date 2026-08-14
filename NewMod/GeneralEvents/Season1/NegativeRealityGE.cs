using MiraAPI.Utilities.Assets;
using NewMod.Components.ScreenEffects;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class NegativeRealityGE : IGeneralEvent
{
    public string Title => "Negative Reality";
    public string Description => "REALITY INVERSION IMMINENT!";
    public LoadableAsset<Sprite> Icon => NewModAsset.NegativeRealityIcon;
    public Color AccentColor => new(0.55f, 0.2f, 1.0f);
    public int OccurrenceChance => 90;
    public float Duration => 30f;

    public void OnEventStart()
    {
        Camera.main.gameObject.AddComponent<NegativeRealityEffect>();
    }

    public void OnEventEnd()
    {
        Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 0f));
    }
}