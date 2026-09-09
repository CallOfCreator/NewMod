using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class VisionaryOptions : AbstractOptionGroup<TheVisionary>
{
    public override string GroupName => "The Visionary";

    [ModdedNumberOption("Screenshot Cooldown", 5, 30)]
    public float ScreenshotCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Screenshots", 1, 5)]
    public float MaxScreenshots { get; set; } = 2f;

    [ModdedNumberOption("Max Display Duration", 5, 10)]
    public float MaxDisplayDuration { get; set; } = 5f;
}