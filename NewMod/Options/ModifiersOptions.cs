using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace NewMod.Options;

public class ModifiersOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Modifiers");
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    [ModdedNumberOption("NewMod.Options.Modifiers.StickyAmount", 0f, 2f)]
    public float StickyAmount { get; set; } = 1f;

    public ModdedNumberOption StickyChance { get; } = new("NewMod.Options.Modifiers.StickyChance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.StickyAmount > 0 };

    [ModdedNumberOption("NewMod.Options.Modifiers.DrowsyAmount", 0f, 2f)]
    public float DrowsyAmount { get; set; } = 1f;

    public ModdedNumberOption DrowsyChance { get; } = new("NewMod.Options.Modifiers.DrowsyChance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount > 0f };

    [ModdedNumberOption("NewMod.Options.Modifiers.AdrenalineAmount", 0f, 2f)]
    public float AdrenalineAmount { get; set; } = 1f;
    
    public ModdedNumberOption AdrenalineChance { get; } = new("NewMod.Options.Modifiers.AdrenalineChance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.AdrenalineAmount > 0f };

    [ModdedNumberOption("NewMod.Options.Modifiers.FatefulAmount", 0f, 6f)]
    public float FatefulAmount { get; set; } = 1f;

    public ModdedNumberOption FatefulChance { get; } = new("NewMod.Options.Modifiers.FatefulChance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.FatefulAmount > 0f };

    [ModdedNumberOption("NewMod.Options.Modifiers.LazyAmount", 0f, 6f)]
    public float LazyAmount { get; set; } = 1f;

    public ModdedNumberOption LazyChance { get; } = new("NewMod.Options.Modifiers.LazyChance", 15f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.LazyAmount > 0f };
    public ModdedNumberOption MomentumChance { get; } = new("NewMod.Options.Modifiers.MomentumChance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.MomentumAmount > 0f };

    [ModdedNumberOption("NewMod.Options.Modifiers.MomentumAmount", 0, 5, 1, MiraNumberSuffixes.None)]
    public float MomentumAmount { get; set; } = 1f;
    
    public ModdedNumberOption MarkedChance { get; } = new("NewMod.Options.Modifiers.MarkedChance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.MarkedAmount > 0f };

    [ModdedNumberOption("NewMod.Options.Modifiers.MarkedAmount", 0, 5, 1, MiraNumberSuffixes.None)]
    public float MarkedAmount { get; set; } = 1f;

    [ModdedNumberOption("NewMod.Options.Modifiers.OverclockedAmount", 0, 5, 1, MiraNumberSuffixes.None)]
    public float OverclockedAmount { get; set; } = 1f;

    public ModdedNumberOption OverclockedChance { get; } = new("NewMod.Options.Modifiers.OverclockedChance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<ModifiersOptions>.Instance.OverclockedAmount > 0f };
}
