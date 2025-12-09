using MiraAPI.Modifiers.Types;
using MiraAPI.GameOptions;
using NewMod.Options;

namespace NewMod.Modifiers
{
    public class LazyModifier : GameModifier, INewModModifier
    {
        public override string ModifierName => "Lazy";
        public ModifierFaction Faction => ModifierFaction.Crew;
        public override bool HideOnUi => false;
        public override int GetAmountPerGame() => (int)OptionGroupSingleton<ModifiersOptions>.Instance.LazyAmount;
        public override int GetAssignmentChance() => OptionGroupSingleton<ModifiersOptions>.Instance.LazyChance;
        public override string GetDescription() => "You only receive a single task.";
    }
}
