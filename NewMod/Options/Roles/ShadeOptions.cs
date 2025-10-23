using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles
{
    public class ShadeOptions : AbstractOptionGroup<Shade>
    {
        public override string GroupName => "Shade Options";

        [ModdedNumberOption("Shadow Cooldown", min: 5f, max: 60f, suffixType: MiraNumberSuffixes.Seconds)]
        public float Cooldown { get; set; } = 25f;

        [ModdedNumberOption("Max Shadow Uses", min: 1f, max: 5f, suffixType: MiraNumberSuffixes.None)]
        public float MaxUses { get; set; } = 2f;

        [ModdedNumberOption("Shadow Duration", min: 5f, max: 40f, suffixType: MiraNumberSuffixes.Seconds)]
        public float Duration { get; set; } = 20f;

        [ModdedNumberOption("Shadow Radius", min: 1f, max: 6f, suffixType: MiraNumberSuffixes.None)]
        public float Radius { get; set; } = 3f;

        [ModdedNumberOption("Required Kills To Win", min: 1f, max: 5f, suffixType: MiraNumberSuffixes.None)]
        public float RequiredKills { get; set; } = 3f;

        [ModdedEnumOption("Shadow Behavior", typeof(ShadowMode))]
        public ShadowMode Behavior { get; set; } = ShadowMode.Invisible;

        public enum ShadowMode
        {
            Invisible,
            KillEnabled,
            Both
        }
    }
}
