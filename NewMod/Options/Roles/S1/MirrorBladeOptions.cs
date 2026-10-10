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
    public float ReflectCooldown { get; set; } = 30f;

    [ModdedNumberOption("Reflect Window", 0.5f, 3f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float ReflectWindow { get; set; } = 1.25f;
}
