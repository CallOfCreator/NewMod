using System.Reflection;
using HarmonyLib;
using MiraAPI.Modifiers.ModifierDisplay;
using NewMod.Modifiers;
using NewMod.Utilities;
using TMPro;

namespace NewMod.Patches
{
    [HarmonyPatch]
    public static class ModifierDisplayPatch
    {
        public static FieldInfo DescriptionTextField = AccessTools.Field(typeof(ModifierUiComponent), "desc");

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ModifierUiComponent), "FixedUpdate");
        }

        public static void Postfix(ModifierUiComponent __instance)
        {
            var modifier = __instance.Modifier;
            if (modifier == null) return;
            if (!ModifierDisplayComponent.Instance.IsOpen) return;

            var descText = DescriptionTextField.GetValue(__instance) as TextMeshPro;
            if (descText == null) return;

            if (modifier is INewModModifier newModModifier)
            {
                descText.text += $"\n\n Faction: {Utils.GetModifierFactionDisplay(newModModifier)}";
            }
        }
    }
}
