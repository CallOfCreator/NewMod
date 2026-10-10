using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects.Effects;

[RegisterInIl2Cpp]
public class NightTimeCameraEffect(nint ptr) : MonoBehaviour(ptr)
{
    public Camera Source;

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var effect = Source ? Source.GetScreenEffect<NightTimeEffect>() : null;
        if (effect != null)
            effect.Render(source, destination);
        else
            Graphics.Blit(source, destination);
    }

    public static void Attach(Camera camera)
    {
        var effect = camera.GetComponent<NightTimeCameraEffect>();
        if (!effect)
            effect = camera.gameObject.AddComponent<NightTimeCameraEffect>();
        effect.Source = Camera.main;
    }
}
