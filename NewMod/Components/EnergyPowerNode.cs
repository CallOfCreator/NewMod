using NewMod.Roles.NeutralRoles;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public sealed class EnergyPowerNode(nint ptr) : MonoBehaviour(ptr)
{
    public static EnergyPowerNode Instance;
    public static int RenderQueue = 4000;
    public static string SpriteShader = "Sprites/Default";

    public SpriteRenderer _renderer;
    public Material _material;
    public ArrowBehaviour _arrow;
    public Vector3 _baseScale;
    public bool _overcharged;

    public static void Create()
    {
        DestroyCurrent();

        var node = new GameObject("EnergyThief_PowerNode") { layer = 5 };
        node.transform.SetParent(ShipStatus.Instance.transform, true);
        node.transform.position = EnergyThief.GetNodePosition();
        Instance = node.AddComponent<EnergyPowerNode>();
    }

    public static void DestroyCurrent()
    {
        if (Instance)
            Destroy(Instance.gameObject);

        Instance = null;
    }

    public void Awake()
    {
        _baseScale = new Vector3(0.25f, 0.25f, 1f);
        transform.localScale = _baseScale;

        _renderer = gameObject.AddComponent<SpriteRenderer>();
        _overcharged = EnergyThief.BreachActive;
        _renderer.sprite = (_overcharged ? NewModAsset.PowerNodeOvercharged : NewModAsset.PowerNodeActive).LoadAsset();
        _material = new Material(Shader.Find(SpriteShader))
        {
            hideFlags = HideFlags.DontSave,
            renderQueue = RenderQueue
        };
        _renderer.sharedMaterial = _material;
        _renderer.maskInteraction = SpriteMaskInteraction.None;
        _renderer.color = Color.white;
        _renderer.sortingLayerID = HudManager.Instance.ShadowQuad.sortingLayerID;
        _renderer.sortingOrder = HudManager.Instance.ShadowQuad.sortingOrder + 1;
    }

    public void Update()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var isOwner = localPlayer && localPlayer.PlayerId == EnergyThief.NodeOwnerId;
        _renderer.enabled = isOwner || EnergyThief.BreachActive;

        if (_overcharged != EnergyThief.BreachActive)
        {
            _overcharged = EnergyThief.BreachActive;
            _renderer.sprite = (_overcharged ? NewModAsset.PowerNodeOvercharged : NewModAsset.PowerNodeActive).LoadAsset();
        }

        if (_renderer.enabled)
        {
            var pulse = 1f + Mathf.Sin(Time.time * (EnergyThief.BreachActive ? 12f : 5f)) * (EnergyThief.BreachActive ? 0.12f : 0.05f);
            transform.localScale = _baseScale * pulse;
        }

        if (isOwner && !EnergyThief.BreachActive)
        {
            if (!_arrow)
            {
                var arrowObject = new GameObject("EnergyThief_NodeArrow") { layer = 5 };
                var arrowRenderer = arrowObject.AddComponent<SpriteRenderer>();
                arrowRenderer.sprite = NewModAsset.Arrow.LoadAsset();
                arrowRenderer.color = new Color(0.86f, 0.3f, 1f);
                _arrow = arrowObject.AddComponent<ArrowBehaviour>();
                _arrow.image = arrowRenderer;
            }

            _arrow.target = transform.position;
        }
        else if (_arrow)
        {
            Destroy(_arrow.gameObject);
            _arrow = null;
        }
    }

    public void OnDestroy()
    {
        if (_material)
            Destroy(_material);

        if (_arrow)
            Destroy(_arrow.gameObject);

        if (Instance == this)
            Instance = null;
    }
}