using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles.S1;

namespace NewMod.Options.Roles.S1
{
    [MiraIgnore]
    public class MirrorBladeOptions : AbstractOptionGroup<MirrorBladeRole>
    {
        public override string GroupName => "MirrorBlade Settings";

        [ModdedNumberOption("Reflect Cooldown", min: 5f, max: 60f, suffixType: MiraNumberSuffixes.Seconds)]
        public float ReflectCooldown { get; set; } = 25f;

        [ModdedNumberOption("Max Reflect Uses", min: 1f, max: 4f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
        public float MaxReflectUses { get; set; } = 2f;

        [ModdedNumberOption("Reflect Window", min: 2f, max: 15f, increment: 1f, suffixType: MiraNumberSuffixes.Seconds)]
        public float ReflectWindow { get; set; } = 6f;
    }
}