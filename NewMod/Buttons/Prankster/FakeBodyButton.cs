using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.PranksterOptions;
using PRK = NewMod.Roles.NeutralRoles.Prankster;
using UnityEngine;
using NewMod.Utilities;

namespace NewMod.Buttons.Prankster
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class FakeBodyButton : CustomActionButton
    {
        /// <summary>
        /// Gets the name displayed on the button.
        /// </summary>
        public override string Name => "Prank";

        /// <summary>
        /// Gets the cooldown duration for using the prank ability, based on <see cref="PranksterOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<PranksterOptions>.Instance.PrankCooldown;

        /// <summary>
        /// Gets the maximum number of times the prank ability can be used, based on <see cref="PranksterOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<PranksterOptions>.Instance.PrankMaxUses;

        /// <summary>
        /// Determines where on the screen this button will appear.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// The duration of any effect caused by this button press; in this case, no effect duration is used.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// The sprite asset representing this button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadBodySprite;

        /// <summary>
        /// Checks whether the Prankster can use this button, ensuring that there is at least one dead player in the game.
        /// </summary>
        /// <returns>True if the base conditions are met and there is a dead player, otherwise false.</returns>
        public override bool CanUse()
        {
            return base.CanUse() && Utils.AnyDeadPlayer();
        }

        /// <summary>
        /// Called when the button is clicked. Spawns a fake dead body at the local player's position.
        /// </summary>
        protected override void OnClick()
        {
            PranksterUtilities.CreatePranksterDeadBody(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.PlayerId);
        }

        /// <summary>
        /// Determines whether this button is enabled for the specified role.
        /// </summary>
        /// <param name="role">The player's current role.</param>
        /// <returns>True if the role is <see cref="PRK"/>, otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is PRK;
        }
    }
}
