using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class VoidwalkerOptions : AbstractOptionGroup<Voidwalker>
{
    public override string GroupName => "Voidwalker";

    [ModdedNumberOption("Enter Void Cooldown", 5f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float EnterVoidCooldown { get; set; } = 30f;

    [ModdedNumberOption("Void Time", 5f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float VoidTime { get; set; } = 10f;

    [ModdedNumberOption("Enter Transition Duration", 0.05f, 1f, 0.05f, MiraNumberSuffixes.Seconds)]
    public float EnterTransitionDuration { get; set; } = 0.32f;

    [ModdedNumberOption("Exit Transition Duration", 0.05f, 1f, 0.05f, MiraNumberSuffixes.Seconds)]
    public float ExitTransitionDuration { get; set; } = 0.24f;
}
