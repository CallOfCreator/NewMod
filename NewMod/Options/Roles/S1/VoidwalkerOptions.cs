using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class VoidwalkerOptions : AbstractOptionGroup<Voidwalker>
{
    public override string GroupName => "Voidwalker";

    [ModdedNumberOption("Enter Void Cooldown", min: 5, max: 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float EnterVoidCooldown { get; set; } = 30f;

    [ModdedNumberOption("Void Time", min: 5, max: 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float VoidTime { get; set; } = 15f;
}