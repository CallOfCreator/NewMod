namespace NewMod.Components.ScreenEffects.Effects;

public class GlitchEffect : ScreenEffect
{
    public override void Initialize()
    {
        CreateMaterial(NewModAsset.GlitchShader.LoadAsset());
    }
}
