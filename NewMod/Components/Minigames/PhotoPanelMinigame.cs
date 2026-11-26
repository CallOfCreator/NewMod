using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using NewMod.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NewMod.Components.Minigames;

[RegisterInIl2Cpp]
public class PhotoPanelMinigame(nint ptr) : Minigame(ptr)
{
    public SpriteMask Mask;
    public PassiveButton RightButton;
    public PassiveButton LeftButton;
    public GameObject RightHover;
    public GameObject LeftHover;
    public PassiveButton CloseButton;
    public SpriteRenderer Photo;
    public List<(int Id, Sprite Sprite)> Photos;
    public int Page;
    public bool Preview;
    public float DisplayDuration;
    public float ExpiresAt;

    public void Awake()
    {
        Mask = transform.Find("UI/Mask").GetComponent<SpriteMask>();
        Mask.enabled = false;
        RightButton = transform.Find("UI/RightButton").GetComponent<PassiveButton>();
        RightHover = RightButton.transform.Find("Hover").gameObject;
        LeftButton = transform.Find("UI/LeftButton").GetComponent<PassiveButton>();
        LeftHover = LeftButton.transform.Find("Hover").gameObject;
        CloseButton = transform.Find("CloseButton").GetComponent<PassiveButton>();

        RightHover.SetActive(false);
        LeftHover.SetActive(false);
        foreach (var (button, hover, step) in new[] { (LeftButton, LeftHover, -1), (RightButton, RightHover, 1) })
        {
            button.OnMouseOver = new UnityEvent();
            button.OnMouseOver.AddListener((UnityAction)(() => hover.SetActive(true)));
            button.OnMouseOut = new UnityEvent();
            button.OnMouseOut.AddListener((UnityAction)(() => hover.SetActive(false)));
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityAction)(() => ChangePage(step)));
        }

        foreach (var button in new[] { LeftButton, RightButton, CloseButton })
        {
            foreach (var renderer in button.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingLayerID = Mask.frontSortingLayerID;
                renderer.sortingOrder = renderer.transform.IsChildOf(RightHover.transform) || renderer.transform.IsChildOf(LeftHover.transform) ? 151 : 150;
                renderer.maskInteraction = SpriteMaskInteraction.None;
            }
        }

        CloseButton.OnClick = new Button.ButtonClickedEvent();
        CloseButton.OnClick.AddListener((UnityAction)(() => Close()));

        Photo = new GameObject("Photo").AddComponent<SpriteRenderer>();
        Photo.gameObject.layer = Mask.gameObject.layer;
        Photo.transform.SetParent(Mask.transform, false);
        Photo.maskInteraction = SpriteMaskInteraction.None;
        Photo.sortingLayerID = Mask.frontSortingLayerID;
        Photo.sortingOrder = 1;
        var frame = transform.Find("UI").GetComponent<SpriteRenderer>();
        frame.maskInteraction = SpriteMaskInteraction.None;
        frame.sortingLayerID = Mask.frontSortingLayerID;
        frame.sortingOrder = 0;
    }

    [HideFromIl2Cpp]
    public static PhotoPanelMinigame CreateMinigame(List<(int Id, Sprite Sprite)> photos, float duration, bool preview)
    {
        var gameObject = Instantiate(NewModAsset.PhotoPanelMinigame.LoadAsset(), HudManager.Instance.transform);
        gameObject.transform.localPosition = new Vector3(0f, 0f, Depth);
        gameObject.transform.localScale = Vector3.one * 0.5f;
        foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = LayerMask.NameToLayer("UI");
        var minigame = gameObject.AddComponent<PhotoPanelMinigame>();
        minigame.Photos = photos;
        minigame.Preview = preview;
        minigame.DisplayDuration = duration;
        minigame.Page = preview ? Math.Max(0, photos.FindIndex(photo => photo.Id == VisionaryUtilities.SelectedCamera)) : 0;
        minigame.Open();
        return minigame;
    }

    public void Open()
    {
        foreach (var button in new[] { LeftButton, RightButton })
        {
            button.gameObject.SetActive(true);
            button.enabled = Preview && Photos.Count > 1;
            button.GetComponent<SpriteRenderer>().color = button.enabled ? Color.white : Color.gray;
        }

        ChangePage(0);
        Begin(null);
        SetupInput(false, true);
    }

    public void ChangePage(int step)
    {
        if (step != 0 && (!Preview || Photos.Count < 2))
            return;
        Page = (Page + step + Photos.Count) % Photos.Count;
        var photo = Photos[Page];
        Photo.sprite = photo.Sprite;
        var screen = Mask.sprite.bounds;
        var size = Photo.sprite.rect.size / Photo.sprite.pixelsPerUnit;
        var vertices = Mask.sprite.vertices;
        for (var i = 0; i < vertices.Length; i++)
            vertices[i] = new Vector2(Mathf.InverseLerp(screen.min.x, screen.max.x, vertices[i].x) * Photo.sprite.rect.width, Mathf.InverseLerp(screen.min.y, screen.max.y, vertices[i].y) * Photo.sprite.rect.height);
        Photo.sprite.OverrideGeometry(vertices, Mask.sprite.triangles);
        Photo.transform.localPosition = screen.center;
        Photo.transform.localScale = new Vector3(screen.size.x / size.x, screen.size.y / size.y, 1f);
        if (Preview)
            VisionaryUtilities.SelectedCamera = photo.Id;
        ExpiresAt = Time.time + DisplayDuration;
    }

    public void Update()
    {
        if (!amOpening && amClosing == CloseState.None && (Time.time >= ExpiresAt || Input.GetKeyDown(KeyCode.Escape)))
            Close();
    }

    public void OnDisable()
    {
        LeftHover.SetActive(false);
        RightHover.SetActive(false);
    }
}
