using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class WraithCallerOptions : AbstractOptionGroup<WraithCaller>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.WraithCaller");

    [ModdedNumberOption("NewMod.Options.WraithCaller.CallWraithCooldown", 5, 60)]
    public float CallWraithCooldown { get; set; } = 25f;

    [ModdedNumberOption("NewMod.Options.WraithCaller.TraceRange", 0.5f, 2f)]
    public float TraceRange { get; set; } = 1f;

    [ModdedNumberOption("NewMod.Options.WraithCaller.HuntDuration", 5, 45)]
    public float HuntDuration { get; set; } = 20f;

    [ModdedNumberOption("NewMod.Options.WraithCaller.RequiredNPCsToSend", 1, 5)]
    public float RequiredNPCsToSend { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.WraithCaller.NPCSpeed", 1, 5)]
    public float NPCSpeed { get; set; } = 1.25f;

    [ModdedToggleOption("NewMod.Options.WraithCaller.ShouldSwitchCamToNPC")]
    public bool ShouldSwitchCamToNPC { get; set; } = false;
}
