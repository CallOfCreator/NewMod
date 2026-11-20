using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class VisionaryOptions : AbstractOptionGroup<TheVisionary>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.TheVisionary");

    [ModdedNumberOption("NewMod.Options.TheVisionary.ScreenshotCooldown", 5, 30)]
    public float ScreenshotCooldown { get; set; } = 20f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.MaxScreenshots", 1, 5)]
    public float MaxScreenshots { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.MaxDisplayDuration", 5, 10)]
    public float MaxDisplayDuration { get; set; } = 5f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.CaptureDelay", 3f, 20f)]
    public float CaptureDelay { get; set; } = 8f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.PhotoHeight", 360f, 1080f, 180f)]
    public float PhotoHeight { get; set; } = 720f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.PlacementRange", 1f, 4f, 0.5f)]
    public float PlacementRange { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.PhotoViewHeight", 4f, 8f, 0.5f)]
    public float PhotoViewHeight { get; set; } = 6f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.PhotoAspect", 1f, 2f, 0.1f)]
    public float PhotoAspect { get; set; } = 1.6f;

    [ModdedNumberOption("NewMod.Options.TheVisionary.PhotoQuality", 60f, 95f, 5f)]
    public float PhotoQuality { get; set; } = 90f;
}
