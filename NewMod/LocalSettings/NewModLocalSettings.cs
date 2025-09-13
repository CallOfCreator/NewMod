using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
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

        public override void OnOptionChanged(ConfigEntryBase configEntry)
        {
            base.OnOptionChanged(configEntry);

            if (configEntry == FrameRateLimit)
            {
                Application.targetFrameRate = (int)FrameRateLimit.Value;
            }
        }
    }
}