using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace NewMod.Options;

public class ModifiersOptions : AbstractOptionGroup
{
    public override string GroupName => "Modifiers Settings";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    [ModdedNumberOption("Sticky Amount", 0f, 2f)]
    public float StickyAmount { get; set; } = 1f;

    public ModdedNumberOption StickyChance { get; } = new("Sticky Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.StickyAmount > 0 };

    [ModdedNumberOption("Drowsy Amount", 0f, 2f)]
    public float DrowsyAmount { get; set; } = 1f;

    public ModdedNumberOption DrowsyChance { get; } = new("Drowsy Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount > 0f };

    [ModdedNumberOption("Adrenaline Amount", 0f, 2f)]
    public float AdrenalineAmount { get; set; } = 1f;

    [ModdedNumberOption("Fateful Amount", 0f, 6f)]
    public float FatefulAmount { get; set; } = 1f;

    public ModdedNumberOption FatefulChance { get; } = new("Fateful Chance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.FatefulAmount > 0f };

    [ModdedNumberOption("Lazy Amount", 0f, 6f)]
    public float LazyAmount { get; set; } = 1f;

    public ModdedNumberOption LazyChance { get; } = new("Lazy Chance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.LazyAmount > 0f };
    public ModdedNumberOption AdrenalineChance { get; } = new("Adrenaline Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.AdrenalineAmount > 0f };
    public ModdedNumberOption MomentumChance { get; } = new("Momentum Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.MomentumAmount > 0f };
    public ModdedNumberOption MarkedChance { get; } = new("Marked Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.MarkedAmount > 0f };

    [ModdedNumberOption("Momentum Amount", min: 0, max: 5, increment: 1, suffixType: MiraNumberSuffixes.None)]
    public float MomentumAmount { get; set; } = 1f;
    
    [ModdedNumberOption("Marked Amount", min: 0, max: 5, increment: 1, suffixType: MiraNumberSuffixes.None)]
    public float MarkedAmount { get; set; } = 1f;
}
