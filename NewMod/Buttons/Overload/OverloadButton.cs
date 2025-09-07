using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Overload
{
    /// <summary>
    /// Defines a custom action button for the Overload role.
    /// This button mimics another role's ability by adopting its appearance and functionality.
    /// </summary>
    public class OverloadButton : CustomActionButton
    {
        public CustomActionButton absorbed;
        /// <summary>
        /// The display text shown on the button UI.
        /// Set by the absorbed ability.
        /// </summary>
        public string absorbedText = "";

        /// <summary>
        /// The sprite icon used on the button.
        /// Loaded from the absorbed ability.
        /// </summary>
        public LoadableAsset<Sprite> absorbedSprite;

        /// <summary>
        /// The method invoked when the Overload button is clicked.
        /// Mirrors the absorbed button's behavior.
        /// </summary>
        public System.Action absorbedOnClick;

        /// <summary>
        /// Maximum number of times the button can be used.
        /// Mirrors the absorbed button's uses.
        /// </summary>
        public int absorbedMaxUses;

        /// <summary>
        /// Cooldown in seconds for reusing the button.
        /// Mirrors the absorbed button's cooldown.
        /// </summary>
        public float absorbedCooldown;

        /// <summary>
        /// Stores the default key assigned to the absorbed button's action.
        /// Mirrors the keybind of the original absorbed button.
        /// </summary>
        public MiraKeybind absorbedKeybind;

        /// <summary>
        /// The name displayed on the button.
        /// </summary>
        public override string Name => absorbedText;

        /// <summary>
        /// Cooldown duration before the button can be reused.
        /// </summary>
        public override float Cooldown => absorbedCooldown;

        /// <summary>
        /// Number of remaining uses. Zero means unlimited.
        /// </summary>
        public override int MaxUses => absorbedMaxUses;

        /// <summary>
        /// Default keybind for Overload's Overload ability.
        /// </summary>
        public override MiraKeybind Keybind => absorbedKeybind;

        /// <summary>
        /// Determines how long the effect from clicking the button lasts. In this case, no duration is set.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// The UI position for the button.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// The icon displayed on the button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => absorbedSprite;

        /// <summary>
        /// Copies functionality and appearance from another role's button.
        /// </summary>
        /// <param name="target">The button to absorb.</param>
        public void Absorb(CustomActionButton target)
        {
            absorbed = target;

            absorbedText = target.Name;
            absorbedCooldown = target.Cooldown;
            absorbedMaxUses = target.MaxUses;
            absorbedSprite = target.Sprite;
            absorbedKeybind = target.Keybind;
            absorbedOnClick = () => target.GetType().GetMethod("OnClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              ?.Invoke(target, null);

            OverrideName(absorbedText);
            OverrideSprite(absorbedSprite.LoadAsset());

            if (absorbedMaxUses <= 0f)
            {
                Button.SetInfiniteUses();
            }
            else
            {
                SetUses(absorbedMaxUses);
            }

            SetTimer(0f);

            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, false);
            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, true);
        }

        /// <summary>
        /// Called when the player presses the button.
        /// </summary>
        protected override void OnClick()
        {
            NewMod.Instance.Log.LogError("Overload invoking absorbed action...");
            absorbedOnClick?.Invoke();
        }

        /// <summary>
        /// Determines whether this button should be active on the HUD.
        /// Only visible when the role is Overload and an ability has been absorbed.
        /// </summary>
        /// <param name="role">The role of the player.</param>
        /// <returns>True if Overload with a valid absorbed ability.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is OverloadRole && absorbed != null;
        }

        /// <summary>
        /// Determines if the button can currently be pressed.
        /// </summary>
        /// <returns>True if usable.</returns>
        public override bool CanUse()
        {
            absorbed?.FixedUpdateHandler(PlayerControl.LocalPlayer);
            return base.CanUse() && absorbed != null;
        }
    }
}
