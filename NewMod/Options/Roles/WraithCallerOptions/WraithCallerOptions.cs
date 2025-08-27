using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles.WraithCallerOptions
{
    public class WraithCallerOptions : AbstractOptionGroup<WraithCaller>
    {
        public override string GroupName => "Wraith Caller";

        [ModdedNumberOption("Wraith Summon Cooldown", min: 5, max: 60)]
        public float CallWraithCooldown { get; set; } = 20f;

        [ModdedNumberOption("Max Wraith Summons", min: 1, max: 5)]
        public float CallWraithMaxUses { get; set; } = 3f;

        [ModdedNumberOption("Required NPCs to Send", min: 1, max: 5)]
        public float RequiredNPCsToSend { get; set; } = 2f;

        [ModdedToggleOption("Show Summon Warnings")]
        public bool ShowSummonWarnings { get; set; } = true;
    }
}
