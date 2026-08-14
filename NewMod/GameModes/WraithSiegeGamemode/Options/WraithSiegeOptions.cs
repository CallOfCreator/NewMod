using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace NewMod.GameModes.WraithSiegeGamemode.Options;

[MiraIgnore]
public sealed class WraithSiegeOptions : AbstractOptionGroup<WraithSiege>
{
    public override string GroupName => "Wraith Siege";

    [ModdedNumberOption("Wraith Players", min: 1f, max: 5f, increment: 1f)]
    public float WraithPlayers { get; set; } = 2f;

    [ModdedNumberOption("Round Time", min: 60f, max: 600f, increment: 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float RoundTime { get; set; } = 300f;

    [ModdedNumberOption("Reviver Tickets", min: 1f, max: 30f, increment: 1f)]
    public float Tickets { get; set; } = 12f;

    [ModdedNumberOption("Required Deliveries", min: 3f, max: 30f, increment: 1f)]
    public float RequiredDeliveries { get; set; } = 10f;

    [ModdedNumberOption("Wraith NPC Pool", min: 5f, max: 50f, increment: 1f)]
    public float NpcPool { get; set; } = 24f;

    [ModdedNumberOption("Maximum Energy", min: 50f, max: 200f, increment: 5f)]
    public float MaxEnergy { get; set; } = 100f;

    [ModdedNumberOption("Energy Regeneration", min: 1f, max: 20f, increment: 1f)]
    public float EnergyRegen { get; set; } = 5f;

    [ModdedNumberOption("Wraith Summon Cost", min: 10f, max: 100f, increment: 5f)]
    public float SummonCost { get; set; } = 45f;

    [ModdedNumberOption("Summon Cooldown", min: 1f, max: 10f, increment: 0.5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float SummonCooldown { get; set; } = 3f;

    [ModdedNumberOption("Max Active NPCs Per Wraith", min: 1f, max: 6f, increment: 1f)]
    public float MaxActiveNpcsPerWraith { get; set; } = 3f;

    [ModdedNumberOption("NPC Speed", min: 1f, max: 8f, increment: 0.25f)]
    public float NpcSpeed { get; set; } = 2.5f;

    [ModdedNumberOption("Flag Radius", min: 1f, max: 4f, increment: 0.25f)]
    public float FlagRadius { get; set; } = 1.75f;

    [ModdedNumberOption("Flag Capacity", min: 1f, max: 5f, increment: 1f)]
    public float FlagCapacity { get; set; } = 3f;

    [ModdedNumberOption("Delivery Time", min: 0.5f, max: 5f, increment: 0.25f, suffixType: MiraNumberSuffixes.Seconds)]
    public float DeliveryTime { get; set; } = 1.75f;

    [ModdedNumberOption("Wraith Burn Time", min: 0.5f, max: 5f, increment: 0.25f, suffixType: MiraNumberSuffixes.Seconds)]
    public float BurnTime { get; set; } = 1.5f;

    [ModdedNumberOption("Wraith Respawn Time", min: 1f, max: 10f, increment: 1f, suffixType: MiraNumberSuffixes.Seconds)]
    public float WraithRespawnTime { get; set; } = 5f;

    [ModdedNumberOption("Revive Range", min: 0.5f, max: 4f, increment: 0.25f)]
    public float ReviveRange { get; set; } = 1.5f;

    [ModdedNumberOption("Banish Range", min: 0.5f, max: 4f, increment: 0.25f)]
    public float BanishRange { get; set; } = 1.6f;

    [ModdedNumberOption("Banish Cooldown", min: 0.25f, max: 5f, increment: 0.25f, suffixType: MiraNumberSuffixes.Seconds)]
    public float BanishCooldown { get; set; } = 1f;

    [ModdedNumberOption("Wraith Kill Cooldown", min: 3f, max: 30f, increment: 1f, suffixType: MiraNumberSuffixes.Seconds)]
    public float WraithKillCooldown { get; set; } = 10f;
}