using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.AegisOptions;
using UnityEngine;
using AG = NewMod.Roles.CrewmateRoles.Aegis;
using NewMod.Utilities;
using MiraAPI.Keybinds;

namespace NewMod.Buttons.Aegis
{
    /// <summary>
    /// Custom action button for the Aegis role. Places a configurable shield zone.
    /// </summary>
    public class AegisButton : CustomActionButton
    {
        /// <summary>
        /// Gets the display name for this button.
        /// </summary>
        public override string Name => "Sentinel Ward";

        /// <summary>
        /// Cooldown is driven by AegisOptions.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<AegisOptions>.Instance.AegisCooldown;

        /// <summary>
        /// Maximum number of uses is driven by AegisOptions.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<AegisOptions>.Instance.MaxCharges;

        /// <summary>
        /// Button location on HUD.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// Default keybind for Aegis.
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

        /// <summary>
        /// No “hold” effect, instant cast.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Icon for the button (replace with your actual asset).
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.Shield;

        /// <summary>
        /// Enabled only for the Aegis role.
        /// </summary>
        public override bool Enabled(RoleBehaviour role) => role is AG;

        /// <summary>
        /// On click, place the Aegis shield at the player's current position using the configured settings.
        /// </summary>
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;

            AegisUtilities.ActivateShield(player, (Vector2)player.transform.position);
        }
    }
}
