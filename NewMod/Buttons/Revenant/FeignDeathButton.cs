using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.RevenantOptions;
using Rev = NewMod.Roles.ImpostorRoles.Revenant;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;
using Rewired;

namespace NewMod.Buttons.Revenant
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class FeignDeathButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on the button.
        /// </summary>
        public override string Name => "Feign Death";

        /// <summary>
        /// The cooldown time for the Feign Death ability, as set in <see cref="RevenantOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<RevenantOptions>.Instance.FeignDeathCooldown;

        /// <summary>
        /// The maximum uses of the Feign Death ability, as set in <see cref="RevenantOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<RevenantOptions>.Instance.FeignDeathMaxUses;

        /// <summary>
        /// Determines where on the screen this button appears.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// Default keybind for Revenant's Feign Death ability.  
        /// Requires Ctrl as a modifier to prevent accidental use.
        /// </summary>
        public override KeyboardKeyCode Defaultkeybind => KeyboardKeyCode.T;
        public override ModifierKey Modifier1 => ModifierKey.Control;

        /// <summary>
        /// The duration of any effect from this button. In this case, zero.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// The icon or sprite used for this button. Here, set to an empty sprite.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

        /// <summary>
        /// Specifies whether the button is enabled for the given role, ensuring Feign Death hasn't been used yet.
        /// </summary>
        /// <param name="role">The player's current role.</param>
        /// <returns>True if the role is <see cref="Rev"/> and Feign Death hasn't been used, otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is Rev && !Rev.HasUsedFeignDeath;
        }
        /// <summary>
        /// Checks if this button can be used
        /// </summary>
        /// <returns>True if base conditions are met and the player hasn't used Feign Death; otherwise, false.</returns>
        public override bool CanUse()
        {
            return base.CanUse() && !Rev.HasUsedFeignDeath;
        }

        /// <summary>
        /// Invoked when the Feign Death button is clicked, starting the feign death coroutine.
        /// </summary>
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;
            Coroutines.Start(Utils.StartFeignDeath(player));
        }
    }
}
