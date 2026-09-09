using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class TyrantOptions : AbstractOptionGroup<Tyrant>
{
    public override string GroupName => "Tyrant";

    [ModdedNumberOption("Fear Pulse Radius", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float FearPulseRadius { get; set; } = 4f;

    [ModdedNumberOption("Fear Pulse Duration", 1f, 12f, suffixType: MiraNumberSuffixes.Seconds)]
    public float FearPulseDuration { get; set; } = 4f;

    [ModdedNumberOption("Fear Pulse Speed Reduction %", 30f, 80f, suffixType: MiraNumberSuffixes.Percent)]
    public float FearPulseSpeed { get; set; } = 40f;

    [ModdedNumberOption("Dome Radius", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float DomeRadius { get; set; } = 4f;

    [ModdedNumberOption("Dome Duration", 2f, 12f, suffixType: MiraNumberSuffixes.Seconds)]
    public float DomeDuration { get; set; } = 5f;

    [ModdedNumberOption("Witness Range", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float WitnessRange { get; set; } = 4f;

    [ModdedNumberOption("Witness Freeze Duration", 2f, 6f, suffixType: MiraNumberSuffixes.Seconds)]
    public float WitnessFreezeDuration { get; set; } = 2f;

    [ModdedNumberOption("Witness Arm Window", 1f, 12f, suffixType: MiraNumberSuffixes.Seconds)]
    public float WitnessArmWindow { get; set; } = 6f;
}