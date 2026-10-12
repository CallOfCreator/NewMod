using System.Collections;
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
            Coroutines.Start(CoCapture(evt, ability.Category));
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

        var category = EnergyThiefAbilityEvents.GetVanillaCategory(PlayerControl.LocalPlayer.Data.Role.Role);

        if (!evt.IsCancelled && category.HasValue && evt.Button.isActiveAndEnabled && evt.Button.CanInteract() && !evt.Button.IsOnCooldown)
            DeadwireRole.RpcRequestCapture(PlayerControl.LocalPlayer, (byte)category.Value);
    }

    public static IEnumerator CoCapture(MiraButtonClickEvent evt, EnergyCategory category)
    {
        var uses = evt.Button.UsesLeft;
        yield return null;
        if (!evt.IsCancelled && (evt.Button.EffectActive || evt.Button.Timer > 0f || evt.Button.UsesLeft < uses))
            DeadwireRole.RpcRequestCapture(PlayerControl.LocalPlayer, (byte)category);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source.AmOwner)
            DeadwireRole.RpcRequestCapture(evt.Source, (byte)EnergyCategory.Aggression);
    }
}
