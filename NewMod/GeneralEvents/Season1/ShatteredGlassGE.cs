using System.Collections;
using MiraAPI.Utilities.Assets;
using NewMod.Components.ScreenEffects;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class ShatteredGlassGE : IGeneralEvent
{
    public string Title => "Shattered Glass";
    public string Description => "REALITY HAS FRACTURED!";
    public LoadableAsset<Sprite> Icon => MiraAssets.Empty; // for now
    public Color AccentColor => new(0.72f, 0.9f, 1f);
    public int OccurrenceChance => 90;
    public float Duration => 30f;

    public void OnEventStart()
    {
        var cam = Camera.main;

        if (!cam)
            return;

        var effect = cam.GetComponent<ShatteredGlassEffect>();

        if (!effect)
            effect = cam.gameObject.AddComponent<ShatteredGlassEffect>();

        effect.damageAmount = 0f;
        effect.SetImpact(new Vector2(Random.Range(0.25f, 0.75f), Random.Range(0.3f, 0.7f)));

        Coroutines.Start(CoShatter(effect));
    }

    public void OnEventEnd()
    {
        var cam = Camera.main;

        if (!cam)
            return;

        var effect = cam.GetComponent<ShatteredGlassEffect>();

        if (effect)
            Object.Destroy(effect);
    }

    private static IEnumerator CoShatter(ShatteredGlassEffect effect)
    {
        const float duration = 0.65f;
        var timer = 0f;

        while (effect && timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            var progress = Mathf.Clamp01(timer / duration);
            effect.damageAmount = Mathf.SmoothStep(0f, 1f, progress);

            yield return null;
        }

        if (effect)
            effect.damageAmount = 1f;
    }
}