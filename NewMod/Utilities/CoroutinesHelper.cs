using System.Collections;
using System.Collections.Generic;
using NewMod.Components.ScreenEffects;
using TMPro;
using UnityEngine;

namespace NewMod.Utilities;

/// <summary>
/// Coroutines for notifications and screen effects.
/// </summary>
public static class CoroutinesHelper
{
    private static readonly Queue<string> Notifications = new();
    private static GameObject _notification;
    private static HudManager _notificationHud;

    /// <summary>
    /// Queues a notification on the task-complete overlay.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>The notification coroutine.</returns>
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

                var y = elapsed switch
                {
                    < 0.25f => Mathf.SmoothStep(-8f, 0f, elapsed / 0.25f),
                    < 2.5f => 0f,
                    _ => Mathf.SmoothStep(0f, 8f, (elapsed - 2.5f) / 0.25f),
                };
                obj.transform.localPosition = new Vector3(0f, y, Minigame.Depth - 20f);
                yield return null;
            }

            Object.Destroy(obj);
            _notification = null;
        }
    }

    /// <summary>
    /// Clears camera screen effects after a delay.
    /// </summary>
    /// <param name="cam">The camera to clear.</param>
    /// <param name="duration">Delay in seconds.</param>
    /// <returns>The effect cleanup coroutine.</returns>
    public static IEnumerator RemoveCameraEffect(Camera cam, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (cam && cam.TryGetComponent<ScreenEffectSystem>(out var effects))
            effects.Clear();
    }
}
