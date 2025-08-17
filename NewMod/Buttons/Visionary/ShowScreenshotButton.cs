using MiraAPI.Utilities.Assets;
using MiraAPI.Hud;
using MiraAPI.GameOptions;
using NewMod.Options.Roles.VisionaryOptions;
using NewMod.Roles.CrewmateRoles;
using UnityEngine;
using NewMod.Utilities;
using Reactor.Utilities;
using System.Linq;
using Rewired;
using System.IO;

namespace NewMod.Buttons.Visionary
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class ShowScreenshotButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on this button.
        /// </summary>
        public override string Name => "";

        /// <summary>
        /// The cooldown time for this button, based on <see cref="VisionaryOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<VisionaryOptions>.Instance.ScreenshotCooldown;

        /// <summary>
        /// The duration of any effect triggered by this button; in this case, none.
        /// </summary>
        public override float EffectDuration => 0;

        /// <summary>
        /// The maximum number of times screenshots can be shown, based on <see cref="VisionaryOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<VisionaryOptions>.Instance.MaxScreenshots;

        /// <summary>
        /// The sprite asset for this button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.ShowScreenshotButton;

        /// <summary>
        /// The on-screen location where the button will appear.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// Default keybind for Visionary's Show ability.
        /// </summary>
        public override KeyboardKeyCode Defaultkeybind => KeyboardKeyCode.M;

        /// <summary>
        /// Checks if the button can be used, ensuring there's at least one captured screenshot.
        /// </summary>
        /// <returns>True if base conditions are met and there is a screenshot, otherwise false.</returns>
        public override bool CanUse()
        {
            return base.CanUse() && Directory.GetFiles(VisionaryUtilities.ScreenshotDirectory, "screenshot_*.png").Any();
        }

        /// <summary>
        /// Invoked when the button is clicked, starting the coroutine to display screenshots.
        /// </summary>
        protected override void OnClick()
        {
            Coroutines.Start(VisionaryUtilities.ShowScreenshots(OptionGroupSingleton<VisionaryOptions>.Instance.MaxDisplayDuration));
        }

        /// <summary>
        /// Determines whether this button is enabled for the specified role.
        /// </summary>
        /// <param name="role">The player's current role.</param>
        /// <returns>True if the role is <see cref="TheVisionary"/>, otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is TheVisionary;
        }
    }
}
