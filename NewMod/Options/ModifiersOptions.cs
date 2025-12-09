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

    [ModdedNumberOption("Sticky Amount", min: 0f, max: 6f)]
    public float StickyAmount { get; set; } = 10f;

    public ModdedNumberOption StickyChance { get; } = new("Sticky Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.StickyAmount > 0
    };

    [ModdedNumberOption("Drowsy Amount", min: 0f, max: 6f)]
    public float DrowsyAmount { get; set; } = 10f;

    public ModdedNumberOption DrowsyChance { get; } = new("Drowsy Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount > 0f
    };

    [ModdedNumberOption("Adrenaline Amount", min: 0f, max: 6f)]
    public float AdrenalineAmount { get; set; } = 10f;

    public ModdedNumberOption AdrenalineChance { get; } = new("Adrenaline Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.AdrenalineAmount > 0f
    };

    [ModdedNumberOption("Fateful Amount", min: 0f, max: 6f)]
    public float FatefulAmount { get; set; } = 1f;

    public ModdedNumberOption FatefulChance { get; } = new("Fateful Chance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.FatefulAmount > 0f
    };

    [ModdedNumberOption("Lazy Amount", min: 0f, max: 6f)]
    public float LazyAmount { get; set; } = 1f;

    public ModdedNumberOption LazyChance { get; } = new("Lazy Chance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.LazyAmount > 0f
    };
}