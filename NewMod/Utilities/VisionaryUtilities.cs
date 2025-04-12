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
        public static List<string> CapturedScreenshotPaths = new();

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

        /// <summary>
        /// Displays the most recent screenshot to the Visionary for a specified duration.
        /// </summary>
        /// <param name="displayDuration">The duration, in seconds, to display the screenshot.</param>
        /// <returns>An IEnumerator that retrieves the latest screenshot.</returns>
        public static IEnumerator ShowScreenshots(float displayDuration)
        {
            string[] files = Directory.GetFiles(ScreenshotDirectory, "screenshot_*.png");
            if (files.Length == 0) yield break;

            Array.Sort(files);
            string latestScreenshot = files[files.Length - 1];
            NewMod.Instance.Log.LogInfo($"Displaying the latest screenshot: {latestScreenshot}");

            while (!File.Exists(latestScreenshot))
            {
                yield return null;
            }

            byte[] data = File.ReadAllBytes(latestScreenshot);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            var screenshotPanel = new GameObject("Visionary_ScreenshotPanel");
            var canvas = screenshotPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
           
            var group = screenshotPanel.AddComponent<CanvasGroup>();
            group.alpha = 0;

            var imageObj = new GameObject("ScreenshotImage");
            imageObj.transform.SetParent(screenshotPanel.transform, false);
            var image = imageObj.AddComponent<Image>();
            image.sprite = screenshotSprite;
            image.preserveAspect = true;

            var rt = imageObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 600);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var bgObj = new GameObject("BorderOnBG");
            bgObj.transform.SetParent(screenshotPanel.transform, false);
            var bjImage = bgObj.AddComponent<Image>();
            bjImage.color = new Color(0f, 0f, 0f, 0.6f);

            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = bgRT.anchorMax = bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(810, 610);
            bgRT.anchoredPosition = Vector2.zero;
            bgObj.transform.SetAsFirstSibling();

            var labelObj = new GameObject("Screenshot Label");
            labelObj.transform.SetParent(screenshotPanel.transform, false);
            var label = labelObj.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 20;
            DateTime captureTime = File.GetCreationTime(latestScreenshot);
            label.text = $"<color=green>*Screenshot taken at: {captureTime.ToShortTimeString()}*</color>";

            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = labelRT.anchorMax = labelRT.pivot = new Vector2(0.5f, 0.5f);
            labelRT.anchoredPosition = new Vector2(0, 380);
            labelRT.sizeDelta = new Vector2(800, 50);

            float fadeDuration = 1f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                group.alpha = alpha;
                yield return null;
            }
            group.alpha = 1f;

            yield return new WaitForSeconds(displayDuration);

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                group.alpha = alpha;
                yield return null;
            }
            group.alpha = 0f;

            Object.Destroy(screenshotPanel);
        }

        /// <summary>
        /// Displays a screenshot from the given file path to the Visionary for a specified duration.
        /// </summary>
        /// <param name="filePath">The full file path of the screenshot to display.</param>
        /// <param name="displayDuration">The duration, in seconds, to display the screenshot.</param>
        /// <returns>An IEnumerator that handles fading the screenshot in and out.</returns>
        public static IEnumerator ShowScreenshotByPath(string filePath, float displayDuration)
        {
            if (!File.Exists(filePath)) yield break;

            byte[] data = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            var screenshotPanel = new GameObject("Visionary_ScreenshotPanel");
            var canvas = screenshotPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var group = screenshotPanel.AddComponent<CanvasGroup>();
            group.alpha = 0;

            var imageObj = new GameObject("ScreenshotImage");
            imageObj.transform.SetParent(screenshotPanel.transform, false);
            var image = imageObj.AddComponent<Image>();
            image.sprite = screenshotSprite;
            image.preserveAspect = true;

            var rt = imageObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 600);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var bgObj = new GameObject("BorderOnBG");
            bgObj.transform.SetParent(screenshotPanel.transform, false);
            var bjImage = bgObj.AddComponent<Image>();
            bjImage.color = new Color(0f, 0f, 0f, 0.6f);

            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = bgRT.anchorMax = bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(810, 610);
            bgRT.anchoredPosition = Vector2.zero;
            bgObj.transform.SetAsFirstSibling();

            var labelObj = new GameObject("Screenshot Label");
            labelObj.transform.SetParent(screenshotPanel.transform, false);
            var label = labelObj.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 20;
            DateTime captureTime = File.GetCreationTime(filePath);
            label.text = $"<color=green>*Screenshot taken at: {captureTime.ToShortTimeString()}*</color>";

            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = labelRT.anchorMax = labelRT.pivot = new Vector2(0.5f, 0.5f);
            labelRT.anchoredPosition = new Vector2(0, 380);
            labelRT.sizeDelta = new Vector2(800, 50);

            float fadeDuration = 1f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                group.alpha = alpha;
                yield return null;
            }
            group.alpha = 1f;

            yield return new WaitForSeconds(displayDuration);

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                group.alpha = alpha;
                yield return null;
            }
            group.alpha = 0f;

            Object.Destroy(screenshotPanel);
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
