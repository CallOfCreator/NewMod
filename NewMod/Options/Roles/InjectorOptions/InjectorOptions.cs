using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Options.Roles.InjectorOptions;

public class InjectorOptions : AbstractOptionGroup<InjectorRole>
{
    public override string GroupName => "Injector Settings";

    [ModdedNumberOption("Serum Cooldown", min: 5, max: 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float SerumCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Serum Uses", min: 1, max: 10)]
    public float MaxSerumUses { get; set; } = 3f;

    [ModdedNumberOption("Injections Required to Win", min: 1, max: 10)]
    public float RequiredInjectCount { get; set; } = 3f;

    [ModdedNumberOption("Adrenaline Effect (+% Speed)", min: 10, max: 200, increment: 5, suffixType: MiraNumberSuffixes.Percent)]
    public float AdrenalineSpeedBoost { get; set; } = 10f;

    [ModdedNumberOption("Immobilize Duration", min: 1, max: 10, suffixType: MiraNumberSuffixes.Seconds)]
    public float ParalysisDuration { get; set; } = 4f;

    [ModdedNumberOption("Bounce Force (Horizontal)", min: 1f, max: 2f, increment: 0.1f)]
    public float BounceForceHorizontal { get; set; } = 2f;

    [ModdedToggleOption("Enable Random Bounce Effects")]
    public bool EnableBounceVariants { get; set; } = true;

    [ModdedNumberOption("Bounce Duration", min: 1, max: 10, suffixType: MiraNumberSuffixes.Seconds)]
    public float BounceDuration { get; set; } = 10f;

    public ModdedNumberOption BounceRotateEffect { get; } = new("Bounce Rotate Effect", 180f, min: 0f, max: 180f, increment: 10f, suffixType: MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<InjectorOptions>.Instance.EnableBounceVariants
    };
    public ModdedNumberOption BounceStretchScale { get; } = new("Bounce Stretch Scale", 1.5f, min: 1f, max: 1.5f, increment: 0.01f, suffixType: MiraNumberSuffixes.Multiplier)
    {
        Visible = () => OptionGroupSingleton<InjectorOptions>.Instance.EnableBounceVariants
    };

    [ModdedNumberOption("Repel Duration", min: 1, max: 10, suffixType: MiraNumberSuffixes.Seconds)]
    public float RepelDuration { get; set; } = 10f;

    [ModdedNumberOption("Repel Range", min: 0.5f, max: 4f, increment: 0.1f)]
    public float RepelRange { get; set; } = 2f;

    [ModdedNumberOption("Repel Force", min: 0.1f, max: 2f, increment: 0.1f, suffixType: MiraNumberSuffixes.Multiplier)]
    public float RepelForce { get; set; } = 0.3f;
}


