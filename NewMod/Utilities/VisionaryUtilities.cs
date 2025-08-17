using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using TMPro;

namespace NewMod.Utilities
{
    public static class VisionaryUtilities
    {
        /// <summary>
        /// The active screenshot panel currently displayed on screen.
        /// </summary>
        public static GameObject _panel;

        /// <summary>
        /// Indicates whether a screenshot is currently being displayed.
        /// </summary>
        public static bool _showing;

        /// <summary>
        /// Gets whether the Visionary screenshot panel is currently active and showing.
        /// </summary>
        public static bool IsShowing => _showing;

        /// <summary>
        /// Gets the directory where Visionary screenshots are stored. If the directory does not exist, it is created.
        /// </summary>
        public static string ScreenshotDirectory
        {
            get
            {
                string directory = Path.Combine(BepInEx.Paths.GameRootPath, "NewMod", "Screenshots");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                return directory;
            }
        }

        // <summary>
        /// Displays the most recent screenshot to the Visionary for a specified duration.  
        /// Skips if a meeting is active or another screenshot is already showing.
        /// <param name="displayDuration">The duration, in seconds, to display the screenshot.</param>
        /// <returns>An IEnumerator coroutine to manage screenshot display.</returns>
        /// </summary>
        public static IEnumerator ShowScreenshots(float displayDuration)
        {
            if (MeetingHud.Instance) yield break;
            if (_showing) yield break;
            string[] files = Directory.GetFiles(ScreenshotDirectory, "screenshot_*.png");
            if (files.Length == 0) yield break;

            Array.Sort(files);
            string latestScreenshot = files[^1];
            NewMod.Instance.Log.LogInfo($"Displaying the latest screenshot: {latestScreenshot}");

            float t = 0f;
            const float timeout = 3f;

            while (t < timeout)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(latestScreenshot, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (fs.Length > 0) break;
                }
                finally { fs.Dispose(); }
                t += Time.deltaTime;
                yield return null;
            }

            if (t >= timeout) yield break;

            byte[] data = File.ReadAllBytes(latestScreenshot);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            yield return ShowScreenshot(screenshotSprite, File.GetCreationTime(latestScreenshot), displayDuration);
        }

        /// <summary>
        /// Displays a screenshot sprite on screen with fade-in and fade-out effects.
        /// </summary>
        /// <param name="sprite">The screenshot sprite to display.</param>
        /// <param name="timestamp">The time the screenshot was taken.</param>
        /// <param name="duration">The duration, in seconds, to display the screenshot.</param>
        /// <returns>An IEnumerator coroutine</returns>
        public static IEnumerator ShowScreenshot(Sprite sprite, DateTime timestamp, float duration)
        {
            if (_panel) Object.Destroy(_panel);
            _panel = new GameObject("Visionary_ScreenshotPanel");
            _showing = true;

            var canvas = _panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            _panel.AddComponent<CanvasGroup>();
            _panel.AddComponent<GraphicRaycaster>();

            var group = _panel.GetComponent<CanvasGroup>();
            group.alpha = 0f;

            var bgObj = new GameObject("BorderOnBG");
            bgObj.transform.SetParent(_panel.transform, false);
            var bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = bgRT.anchorMax = bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(810, 610);
            bgRT.anchoredPosition = Vector2.zero;

            var imageObj = new GameObject("ScreenshotImage");
            imageObj.transform.SetParent(_panel.transform, false);
            var img = imageObj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            var imgRT = imageObj.GetComponent<RectTransform>();
            imgRT.sizeDelta = new Vector2(800, 600);
            imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0.5f, 0.5f);
            imgRT.anchoredPosition = Vector2.zero;

            var labelObj = new GameObject("ScreenshotLabel");
            labelObj.transform.SetParent(_panel.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20;
            tmp.text = $"<color=green>*Screenshot taken at: {timestamp.ToShortTimeString()}*</color>";
            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = labelRT.anchorMax = labelRT.pivot = new Vector2(0.5f, 0.5f);
            labelRT.sizeDelta = new Vector2(800, 50);
            labelRT.anchoredPosition = new Vector2(0, 380);

            float fade = 0.15f;
            float e = 0f;
            while (e < fade) { e += Time.deltaTime; group.alpha = Mathf.Clamp01(e / fade); yield return null; }

            yield return new WaitForSeconds(duration);

            e = 0f;
            while (e < fade) { e += Time.deltaTime; group.alpha = 1f - Mathf.Clamp01(e / fade); yield return null; }

            Object.Destroy(_panel);
            _panel = null;
            _showing = false;
        }

        // <summary>
        /// Loads and displays a screenshot from a given file path.  
        /// If the file does not exist, no action is taken.
        /// </summary>
        /// <param name="filePath">The full path of the screenshot file to display.</param>
        /// <param name="displayDuration">The duration, in seconds, to display the screenshot.</param>
        /// <returns>An IEnumerator coroutine for handling display.</returns>
        public static IEnumerator ShowScreenshotByPath(string filePath, float displayDuration)
        {
            if (!File.Exists(filePath)) yield break;

            byte[] data = File.ReadAllBytes(filePath);
            Texture2D tex = new(2, 2);
            tex.LoadImage(data);
            Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            yield return ShowScreenshot(screenshotSprite, File.GetCreationTime(filePath), displayDuration);
        }

        /// <summary>
        /// Deletes all screenshots from the Visionary screenshot directory.
        /// </summary>
        public static void DeleteAllScreenshots()
        {
            if (Directory.Exists(ScreenshotDirectory))
            {
                foreach (string file in Directory.GetFiles(ScreenshotDirectory, "*.png"))
                {
                    File.Delete(file);
                    NewMod.Instance.Log.LogInfo($"Deleted screenshot: {file}");
                }
            }
        }
    }
}
