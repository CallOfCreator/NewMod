using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class MirrorBladeOptions : AbstractOptionGroup<MirrorBladeRole>
{
    public override string GroupName => "MirrorBlade Settings";

    [ModdedNumberOption("Reflect Cooldown", 5f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ReflectCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Reflect Uses", 1f, 4f, 1f, MiraNumberSuffixes.None)]
    public float MaxReflectUses { get; set; } = 2f;

    [ModdedNumberOption("Reflect Window", 2f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float ReflectWindow { get; set; } = 6f;
}