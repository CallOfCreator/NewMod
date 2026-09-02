using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewMod.Utilities;

public static class VisionaryUtilities
{
    public static GameObject _panel;
    public static bool _showing;

    public static bool IsShowing => _showing;
    public static bool IsCapturing { get; set; }
    public static bool HasScreenshots { get; set; }

    public static string ScreenshotDirectory
    {
        get
        {
            var directory = Path.Combine(Application.persistentDataPath, "NewMod", "Screenshots");
            Directory.CreateDirectory(directory);
            return directory;
        }
    }

    public static IEnumerator ShowScreenshots(float displayDuration)
    {
        if (MeetingHud.Instance || _showing || IsCapturing || !HasScreenshots)
            yield break;

        var files = Directory.GetFiles(ScreenshotDirectory, "screenshot_*.png");

        if (files.Length == 0)
        {
            HasScreenshots = false;
            yield break;
        }

        Array.Sort(files, StringComparer.Ordinal);
        var latestScreenshot = files[^1];

        var data = File.ReadAllBytes(latestScreenshot);
        var texture = new Texture2D(2, 2);

        if (!texture.LoadImage(data))
        {
            Object.Destroy(texture);
            yield break;
        }

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        yield return ShowScreenshot(sprite, File.GetCreationTime(latestScreenshot), displayDuration);

        Object.Destroy(sprite);
        Object.Destroy(texture);
    }

    public static IEnumerator ShowScreenshot(Sprite sprite, DateTime timestamp, float duration)
    {
        if (_panel)
            Object.Destroy(_panel);

        _panel = new GameObject("Visionary_ScreenshotPanel");
        _showing = true;

        var canvas = _panel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = _panel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        var group = _panel.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        _panel.AddComponent<GraphicRaycaster>();

        var backgroundObject = new GameObject("BorderOnBG");
        backgroundObject.transform.SetParent(_panel.transform, false);
        var background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.6f);
        var backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = backgroundRect.anchorMax = backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(810f, 610f);

        var imageObject = new GameObject("ScreenshotImage");
        imageObject.transform.SetParent(_panel.transform, false);
        var image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        var imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = imageRect.anchorMax = imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.sizeDelta = new Vector2(800f, 600f);

        var labelObject = new GameObject("ScreenshotLabel");
        labelObject.transform.SetParent(_panel.transform, false);
        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.font = HudManager.Instance.TaskPanel.taskText.font;
        label.fontSharedMaterial = HudManager.Instance.TaskPanel.taskText.fontSharedMaterial;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 20f;
        label.text = $"<color=#58E8BE>Captured at {timestamp.ToShortTimeString()}</color>";
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = labelRect.anchorMax = labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(800f, 50f);
        labelRect.anchoredPosition = new Vector2(0f, 330f);

        const float fadeDuration = 0.15f;
        for (var elapsed = 0f; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            group.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;
        yield return new WaitForSeconds(duration);

        for (var elapsed = 0f; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            group.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        Object.Destroy(_panel);
        _panel = null;
        _showing = false;
    }

    public static void DeleteAllScreenshots()
    {
        if (_panel)
            Object.Destroy(_panel);

        _panel = null;
        _showing = false;
        IsCapturing = false;
        HasScreenshots = false;

        foreach (var file in Directory.GetFiles(ScreenshotDirectory, "screenshot_*.png"))
            File.Delete(file);
    }
}