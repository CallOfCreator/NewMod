using System.Collections;
using System.Collections.Generic;
using NewMod.Components.ScreenEffects;
using TMPro;
using UnityEngine;

namespace NewMod.Utilities;

/// <summary>
///     Provides helper coroutines and utility methods.
/// </summary>
public static class CoroutinesHelper
{
    private static readonly Queue<string> Notifications = new();
    private static GameObject _notification;
    private static HudManager _notificationHud;

    /// <summary>
    ///     Displays a temporary notification on the screen using an overlay animation.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator CoNotify(string message)
    {
        var hud = HudManager.Instance;
        if (!hud || !hud.TaskCompleteOverlay)
            yield break;

        if (_notificationHud != hud)
        {
            Notifications.Clear();
            _notification = null;
            _notificationHud = hud;
        }

        Notifications.Enqueue(message);

        if (_notification)
            yield break;

        while (hud && hud == _notificationHud && Notifications.Count > 0)
        {
            message = Notifications.Dequeue();

            if (Constants.ShouldPlaySfx())
                SoundManager.Instance.PlaySound(hud.TaskCompleteSound, false);

            var obj = Object.Instantiate(hud.TaskCompleteOverlay.gameObject, hud.transform);
            _notification = obj;
            obj.transform.localPosition = new Vector3(0f, -8f, Minigame.Depth - 20f);
            obj.transform.SetAsLastSibling();
            obj.SetActive(true);
            var textComponent = obj.GetComponentInChildren<TextMeshPro>(true);
            var translator = textComponent.GetComponent<TextTranslatorTMP>();

            if (translator)
            {
                translator.enabled = false;
                Object.Destroy(translator);
            }

            textComponent.text = message;
            textComponent.fontSize = Mathf.Clamp(3.5f - message.Length / 20f, 2f, 3.5f);
            for (var elapsed = 0f; elapsed < 2.75f; elapsed += Time.unscaledDeltaTime)
            {
                if (!hud || !obj || hud != _notificationHud)
                    yield break;

                var y = elapsed < 0.25f ? Mathf.SmoothStep(-8f, 0f, elapsed / 0.25f) : elapsed < 2.5f ? 0f : Mathf.SmoothStep(0f, 8f, (elapsed - 2.5f) / 0.25f);
                obj.transform.localPosition = new Vector3(0f, y, Minigame.Depth - 20f);
                yield return null;
            }

            Object.Destroy(obj);
            _notification = null;
        }
    }

    /// <summary>
    ///     Coroutine that waits for a given duration and then removes
    ///     specific visual effects from a Camera.
    /// </summary>
    /// <param name="cam">The Camera to check for and remove effects from.</param>
    /// <param name="duration">The time in seconds to wait before removing the effects.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public static IEnumerator RemoveCameraEffect(Camera cam, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (cam && cam.TryGetComponent<ScreenEffectSystem>(out var effects))
            effects.Clear();
    }
}