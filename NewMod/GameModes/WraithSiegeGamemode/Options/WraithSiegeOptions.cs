using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace NewMod.GameModes.WraithSiegeGamemode.Options;

[MiraIgnore]
public sealed class WraithSiegeOptions : AbstractOptionGroup<WraithSiege>
{
    public override string GroupName => "Wraith Siege";

    [ModdedNumberOption("Wraith Players", 1f, 5f, 1f)]
    public float WraithPlayers { get; set; } = 2f;

    [ModdedNumberOption("Reviver Players (0 = Auto)", 0f, 14f, 1f)]
    public float ReviverPlayers { get; set; }

    [ModdedNumberOption("Round Time", 60f, 600f, 30f, MiraNumberSuffixes.Seconds)]
    public float RoundTime { get; set; } = 300f;

    [ModdedNumberOption("Reviver Tickets", 1f, 30f, 1f)]
    public float Tickets { get; set; } = 12f;

    [ModdedNumberOption("Required Deliveries", 3f, 30f, 1f)]
    public float RequiredDeliveries { get; set; } = 10f;

    [ModdedNumberOption("Wraith NPC Pool", 5f, 50f, 1f)]
    public float NpcPool { get; set; } = 24f;

    [ModdedNumberOption("Maximum Energy", 50f, 200f, 5f)]
    public float MaxEnergy { get; set; } = 100f;

    [ModdedNumberOption("Energy Regeneration", 1f, 20f, 1f)]
    public float EnergyRegen { get; set; } = 5f;

    [ModdedNumberOption("Wraith Summon Cost", 10f, 100f, 5f)]
    public float SummonCost { get; set; } = 45f;

    [ModdedNumberOption("Summon Cooldown", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float SummonCooldown { get; set; } = 3f;

    [ModdedNumberOption("Max Active NPCs Per Wraith", 1f, 6f, 1f)]
    public float MaxActiveNpcsPerWraith { get; set; } = 3f;

    [ModdedNumberOption("NPC Speed", 1f, 8f, 0.25f)]
    public float NpcSpeed { get; set; } = 2.5f;

    [ModdedNumberOption("Flag Radius", 1f, 4f, 0.25f)]
    public float FlagRadius { get; set; } = 1.75f;

    [ModdedNumberOption("Flag Capacity", 1f, 5f, 1f)]
    public float FlagCapacity { get; set; } = 3f;

    [ModdedNumberOption("Delivery Time", 0.5f, 5f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float DeliveryTime { get; set; } = 1.75f;

    [ModdedNumberOption("Wraith Burn Time", 0.5f, 5f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float BurnTime { get; set; } = 1.5f;

    [ModdedNumberOption("Wraith Respawn Time", 1f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float WraithRespawnTime { get; set; } = 5f;

    [ModdedNumberOption("Revive Range", 0.5f, 4f, 0.25f)]
    public float ReviveRange { get; set; } = 1.5f;

    [ModdedNumberOption("Banish Range", 0.5f, 4f, 0.25f)]
    public float BanishRange { get; set; } = 1.6f;

    [ModdedNumberOption("Banish Cooldown", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float BanishCooldown { get; set; } = 1f;

    [ModdedNumberOption("Wraith Kill Cooldown", 3f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float WraithKillCooldown { get; set; } = 10f;
}