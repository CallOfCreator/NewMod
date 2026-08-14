using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using NewMod.Options;
using NewMod.Options.Modifiers;

namespace NewMod.Modifiers;

public class AdrenalineModifier : GameModifier
{
    public override string ModifierName => "Adrenaline";
    public override bool HideOnUi => false;

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.AdrenalineChance.Value;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.AdrenalineAmount;
    }

    public override string GetDescription()
    {
        return $"Move faster (x{OptionGroupSingleton<AdrenalineModifierOptions>.Instance.SpeedMultiplier:0.##}).";
    }

    public override void OnActivate()
    {
        Player.MyPhysics.Speed *= OptionGroupSingleton<AdrenalineModifierOptions>.Instance.SpeedMultiplier;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed /= OptionGroupSingleton<AdrenalineModifierOptions>.Instance.SpeedMultiplier;
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.MyPhysics.Speed /= OptionGroupSingleton<AdrenalineModifierOptions>.Instance.SpeedMultiplier;
    }
}