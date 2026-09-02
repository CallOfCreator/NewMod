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
    private Animator _animator;
    private SpriteRenderer _background;
    private Vector3 _backgroundScale;
    private Color _descColor;
    private Vector3 _descPosition;
    private TextMeshPro _descText;
    private IEnumerator _hideCoro;
    private SpriteRenderer _logo;

    private Color _logoColor;
    private Vector3 _logoRotation;
    private Vector3 _logoScale;

    private IEnumerator _showCoro;
    private Color _titleColor;
    private Vector3 _titlePosition;
    private TextMeshPro _titleText;
    private Transform _ui;

    private Vector3 _uiPosition;

    public void Awake()
    {
        _ui = transform.Find("UI");
        _animator = _ui.GetComponent<Animator>();
        _background = transform.Find("UI/Background").GetComponent<SpriteRenderer>();
        _titleText = transform.Find("UI/TitleText").GetComponent<TextMeshPro>();
        _descText = transform.Find("UI/DescText").GetComponent<TextMeshPro>();
        _logo = transform.Find("UI/GE_Logo").GetComponent<SpriteRenderer>();

        _animator.enabled = false;
        _background.transform.localPosition = new Vector3(0f, 0f, 0.1f);

        _uiPosition = _ui.localPosition;
        _backgroundScale = _background.transform.localScale;
        _logoScale = _logo.transform.localScale;
        _logoRotation = _logo.transform.localEulerAngles;
        _titlePosition = _titleText.transform.localPosition;
        _descPosition = _descText.transform.localPosition;

        _logoColor = _logo.color;
        _titleColor = _titleText.color;
        _descColor = _descText.color;

        gameObject.SetActive(false);
    }

    public void Update()
    {
        GeneralEventManager.CurrentEvent?.Tick();
    }

    [HideFromIl2Cpp]
    public void Show(IGeneralEvent ge)
    {
        gameObject.SetActive(true);

        if (_showCoro != null)
            Coroutines.Stop(_showCoro);

        if (_hideCoro != null)
            Coroutines.Stop(_hideCoro);

        var accent = ge.AccentColor;
        var accentHex = ColorUtility.ToHtmlStringRGB(accent);

        _titleText.text = ge.Title.ToUpperInvariant();
        _logo.sprite = ge.Icon?.LoadAsset();

        var panelColor = Color.Lerp(new Color(0.025f, 0.03f, 0.045f, 1f), accent, 0.26f);

        _background.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0.96f);

        _titleText.color = Color.Lerp(Color.white, accent, 0.2f);

        _descText.text = $"<size=58%><color=#{accentHex}><cspace=0.14em>G E N E R A L   E V E N T</cspace></color></size>\n" + $"<size=100%><color=#FFFFFF><b>{ge.Description}</b></color></size>";

        _descText.ForceMeshUpdate();
        _descText.maxVisibleCharacters = 0;

        _showCoro = Coroutines.Start(CoShow());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoShow()
    {
        var logoColor = _logoColor;
        logoColor.a = 0f;
        _logo.color = logoColor;

        var titleColor = _titleText.color;
        titleColor.a = 0f;
        _titleText.color = titleColor;

        var descColor = _descColor;
        descColor.a = 0f;
        _descText.color = descColor;

        _ui.localPosition = _uiPosition + new Vector3(0.38f, 0.05f, 0f);
        _ui.localScale = new Vector3(0.96f, 0.92f, 1f);

        _background.transform.localScale = new Vector3(_backgroundScale.x * 0.06f, _backgroundScale.y * 0.84f, _backgroundScale.z);

        _logo.transform.localScale = _logoScale * 1.65f;
        _logo.transform.localEulerAngles = _logoRotation + new Vector3(0f, 0f, -12f);

        _titleText.transform.localPosition = _titlePosition + new Vector3(0.22f, 0f, 0f);

        _descText.transform.localPosition = _descPosition + new Vector3(0.12f, -0.03f, 0f);

        var backgroundColor = _background.color;
        var targetBackgroundAlpha = backgroundColor.a;
        backgroundColor.a = 0f;
        _background.color = backgroundColor;

        HudManager.Instance.PlayerCam.ShakeScreen(0.12f, 0.025f);

        for (var time = 0f; time < 0.14f; time += Time.deltaTime)
        {
            var progress = Mathf.SmoothStep(0f, 1f, time / 0.14f);

            _background.transform.localScale = new Vector3(Mathf.Lerp(_backgroundScale.x * 0.06f, _backgroundScale.x * 1.06f, progress), Mathf.Lerp(_backgroundScale.y * 0.84f, _backgroundScale.y * 1.02f, progress), _backgroundScale.z);

            backgroundColor.a = targetBackgroundAlpha * progress;
            _background.color = backgroundColor;

            _ui.localPosition = Vector3.Lerp(_uiPosition + new Vector3(0.38f, 0.05f, 0f), _uiPosition, progress);

            _ui.localScale = Vector3.Lerp(new Vector3(0.96f, 0.92f, 1f), new Vector3(1.015f, 1.015f, 1f), progress);

            yield return null;
        }

        backgroundColor.a = targetBackgroundAlpha;
        _background.color = backgroundColor;
        _ui.localPosition = _uiPosition;

        for (var time = 0f; time < 0.12f; time += Time.deltaTime)
        {
            var progress = Mathf.SmoothStep(0f, 1f, time / 0.12f);

            _background.transform.localScale = Vector3.Lerp(new Vector3(_backgroundScale.x * 1.06f, _backgroundScale.y * 1.02f, _backgroundScale.z), _backgroundScale, progress);

            _ui.localScale = Vector3.Lerp(new Vector3(1.015f, 1.015f, 1f), Vector3.one, progress);

            logoColor.a = progress;
            _logo.color = logoColor;

            _logo.transform.localScale = Vector3.Lerp(_logoScale * 1.65f, _logoScale * 0.92f, progress);

            _logo.transform.localEulerAngles = Vector3.Lerp(_logoRotation + new Vector3(0f, 0f, -12f), _logoRotation + new Vector3(0f, 0f, 2f), progress);

            yield return null;
        }

        for (var time = 0f; time < 0.12f; time += Time.deltaTime)
        {
            var progress = Mathf.SmoothStep(0f, 1f, time / 0.12f);

            _logo.transform.localScale = Vector3.Lerp(_logoScale * 0.92f, _logoScale, progress);

            _logo.transform.localEulerAngles = Vector3.Lerp(_logoRotation + new Vector3(0f, 0f, 2f), _logoRotation, progress);

            titleColor.a = progress;
            _titleText.color = titleColor;

            _titleText.transform.localPosition = Vector3.Lerp(_titlePosition + new Vector3(0.22f, 0f, 0f), _titlePosition, progress);

            yield return null;
        }

        for (var time = 0f; time < 0.1f; time += Time.deltaTime)
        {
            var progress = Mathf.SmoothStep(0f, 1f, time / 0.1f);

            descColor.a = progress;
            _descText.color = descColor;

            _descText.transform.localPosition = Vector3.Lerp(_descPosition + new Vector3(0.12f, -0.03f, 0f), _descPosition, progress);

            yield return null;
        }

        _descText.ForceMeshUpdate();

        var characters = _descText.textInfo.characterCount;
        var revealDuration = Mathf.Clamp(characters * 0.014f, 0.45f, 1.1f);

        for (var time = 0f; time < revealDuration; time += Time.deltaTime)
        {
            var progress = Mathf.Clamp01(time / revealDuration);
            _descText.maxVisibleCharacters = Mathf.CeilToInt(characters * progress);
            yield return null;
        }

        _descText.maxVisibleCharacters = characters;

        for (var time = 0f; time < 0.14f; time += Time.deltaTime)
        {
            var progress = Mathf.SmoothStep(0f, 1f, time / 0.14f);

            _logo.transform.localScale = Vector3.Lerp(_logoScale * 1.06f, _logoScale, progress);

            yield return null;
        }

        _background.transform.localScale = _backgroundScale;
        _ui.localScale = Vector3.one;
        _logo.transform.localScale = _logoScale;
        _logo.transform.localEulerAngles = _logoRotation;
        _logo.color = _logoColor;
        titleColor.a = 1f;
        _titleText.color = titleColor;
        _titleText.transform.localPosition = _titlePosition;
        _descText.color = _descColor;
        _descText.transform.localPosition = _descPosition;
        _showCoro = null;
    }

    [HideFromIl2Cpp]
    public void Hide()
    {
        if (_hideCoro != null)
            return;

        if (_showCoro != null)
        {
            Coroutines.Stop(_showCoro);
            _showCoro = null;
        }

        _hideCoro = Coroutines.Start(CoHide());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoHide()
    {
        var startBackgroundScale = _background.transform.localScale;
        var startUiPosition = _ui.localPosition;
        var startLogoScale = _logo.transform.localScale;

        var backgroundColor = _background.color;
        var logoColor = _logo.color;
        var titleColor = _titleText.color;
        var descColor = _descText.color;

        for (var time = 0f; time < 0.2f; time += Time.deltaTime)
        {
            var progress = Mathf.SmoothStep(0f, 1f, time / 0.2f);

            descColor.a = 1f - progress;
            titleColor.a = 1f - progress;
            logoColor.a = 1f - progress;

            _descText.color = descColor;
            _titleText.color = titleColor;
            _logo.color = logoColor;

            _logo.transform.localScale = Vector3.Lerp(startLogoScale, _logoScale * 0.72f, progress);

            _titleText.transform.localPosition = _titlePosition + new Vector3(progress * 0.14f, 0f, 0f);

            _ui.localPosition = Vector3.Lerp(startUiPosition, _uiPosition + new Vector3(0.18f, 0f, 0f), progress);

            _background.transform.localScale = new Vector3(Mathf.Lerp(startBackgroundScale.x, _backgroundScale.x * 0.08f, progress), Mathf.Lerp(startBackgroundScale.y, _backgroundScale.y * 0.82f, progress), startBackgroundScale.z);

            backgroundColor.a = Mathf.Lerp(backgroundColor.a, 0f, progress);
            _background.color = backgroundColor;

            yield return null;
        }

        Destroy(gameObject);
    }

    [HideFromIl2Cpp]
    public static GeneralEventHud Create()
    {
        var go = Instantiate(NewModAsset.GeneralEventHud.LoadAsset(), HudManager.Instance.transform);

        go.transform.localPosition = new Vector3(0.1018f, 2.1127f, -20f);
        go.transform.localScale = Vector3.one;

        return go.AddComponent<GeneralEventHud>();
    }
}