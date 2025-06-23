using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace NewMod.Options;
public class GeneralOption : AbstractOptionGroup
{
    public override string GroupName => "NewMod Group";

    [ModdedToggleOption("Enable Teleportation")]
    public bool EnableTeleportation { get; set; } = true;

    [ModdedToggleOption("Can Open Cams")]
    public bool CanOpenCams { get; set; } = true;
}