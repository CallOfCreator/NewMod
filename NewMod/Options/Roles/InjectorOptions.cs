using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class InjectorOptions : AbstractOptionGroup<InjectorRole>
{
    public override string GroupName => "Injector Settings";

    [ModdedNumberOption("Serum Cooldown", 5, 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float SerumCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Serum Uses", 1, 10)]
    public float MaxSerumUses { get; set; } = 3f;

    [ModdedNumberOption("Injections Required to Win", 1, 10)]
    public float RequiredInjectCount { get; set; } = 3f;

    [ModdedNumberOption("Adrenaline Effect (+% Speed)", 10, 200, 5, MiraNumberSuffixes.Percent)]
    public float AdrenalineSpeedBoost { get; set; } = 10f;

    [ModdedNumberOption("Immobilize Duration", 1, 10, suffixType: MiraNumberSuffixes.Seconds)]
    public float ParalysisDuration { get; set; } = 4f;

    [ModdedNumberOption("Bounce Force (Horizontal)", 1f, 2f, 0.1f)]
    public float BounceForceHorizontal { get; set; } = 2f;

    [ModdedToggleOption("Enable Random Bounce Effects")]
    public bool EnableBounceVariants { get; set; } = true;

    [ModdedNumberOption("Bounce Duration", 1, 10, suffixType: MiraNumberSuffixes.Seconds)]
    public float BounceDuration { get; set; } = 10f;

    public ModdedNumberOption BounceRotateEffect { get; } = new("Bounce Rotate Effect", 180f, 0f, 180f, 10f, MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<InjectorOptions>.Instance.EnableBounceVariants
    };

    public ModdedNumberOption BounceStretchScale { get; } = new("Bounce Stretch Scale", 1.5f, 1f, 1.5f, 0.01f, MiraNumberSuffixes.Multiplier)
    {
        Visible = () => OptionGroupSingleton<InjectorOptions>.Instance.EnableBounceVariants
    };

    [ModdedNumberOption("Repel Duration", 1, 10, suffixType: MiraNumberSuffixes.Seconds)]
    public float RepelDuration { get; set; } = 10f;

    [ModdedNumberOption("Repel Range", 0.5f, 4f, 0.1f)]
    public float RepelRange { get; set; } = 2f;

    [ModdedNumberOption("Repel Force", 0.1f, 2f, 0.1f, MiraNumberSuffixes.Multiplier)]
    public float RepelForce { get; set; } = 0.3f;
}