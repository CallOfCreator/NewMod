using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using NewMod.Buttons.Roles.S1;
using NewMod.Options.Roles.S1;
using Reactor.Utilities;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class JustLeftVoid : TimedModifier
{
    public bool DidKill;
    public override string ModifierName => "Just Left Void";
    public override float Duration => 4f;
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;

    public override void OnActivate()
    {
        Player.SetKillTimer(0f);
    }

    public override void OnDeactivate()
    {
        if (DidKill)
            return;

        CustomButtonSingleton<EnterVoid>.Instance.ResetCooldownAndOrEffect();
        Player.SetKillTimer(float.MaxValue);
    }

    [RegisterEvent]
    public static void OnMurderEvent(AfterMurderEvent @event)
    {
        if (!@event.Source.HasModifier<JustLeftVoid>() || !@event.Source.AmOwner)
            return;

        @event.Source.GetModifier<JustLeftVoid>()!.DidKill = true;

        Logger<NewMod>.Warning(CustomButtonSingleton<EnterVoid>.Instance.Timer);
        CustomButtonSingleton<EnterVoid>.Instance.SetTimer(OptionGroupSingleton<VoidwalkerOptions>.Instance.EnterVoidCooldown / 2f);
    }
}