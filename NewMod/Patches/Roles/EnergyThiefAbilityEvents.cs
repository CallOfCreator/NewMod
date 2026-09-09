using HarmonyLib;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Reactor.Utilities;

namespace NewMod.Patches.Roles;

public static class EnergyThiefAbilityEvents
{
    [RegisterEvent]
    public static void BlockBrownout(MiraButtonClickEvent evt)
    {
        if (evt.Button is not IEnergyAbility ability || !EnergyThief.IsBrownedOut(PlayerControl.LocalPlayer.PlayerId, ability.Category))
            return;

        evt.Cancel();
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#D96BFF>Brownout:</color> {ability.Category} abilities are locked."));
    }

    [RegisterEvent]
    public static void CaptureAbility(MiraButtonClickEvent evt)
    {
        if (!evt.IsCancelled && evt.Button is IEnergyAbility { CaptureOnClick: true } ability && evt.Button.CanClick())
            EnergyThief.ReportAbilityUse(ability.Category);
    }

    [RegisterEvent]
    public static void BlockVanillaBrownout(VanillaButtonClickEvent evt)
    {
        var category = PlayerControl.LocalPlayer.Data.Role.Role switch
        {
            RoleTypes.Scientist or RoleTypes.Tracker or RoleTypes.Detective or RoleTypes.Judge => EnergyCategory.Intelligence,
            RoleTypes.Engineer or RoleTypes.Phantom => EnergyCategory.Mobility,
            RoleTypes.GuardianAngel => EnergyCategory.Protection,
            RoleTypes.Shapeshifter => EnergyCategory.Control,
            RoleTypes.Viper => EnergyCategory.Aggression,
            _ => (EnergyCategory?)null
        };

        if (!category.HasValue || !EnergyThief.IsBrownedOut(PlayerControl.LocalPlayer.PlayerId, category.Value))
            return;

        evt.Cancel();
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#D96BFF>Brownout:</color> {category.Value} abilities are locked."));
    }

    [RegisterEvent]
    public static void CaptureVanillaAbility(VanillaButtonClickEvent evt)
    {
        if (evt.IsCancelled)
            return;

        var category = PlayerControl.LocalPlayer.Data.Role.Role switch
        {
            RoleTypes.Scientist or RoleTypes.Tracker or RoleTypes.Detective or RoleTypes.Judge => EnergyCategory.Intelligence,
            RoleTypes.Engineer or RoleTypes.Phantom => EnergyCategory.Mobility,
            RoleTypes.GuardianAngel => EnergyCategory.Protection,
            RoleTypes.Shapeshifter => EnergyCategory.Control,
            RoleTypes.Viper => EnergyCategory.Aggression,
            _ => (EnergyCategory?)null
        };

        if (category.HasValue)
            EnergyThief.ReportAbilityUse(category.Value);
    }

    [RegisterEvent]
    public static void BlockKillBrownout(BeforeMurderEvent evt)
    {
        if (!EnergyThief.IsBrownedOut(evt.Source.PlayerId, EnergyCategory.Aggression))
            return;

        evt.Cancel();

        if (evt.Source.AmOwner)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#D96BFF>Brownout:</color> Aggression abilities are locked."));
    }

    [RegisterEvent]
    public static void CaptureKill(AfterMurderEvent evt)
    {
        if (evt.Source.AmOwner)
            EnergyThief.ReportAbilityUse(EnergyCategory.Aggression);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class NewModHostTickPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        if (EnergyThief.TetherTargets.Count > 0 || EnergyThief.BreachActive)
            EnergyThief.HostFixedUpdate();

        if (Bounty.Contracts.Count > 0)
            Bounty.HostFixedUpdate();

        if (Usurper.CrownPositions.Count > 0)
            Usurper.HostFixedUpdate();

        if (Nomad.Anchors.Count == 0)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.Role is Nomad)
                Nomad.FixedUpdate(player);
    }
}