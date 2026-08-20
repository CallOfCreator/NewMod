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

    [ModdedNumberOption("Judgment Tokens To Win", min: 2f, max: 5f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
    public float JudgmentTokensToWin { get; set; } = 3f;

    [ModdedNumberOption("Defend Votes Required", min: 1f, max: 5f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
    public float DefendVotesRequired { get; set; } = 2f;

    [ModdedNumberOption("Leverage Cooldown", min: 5f, max: 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float LeverageCooldown { get; set; } = 25f;

    [ModdedNumberOption("Leverage Range", min: 0.5f, max: 3f, increment: 0.25f, suffixType: MiraNumberSuffixes.None)]
    public float LeverageRange { get; set; } = 1.5f;
}