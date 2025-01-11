using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace NewMod.Options;

public class GeneralOption : AbstractOptionGroup
{
    public override string GroupName => "NewMod Group";
    
    [ModdedToggleOption("Enable Teleportation")]
    public bool EnableTeleportation { get; set; } = true;

    [ModdedToggleOption("Can Open Cams")]
    public bool CanOpenCams {get; set;} = true;

    [ModdedNumberOption("Kill Distance (used by explosive modifier)", min: 5f, max: 20f, increment: 1f, MiraNumberSuffixes.None)]
    public float KillDistance { get; set; } = 10f;

    [ModdedNumberOption("ExplosiveModifier duration", min: 40f, max: 60f, increment: 1f, MiraNumberSuffixes.None)]
    public float Duration { get; set; } = 50f;
}