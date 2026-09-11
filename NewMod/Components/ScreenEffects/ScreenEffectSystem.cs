using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects;

[RegisterInIl2Cpp]
public class ScreenEffectSystem(nint ptr) : MonoBehaviour(ptr)
{
    public readonly List<ScreenEffect> Effects = new();

    [HideFromIl2Cpp]
    public T Get<T>() where T : ScreenEffect
    {
        foreach (var effect in Effects)
            if (effect is T match && effect.Active)
                return match;
        return null;
    }

    [HideFromIl2Cpp]
    public T Add<T>() where T : ScreenEffect, new()
    {
        var existing = Get<T>();
        if (existing != null)
            return existing;

        var effect = new T { Owner = this };
        effect.Initialize();
        if (effect.Active)
            Effects.Add(effect);
        else
            effect.Dispose();
        return effect;
    }

    [HideFromIl2Cpp]
    public void Remove(ScreenEffect effect)
    {
        if (Effects.Remove(effect))
            effect.Dispose();
    }

    public void Clear()
    {
        foreach (var effect in Effects)
            effect.Dispose();
        Effects.Clear();
    }

    public void Update()
    {
        for (var i = Effects.Count - 1; i >= 0; i--)
            Effects[i].Tick();
    }

    public void OnDisable()
    {
        Clear();
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Effects.Count == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        var current = source;
        RenderTexture temporary = null;
        var descriptor = source.descriptor;
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;
        
        try
        {
            for (var i = 0; i < Effects.Count; i++)
            {
                temporary = i == Effects.Count - 1 ? null : RenderTexture.GetTemporary(descriptor);
                var target = temporary ? temporary : destination;
                Effects[i].Render(current, target);
                if (current != source)
                    RenderTexture.ReleaseTemporary(current);
                current = target;
                temporary = null;
            }
        }
        finally
        {
            if (temporary)
                RenderTexture.ReleaseTemporary(temporary);
            if (current != source && current != destination)
                RenderTexture.ReleaseTemporary(current);
        }
    }
}

public static class ScreenEffectExtensions
{
    public static T GetScreenEffect<T>(this Camera camera) where T : ScreenEffect
    {
        var system = camera.GetComponent<ScreenEffectSystem>();
        return system ? system.Get<T>() : null;
    }

    public static T AddScreenEffect<T>(this Camera camera) where T : ScreenEffect, new()
    {
        var system = camera.GetComponent<ScreenEffectSystem>();
        if (!system)
            system = camera.gameObject.AddComponent<ScreenEffectSystem>();
        return system.Add<T>();
    }
}
