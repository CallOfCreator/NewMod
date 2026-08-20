using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using NewMod.Options;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class LazyModifier : GameModifier, INewModModifier
{
    public override string ModifierName => "Lazy";
    public override bool HideOnUi => false;
    public ModifierFaction Faction => ModifierFaction.Crew;

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.LazyAmount;
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<ModifiersOptions>.Instance.LazyChance;
    }

    public override void OnActivate()
    {
        if (!AmongUsClient.Instance.AmHost || Player.Data.Tasks.Count <= 1)
            return;

        Player.Data.RpcSetTasks(new byte[] { Player.Data.Tasks[0].TypeId });
    }

    public override string GetDescription()
    {
        return "You only receive a single task.";
    }
}