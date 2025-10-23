using BepInEx.Configuration;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
using NewMod.Patches;
using UnityEngine;

namespace NewMod.LocalSettings
{
    public class NewModLocalSettings(ConfigFile config) : LocalSettingsTab(config)
    {
        public override string TabName => "NewMod";
        public override LocalSettingTabAppearance TabAppearance { get; } = new()
        {
            TabButtonColor = Color.cyan,
            TabButtonHoverColor = Color.magenta,
            TabIcon = NewModAsset.NMIcon,
        };

        [LocalSliderSetting("Frame Rate Limit", min: 30f, max: 240f, "Lock your framerate (FPS)", displayValue: true, formatString: "0", roundValue: true)]
        public ConfigEntry<float> FrameRateLimit { get; } = config.Bind(
            "Performance",
            "FrameRateLimit",
            165f,
            "Frames per second limit"
        );

        [LocalToggleSetting("Enable Custom Cursor", "Enable the custom cursor from the birthday update")]
        public ConfigEntry<bool> EnableCustomCursor { get; } = config.Bind(
            "Features",
            "Cursor",
            true,
            "Enable the custom cursor from the birthday update"
        );

        [LocalToggleSetting("Always Buttons on Left (Only for Android)", "Places all custom buttons on the left side (recommended for Android devices)")]
        public ConfigEntry<bool> AlwaysButtonsLeft { get; } = config.Bind(
             "Features",
             "AlwaysButtonsLeft",
              true,
             "Place all custom buttons on the left side"
        );

        public override void OnOptionChanged(ConfigEntryBase configEntry)
        {
            base.OnOptionChanged(configEntry);

            if (configEntry == FrameRateLimit)
            {
                Application.targetFrameRate = (int)FrameRateLimit.Value;
            }

            if (configEntry == AlwaysButtonsLeft && Application.platform == RuntimePlatform.Android)
            {
                foreach (var btn in CustomButtonManager.Buttons)
                {
                    btn.SetButtonLocation(ButtonLocation.BottomLeft);
                }
            }
            if (configEntry == EnableCustomCursor)
            {
                if (EnableCustomCursor.Value)
                {
                    var cur = NewModAsset.CustomCursor.LoadAsset();
                    var tex = cur?.texture;

                    if (tex != null)
                    {
                        Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                        MainMenuPatch._cachedCursor = tex;
                    }
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }
    }
}