using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.OverloadOptions;
using NewMod.Roles.NeutralRoles;
using Rewired;
using UnityEngine;

namespace NewMod.Buttons.Overload
{
    /// <summary>
    /// The Final Ability button for Overload.
    /// Unlocks after reaching the required absorbed charge count.
    /// </summary>
    public class FinalAbilityButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on the button (if any).
        /// </summary>
        public override string Name => "OVERLOAD";

        /// <summary>
        /// Cooldown (none for final ability).
        /// </summary>
        public override float Cooldown => 0f;

        /// <summary>
        /// One-time use. Set to 1.
        /// </summary>
        public override int MaxUses => 1;

        /// <summary>
        /// Default keybind for the Final Ability button.
        /// </summary>
        public override KeyboardKeyCode Defaultkeybind => KeyboardKeyCode.B;

        /// <summary>
        /// No duration effect.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Screen location of the button on the HUD.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// Icon sprite
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.FinalButton;

        /// <summary>
        /// Determines when the button should appear.
        /// Only enabled once Overload has enough absorbed abilities.
        /// </summary>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is OverloadRole &&
                   OverloadRole.AbsorbedAbilityCount >= OptionGroupSingleton<OverloadOptions>.Instance.NeededCharge;
        }

        /// <summary>
        /// What happens when the final ability button is clicked.
        /// Ends the game with Overload win.
        /// </summary>
        protected override void OnClick()
        {
            GameManager.Instance.RpcEndGame(
                (GameOverReason)NewModEndReasons.OverloadWin,
                false
            );
        }
    }
}
