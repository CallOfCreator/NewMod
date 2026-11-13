using System.Collections;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Patches;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public sealed class MainMenuLayout(nint ptr) : MonoBehaviour(ptr)
{
    public const string DiscordUrl = "https://discord.gg/7UKG528WxZ";
    public const string WebsiteUrl = "";

    public MainMenuManager Menu;
    public Transform LayoutRoot;
    public Transform Home;
    public Transform RightPanel;
    public SpriteRenderer Background;
    public SpriteRenderer Logo;
    public PassiveButton BackButton;
    public Sprite PrimarySprite;
    public Sprite PlainSprite;
    public TextMeshPro VersionText;
    public Transform AccountHeader;
    public bool AccountHeaderWasActive;
    public Camera MenuCamera;
    public Rect LastSafeArea;
    public float LastAspect;
    public float LastCameraSize;
    public bool ShowingSubmenu;
    public Transform AccountActions;
    public AccountTab AccountCard;
    public float AccountSlide;

    public void Initialize(MainMenuManager menu)
    {
        Menu = menu;
        MenuCamera = Camera.main;
        LayoutRoot = new GameObject("NewModMenu").transform;
        LayoutRoot.SetParent(menu.mainMenuUI.transform, false);
        Home = new GameObject("Home").transform;
        Home.SetParent(LayoutRoot, false);
        PrimarySprite = CreateButtonSprite(new Color32(13, 96, 81, 255), new Color32(30, 175, 173, 255));
        PlainSprite = CreateButtonSprite(Color.white, Color.white);

        Background = menu.transform.Find("MainUI/AspectScaler/BackgroundTexture").GetComponent<SpriteRenderer>();
        FreezeLayout(Background.transform);
        Background.transform.SetParent(menu.mainMenuUI.transform, false);
        Background.sprite = NewModAsset.MainMenuBackground.LoadAsset();
        Background.drawMode = SpriteDrawMode.Simple;
        Background.maskInteraction = SpriteMaskInteraction.None;
        Background.color = Color.white;
        FreezeLayout(menu.screenTint.transform);
        menu.screenTint.transform.SetParent(menu.mainMenuUI.transform, false);
        menu.screenTint.drawMode = SpriteDrawMode.Simple;
        menu.screenTint.maskInteraction = SpriteMaskInteraction.None;

        var amongUsLogo = menu.transform.Find("MainUI/AspectScaler/LeftPanel/Sizer/LOGO-AU").GetComponent<SpriteRenderer>();
        PlaceLogo(amongUsLogo, 3.95f, 2.2f);
        Logo = new GameObject("NewModLogo").AddComponent<SpriteRenderer>();
        Logo.sprite = NewModAsset.NewModLogo.LoadAsset();
        PlaceLogo(Logo, 3.1f, 6f);

        PlaceButton(menu.inventoryButton, -4.4f, -1.45f, 3.55f, true);
        PlaceButton(menu.playButton, 0f, -1.45f, 4.8f, true);
        PlaceButton(menu.shopButton, 4.4f, -1.45f, 3.55f, true);
        PlaceButton(menu.newsButton, -3.3f, -2.45f, 3.15f, false);
        PlaceButton(menu.myAccountButton, 0f, -2.45f, 3.15f, false);
        PlaceButton(menu.settingsButton, 3.3f, -2.45f, 3.15f, false);
        PlaceButton(menu.creditsButton, -6.75f, -3.85f, 1.7f, false);
        PlaceButton(menu.quitButton, 6.75f, -3.85f, 1.7f, false);
        CreateLinkButton(
            "Discord", -7.35f, DiscordUrl, NewModAsset.DiscordNormal.LoadAsset(), NewModAsset.DiscordHover.LoadAsset(), NewModAsset.DiscordPressed.LoadAsset());
        CreateLinkButton(
            "Website", 7.35f, WebsiteUrl, NewModAsset.WebsiteNormal.LoadAsset(), NewModAsset.WebsiteHover.LoadAsset(), NewModAsset.WebsitePressed.LoadAsset());
        menu.transform.Find("MainUI/AspectScaler/LeftPanel").gameObject.SetActive(false);

        BackButton = Instantiate(menu.creditsButton, LayoutRoot);
        BackButton.name = "BackToMainMenu";
        BackButton.OnClick = new Button.ButtonClickedEvent();
        BackButton.OnClick.AddListener((System.Action)ReturnHome);
        var backTranslator = BackButton.buttonText.GetComponent<TextTranslatorTMP>();
        backTranslator.TargetText = StringNames.BackButton;
        backTranslator.defaultStr = string.Empty;
        backTranslator.ResetOnlyWhenNoDefault = false;
        backTranslator.ResetText();
        StyleLabel(BackButton, false);
        BackButton.gameObject.SetActive(false);
        menu.ControllerSelectable.Add(BackButton);

        RightPanel = menu.transform.Find("MainUI/AspectScaler/RightPanel");
        FreezeLayout(RightPanel);
        RightPanel.GetComponent<SpriteRenderer>().enabled = false;
        RightPanel.Find("WindowShine").gameObject.SetActive(false);
        RightPanel.Find("ScreenCover").GetComponent<SpriteRenderer>().enabled = false;
        var contents = RightPanel.Find("MaskedBlackScreen");
        contents.GetComponent<SpriteRenderer>().enabled = false;
        contents.localPosition = new Vector3(0f, 0f, contents.localPosition.z);
        foreach (var size in RightPanel.GetComponentsInChildren<AspectSize>(true))
        {
            size.enabled = false;
            size.transform.localScale = Vector3.one;
        }

        foreach (var element in RightPanel.GetComponentsInChildren<UiElement>(true))
            element.ControllerNav = new ControllerNavigation { mode = ControllerNavigation.Mode.Automatic };

        var credits = RightPanel.Find("CreditsSizer");
        credits.localPosition = new Vector3(-1.2f, 0f, 8f);
        credits.localScale = new Vector3(0.85f, 0.85f, 1f);
        VersionText = ReactorVersionShower.Text;
        if (VersionText)
        {
            VersionText.transform.SetParent(LayoutRoot, false);
            VersionText.rectTransform.pivot = new Vector2(0f, 1f);
            VersionText.rectTransform.sizeDelta = new Vector2(4.4f, 5f);
            VersionText.fontSize = 1.6f;
            VersionText.enableWordWrapping = true;
            VersionText.gameObject.SetActive(false);
        }

        AccountHeader = DestroyableSingleton<AccountManager>.Instance.transform.Find("AccountTab/GameHeader");
        AccountHeaderWasActive = AccountHeader.gameObject.activeSelf;
        AccountCard = DestroyableSingleton<AccountManager>.Instance.accountTab;
        AccountActions = new GameObject("AccountActions").transform;
        AccountActions.SetParent(menu.accountButtons.transform, false);
        menu.accountButtons.transform.Find("Account_CTA").SetParent(AccountActions, false);
        menu.accountButtons.transform.Find("Stats_CTA").SetParent(AccountActions, false);
        menu.accountButtons.transform.Find("Code_CTA_Parent").SetParent(AccountActions, false);

        menu.ejectMenu.gameObject.SetActive(false);
        menu.transform.Find("MainUI/AspectScaler/DoNotPress").gameObject.SetActive(false);
        ApplyLayout();
    }

    public void CreateLinkButton(string name, float x, string url, Sprite normal, Sprite hover, Sprite pressed)
    {
        var root = new GameObject(name);
        root.SetActive(false);
        root.layer = Menu.playButton.gameObject.layer;
        root.transform.SetParent(Home, false);
        root.transform.localPosition = new Vector3(x, 3.9f, -3f);
        root.transform.localScale = Vector3.one * (0.54f / 2.56f);
        root.AddComponent<BoxCollider2D>().size = new Vector2(2.56f, 2.56f);
        var button = root.AddComponent<PassiveButton>();
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick = new Button.ButtonClickedEvent();
        button.ClickSound = Menu.playButton.ClickSound;
        button.HoverSound = Menu.playButton.HoverSound;
        button.OnUp = true;
        button.OnDown = false;
        button.OnUpGraphic = false;
        button.OnDownGraphic = false;
        button.inactiveSprites = CreateLinkState(root.transform, "Normal", normal);
        button.activeSprites = CreateLinkState(root.transform, "Hover", hover);
        button.onClickSprites = CreateLinkState(root.transform, "Pressed", pressed);
        button.OnClick.AddListener((System.Action)(() => Coroutines.Start(OpenLink(button, url))));
        Menu.ControllerSelectable.Add(button);
        root.SetActive(true);
        button.SetPassiveButtonHoverStateInactive();
    }

    public static GameObject CreateLinkState(Transform parent, string state, Sprite sprite)
    {
        var renderer = new GameObject(state).AddComponent<SpriteRenderer>();
        renderer.gameObject.layer = parent.gameObject.layer;
        renderer.transform.SetParent(parent, false);
        renderer.sprite = sprite;
        renderer.gameObject.SetActive(false);
        return renderer.gameObject;
    }

    public static IEnumerator OpenLink(PassiveButton button, string url)
    {
        if (!button.enabled)
            yield break;

        button.enabled = false;
        button.inactiveSprites.SetActive(false);
        button.activeSprites.SetActive(false);
        button.onClickSprites.SetActive(true);
        yield return new WaitForSecondsRealtime(0.12f);
        if (!button)
            yield break;

        button.onClickSprites.SetActive(false);
        button.enabled = true;
        button.SetPassiveButtonHoverStateInactive();
        if (button.gameObject.activeInHierarchy)
            Application.OpenURL(url);
    }

    public void FreezeLayout(Transform target)
    {
        var scaler = Menu.mainMenuUI.GetComponent<SlicedAspectScaler>();
        foreach (var asset in target.GetComponentsInChildren<AspectScaledAsset>(true))
        {
            asset.ScaleObject(1f);
            scaler.objectsToScale.Remove(asset);
        }

        foreach (var anchor in target.GetComponentsInChildren<AspectPosition>(true))
            anchor.enabled = false;
    }

    public void PlaceLogo(SpriteRenderer logo, float y, float width)
    {
        logo.transform.SetParent(Home, false);
        logo.transform.localPosition = new Vector3(0f, y, -1f);
        var scale = width / logo.sprite.bounds.size.x;
        logo.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void PlaceButton(PassiveButton button, float x, float y, float width, bool primary)
    {
        FreezeLayout(button.transform);
        var scale = width / button.inactiveSprites.GetComponent<SpriteRenderer>().size.x;
        button.transform.SetParent(Home, false);
        button.transform.localPosition = new Vector3(x, y, -3f);
        button.transform.localScale = new Vector3(scale, scale, 1f);
        button.ControllerNav = new ControllerNavigation { mode = ControllerNavigation.Mode.Automatic };
        StyleState(button.inactiveSprites, primary, false, false);
        StyleState(button.activeSprites, primary, true, false);
        StyleState(button.selectedSprites, primary, true, false);
        StyleState(button.selectedInactiveSprites, primary, false, false);
        StyleState(button.disabledSprites, primary, false, true);
        StyleState(button.onClickSprites, primary, true, false);
        StyleLabel(button, primary);
    }

    public static void StyleLabel(PassiveButton button, bool primary)
    {
        var text = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        button.buttonText = text;
        text.alignment = TextAlignmentOptions.MidlineRight;
        text.enableWordWrapping = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 1f;
        text.fontSizeMax = primary ? 5.5f : 4.5f;
        text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, primary ? 0.75f : 0.5f);
        button.inactiveTextColor = primary ? new Color32(4, 38, 37, 255) : new Color32(198, 254, 245, 255);
        button.activeTextColor = Color.white;
        button.selectedTextColor = Color.white;
        button.selectedInactiveTextColor = button.inactiveTextColor;
        button.disabledTextColor = Color.gray;
        text.color = button.inactiveTextColor;
    }

    public void StyleState(GameObject state, bool primary, bool highlighted, bool disabled)
    {
        if (!state)
            return;

        var renderer = state.GetComponent<SpriteRenderer>();
        var size = renderer.size;
        renderer.sprite = primary ? PrimarySprite : PlainSprite;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        if (primary)
            renderer.color = highlighted ? new Color(0.82f, 1f, 1f) : Color.white;
        else
            renderer.color = highlighted ? new Color32(19, 94, 83, 255) : new Color32(10, 56, 51, 255);
        if (disabled)
            renderer.color *= new Color(0.5f, 0.5f, 0.5f, 1f);

        var shine = state.transform.Find("Shine");
        if (shine)
            shine.GetComponent<SpriteRenderer>().color = new Color32(198, 254, 245, 255);
    }

    public static Sprite CreateButtonSprite(Color bottom, Color top)
    {
        var texture = new Texture2D(256, 64, TextureFormat.RGBA32, false)
        {
            name = "NewModMenuButton",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        var pixels = new Il2CppStructArray<Color>(256 * 64);
        for (var y = 0; y < 64; y++)
        {
            var color = Color.Lerp(bottom, top, y / 63f);
            for (var x = 0; x < 256; x++)
            {
                var corner = new Vector2(
                    Mathf.Max(8f - Mathf.Min(x + 0.5f, 255.5f - x), 0f),
                    Mathf.Max(8f - Mathf.Min(y + 0.5f, 63.5f - y), 0f));
                color.a = Mathf.Clamp01(8f - corner.magnitude);
                pixels[y * 256 + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, 256f, 64f),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(8f, 8f, 8f, 8f));
    }

    public void ApplyLayout()
    {
        LastSafeArea = Screen.safeArea;
        LastAspect = MenuCamera.aspect;
        LastCameraSize = MenuCamera.orthographicSize;
        var height = LastCameraSize * 2f;
        var width = height * LastAspect;
        var safeWidth = width * LastSafeArea.width / Screen.width;
        var safeHeight = height * LastSafeArea.height / Screen.height;
        var center = MenuCamera.ViewportToWorldPoint(new Vector3(
            LastSafeArea.center.x / Screen.width, LastSafeArea.center.y / Screen.height, -MenuCamera.transform.position.z));
        var scale = Mathf.Min(safeWidth / 16f, safeHeight / 9f);
        LayoutRoot.position = new Vector3(center.x, center.y, 0f);
        LayoutRoot.localScale = new Vector3(scale, scale, 1f);
        Background.transform.position = new Vector3(MenuCamera.transform.position.x, MenuCamera.transform.position.y, 8f);
        var bounds = Background.sprite.bounds.size;
        Background.transform.localScale = new Vector3(width / bounds.x, height / bounds.y, 1f);
        Menu.screenTint.transform.position = new Vector3(center.x, center.y, 1f);
        var tintBounds = Menu.screenTint.sprite.bounds.size;
        Menu.screenTint.transform.localScale = new Vector3(width / tintBounds.x, height / tintBounds.y, 1f);
        RightPanel.position = new Vector3(center.x, center.y, -5f);
        RightPanel.localScale = new Vector3(scale * 1.5f, scale * 1.5f, 1f);
        if (VersionText)
            VersionText.transform.localPosition = new Vector3(2.5f, 2.7f, -6f);
    }

    public void LateUpdate()
    {
        if (Screen.safeArea != LastSafeArea || !Mathf.Approximately(MenuCamera.aspect, LastAspect) || !Mathf.Approximately(MenuCamera.orthographicSize, LastCameraSize))
            ApplyLayout();

        var submenu = Menu.gameModeButtons.activeSelf || Menu.onlineButtons.activeSelf || Menu.enterCodeButtons.activeSelf ||
            Menu.accountButtons.activeSelf || Menu.creditsScreen.activeSelf || Menu.redeemCodeContainer.gameObject.activeSelf ||
            Menu.createGameScreen.gameObject.activeSelf;
        if (submenu != ShowingSubmenu)
        {
            ShowingSubmenu = submenu;
            Home.gameObject.SetActive(!submenu);
            if (!submenu)
                ControllerManager.Instance.UnsetBackButton(BackButton);
        }

        var accountOpen = Menu.accountButtons.activeInHierarchy && AccountCard.clickToCloseCollider.enabled;
        var slide = Mathf.MoveTowards(AccountSlide, accountOpen ? 1f : 0f, Time.unscaledDeltaTime / 0.3f);
        if (!Mathf.Approximately(slide, AccountSlide))
        {
            AccountSlide = slide;
            AccountActions.localPosition = Vector3.left * (3.2f * Mathf.SmoothStep(0f, 1f, slide));
        }

        var needsBack = Menu.gameModeButtons.activeSelf || Menu.accountButtons.activeSelf || Menu.creditsScreen.activeSelf;
        if (BackButton.gameObject.activeSelf != needsBack)
            BackButton.gameObject.SetActive(needsBack);

        if (Menu.ejectMenu.gameObject.activeSelf)
            Menu.ejectMenu.gameObject.SetActive(false);

        if (VersionText && VersionText.gameObject.activeSelf != Menu.creditsScreen.activeSelf)
            VersionText.gameObject.SetActive(Menu.creditsScreen.activeSelf);

        var showAccountHeader = !Menu.finishStartup || Menu.accountButtons.activeSelf || !Menu.mainMenuUI.activeInHierarchy;
        if (AccountHeader.gameObject.activeSelf != showAccountHeader)
            AccountHeader.gameObject.SetActive(showAccountHeader);
    }

    public void OpenSubmenu(UiElement selection)
    {
        Home.gameObject.SetActive(false);
        ShowingSubmenu = true;
        BackButton.gameObject.SetActive(true);
        ControllerManager.Instance.SetBackButton(BackButton);
        ControllerManager.Instance.SetCurrentSelected(selection);
    }

    public void ReturnHome()
    {
        if (Menu.animating)
            return;

        Menu.ResetScreen();
        ControllerManager.Instance.UnsetBackButton(BackButton);
        Home.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(false);
        ShowingSubmenu = false;
        ControllerManager.Instance.SetCurrentSelected(Menu.playButton);
    }

    public void OnDestroy()
    {
        if (AccountHeader)
            AccountHeader.gameObject.SetActive(AccountHeaderWasActive);
        Destroy(PrimarySprite.texture);
        Destroy(PrimarySprite);
        Destroy(PlainSprite.texture);
        Destroy(PlainSprite);
    }
}
