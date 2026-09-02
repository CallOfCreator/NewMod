using HarmonyLib;
using MiraAPI.Modifiers.ModifierDisplay;
using NewMod.Modifiers;
using NewMod.Utilities;
using TMPro;

namespace NewMod.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class ModifierDisplayPatch
{
    public static void Postfix()
    {
        var display = ModifierDisplayComponent.Instance;
        if (!display || !display.IsOpen)
            return;

        // Use the game's HUD hook; do not detour Mira's managed FixedUpdate.
        foreach (var entry in display.Modifiers)
        {
            var component = entry.Value;
            if (entry.Key is not INewModModifier modifier || !component || !component.gameObject.activeInHierarchy)
                continue;

            var description = component.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
            if (description)
                description.text = $"{entry.Key.GetDescription()}\n\nFaction: {Utils.GetModifierFactionDisplay(modifier)}";
        }
    }
}