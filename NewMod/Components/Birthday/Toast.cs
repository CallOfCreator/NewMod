using System;
using System.Collections;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using NewMod;
using NewMod.Utilities;
using Reactor.Utilities.Attributes;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine.Events;

[RegisterInIl2Cpp]
public class Toast(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer toastRend;
    public TextMeshPro TimerText;
    public float expandDuration = 0.20f;
    public float collapseDuration = 0.18f;
    public float overshoot = 1.06f;
    private float collapsedX = 0.15f;
    public bool isExpanded = false;

    public void Awake()
    {
        toastRend = transform.Find("Background").GetComponent<SpriteRenderer>();
        TimerText = transform.Find("Timer").GetComponent<TextMeshPro>();
    }

    public static Toast CreateToast()
    {
        var gameObject = Instantiate(NewModAsset.Toast.LoadAsset(), HudManager.Instance.transform);
        var toast = gameObject.AddComponent<Toast>();
        return toast;
    }
    [HideFromIl2Cpp]
    public void SetText(string msg)
    {
        if (TimerText) TimerText.text = msg;
    }

    [HideFromIl2Cpp]
    public void StartCountdown(TimeSpan duration)
    {
        Coroutines.Start(CoCountdown(duration));
    }
    [HideFromIl2Cpp]
    public IEnumerator CoCountdown(TimeSpan span)
    {
        var end = DateTime.UtcNow + span;
        while (true)
        {
            var left = end - DateTime.UtcNow;
            if (left.TotalSeconds <= 0) break;

            if (TimerText)
                TimerText.text = Utils.FormatSpan(left);

            yield return new WaitForSecondsRealtime(0.2f);
        }
        if (TimerText) TimerText.text = "00:00:00:00";

        ShowPopup();
    }
    [HideFromIl2Cpp]
    public static void ShowPopup()
    {
        DisconnectPopup.Instance.ShowCustom("The Birthday Update is now live! Please restart to see the new lobby and menu style.");
    }
}