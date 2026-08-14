using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using NewMod.Options;
using NewMod.Options.Modifiers;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class FatefulModifier : GameModifier, INewModModifier
{
    public override string ModifierName => "Fateful";
    public override bool HideOnUi => false;
    public ModifierFaction Faction => ModifierFaction.Crew;

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.FatefulAmount;
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<ModifiersOptions>.Instance.FatefulChance;
    }

    public override string GetDescription()
    {
        return $"Each completed task has a {OptionGroupSingleton<FatefulModifierOptions>.Instance.DeathChance}% chance to kill you.";
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        if (!evt.Player.HasModifier<FatefulModifier>()) return;

        var chance = OptionGroupSingleton<FatefulModifierOptions>.Instance.DeathChance;
        var roll = Random.Range(0f, 100f);
        if (roll >= chance) return;

        evt.Player.RpcMurderPlayer(evt.Player, true);
        evt.Player.RemoveModifier<FatefulModifier>();
    }
}