using System;
using System.IO;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

/// <summary>
///     Defines a custom action button for the role.
/// </summary>
public class CaptureButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;

    /// <summary>
    ///     The name shown on this button.
    /// </summary>
    public override string Name => "Capture";

    /// <summary>
    ///     The cooldown time before this button can be used again, based on <see cref="VisionaryOptions" />.
    /// </summary>
    public override float Cooldown => OptionGroupSingleton<VisionaryOptions>.Instance.ScreenshotCooldown;

    /// <summary>
    ///     The duration of any effect triggered by this button; here, none.
    /// </summary>
    public override float EffectDuration => 0;

    /// <summary>
    ///     The maximum number of screenshots the user can capture, based on <see cref="VisionaryOptions" />.
    /// </summary>
    public override int MaxUses => (int)OptionGroupSingleton<VisionaryOptions>.Instance.MaxScreenshots;

    /// <summary>
    ///     The icon or sprite associated with this button, set to a camera sprite.
    /// </summary>
    public override LoadableAsset<Sprite> Sprite => NewModAsset.Camera;

    /// <summary>
    ///     The location on-screen where this button appears.
    /// </summary>
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    /// <summary>
    ///     Default keybind for Visionary's Capture ability.
    /// </summary>
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

    /// <summary>
    ///     Handles the button click, capturing a screenshot and saving it to a unique path.
    /// </summary>
    protected override void OnClick()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var path = Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");
        Coroutines.Start(Utils.CaptureScreenshot(path));
    }

    /// <summary>
    ///     Determines whether this button is enabled for the given role.
    /// </summary>
    /// <param name="role">The current player's role.</param>
    /// <returns>True if the role is <see cref="TheVisionary" />, otherwise false.</returns>
    public override bool Enabled(RoleBehaviour role)
    {
        return role is TheVisionary;
    }
}