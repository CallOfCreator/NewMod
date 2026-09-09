using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class AegisOptions : AbstractOptionGroup<Aegis>
{
    public enum AegisMode
    {
        BlockAndReveal,
        Block,
        WarnOnly
    }

    public enum WardVisibilityMode
    {
        OwnerOnly,
        TeamOnly,
        AllPlayers
    }

    public override string GroupName => "Aegis Options";

    [ModdedNumberOption("Barrier Cooldown", 5f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float AegisCooldown { get; set; } = 25f;

    [ModdedNumberOption("Barrier Max Uses", 1f, 5f, suffixType: MiraNumberSuffixes.None)]
    public float MaxCharges { get; set; } = 2f;

    [ModdedNumberOption("Barrier Duration", 2f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float DurationSeconds { get; set; } = 10f;

    [ModdedNumberOption("Barrier Radius", 1f, 7f, suffixType: MiraNumberSuffixes.None)]
    public float Radius { get; set; } = 3f;

    [ModdedEnumOption("Barrier Behavior", typeof(AegisMode))]
    public AegisMode Behavior { get; set; } = AegisMode.Block;

    [ModdedEnumOption("Ward Visibility", typeof(WardVisibilityMode))]
    public WardVisibilityMode Visibility { get; set; } = WardVisibilityMode.AllPlayers;
}