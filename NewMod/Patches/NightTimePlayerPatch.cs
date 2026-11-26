using System.Collections.Generic;
using HarmonyLib;
using NewMod.Components.ScreenEffects;
using NewMod.Components.ScreenEffects.Effects;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class NightTimePlayerPatch
{
    public static readonly Dictionary<(Material Material, int Property), (Color Original, Color Applied)> Colors = [];
    public static readonly int[] ColorProperties =
    {
        Shader.PropertyToID("_BodyColor"), Shader.PropertyToID("_BackColor"),
        Shader.PropertyToID("_VisorColor"), Shader.PropertyToID("_Color"),
    };

    public static void Postfix(PlayerControl __instance)
    {
        var camera = Camera.main;
        var effect = camera ? camera.GetScreenEffect<NightTimeEffect>() : null;
        var strength = effect != null && effect.ShouldRender()
            ? effect.currentNightAmount * effect.currentMidnightAmount : 0f;
        if (strength <= 0f)
        {
            Restore();
            return;
        }

        var cosmetics = __instance.cosmetics;
        if (!cosmetics || cosmetics.currentBodySprite == null) return;
        var brightness = Mathf.Lerp(1f, Mathf.Max(1f, effect.midnightPlayerBrightness), strength);
        var body = cosmetics.currentBodySprite;
        Brighten(body.BodySprite, brightness);
        foreach (var part in body.LongModeParts)
            Brighten(part, brightness);
        Brighten(body.HandHat, brightness);
        if (body.PettingHand)
            Brighten(body.PettingHand.HandSprite, brightness);
        if (cosmetics.skin)
            Brighten(cosmetics.skin.layer, brightness);
        if (cosmetics.visor)
            Brighten(cosmetics.visor.Image, brightness);
        if (cosmetics.hat)
        {
            Brighten(cosmetics.hat.FrontLayer, brightness);
            Brighten(cosmetics.hat.BackLayer, brightness);
        }
    }

    public static void Brighten(SpriteRenderer renderer, float brightness)
    {
        if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy) return;
        var material = renderer.material;
        if (!material) return;
        foreach (var property in ColorProperties)
        {
            if (!material.HasProperty(property)) continue;
            var key = (material, property);
            var current = material.GetColor(property);
            var original = Colors.TryGetValue(key, out var saved) && current == saved.Applied
                ? saved.Original : current;
            var brightened = new Color(
                original.r * brightness,
                original.g * brightness,
                original.b * brightness,
                original.a);
            if (current != brightened)
                material.SetColor(property, brightened);
            Colors[key] = (original, brightened);
        }
    }

    public static void Restore()
    {
        foreach (var entry in Colors)
        {
            var (material, property) = entry.Key;
            if (material && material.GetColor(property) == entry.Value.Applied)
                material.SetColor(property, entry.Value.Original);
        }
        Colors.Clear();
    }
}
