using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.EdgeveilOptions;
using EV = NewMod.Roles.ImpostorRoles.Edgeveil;
using Rewired;
using UnityEngine;
using NewMod.Components.ScreenEffects;
using NewMod.Utilities;
using Reactor.Utilities;
using MiraAPI.Keybinds;

namespace NewMod.Buttons.Edgeveil
{
    /// <summary>
    /// Defines a custom action button for Edgeviel's Arc ability.
    /// </summary>
    public class ArcButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on the button.
        /// </summary>
        public override string Name => "Arc";

        /// <summary>
        /// Gets the cooldown time for this button, based on <see cref="EdgeveilOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<EdgeveilOptions>.Instance.SlashCooldown;

        /// <summary>
        /// Gets the maximum number of uses for this button (0 = infinite).
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<EdgeveilOptions>.Instance.SlashMaxUses;

        /// <summary>
        /// Determines how long the effect lasts. For Arc, none.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Default keybind for Edgeveil's Arc ability.
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

        /// <summary>
        /// Defines where on the screen this button should appear.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// The visual icon for this button, set to the Edgeveil Arc sprite asset.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.SlashIcon;

        /// <summary>
        /// Invoked when the Arc button is clicked.
        /// </summary>
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;

            bool flipLeft = player.cosmetics.currentBodySprite.BodySprite.flipX;
            Vector2 dir = flipLeft ? Vector2.left : Vector2.right;

            float spawnOffset = 0.55f;
            var spawnPos = player.GetTruePosition() + dir * spawnOffset;

            var tray = SlashTray.CreateTray();
            tray.transform.SetParent(ShipStatus.Instance.transform, worldPositionStays: true);
            tray.transform.SetPositionAndRotation(
                new Vector3(spawnPos.x, spawnPos.y, player.transform.position.z),
                 Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f))
            );

            tray.Owner = player;
            tray.SetMotion(dir, OptionGroupSingleton<EdgeveilOptions>.Instance.SlashSpeed);

            float effectDuration = OptionGroupSingleton<EdgeveilOptions>.Instance.EffectDuration;

            HudManager.Instance.PlayerCam.ShakeScreen(effectDuration, 2f);
        }
        /// <summary>
        /// Determines whether this button is enabled for the role, returning true if the role is <see cref="EdgevielRole"/>.
        /// </summary>
        /// <param name="role">The current player's role.</param>
        /// <returns>True if the role is Edgeveil; otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is EV;
        }
    }
}
