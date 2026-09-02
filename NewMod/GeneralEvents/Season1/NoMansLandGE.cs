using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class NoMansLandGE : IGeneralEvent
{
    public static bool Active { get; private set; }
    public static bool BlocksLocalPlayer => Active && !PlayerControl.LocalPlayer.HasModifier<InVoid>();

    public string Title => "No Man's Land";
    public string Description => "ALL COMBAT AND ABILITIES ARE OFFLINE!";
    public LoadableAsset<Sprite> Icon => NewModAsset.NoMansLandIcon;
    public Color AccentColor => new(0.78f, 0.78f, 0.78f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<global::NewMod.Options.GEOptions>.Instance.NoMansLandWeight;
    public float Duration => 20f;

    public void OnEventStart()
    {
        Active = true;

        if (!BlocksLocalPlayer)
            return;

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

        if (!PlayerControl.LocalPlayer.HasModifier<InVoid>())
            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);
    }

    public void Tick()
    {
        if (!BlocksLocalPlayer)
            return;

        foreach (var button in CustomButtonManager.Buttons)
            button.Button?.SetDisabled();
    }

    [RegisterEvent]
    public static void OnMiraButtonClick(MiraButtonClickEvent evt)
    {
        if (BlocksLocalPlayer)
            evt.Cancel();
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (Active && !evt.Source.HasModifier<InVoid>())
            evt.Cancel();
    }
}

[HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
public static class NoMansLandUseTargetPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref IUsable target)
    {
        if (NoMansLandGE.BlocksLocalPlayer)
            target = null;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.UseClosest))]
public static class NoMansLandUsePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.BlocksLocalPlayer;
    }
}

[HarmonyPatch(typeof(ReportButton), nameof(ReportButton.SetActive))]
public static class NoMansLandReportStatePatch
{
    [HarmonyPrefix]
    public static void Prefix(ref bool isActive)
    {
        if (NoMansLandGE.BlocksLocalPlayer)
            isActive = false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportClosest))]
public static class NoMansLandReportPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.BlocksLocalPlayer;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class NoMansLandReportAuthorityPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance)
    {
        return !NoMansLandGE.Active || __instance.HasModifier<InVoid>();
    }
}

[HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
public static class NoMansLandKillTargetPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref PlayerControl target)
    {
        if (NoMansLandGE.BlocksLocalPlayer)
            target = null;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
public static class NoMansLandMurderPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance)
    {
        if (!NoMansLandGE.Active || __instance.HasModifier<InVoid>())
            return true;

        __instance.isKilling = false;
        return false;
    }
}

[HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
public static class NoMansLandVentPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.BlocksLocalPlayer;
    }
}

[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
public static class NoMansLandSabotagePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.BlocksLocalPlayer;
    }
}

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
public static class NoMansLandAbilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.BlocksLocalPlayer;
    }
}

[HarmonyPatch(typeof(SecondaryAbilityButton), nameof(SecondaryAbilityButton.DoClick))]
public static class NoMansLandSecondaryAbilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !NoMansLandGE.BlocksLocalPlayer;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
public static class NoMansLandMapPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MapOptions options)
    {
        return !NoMansLandGE.BlocksLocalPlayer || options.Mode != MapOptions.Modes.Sabotage;
    }
}
