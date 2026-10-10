using System;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class NewModLoadingBar(nint ptr) : MonoBehaviour(ptr)
{
    public AmongUsLoadingBar Bar;
    public Image Sheen;
    public Image Edge;
    public RectTransform FillMask;
    public Texture2D FillTexture;
    public Texture2D LightTexture;
    public Sprite FillSprite;
    public Sprite LightSprite;
    public float Target;
    public float Displayed;
    public float Velocity;

    public void Initialize(AmongUsLoadingBar bar)
    {
        Bar = bar;
        Target = Displayed = Velocity = 0f;

        if (!FillMask)
        {
            CreateTextures();
            Bar.barFill.overrideSprite = FillSprite;
            Bar.barFill.type = Image.Type.Filled;
            Bar.barFill.fillMethod = Image.FillMethod.Horizontal;
            Bar.barFill.fillOrigin = 0;
            Bar.barFill.color = Color.white;

            var mask = new GameObject("NewModFillMask") { layer = gameObject.layer };
            FillMask = mask.AddComponent<RectTransform>();
            FillMask.SetParent(transform, false);
            FillMask.anchorMin = Vector2.zero;
            FillMask.anchorMax = Vector2.one;
            FillMask.sizeDelta = Vector2.zero;
            mask.AddComponent<RectMask2D>();

            Sheen = CreateImage("NewModLoadingSheen", FillMask, LightSprite);
            Sheen.color = new Color(1f, 0.8f, 1f, 0.5f);
            Edge = CreateImage("NewModLoadingEdge", FillMask, LightSprite);
            Edge.color = new Color(1f, 0.85f, 1f, 0.85f);
            foreach (var image in Bar.GetComponentsInChildren<Image>(true))
            {
                if (!image.sprite || !image.sprite.name.Contains("logo", StringComparison.OrdinalIgnoreCase))
                    continue;

                image.overrideSprite = NewModAsset.NewModLogo.LoadAsset();
                image.preserveAspect = true;
                image.color = Color.white;
                image.rectTransform.anchoredPosition += Vector2.down * (Bar.barFill.rectTransform.rect.height * 0.35f);
            }
        }

        Refresh();
    }

    public Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        var image = new GameObject(name) { layer = gameObject.layer };
        image.AddComponent<RectTransform>().SetParent(parent, false);
        var graphic = image.AddComponent<Image>();
        graphic.sprite = sprite;
        graphic.raycastTarget = false;
        return graphic;
    }

    public void CreateTextures()
    {
        FillTexture = new Texture2D(256, 32, TextureFormat.RGBA32, false);
        LightTexture = new Texture2D(64, 32, TextureFormat.RGBA32, false);
        FillTexture.wrapMode = LightTexture.wrapMode = TextureWrapMode.Clamp;
        FillTexture.filterMode = LightTexture.filterMode = FilterMode.Bilinear;
        var accent = new Color32(167, 2, 172, 255);
        for (var y = 0; y < 32; y++)
        {
            var height = y / 31f;
            for (var x = 0; x < 256; x++)
            {
                var color = Color.Lerp(new Color32(48, 8, 77, 255), accent, Mathf.SmoothStep(0.3f, 1f, x / 255f));
                color = Color.Lerp(color * 0.55f, color, Mathf.Sin(height * Mathf.PI));
                var gloss = Mathf.Exp(-Mathf.Pow((height - 0.78f) / 0.16f, 2f)) * 0.32f;
                color = Color.Lerp(color, new Color(0.95f, 0.65f, 1f), gloss);
                color.a = 1f;
                FillTexture.SetPixel(x, y, color);
            }

            for (var x = 0; x < 64; x++)
            {
                var band = (x / 63f - 0.5f + (height - 0.5f) * 0.3f) / 0.2f;
                var alpha = Mathf.Exp(-band * band * 2f) * Mathf.Sin(height * Mathf.PI);
                LightTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        FillTexture.Apply(false);
        LightTexture.Apply(false);
        FillSprite = Sprite.Create(FillTexture, new Rect(0f, 0f, 256f, 32f), new Vector2(0.5f, 0.5f));
        LightSprite = Sprite.Create(LightTexture, new Rect(0f, 0f, 64f, 32f), new Vector2(0.5f, 0.5f));
    }

    public void SetProgress(float percent)
    {
        Target = Mathf.Clamp01(percent / 100f);
        if (Target < Displayed)
        {
            Displayed = Target;
            Velocity = 0f;
        }

        Refresh();
    }

    public void Update()
    {
        if (!Bar)
            return;

        Displayed = Mathf.SmoothDamp(Displayed, Target, ref Velocity, 0.2f, Mathf.Infinity, Time.unscaledDeltaTime);
        if (Mathf.Abs(Target - Displayed) < 0.0005f)
            Displayed = Target;
        Refresh();
    }

    public void Refresh()
    {
        var rect = Bar.barFill.rectTransform;
        Bar.barFill.fillAmount = Displayed;
        FillMask.anchorMax = new Vector2(Displayed, 1f);
        var width = rect.rect.width * Displayed;
        var height = rect.rect.height;
        Sheen.rectTransform.sizeDelta = new Vector2(rect.rect.width * 0.2f, height);
        Sheen.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-width * 0.5f - Sheen.rectTransform.sizeDelta.x,
            width * 0.5f + Sheen.rectTransform.sizeDelta.x, Mathf.Repeat(Time.unscaledTime * 0.3f, 1f)), 0f);
        Edge.rectTransform.sizeDelta = new Vector2(height * 0.7f, height);
        Edge.rectTransform.anchoredPosition = new Vector2(width * 0.5f - height * 0.12f, 0f);

        var position = Bar.crewmate.position;
        position.x = Mathf.Lerp(rect.TransformPoint(new Vector3(rect.rect.xMin, 0f, 0f)).x,
            rect.TransformPoint(new Vector3(rect.rect.xMax, 0f, 0f)).x, Displayed);
        Bar.crewmate.position = position;
    }

    public void OnDestroy()
    {
        Destroy(FillSprite);
        Destroy(LightSprite);
        Destroy(FillTexture);
        Destroy(LightTexture);
    }
}
