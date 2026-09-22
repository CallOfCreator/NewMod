using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class TyrantOptions : AbstractOptionGroup<Tyrant>
{
    public override string GroupName => "Tyrant";
    [ModdedNumberOption("Intimidate Cooldown", 10f, 60f)]
    public float PulseCooldown { get; set; } = 25f;
    [ModdedNumberOption("Intimidate Warning", 0.5f, 3f, 0.5f)]
    public float PulseWarning { get; set; } = 1f;
    [ModdedNumberOption("Fear Pulse Radius", 1f, 5f, 0.5f)]
    public float FearPulseRadius { get; set; } = 2.5f;
    [ModdedNumberOption("Fear Pulse Duration", 1f, 6f)]
    public float FearPulseDuration { get; set; } = 3f;
    [ModdedNumberOption("Fear Pulse Slowdown", 5f, 40f, 5f)]
    public float FearPulseSpeed { get; set; } = 20f;
}
