using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.WraithCallerOptions;
using Wraith = NewMod.Roles.NeutralRoles.WraithCaller;
using UnityEngine;
using NewMod.Utilities;
using Rewired;
using System.Collections.Generic;
using System.Linq;

namespace NewMod.Buttons.WraithCaller
{
    /// <summary>
    /// Defines the Call Wraith ability button for the Wraith Caller role.
    /// </summary>
    public class CallWraithButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on the button.
        /// </summary>
        public override string Name => "Call Wraith";

        /// <summary>
        /// The cooldown time for the Call Wraith ability, as set in <see cref="WraithCallerOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<WraithCallerOptions>.Instance.CallWraithCooldown;

        /// <summary>
        /// The maximum uses for the Call Wraith ability, as set in <see cref="WraithCallerOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<WraithCallerOptions>.Instance.CallWraithMaxUses;

        /// <summary>
        /// Location on the screen for the Call Wraith button.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// Default keybind for the Call Wraith ability.
        /// </summary>
        public override KeyboardKeyCode Defaultkeybind => KeyboardKeyCode.M;

        /// <summary>
        /// The duration of any effect triggered by this ability.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// The icon for the Call Wraith button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.CallWraith;

        /// <summary>
        /// Enables the button for the Wraith Caller role only.
        /// </summary>
        /// <param name="role">Current player's role</param>
        /// <returns>True if role is Wraith Caller, otherwise false</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is Wraith;
        }

        /// <summary>
        /// Triggered when the Call Wraith button is clicked.
        /// </summary>
        protected override void OnClick()
        {
            //TODO: Replace this with the custom minigame once it’s fixed
            CustomPlayerMenu menu = CustomPlayerMenu.Create();

            SetTimerPaused(true);

            var allowedPlayers = new HashSet<byte>();

            foreach (var info in GameData.Instance.AllPlayers)
            {
                if (info.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                if (info.IsDead || info.Disconnected) continue;

                allowedPlayers.Add(info.PlayerId);
            }

            menu.Begin(player => allowedPlayers.Contains(player.PlayerId) && !player.notRealPlayer,
                       player =>
                       {
                           menu.Close();
                           WraithCallerUtilities.RpcSummonNPC(PlayerControl.LocalPlayer, player);
                           SetTimerPaused(false);
                       });

            foreach (var panel in menu.potentialVictims)
            {
                var icon = panel.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(sr => sr.name == "ShapeshifterIcon");
                icon.sprite = NewModAsset.WraithIcon.LoadAsset();
            }
        }
    }
}
