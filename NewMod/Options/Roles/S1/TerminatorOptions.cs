using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class TerminatorOptions : AbstractOptionGroup<TerminatorRole>
{
    public override string GroupName => "Terminator Settings";

    [ModdedNumberOption("Meetings Before Final Objective", 1f, 5f, 1f, MiraNumberSuffixes.None)]
    public float MeetingsBeforeObjective { get; set; } = 3f;

    [ModdedNumberOption("Speed Bonus Per Meeting", 0f, 30f, 5f, MiraNumberSuffixes.Percent)]
    public float SpeedBonusPerMeeting { get; set; } = 10f;

    [ModdedNumberOption("Final Objective Radius", 0.5f, 3f, 0.25f, MiraNumberSuffixes.None)]
    public float FinalObjectiveRadius { get; set; } = 1.25f;
}