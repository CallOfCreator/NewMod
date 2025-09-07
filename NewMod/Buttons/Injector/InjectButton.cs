using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using NewMod.Roles.NeutralRoles;
using NewMod.Options.Roles.InjectorOptions;
using MiraAPI.Utilities;
using System;
using static NewMod.Utilities.Utils;
using MiraAPI.Keybinds;

namespace NewMod.Buttons.Injector
{
    /// <summary>
    /// Represents the serum injection button for the Injector role.
    /// Allows injecting a random serum into nearby players.
    /// </summary>
    public class InjectButton : CustomActionButton<PlayerControl>
    {
        /// <summary>
        /// The name displayed on the button (if any).
        /// </summary>
        public override string Name => "Inject";

        /// <summary>
        /// Cooldown time between uses, configured via <see cref="InjectorOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<InjectorOptions>.Instance.SerumCooldown;

        /// <summary>
        /// Maximum allowed injections, configured via <see cref="InjectorOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<InjectorOptions>.Instance.MaxSerumUses;

        /// <summary>
        /// Effect duration — unused here since injection is instant.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Screen location of the button on the HUD.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// Default keybind for Injector's Inject ability.
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

        /// <summary>
        /// Sprite/icon displayed on the button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.InjectButton;

        /// <summary>
        /// Returns the closest valid player target within range,
        /// used by the Injector to determine who can be injected.
        /// </summary>
        /// <returns>The nearest PlayerControl instance, or null if none is in range.</returns>
        public override PlayerControl GetTarget()
        {
            return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, false);
        }

        /// <summary>
        /// Sets an outline around the target player to visually indicate interaction,
        /// such as highlighting a valid injection target for the Injector role.
        /// </summary>
        /// <param name="active">True to show the outline; false to hide it.</param>
        public override void SetOutline(bool active)
        {
            Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.AcceptedGreen));
        }

        /// <summary>
        /// Determines whether this button is available for the current role.
        /// </summary>
        /// <param name="role">The current player's role.</param>
        /// <returns>True only for the Injector role.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is InjectorRole;
        }

        /// <summary>
        /// Called when the button is clicked. Applies a serum to the closest valid target.
        /// </summary>
        protected override void OnClick()
        {
            var values = (SerumType[])Enum.GetValues(typeof(SerumType));
            var serum = values[UnityEngine.Random.Range(0, values.Length)];

            RpcApplySerum(PlayerControl.LocalPlayer, Target, serum);
        }
    }
}
