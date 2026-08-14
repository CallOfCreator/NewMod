using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace NewMod.Options;

public class GeneralOption : AbstractOptionGroup
{
    public override string GroupName => "NewMod Group";

    [ModdedToggleOption("Allows dead players to open cams anywhere")]
    public bool AllowCams { get; set; } = false;

    [ModdedNumberOption("Total Neutrals", 0f, 10)]
    public float TotalNeutrals { get; set; } = 3f;

    [ModdedToggleOption("Keep Crew Majority")]
    public bool KeepCrewMajority { get; set; } = true;

    [ModdedToggleOption("Prefer Variety")] public bool PreferVariety { get; set; } = true;

    [ModdedToggleOption("Dead players can see roles in meetings")]
    public bool ShouldDeadPlayersSeeRoles { get; set; } = true;

    [ModdedToggleOption("Anonymous Names in Meetings")]
    public bool EnableAnonymousNamesInMeetings { get; set; } = false;

    public ModdedNumberOption SpawnChanceOfGlitchEffect { get; } = new("Spawn Chance of Glitch Effect", 0f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedPlayerOption ChosenPlayer { get; } = new("Player who will receive the effect")
    {
        Visible = () => OptionGroupSingleton<GeneralOption>.Instance.SpawnChanceOfGlitchEffect.Value > 0f
    };

    /*[ModdedToggleOption("Should spawn NPC after round start")]
    public bool SpawnNpcAfterRoundStart { get; set; } = false;*/
}