using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using NewMod.Components.ScreenEffects;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class InVoid : BaseModifier
{
    private const float EnterTransitionDuration = 0.32f;
    private const float ExitTransitionDuration = 0.24f;

    private bool _voidActive;

    public override string ModifierName => "InVoid";
    public override bool ShowInFreeplay => true;
    public override bool HideOnUi => true;

    public override string GetDescription()
    {
        return "In Void";
    }

    public override void OnActivate()
    {
        _voidActive = true;

        if (Player.AmOwner)
        {
            HudManager.Instance.KillButton.Hide();
            Player.killTimer = 240f;

            var cam = Camera.main;

            if (cam)
            {
                var oldTransition = cam.GetComponent<VoidwalkerTransitionEffect>();

                if (oldTransition)
                    Object.Destroy(oldTransition);

                var voidEffect = cam.GetComponent<VoidwalkerVoidEffect>() ?? cam.gameObject.AddComponent<VoidwalkerVoidEffect>();
                var transition = cam.gameObject.AddComponent<VoidwalkerTransitionEffect>();

                voidEffect.amount = 0f;
                transition.SetEnter(0f);

                Coroutines.Start(CoEnterVoidEffect(voidEffect, transition));
            }

            foreach (var door in ShipStatus.Instance.AllDoors)
                door.gameObject.SetActive(false);
        }

        if (!Player.AmOwner)
            Player.Visible = false;
    }

    public override void OnDeactivate()
    {
        _voidActive = false;

        if (Player.AmOwner)
        {
            var cam = Camera.main;

            if (cam)
            {
                var voidEffect = cam.GetComponent<VoidwalkerVoidEffect>();
                var transition = cam.GetComponent<VoidwalkerTransitionEffect>();

                if (!transition)
                    transition = cam.gameObject.AddComponent<VoidwalkerTransitionEffect>();

                transition.SetExit(0f);
                Coroutines.Start(CoExitVoidEffect(voidEffect, transition));
            }

            HudManager.Instance.KillButton.Show();

            foreach (var door in ShipStatus.Instance.AllDoors)
                door.gameObject.SetActive(true);

            Player.RpcAddModifier<JustLeftVoid>();
        }

        if (!Player.AmOwner)
            Player.Visible = true;
    }

    private IEnumerator CoEnterVoidEffect(VoidwalkerVoidEffect voidEffect, VoidwalkerTransitionEffect transition)
    {
        var timer = 0f;

        while (_voidActive && timer < EnterTransitionDuration)
        {
            timer += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(timer / EnterTransitionDuration);

            if (voidEffect)
                voidEffect.amount = Mathf.SmoothStep(0f, 1f, progress);

            if (transition)
                transition.SetEnter(progress);

            yield return null;
        }

        if (!_voidActive)
            yield break;

        if (voidEffect)
            voidEffect.amount = 1f;

        if (transition)
            Object.Destroy(transition);
    }

    private IEnumerator CoExitVoidEffect(VoidwalkerVoidEffect voidEffect, VoidwalkerTransitionEffect transition)
    {
        var timer = 0f;

        while (timer < ExitTransitionDuration)
        {
            timer += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(timer / ExitTransitionDuration);

            if (voidEffect)
                voidEffect.amount = Mathf.SmoothStep(1f, 0f, progress);

            if (transition)
                transition.SetExit(progress);

            yield return null;
        }

        if (transition)
            Object.Destroy(transition);

        if (voidEffect)
            Object.Destroy(voidEffect);
    }

    [RegisterEvent]
    public static void BeforeMurderEventThing(BeforeMurderEvent @event)
    {
        if (@event.Source.HasModifier<InVoid>())
            @event.Cancel();
    }
}