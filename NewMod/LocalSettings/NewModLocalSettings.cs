using System.Collections.Generic;
using BepInEx.Configuration;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
using NewMod.Patches;
using UnityEngine;

namespace NewMod.LocalSettings;

public class NewModLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "NewMod";

    public override LocalSettingTabAppearance TabAppearance { get; } = new() { TabButtonColor = Color.cyan, TabButtonHoverColor = Color.magenta, TabIcon = NewModAsset.NMIcon };

    [LocalToggleSetting("Enable Custom Cursor", "Enable the custom cursor from the birthday update")]
    public ConfigEntry<bool> EnableCustomCursor { get; } = config.Bind("Features", "Cursor", true, "Enable the custom cursor from the birthday update");

    [LocalToggleSetting("Always Buttons on Left (Only for Android)", "Places all custom buttons on the left side (recommended for Android devices)")]
    public ConfigEntry<bool> AlwaysButtonsLeft { get; } = config.Bind("Features", "AlwaysButtonsLeft", true, "Place all custom buttons on the left side");

    public readonly Dictionary<CustomActionButton, ButtonLocation> OriginalButtonLocations = [];

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);

        if (configEntry == AlwaysButtonsLeft && Application.platform == RuntimePlatform.Android)
        {
            if (AlwaysButtonsLeft.Value)
            {
                foreach (var button in CustomButtonManager.Buttons)
                {
                    OriginalButtonLocations.TryAdd(button, button.Location);
                    button.SetButtonLocation(ButtonLocation.BottomLeft);
                }
            }
            else
            {
                foreach (var (button, location) in OriginalButtonLocations)
                    button.SetButtonLocation(location);

                OriginalButtonLocations.Clear();
            }
        }

        if (configEntry == EnableCustomCursor)
        {
            if (EnableCustomCursor.Value)
            {
                var tex = MainMenuPatch.GetCursorTexture();

                if (tex != null)
                {
                    Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
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
