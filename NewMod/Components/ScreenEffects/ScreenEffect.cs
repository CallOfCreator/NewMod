using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NewMod.Components.ScreenEffects;

public abstract class ScreenEffect
{
    public ScreenEffectSystem Owner;
    public Material _mat;
    public bool Active = true;
    public readonly List<ScreenEffectProperty> Properties = [];

    public void CreateMaterial(Shader shader, params string[] animatedProperties)
    {
        if (_mat)
            UnityEngine.Object.Destroy(_mat);
        Properties.Clear();
        _mat = new Material(shader) { hideFlags = HideFlags.DontSave };
        for (var index = 0; index < shader.GetPropertyCount(); index++)
        {
            var name = shader.GetPropertyName(index);
            var type = shader.GetPropertyType(index);
            Properties.Add(new ScreenEffectProperty
            {
                Id = shader.GetPropertyNameId(index),
                Name = name,
                Label = shader.GetPropertyDescription(index),
                Type = type,
                Range = type == ShaderPropertyType.Range ? shader.GetPropertyRangeLimits(index) : Vector2.zero,
                Editable = name != "_MainTex" && Array.IndexOf(animatedProperties, name) < 0 &&
                    (shader.GetPropertyFlags(index) & ShaderPropertyFlags.HideInInspector) == 0,
            });
        }
    }

    public virtual void Initialize()
    {
    }

    public virtual void OnHudUpdate()
    {
    }

    public virtual void Render(RenderTexture source, RenderTexture destination)
    {
        if (_mat)
            Graphics.Blit(source, destination, _mat);
        else
            Graphics.Blit(source, destination);
    }

    public void Remove()
    {
        Owner.Remove(this);
    }

    public virtual void Dispose()
    {
        Active = false;
        if (_mat)
            UnityEngine.Object.Destroy(_mat);
        _mat = null;
        Properties.Clear();
    }
}
