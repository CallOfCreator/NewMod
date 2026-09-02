using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public sealed class NomadOptions : AbstractOptionGroup<Nomad>
{
    public override string GroupName => "Nomad Settings";

    [ModdedNumberOption("Anchor Cooldown", 15f, 45f, 5f, MiraNumberSuffixes.Seconds)]
    public float AnchorCooldown { get; set; } = 25f;

    [ModdedNumberOption("Rooms Per Route", 2f, 5f)]
    public float RoomsPerRoute { get; set; } = 3f;

    [ModdedNumberOption("Score To Qualify", 2f, 6f)]
    public float ScoreGoal { get; set; } = 4f;

    [ModdedNumberOption("Wander Cooldown", 5f, 20f, 5f, MiraNumberSuffixes.Seconds)]
    public float WanderCooldown { get; set; } = 10f;
}