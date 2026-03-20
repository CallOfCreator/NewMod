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
using MiraAPI.Utilities;
using System.Linq;

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
            var local = PlayerControl.LocalPlayer;

            var body = Helpers.GetNearestDeadBodies(
                local.GetTruePosition(),
                ShipStatus.Instance.MaxLightRadius,
                Helpers.CreateFilter(Constants.NotShipMask))
                .Where(b => b != null && !PranksterUtilities.IsPranksterBody(b))
                .OrderBy(b => Vector2.Distance(local.GetTruePosition(), b.TruePosition))
                .FirstOrDefault();

            if (body == null) return;

            SoundManager.Instance.PlaySound(NewModAsset.ReviveSound?.LoadAsset(), false, 2f);

            Utils.HandleRevive(
                local,
                body.ParentId,
                RoleTypes.Crewmate,
                body.transform.position.x,
                body.transform.position.y
            );
            NecromancerRole.RevivedPlayers[body.ParentId] = local.PlayerId;
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
            var bodiesInRange = Helpers.GetNearestDeadBodies(
                PlayerControl.LocalPlayer.transform.position,
                ShipStatus.Instance.MaxLightRadius,
                Helpers.CreateFilter(Constants.NotShipMask));

            bool canUse = bodiesInRange.Any(body =>
            {
                if (PranksterUtilities.IsPranksterBody(body)) return false;

                var killedPlayer = GameData.Instance.GetPlayerById(body.ParentId)?.Object;
                if (killedPlayer == null) return false;

                var killer = Utils.GetKiller(killedPlayer);
                if (killer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    return false;

                return true;
            });

            return canUse;
        }
    }
}
