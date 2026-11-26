using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class WraithCallerOptions : AbstractOptionGroup<WraithCaller>
{
    public override string GroupName => "Wraith Caller";

    [ModdedNumberOption("Wraith Summon Cooldown", 5, 60)]
    public float CallWraithCooldown { get; set; } = 25f;

    [ModdedNumberOption("Trace Collection Range", 0.5f, 2f)]
    public float TraceRange { get; set; } = 1f;

    [ModdedNumberOption("Hunt Duration", 5, 45)]
    public float HuntDuration { get; set; } = 20f;

    [ModdedNumberOption("Required NPC Kills", 1, 5)]
    public float RequiredNPCsToSend { get; set; } = 2f;

    [ModdedNumberOption("NPC Speed", 1, 5)]
    public float NPCSpeed { get; set; } = 1.25f;

    [ModdedToggleOption("Switch cam to NPC on target send")]
    public bool ShouldSwitchCamToNPC { get; set; } = false;
}
