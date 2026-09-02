using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class ArbitratorOptions : AbstractOptionGroup<ArbitratorRole>
{
    public override string GroupName => "Arbitrator";

    [ModdedNumberOption("Judgment Tokens To Win", 2f, 5f, 1f, MiraNumberSuffixes.None)]
    public float JudgmentTokensToWin { get; set; } = 3f;

    [ModdedNumberOption("Defend Votes Required", 1f, 5f, 1f, MiraNumberSuffixes.None)]
    public float DefendVotesRequired { get; set; } = 2f;

    [ModdedNumberOption("Leverage Cooldown", 5f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float LeverageCooldown { get; set; } = 25f;

    [ModdedNumberOption("Leverage Range", 0.5f, 3f, 0.25f, MiraNumberSuffixes.None)]
    public float LeverageRange { get; set; } = 1.5f;
}