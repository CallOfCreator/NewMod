using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace NewMod.Utilities
{
    public static class VisionaryUtilities
    {
        /// <summary>
        ///  Gets the directory where Visionary screenshots are stored. If the directory does not exist, it is created.
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
        /// Displays the most recent screenshot to all players if F5 is pressed for a specified duration.
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
            var oldSprite = HudManager.Instance.FullScreen.sprite;
            if (Input.GetKeyDown(KeyCode.F5))
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.AmOwner)
                    {
                        HudManager.Instance.FullScreen.sprite = screenshotSprite;
                        HudManager.Instance.FullScreen.gameObject.SetActive(true);
                    }
                }
            }
            yield return new WaitForSeconds(displayDuration);
            HudManager.Instance.FullScreen.sprite = oldSprite;
            HudManager.Instance.FullScreen.gameObject.SetActive(false);
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