using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles
{
    public class EdgeveilOptions : AbstractOptionGroup<Edgeveil>
    {
        public override string GroupName => "Edgeveil Settings";

        [ModdedNumberOption("Slash Cooldown", min: 15f, max: 60f, increment: 1f, suffixType: MiraNumberSuffixes.Seconds)]
        public float SlashCooldown { get; set; } = 20f;

        [ModdedNumberOption("Slash Max Uses", min: 1f, max: 3f, increment: 1f, suffixType: MiraNumberSuffixes.Seconds)]
        public float SlashMaxUses { get; set; } = 1f;

        [ModdedNumberOption("Slash Range", min: 1f, max: 6f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
        public float SlashRange { get; set; } = 3f;

        [ModdedNumberOption("Slash Tray Speed", min: 1f, max: 6f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
        public float SlashSpeed { get; set; } = 3f;

        [ModdedNumberOption("Duration of Shake Effect", min: 5f, max: 6f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
        public float EffectDuration { get; set; } = 3f;

        [ModdedNumberOption("Max players the Arc can kill", min: 1f, max: 6f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
        public float PlayersToKill { get; set; } = 4f;

    }
}
