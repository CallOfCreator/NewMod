using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using NewMod.Options;
using NewMod.Options.Modifiers;
using UnityEngine;

namespace NewMod.Modifiers
{
    public sealed class DrowsyModifier : GameModifier
    {
        public override string ModifierName => "Drowsy";
        public override bool ShowInFreeplay => true;
        public override Color FreeplayFileColor => new(0.6f, 0.7f, 1f);
        public override string GetDescription() =>
            $"Move slower (x{OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount:0.##}).";

        public override int GetAssignmentChance() => (int)OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyChance.Value;

        public override int GetAmountPerGame() => (int)OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount;

        public override void OnActivate()
        {
            Player.MyPhysics.Speed *= OptionGroupSingleton<DrowsyModifierOptions>.Instance.SpeedMultiplier;
        }

        public override void OnDeactivate()
        {
            Player.MyPhysics.Speed /= OptionGroupSingleton<DrowsyModifierOptions>.Instance.SpeedMultiplier;
        }

        public override void OnDeath(DeathReason reason)
        {
            Player.MyPhysics.Speed /= OptionGroupSingleton<DrowsyModifierOptions>.Instance.SpeedMultiplier;
        }
    }
}
