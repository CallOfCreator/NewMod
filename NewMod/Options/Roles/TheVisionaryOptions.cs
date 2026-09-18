using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class VisionaryOptions : AbstractOptionGroup<TheVisionary>
{
    public override string GroupName => "The Visionary";

    [ModdedNumberOption("Camera Placement Cooldown", 5, 30)]
    public float ScreenshotCooldown { get; set; } = 20f;

    [ModdedNumberOption("Camera Uses", 1, 5)]
    public float MaxScreenshots { get; set; } = 2f;

    [ModdedNumberOption("Max Display Duration", 5, 10)]
    public float MaxDisplayDuration { get; set; } = 5f;

    [ModdedNumberOption("Capture Delay", 3f, 20f)]
    public float CaptureDelay { get; set; } = 8f;

    [ModdedNumberOption("Photo Height (Pixels)", 360f, 1080f, 180f)]
    public float PhotoHeight { get; set; } = 720f;

    [ModdedNumberOption("Camera Placement Range", 1f, 4f, 0.5f)]
    public float PlacementRange { get; set; } = 2f;

    [ModdedNumberOption("Photo View Height", 4f, 8f, 0.5f)]
    public float PhotoViewHeight { get; set; } = 6f;

    [ModdedNumberOption("Photo Aspect Ratio", 1f, 2f, 0.1f)]
    public float PhotoAspect { get; set; } = 1.6f;

    [ModdedNumberOption("Photo JPEG Quality", 60f, 95f, 5f)]
    public float PhotoQuality { get; set; } = 90f;
}