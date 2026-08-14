using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace NewMod.GeneralEvents;

[RegisterInIl2Cpp]
public class GeneralEventHud(IntPtr ptr) : MonoBehaviour(ptr)
{
    public TextMeshPro _titleText;
    public TextMeshPro _descText;
    public SpriteRenderer _logo;
    public SpriteRenderer _background;
    public Animator _animator;
    public IEnumerator _typewriterCoro;

    public void Awake()
    {
        _animator = transform.Find("UI").GetComponent<Animator>();
        _background = transform.Find("UI/Background").GetComponent<SpriteRenderer>();
        _titleText = transform.Find("UI/TitleText").GetComponent<TextMeshPro>();
        _descText = transform.Find("UI/DescText").GetComponent<TextMeshPro>();
        _logo = transform.Find("UI/GE_Logo").GetComponent<SpriteRenderer>();

        gameObject.SetActive(false);
    }

    [HideFromIl2Cpp]
    public void Show(IGeneralEvent ge)
    {
        gameObject.SetActive(true);

        _titleText.text = ge.Title;
        _logo.sprite = ge.Icon?.LoadAsset();

        var accent = ge.AccentColor;
        _background.color = new Color(accent.r, accent.g, accent.b, 0.85f);

        _descText.maxVisibleCharacters = 0;
        _descText.text = $"<size=70%><color=#FFD700><cspace=0.15em>W A R N I N G</cspace></color></size>\n" + $"<size=100%><color=#FF4D00><b>{ge.Description}</b></color></size>";

        if (_typewriterCoro != null)
            Coroutines.Stop(_typewriterCoro);

        _typewriterCoro = Coroutines.Start(CoTypewriter(_descText));

        if (_animator)
            _animator.Play("GEShow");
    }

    [HideFromIl2Cpp]
    public void Hide()
    {
        Coroutines.Start(CoHide());
    }

    [HideFromIl2Cpp]
    public IEnumerator CoHide()
    {
        if (_animator)
        {
            _animator.Play("GEHide");
            yield return new WaitForSeconds(0.25f);
        }

        Destroy(gameObject);
    }

    [HideFromIl2Cpp]
    public IEnumerator CoTypewriter(TMP_Text tmp)
    {
        yield return new WaitForSeconds(0.3f);

        var total = tmp.text.Length;
        for (var i = 0; i <= total; i++)
        {
            tmp.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.022f);
        }
    }

    public static GeneralEventHud Create()
    {
        var go = Instantiate(NewModAsset.GeneralEventHud.LoadAsset(), HudManager.Instance.transform);
        go.transform.localPosition = new Vector3(0.1018f, 2.1127f, 0f);
        var hud = go.AddComponent<GeneralEventHud>();
        return hud;
    }
}