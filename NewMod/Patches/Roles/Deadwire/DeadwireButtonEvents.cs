using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Utilities;
using DeadwireRole = NewMod.Roles.ImpostorRoles.S1.Deadwire;

namespace NewMod.Patches.Roles.Deadwire;

public static class DeadwireButtonEvents
{
    [RegisterEvent]
    public static void OnButtonClick(MiraButtonClickEvent evt)
    {
        if (DeadwireRole.IsJammed(PlayerControl.LocalPlayer.PlayerId))
        {
            evt.Cancel();
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF6B72>Jammed:</color> Abilities are disabled."));
            return;
        }

        if (!evt.IsCancelled && evt.Button is IEnergyAbility { CaptureOnClick: true } ability && evt.Button.CanClick())
            DeadwireRole.RpcRequestCapture(PlayerControl.LocalPlayer, (byte)ability.Category);
    }

    [RegisterEvent]
    public static void OnVanillaButtonClick(VanillaButtonClickEvent evt)
    {
        if (DeadwireRole.IsJammed(PlayerControl.LocalPlayer.PlayerId))
        {
            evt.Cancel();
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF6B72>Jammed:</color> Abilities are disabled."));
            return;
        }

        var category = PlayerControl.LocalPlayer.Data.Role.Role switch
        {
            RoleTypes.Scientist or RoleTypes.Tracker or RoleTypes.Detective or RoleTypes.Judge => EnergyCategory.Intelligence,
            RoleTypes.Engineer or RoleTypes.Phantom => EnergyCategory.Mobility,
            RoleTypes.GuardianAngel => EnergyCategory.Protection,
            RoleTypes.Shapeshifter => EnergyCategory.Control,
            RoleTypes.Viper => EnergyCategory.Aggression,
            _ => (EnergyCategory?)null
        };

        if (!evt.IsCancelled && category.HasValue)
            DeadwireRole.RpcRequestCapture(PlayerControl.LocalPlayer, (byte)category.Value);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source.AmOwner)
            DeadwireRole.RpcRequestCapture(evt.Source, (byte)EnergyCategory.Aggression);
    }
}