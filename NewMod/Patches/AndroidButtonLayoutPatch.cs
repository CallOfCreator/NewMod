using System.Collections;
using HarmonyLib;
using MiraAPI;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using MiraHudManagerPatches = MiraAPI.Patches.HudManagerPatches;

namespace NewMod.Patches;

[HarmonyPatch]
public static class AndroidButtonLayoutPatch
{
    private const float JoystickButtonPadding = 0.05f;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyAfter(MiraApiPlugin.Id)]
    public static void HudManagerStartPostfix(HudManager __instance)
    {
        Coroutines.Start(CoPositionAfterJoystickCreated(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.SetControlType))]
    public static void SetControlTypePostfix()
    {
        PositionLeftButtons(HudManager.Instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.UpdateJoystickSize))]
    public static void UpdateJoystickSizePostfix()
    {
        PositionLeftButtons(HudManager.Instance);
    }

    private static IEnumerator CoPositionAfterJoystickCreated(HudManager hudManager)
    {
        while (hudManager && hudManager.joystick == null) yield return null;

        if (hudManager) PositionLeftButtons(hudManager);
    }

    private static void PositionLeftButtons(HudManager hudManager)
    {
        if (Application.platform != RuntimePlatform.Android) return;

        var bottomLeft = MiraHudManagerPatches.BottomLeft;
        var bottomRight = MiraHudManagerPatches.BottomRight;

        if (bottomLeft == null || bottomRight == null) return;

        var bottomLeftPosition = bottomLeft.GetComponent<AspectPosition>();
        var bottomRightPosition = bottomRight.GetComponent<AspectPosition>();
        var distanceFromEdge = bottomLeftPosition.DistanceFromEdge;

        distanceFromEdge.x = bottomRightPosition.DistanceFromEdge.x;

        var joystick = hudManager.joystick?.TryCast<VirtualJoystick>();
        if (joystick != null)
        {
            var joystickPosition = joystick.GetComponent<AspectPosition>();
            var gridArrange = bottomLeft.GetComponent<GridArrange>();
            var joystickRadius = joystick.OuterRadius * Mathf.Abs(joystick.transform.localScale.x);
            var buttonRadius = gridArrange.CellSize.x * 0.5f;

            distanceFromEdge.x = Mathf.Max(distanceFromEdge.x, joystickPosition.DistanceFromEdge.x + joystickRadius + buttonRadius + JoystickButtonPadding);
        }

        bottomLeftPosition.DistanceFromEdge = distanceFromEdge;
        bottomLeftPosition.AdjustPosition();
    }
}