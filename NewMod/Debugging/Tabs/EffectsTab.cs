using NewMod.Components.ScreenEffects;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Debugging.Tabs;

public class EffectsTab : IDebugTab
{
    public string Name => "EFFECTS";
    public bool ShouldShow => ShipStatus.Instance != null && PlayerControl.LocalPlayer;

    public void BuildGUI()
    {
        var camera = Camera.main;
        var glitch = camera.GetComponent<GlitchEffect>();
        var distortion = camera.GetComponent<DistorationWaveEffect>();
        var flux = camera.GetComponent<ShadowFluxEffect>();
        
        GUILayout.Label("EFFECTS");

        if (GUILayout.Button("Energy Breach")) PlayEnergyThiefBreak();
        if (GUILayout.Button("Glitch")) AddEffect<GlitchEffect>();
        if (GUILayout.Button("Earthquake")) AddEffect<EarthquakeEffect>();
        if (GUILayout.Button("Pulse Hue")) AddEffect<SlowPulseHueEffect>();
        if (GUILayout.Button("Distortion Wave")) AddEffect<DistorationWaveEffect>();
        if (GUILayout.Button("Shadow Flux")) AddEffect<ShadowFluxEffect>();
        if (GUILayout.Button("Negative Reality")) AddEffect<NegativeRealityEffect>();
        if (GUILayout.Button("Shattered Glass")) AddEffect<ShatteredGlassEffect>();
        if (GUILayout.Button("Glitch V2")) AddEffect<ScrDesyncEffect>();
        if (GUILayout.Button("Remove All")) RemoveEffects();

        if (glitch)
        {
            GUILayout.Label("GLITCH");
            
            GUILayout.BeginHorizontal();

            GUILayout.Label("Intensity");
            glitch.intensity = GUILayout.HorizontalSlider(glitch.intensity, 0f, 1f);
            GUILayout.Label(glitch.intensity.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Block Size");
            glitch.blockSize = GUILayout.HorizontalSlider(glitch.blockSize, 8f, 128f);
            GUILayout.Label(glitch.blockSize.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Color Split");
            glitch.colorSplit = GUILayout.HorizontalSlider(glitch.colorSplit, 0f, 3f);
            GUILayout.Label(glitch.colorSplit.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Speed");
            glitch.speed = GUILayout.HorizontalSlider(glitch.speed, 0f, 10f);
            GUILayout.Label(glitch.speed.ToString());
        
            GUILayout.EndHorizontal();
        }
        
        if (distortion)
        {
            GUILayout.Label("DISTORTION WAVE");
            
            GUILayout.BeginHorizontal();

            GUILayout.Label("Amplitude");
            distortion.amplitude = GUILayout.HorizontalSlider(distortion.amplitude, 0f, 0.25f);
            GUILayout.Label(distortion.amplitude.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Frequency");
            distortion.frequency = GUILayout.HorizontalSlider(distortion.frequency, 0f, 12f);
            GUILayout.Label(distortion.frequency.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Speed");
            distortion.speed = GUILayout.HorizontalSlider(distortion.speed, 0f, 5f);
            GUILayout.Label(distortion.speed.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Radius");
            distortion.radius = GUILayout.HorizontalSlider(distortion.radius, 0f, 1f);
            GUILayout.Label(distortion.radius.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Falloff");
            distortion.falloff = GUILayout.HorizontalSlider(distortion.falloff, 0f, 5f);
            GUILayout.Label(distortion.falloff.ToString());
        
            GUILayout.EndHorizontal();
        }
        
        if (flux)
        {
            GUILayout.Label("SHADOW FLUX");
            
            GUILayout.BeginHorizontal();

            GUILayout.Label("Noise Scale");
            flux.noiseScale = GUILayout.HorizontalSlider(flux.noiseScale, 0f, 5f);
            GUILayout.Label(flux.noiseScale.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Speed");
            flux.speed = GUILayout.HorizontalSlider(flux.speed, 0f, 3f);
            GUILayout.Label(flux.speed.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Edge Width");
            flux.edgeWidth = GUILayout.HorizontalSlider(flux.edgeWidth, 0f, 1f);
            GUILayout.Label(flux.edgeWidth.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Threshold");
            flux.threshold = GUILayout.HorizontalSlider(flux.threshold, 0f, 1f);
            GUILayout.Label(flux.threshold.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Opacity");
            flux.opacity = GUILayout.HorizontalSlider(flux.opacity, 0f, 1f);
            GUILayout.Label(flux.opacity.ToString());
        
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Darkness");
            flux.darkness = GUILayout.HorizontalSlider(flux.darkness, 0f, 1f);
            GUILayout.Label(flux.darkness.ToString());
        
            GUILayout.EndHorizontal();
        }
    }
    
    private static void AddEffect<T>() where T : MonoBehaviour
    {
        if (!Camera.main.GetComponent<T>())
            Camera.main.gameObject.AddComponent<T>();
    }

    private static void PlayEnergyThiefBreak()
    {
        var effect = Camera.main.GetComponent<EnergyThiefBreakEffect>();

        if (effect)
            effect.Restart();
        else
            Camera.main.gameObject.AddComponent<EnergyThiefBreakEffect>();
    }

    private static void RemoveEffects()
    {
        Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 0f));
    }
}