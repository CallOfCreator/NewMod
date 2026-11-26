using UnityEngine;
using UnityEngine.Rendering;

namespace NewMod.Components.ScreenEffects;

public struct ScreenEffectProperty
{
    public int Id;
    public string Name;
    public string Label;
    public ShaderPropertyType Type;
    public Vector2 Range;
    public bool Editable;
}
