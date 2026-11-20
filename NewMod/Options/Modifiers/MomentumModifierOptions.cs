using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

[MiraIgnore]
public class MomentumModifierOptions : AbstractOptionGroup<MomentumModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.MomentumModifier");

    [ModdedNumberOption("NewMod.Options.MomentumModifier.TimeToMaxSpeed", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float TimeToMaxSpeed { get; set; } = 5f;

    [ModdedNumberOption("NewMod.Options.MomentumModifier.MaxSpeedBonus", 5f, 75f, 5f, MiraNumberSuffixes.Percent)]
    public float MaxSpeedBonus { get; set; } = 35f;
}
