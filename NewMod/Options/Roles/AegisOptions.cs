using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles.AegisOptions
{
    public class AegisOptions : AbstractOptionGroup<Aegis>
    {
        public override string GroupName => "Aegis Options";

        [ModdedNumberOption("Barrier Cooldown", min: 5f, max: 60f, suffixType: MiraNumberSuffixes.Seconds)]
        public float AegisCooldown { get; set; } = 20f;

        [ModdedNumberOption("Barrier Max Uses", min: 1f, max: 5f, suffixType: MiraNumberSuffixes.None)]
        public float MaxCharges { get; set; } = 2f;

        [ModdedNumberOption("Barrier Duration", min: 2f, max: 30f, suffixType: MiraNumberSuffixes.Seconds)]
        public float DurationSeconds { get; set; } = 20f;

        [ModdedNumberOption("Barrier Radius", min: 1f, max: 7f, suffixType: MiraNumberSuffixes.None)]
        public float Radius { get; set; } = 4f;

        [ModdedEnumOption("Barrier Behavior", typeof(AegisMode))]
        public AegisMode Behavior { get; set; } = AegisMode.WarnOnly;

        [ModdedEnumOption("Ward Visibility", typeof(WardVisibilityMode))]
        public WardVisibilityMode Visibility { get; set; } = WardVisibilityMode.OwnerOnly;
        public enum AegisMode
        {
            BlockAndReveal,
            Block,
            WarnOnly
        }
        public enum WardVisibilityMode
        {
            OwnerOnly,
            TeamOnly,
            AllPlayers
        }
    }
}
