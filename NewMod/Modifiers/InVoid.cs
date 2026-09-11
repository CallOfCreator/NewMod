using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using NewMod.Components.ScreenEffects;
using NewMod.GeneralEvents;
using NewMod.GeneralEvents.Season1;
using NewMod.Options.Roles.S1;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class InVoid : BaseModifier
{
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

        if (GeneralEventManager.CurrentEvent is IdentityCrisisGE)
            Player.RawSetOutfit(Player.Data.DefaultOutfit, PlayerOutfitType.Default);

        if (Player.AmOwner)
        {
            HudManager.Instance.SetHudActive(Player, Player.Data.Role, true);
            SoundManager.Instance.PlaySoundImmediate(NewModAsset.EnterVoidSFX.LoadAsset(), false, 1f);

            HudManager.Instance.KillButton.Hide();
            Player.killTimer = 240f;

            var cam = Camera.main;

            if (Application.platform == RuntimePlatform.Android)
            {
                var fullScreen = HudManager.Instance.FullScreen;
                fullScreen.transform.localPosition = new Vector3(0f, 0f, -250f);
                fullScreen.color = Color.clear;
                fullScreen.gameObject.SetActive(true);
                Coroutines.Start(CoEnterVoidEffect(null, null));
            }
            else if (cam)
            {
                var oldTransition = cam.GetScreenEffect<VoidwalkerTransitionEffect>();

                if (oldTransition != null && oldTransition.Active)
                    oldTransition.Remove();

                var voidEffect = cam.GetScreenEffect<VoidwalkerVoidEffect>() ?? cam.AddScreenEffect<VoidwalkerVoidEffect>();

                var transition = cam.AddScreenEffect<VoidwalkerTransitionEffect>();

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
            if (Application.platform == RuntimePlatform.Android)
            {
                Coroutines.Start(CoExitVoidEffect(null, null));
            }
            else
            {
                var cam = Camera.main;
                var voidEffect = cam.GetScreenEffect<VoidwalkerVoidEffect>();

                var transition = cam.GetScreenEffect<VoidwalkerTransitionEffect>();

                if (transition == null || !transition.Active) transition = cam.AddScreenEffect<VoidwalkerTransitionEffect>();

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
        var duration = OptionGroupSingleton<VoidwalkerOptions>.Instance.EnterTransitionDuration;

        var timer = 0f;

        while (_voidActive && timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(timer / duration);

            var easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (Application.platform == RuntimePlatform.Android)
                HudManager.Instance.FullScreen.color = new Color(0.19f, 0.035f, 0.32f, 0.42f * easedProgress);
            else if (voidEffect != null && voidEffect.Active)
                voidEffect.amount = easedProgress;

            if (transition != null && transition.Active)
                transition.SetEnter(progress);

            yield return null;
        }

        if (!_voidActive)
            yield break;

        if (Application.platform == RuntimePlatform.Android)
            HudManager.Instance.FullScreen.color = new Color(0.19f, 0.035f, 0.32f, 0.42f);
        else if (voidEffect != null && voidEffect.Active)
            voidEffect.amount = 1f;

        if (transition != null && transition.Active)
            transition.Remove();
    }

    private IEnumerator CoExitVoidEffect(VoidwalkerVoidEffect voidEffect, VoidwalkerTransitionEffect transition)
    {
        var duration = OptionGroupSingleton<VoidwalkerOptions>.Instance.ExitTransitionDuration;

        var timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(timer / duration);

            var easedProgress = Mathf.SmoothStep(1f, 0f, progress);

            if (Application.platform == RuntimePlatform.Android)
                HudManager.Instance.FullScreen.color = new Color(0.19f, 0.035f, 0.32f, 0.42f * easedProgress);
            else if (voidEffect != null && voidEffect.Active)
                voidEffect.amount = easedProgress;

            if (transition != null && transition.Active)
                transition.SetExit(progress);

            yield return null;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            var fullScreen = HudManager.Instance.FullScreen;
            fullScreen.color = Color.clear;
            fullScreen.gameObject.SetActive(false);
            fullScreen.transform.localPosition = new Vector3(0f, 0f, -500f);
        }

        if (transition != null && transition.Active)
            transition.Remove();

        if (voidEffect != null && voidEffect.Active)
            voidEffect.Remove();
    }

    [RegisterEvent]
    public static void BeforeMurderEventThing(BeforeMurderEvent @event)
    {
        if (@event.Source.HasModifier<InVoid>() || @event.Target.HasModifier<InVoid>())
            @event.Cancel();
    }
}