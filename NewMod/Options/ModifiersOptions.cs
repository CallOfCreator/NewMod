using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace NewMod.Options;

public class ModifiersOptions : AbstractOptionGroup
{
    public override string GroupName => "Modifiers Settings";
    public override Color GroupColor => Color.blue;
    public override bool ShowInModifiersMenu => true;

    [ModdedNumberOption("Sticky Amount", min:0f, max:2f)]
    public float StickyAmount { get; set; } = 1f;

    public ModdedNumberOption StickyChance { get; } = new("Sticky Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.StickyAmount > 0
    };

    [ModdedNumberOption("Drowsy Amount", min:0f, max:2f)]
    public float DrowsyAmount { get; set; } = 1f;

    public ModdedNumberOption DrowsyChance { get; } =new("Drowsy Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount > 0f
    };

    [ModdedNumberOption("Adrenaline Amount", min:0f, max:2f)]
    public float AdrenalineAmount { get; set; } = 1f;

    public ModdedNumberOption AdrenalineChance { get; } =new("Adrenaline Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.AdrenalineAmount > 0f
    };

}