using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class CrimsonVortexEscapeHud(nint ptr) : MonoBehaviour(ptr)
{
    public static CrimsonVortexEscapeHud Instance { get; private set; }

    private GameObject _root;
    private SpriteRenderer _glow;
    private SpriteRenderer _fill;
    private TextMeshPro _label;
    private TextMeshPro _percentage;
    private Texture2D _texture;
    private Sprite _sprite;

    private float _pulse;
    private float _successEndsAt;
    private int _lastPercentage = -1;
    private State _state;

    private enum State
    {
        Hidden,
        Mashing,
        Escaping,
        Success
    }

    public void Awake()
    {
        Instance = this;

        _texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        _texture.filterMode = FilterMode.Bilinear;
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.hideFlags = HideFlags.HideAndDontSave;

        _texture.SetPixel(0, 0, Color.white);
        _texture.SetPixel(1, 0, Color.white);
        _texture.SetPixel(0, 1, Color.white);
        _texture.SetPixel(1, 1, Color.white);
        _texture.Apply();

        _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);

        _sprite.hideFlags = HideFlags.HideAndDontSave;

        _root = new GameObject("CrimsonVortexEscapeHud");
        _root.layer = LayerMask.NameToLayer("UI");
        _root.transform.SetParent(transform, false);

        var aspectPosition = _root.AddComponent<AspectPosition>();

        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Bottom;

        aspectPosition.DistanceFromEdge = new Vector3(0f, 1.22f, -40f);

        aspectPosition.AdjustPosition();

        var glowObject = new GameObject("Glow");
        glowObject.layer = LayerMask.NameToLayer("UI");
        glowObject.transform.SetParent(_root.transform, false);
        glowObject.transform.localScale = new Vector3(4.55f, 0.58f, 1f);

        _glow = glowObject.AddComponent<SpriteRenderer>();
        _glow.sprite = _sprite;
        _glow.color = new Color(0.95f, 0.02f, 0.04f, 0.18f);
        _glow.sortingOrder = 90;

        var borderObject = new GameObject("Border");
        borderObject.layer = LayerMask.NameToLayer("UI");
        borderObject.transform.SetParent(_root.transform, false);
        borderObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        borderObject.transform.localScale = new Vector3(4.4f, 0.47f, 1f);

        var border = borderObject.AddComponent<SpriteRenderer>();

        border.sprite = _sprite;
        border.color = new Color(0.9f, 0.035f, 0.05f, 1f);
        border.sortingOrder = 91;

        var backgroundObject = new GameObject("Background");

        backgroundObject.layer = LayerMask.NameToLayer("UI");
        backgroundObject.transform.SetParent(_root.transform, false);

        backgroundObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);

        backgroundObject.transform.localScale = new Vector3(4.26f, 0.33f, 1f);

        var background = backgroundObject.AddComponent<SpriteRenderer>();

        background.sprite = _sprite;
        background.color = new Color(0.24f, 0.008f, 0.015f, 0.98f);
        background.sortingOrder = 92;

        var fillObject = new GameObject("Fill");
        fillObject.layer = LayerMask.NameToLayer("UI");
        fillObject.transform.SetParent(_root.transform, false);
        fillObject.transform.localPosition = new Vector3(-2.1f, 0f, -0.03f);
        fillObject.transform.localScale = new Vector3(0f, 0.23f, 1f);

        _fill = fillObject.AddComponent<SpriteRenderer>();
        _fill.sprite = _sprite;
        _fill.color = new Color(0.95f, 0.025f, 0.045f, 1f);
        _fill.sortingOrder = 93;

        var labelObject = new GameObject("Label");
        labelObject.layer = LayerMask.NameToLayer("UI");
        labelObject.transform.SetParent(_root.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0.49f, -0.04f);

        _label = labelObject.AddComponent<TextMeshPro>();
        _label.font = HudManager.Instance.TaskPanel.taskText.font;
        _label.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        _label.fontSize = 1.65f;
        _label.fontStyle = FontStyles.Bold;
        _label.alignment = TextAlignmentOptions.Center;
        _label.enableWordWrapping = false;
        _label.richText = true;
        _label.rectTransform.sizeDelta = new Vector2(7f, 0.8f);
        _label.renderer.sortingOrder = 94;

        var percentageObject = new GameObject("Percentage");

        percentageObject.layer = LayerMask.NameToLayer("UI");
        percentageObject.transform.SetParent(_root.transform, false);

        percentageObject.transform.localPosition = new Vector3(0f, -0.005f, -0.05f);

        _percentage = percentageObject.AddComponent<TextMeshPro>();

        _percentage.font = HudManager.Instance.TaskPanel.taskText.font;

        _percentage.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;

        _percentage.fontSize = 1.2f;
        _percentage.fontStyle = FontStyles.Bold;
        _percentage.alignment = TextAlignmentOptions.Center;
        _percentage.enableWordWrapping = false;
        _percentage.rectTransform.sizeDelta = new Vector2(7f, 0.8f);
        _percentage.renderer.sortingOrder = 95;

        ForceHide();
    }

    public void LateUpdate()
    {
        if (_state == State.Success && Time.time >= _successEndsAt)
        {
            ForceHide();
            return;
        }

        if (!_root.activeSelf)
            return;

        _pulse = Mathf.MoveTowards(_pulse, 0f, Time.deltaTime * 5.5f);

        var scale = 1f + _pulse * 0.065f;

        _root.transform.localScale = new Vector3(scale, scale, 1f);

        _glow.color = new Color(0.95f, 0.02f, 0.04f, 0.14f + _pulse * 0.28f);
    }

    public void SetProgress(float progress)
    {
        if (_state != State.Mashing)
        {
            _state = State.Mashing;

            _label.text = "MASH <color=#FFFFFF>[SPACE]</color> TO BREAK FREE!";

            _label.color = new Color(0.95f, 0.025f, 0.045f, 1f);
        }

        _root.SetActive(true);

        progress = Mathf.Clamp01(progress);

        var width = 4.2f * progress;

        _fill.transform.localScale = new Vector3(width, 0.23f, 1f);

        _fill.transform.localPosition = new Vector3(-2.1f + width * 0.5f, 0f, -0.03f);

        _fill.color = Color.Lerp(new Color(0.58f, 0.01f, 0.025f, 1f), new Color(0.95f, 0.025f, 0.045f, 1f), progress);

        var percentage = Mathf.RoundToInt(progress * 100f);

        if (percentage != _lastPercentage)
        {
            _lastPercentage = percentage;
            _percentage.text = percentage + "%";
        }
    }

    public void Pulse()
    {
        _pulse = 1f;
    }

    public void ShowBreakingFree()
    {
        _state = State.Escaping;
        _root.SetActive(true);

        _label.text = "BREAKING FREE!";
        _label.color = new Color(1f, 0.72f, 0.15f, 1f);

        _percentage.text = string.Empty;

        _fill.transform.localScale = new Vector3(4.2f, 0.23f, 1f);

        _fill.transform.localPosition = new Vector3(0f, 0f, -0.03f);

        _fill.color = new Color(1f, 0.72f, 0.15f, 1f);

        _pulse = 1f;
    }

    public void ShowSuccess()
    {
        _state = State.Success;
        _successEndsAt = Time.time + 0.8f;
        _root.SetActive(true);

        _label.text = "ESCAPED!";
        _label.color = new Color(0.22f, 1f, 0.64f, 1f);

        _percentage.text = string.Empty;

        _fill.transform.localScale = new Vector3(4.2f, 0.23f, 1f);

        _fill.transform.localPosition = new Vector3(0f, 0f, -0.03f);

        _fill.color = new Color(0.22f, 1f, 0.64f, 1f);

        _pulse = 1f;
    }

    public void Hide()
    {
        if (_state == State.Success && Time.time < _successEndsAt)
        {
            return;
        }

        ForceHide();
    }

    public void ForceHide()
    {
        _state = State.Hidden;
        _pulse = 0f;
        _successEndsAt = 0f;
        _lastPercentage = -1;
        _root.transform.localScale = Vector3.one;
        _root.SetActive(false);
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Object.Destroy(_sprite);
        Object.Destroy(_texture);
    }
}