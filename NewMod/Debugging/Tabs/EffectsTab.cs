using System.Collections.Generic;
using System.Globalization;
using NewMod.Components.ScreenEffects;
using NewMod.Components.ScreenEffects.Effects;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace NewMod.Debugging.Tabs;

public class EffectsTab : IDebugTab
{
    public ScreenEffect SelectedEffect;
    public ScreenEffect InspectedEffect;
    public NightTimeEffect VisibleNightTime;
    public readonly List<ScreenEffect> VisibleEffects = [];
    public readonly List<ScreenEffectProperty> VisibleProperties = [];
    public readonly Dictionary<(int Property, int Component), string> Inputs = [];
    public string Name => "EFFECTS";
    public bool ShouldShow => ShipStatus.Instance != null && PlayerControl.LocalPlayer;

    public void BuildGUI()
    {
        var camera = Camera.main;
        if (Event.current.type == EventType.Layout)
            RefreshLayout(camera);
        var nightTime = VisibleNightTime;

        GUILayout.Label("EFFECTS");

        if (GUILayout.Button("Energy Breach")) PlayEnergyThiefBreak();
        if (GUILayout.Button("Glitch")) AddEffect<GlitchEffect>();
        if (GUILayout.Button("Night Time")) AddEffect<NightTimeEffect>();
        if (GUILayout.Button("Earthquake")) AddEffect<EarthquakeEffect>();
        if (GUILayout.Button("Pulse Hue")) AddEffect<SlowPulseHueEffect>();
        if (GUILayout.Button("Distortion Wave")) AddEffect<DistorationWaveEffect>();
        if (GUILayout.Button("Shadow Flux")) AddEffect<ShadowFluxEffect>();
        if (GUILayout.Button("Negative Reality")) AddEffect<NegativeRealityEffect>();
        if (GUILayout.Button("Glitch V2")) AddEffect<ScrDesyncEffect>();
        if (GUILayout.Button("Remove All")) RemoveEffects();

        DrawShaderProperties();

        if (nightTime != null)
        {
            GUILayout.Label($"Night Time: {nightTime.currentPhase}");
            GUILayout.Label($"Night Strength: {nightTime.currentNightAmount:P0}");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(nightTime.autoCycle ? "Auto Cycle: ON" : "Auto Cycle: OFF"))
            {
                nightTime.autoCycle = !nightTime.autoCycle;

                if (nightTime.autoCycle)
                    nightTime.RestartCycle();
            }

            if (GUILayout.Button("Sunset"))
                nightTime.SetPhase(NightTimeEffect.CyclePhase.Sunset);

            if (GUILayout.Button("Sunrise"))
                nightTime.SetPhase(NightTimeEffect.CyclePhase.Sunrise);

            GUILayout.EndHorizontal();

            nightTime.cycleSpeed = GUILayout.HorizontalSlider(nightTime.cycleSpeed, 0.25f, 10f);
            GUILayout.Label($"Cycle Speed: {nightTime.cycleSpeed:F1}x");
            if (GUILayout.Button(nightTime.midnightMode ? "Midnight: ON" : "Midnight: OFF"))
                nightTime.midnightMode = !nightTime.midnightMode;

            GUILayout.Label($"Midnight Strength: {nightTime.currentMidnightAmount:P0}");
        }
    }

    public void RefreshLayout(Camera camera)
    {
        VisibleEffects.Clear();
        var system = camera.GetComponent<ScreenEffectSystem>();
        if (system)
        {
            foreach (var effect in system.Effects)
            {
                if (effect.Active && effect.Properties.Count > 0)
                    VisibleEffects.Add(effect);
            }
        }

        if (!VisibleEffects.Contains(SelectedEffect))
            SelectedEffect = null;
        if (InspectedEffect != SelectedEffect)
        {
            Inputs.Clear();
            GUI.FocusControl(null);
        }
        InspectedEffect = SelectedEffect;
        VisibleProperties.Clear();
        if (InspectedEffect != null)
            VisibleProperties.AddRange(InspectedEffect.Properties);
        VisibleNightTime = camera.GetScreenEffect<NightTimeEffect>();
    }

    public void DrawShaderProperties()
    {
        foreach (var effect in VisibleEffects)
        {
            if (GUILayout.Button(effect.GetType().Name))
            {
                SelectedEffect = SelectedEffect == effect ? null : effect;
            }
        }
        if (InspectedEffect == null) return;

        var material = InspectedEffect._mat;
        foreach (var property in VisibleProperties)
        {
            if (!property.Editable) continue;
            GUILayout.Label(property.Label);
            switch (property.Type)
            {
                case ShaderPropertyType.Range:
                    var number = GUILayout.HorizontalSlider(material ? material.GetFloat(property.Id) : 0f, property.Range.x, property.Range.y);
                    if (material) material.SetFloat(property.Id, number);
                    GUILayout.Label(number.ToString("G4", CultureInfo.InvariantCulture));
                    break;
                case ShaderPropertyType.Float:
                    var scalar = DrawNumber(property.Id, 0, material ? material.GetFloat(property.Id) : 0f);
                    if (material) material.SetFloat(property.Id, scalar);
                    break;
                case ShaderPropertyType.Int:
                    var integer = DrawNumber(property.Id, 0, material ? material.GetInteger(property.Id) : 0);
                    if (material) material.SetInteger(property.Id, (int)integer);
                    break;
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector:
                    var vector = Vector4.zero;
                    if (material)
                        vector = property.Type == ShaderPropertyType.Color ? (Vector4)material.GetColor(property.Id) : material.GetVector(property.Id);
                    GUILayout.BeginHorizontal();
                    for (var component = 0; component < 4; component++)
                        vector[component] = DrawNumber(property.Id, component, vector[component]);
                    GUILayout.EndHorizontal();
                    if (material)
                    {
                        if (property.Type == ShaderPropertyType.Color)
                            material.SetColor(property.Id, (Color)vector);
                        else
                            material.SetVector(property.Id, vector);
                    }
                    break;
                case ShaderPropertyType.Texture:
                    var texture = material ? material.GetTexture(property.Id) : null;
                    GUILayout.Label(texture ? texture.name : "None");
                    break;
            }
        }
    }

    public float DrawNumber(int property, int component, float value)
    {
        var key = (property, component);
        var control = $"EffectProperty{property}:{component}";
        if (GUI.GetNameOfFocusedControl() != control || !Inputs.ContainsKey(key))
            Inputs[key] = value.ToString("G9", CultureInfo.InvariantCulture);
        GUI.SetNextControlName(control);
        Inputs[key] = GUILayout.TextField(Inputs[key]);
        return float.TryParse(Inputs[key], NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && float.IsFinite(number)
            ? number : value;
    }

    private static void AddEffect<T>() where T : ScreenEffect, new()
    {
        Camera.main.AddScreenEffect<T>();
    }

    private static void PlayEnergyThiefBreak()
    {
        var effect = Camera.main.GetScreenEffect<EnergyThiefBreakEffect>();

        if (effect != null && effect.Active)
            effect.Restart();
        else
            Camera.main.AddScreenEffect<EnergyThiefBreakEffect>();
    }

    private static void RemoveEffects()
    {
        Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 0f));
    }
}
