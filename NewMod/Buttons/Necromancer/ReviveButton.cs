using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.NecromancerOptions;
using NewMod.Roles.ImpostorRoles;
using UnityEngine;
using NewMod.Utilities;
using MiraAPI.Keybinds;
using AmongUs.GameOptions;
using Reactor.Utilities;

namespace NewMod.Buttons.Necromancer
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class ReviveButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on the button. Intentionally left empty to show an existing name elsewhere.
        /// </summary>
        public override string Name => ""; // It's currently empty since the button has already a name on it

        /// <summary>
        /// Gets the cooldown time for this button, based on <see cref="NecromancerOption"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<NecromancerOption>.Instance.ButtonCooldown;

        /// <summary>
        /// Gets the maximum number of uses for this button, based on <see cref="NecromancerOption"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<NecromancerOption>.Instance.AbilityUses;

        /// <summary>
        /// Determines how long the effect from clicking the button lasts. In this case, no duration is set.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Default keybind for Necromancer's Revive ability.
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

        /// <summary>
        /// Defines where on the screen this button should appear.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// The visual icon for this button, set to the necromancer sprite asset.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.NecromancerButton;

        /// <summary>
        /// Invoked when the revive button is clicked. Plays a sound and revives the nearest dead body.
        /// </summary>
        protected override void OnClick()
        {
            var closestBody = Utils.GetClosestBody();
            var killedPlayer = GameData.Instance.GetPlayerById(closestBody.ParentId)?.Object;

            var killer = Utils.GetKiller(killedPlayer);
            if (killer != null && killer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("Heads up: you tried to revive someone you killed, one use has been consumed"));
                return;
            }

            SoundManager.Instance.PlaySound(NewModAsset.ReviveSound?.LoadAsset(), false, 2f);

            Utils.HandleRevive(
                PlayerControl.LocalPlayer,
                closestBody.ParentId,
                RoleTypes.Impostor,
                closestBody.transform.position.x,
                closestBody.transform.position.y
            );
        }
        /// <summary>
        /// Determines whether this button is enabled for the role, returning true if the role is <see cref="NecromancerRole"/>.
        /// </summary>
        /// <param name="role">The current player's role.</param>
        /// <returns>True if the role is Necromancer; otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is NecromancerRole;
        }

        /// <summary>
        /// Checks whether the player can currently use the revive button, ensuring cooldowns, ability uses, and conditions are met.
        /// </summary>
        /// <returns>True if all requirements to use this button are met; otherwise false.</returns>
        public override bool CanUse()
        {
            if (Timer > 0) return false;
            if (UsesLeft <= 0) return false;

            var closestBody = Utils.GetClosestBody();
            if (closestBody == null) return false;

            if (PranksterUtilities.IsPranksterBody(closestBody)) return false;

            return true;
        }
    }
}
