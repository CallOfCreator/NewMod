using System;
using System.Collections;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using Reactor.Utilities.Attributes;
using NewMod;
using Il2CppInterop.Runtime.Attributes;

[RegisterInIl2Cpp]
public class Toast(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer toastRend;
    public SpriteRenderer nmLogo;
    public TextMeshPro TitleText;
    public TextMeshPro Text;
    public void Awake()
    {
        toastRend = transform.Find("Background").GetComponent<SpriteRenderer>();
        nmLogo = transform.FindChild("Background/NM").GetComponent<SpriteRenderer>();
        TitleText = transform.FindChild("Background/TitleText").GetComponent<TextMeshPro>();
        Text = transform.Find("Text").GetComponent<TextMeshPro>();

        transform.localScale = new Vector3(0.3f, 0.1636f, 1f);
        nmLogo.sortingOrder = 100;
    }
    public static Toast CreateToast()
    {
        var gameObject = Instantiate(NewModAsset.Toast.LoadAsset(), HudManager.Instance.transform);
        var toast = gameObject.AddComponent<Toast>();
        return toast;
    }
    public void ShowToast(string title, string text, Color color, float displayDuration)
    {
        if (!LobbyBehaviour.Instance) return;

        TitleText.text = title;
        Text.text = text;
        Text.color = color;

        Coroutines.Start(CoAnimateToast(displayDuration));
    }

    [HideFromIl2Cpp]
    public IEnumerator CoAnimateToast(float duration)
    {
        Vector3 visiblePos = new Vector3(-0.0527f, 2.7741f, 0f);
        Vector3 hiddenPos = visiblePos + new Vector3(0f, 1.5f, 0f);

        transform.localPosition = hiddenPos;

        float t = 0f;
        const float slideTime = 0.4f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideTime;
            transform.localPosition = Vector3.Lerp(hiddenPos, visiblePos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideTime;
            transform.localPosition = Vector3.Lerp(visiblePos, hiddenPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        Destroy(gameObject);
    }
}
