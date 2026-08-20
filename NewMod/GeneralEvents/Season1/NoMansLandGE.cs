using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class NoMansLandGE : IGeneralEvent
{
    public static bool Active { get; private set; }

    public string Title => "No Man's Land";
    public string Description => "ALL COMBAT AND ABILITIES ARE OFFLINE!";
    public LoadableAsset<Sprite> Icon => MiraAssets.Empty;
    public Color AccentColor => new(0.78f, 0.78f, 0.78f);
    public int OccurrenceChance => 10;
    public float Duration => 20f;

    public void OnEventStart()
    {
        Active = true;

        if (MapBehaviour.Instance)
            MapBehaviour.Instance.Close();

        var hud = HudManager.Instance;

        hud.UseButton.SetTarget(null);
        hud.ReportButton.SetActive(false);
        hud.KillButton.SetTarget(null);

        hud.SabotageButton.SetDisabled();
        hud.ImpostorVentButton.SetDisabled();
        hud.AbilityButton.SetDisabled();
        hud.SecondaryAbilityButton.SetDisabled();
    }

    public void OnEventEnd()
    {
        Active = false;

        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (Active)
            evt.Cancel();
    }
}

[HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
public static class NoMansLandUseTargetPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref IUsable target)
    {
        if (NoMansLandGE.Active)
            target = null;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.UseClosest))]
public static class NoMansLandUsePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(ReportButton), nameof(ReportButton.SetActive))]
public static class NoMansLandReportStatePatch
{
    [HarmonyPrefix]
    public static void Prefix(ref bool isActive)
    {
        if (NoMansLandGE.Active)
            isActive = false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportClosest))]
public static class NoMansLandReportPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class NoMansLandReportAuthorityPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
public static class NoMansLandKillTargetPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref PlayerControl target)
    {
        if (NoMansLandGE.Active)
            target = null;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
public static class NoMansLandMurderPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance)
    {
        if (!NoMansLandGE.Active)
            return true;

        __instance.isKilling = false;
        return false;
    }
}

[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.ClickHandler))]
public static class NoMansLandCustomButtonPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.FixedUpdateHandler))]
public static class NoMansLandCustomButtonStatePatch
{
    [HarmonyPostfix]
    public static void Postfix(CustomActionButton __instance)
    {
        if (NoMansLandGE.Active)
            __instance.Button?.SetDisabled();
    }
}

[HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
public static class NoMansLandVentPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
public static class NoMansLandSabotagePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
public static class NoMansLandAbilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(SecondaryAbilityButton), nameof(SecondaryAbilityButton.DoClick))]
public static class NoMansLandSecondaryAbilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.Active;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
public static class NoMansLandMapPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MapOptions options)
    {
        return !NoMansLandGE.Active || options.Mode != MapOptions.Modes.Sabotage;
    }
}