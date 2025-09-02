/*using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace NewMod.Options;

public class GeneralNpcOptions : AbstractOptionGroup
{
    public override string GroupName => "General NPC";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<GeneralOption>.Instance.SpawnNpcAfterRoundStart;
    public ModdedNumberOption GeneralNPCSpeed { get; } = new("General NPC Speed", min: 1f, max: 5f, increment: 1f, defaultValue: 2f, suffixType: MiraAPI.Utilities.MiraNumberSuffixes.None);
    public ModdedNumberOption GeneralNPCStopTime  { get; } = new("General NPC Stop Time", min: 1f, max: 3f, increment: 1f, defaultValue: 1f, suffixType: MiraAPI.Utilities.MiraNumberSuffixes.None);
    public ModdedNumberOption GeneralNPCRunTime { get; } = new("General NPC Run Time", min: 1f, max: 10f, increment: 1f, defaultValue: 3f, suffixType: MiraAPI.Utilities.MiraNumberSuffixes.None);
}*/