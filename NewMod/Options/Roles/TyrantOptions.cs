using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles.TyrantOptions
{
    public class TyrantOptions : AbstractOptionGroup<Tyrant>
    {
        public override string GroupName => "Tyrant";

        [ModdedNumberOption("Fear Pulse Radius", min: 1f, max: 6f, suffixType: MiraNumberSuffixes.None)]
        public float FearPulseRadius { get; set; } = 5f;

        [ModdedNumberOption("Fear Pulse Duration", min: 1f, max: 12f, suffixType: MiraNumberSuffixes.Seconds)]
        public float FearPulseDuration { get; set; } = 6f;

        [ModdedNumberOption("Fear Pulse Speed Reduction %", min: 30f, max: 80f, suffixType: MiraNumberSuffixes.Percent)]
        public float FearPulseSpeed { get; set; } = 20f;

        [ModdedNumberOption("Dome Radius", min: 1f, max: 6f, suffixType: MiraNumberSuffixes.None)]
        public float DomeRadius { get; set; } = 5f;

        [ModdedNumberOption("Dome Duration", min: 2f, max: 12f, suffixType: MiraNumberSuffixes.Seconds)]
        public float DomeDuration { get; set; } = 8f;

        [ModdedNumberOption("Witness Range", min: 1f, max: 6f, suffixType: MiraNumberSuffixes.None)]
        public float WitnessRange { get; set; } = 5f;

        [ModdedNumberOption("Witness Freeze Duration", min: 2f, max: 6f, suffixType: MiraNumberSuffixes.Seconds)]
        public float WitnessFreezeDuration { get; set; } = 3f;

        [ModdedNumberOption("Witness Arm Window", min: 1f, max: 12f, suffixType: MiraNumberSuffixes.Seconds)]
        public float WitnessArmWindow { get; set; } = 8f;
    }
}
