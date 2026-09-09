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

    [ModdedNumberOption("Meetings Before Final Objective", 1f, 5f)]
    public float MeetingsBeforeObjective { get; set; } = 3f;

    [ModdedNumberOption("Final Armor Segments", 1f, 5f)]
    public float ArmorSegments { get; set; } = 3f;

    [ModdedNumberOption("Final Objective Radius", 0.5f, 3f, 0.25f)]
    public float FinalObjectiveRadius { get; set; } = 1.25f;

    [ModdedNumberOption("Final Countdown Duration", 8f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float FinalCountdownDuration { get; set; } = 12f;

    [ModdedNumberOption("Counter Attack Range", 0.75f, 2.5f, 0.25f)]
    public float CounterAttackRange { get; set; } = 1.5f;
}