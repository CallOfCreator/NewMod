using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1
{
    [MiraIgnore]
    public class TerminatorOptions : AbstractOptionGroup<TerminatorRole>
    {
        public override string GroupName => "Terminator Settings";

        [ModdedNumberOption("Meetings Before Final Objective", min: 1f, max: 5f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
        public float MeetingsBeforeObjective { get; set; } = 3f;

        [ModdedNumberOption("Speed Bonus Per Meeting", min: 0f, max: 30f, increment: 5f, suffixType: MiraNumberSuffixes.Percent)]
        public float SpeedBonusPerMeeting { get; set; } = 10f;

        [ModdedNumberOption("Final Objective Radius", min: 0.5f, max: 3f, increment: 0.25f, suffixType: MiraNumberSuffixes.None)]
        public float FinalObjectiveRadius { get; set; } = 1.25f;
    }
}