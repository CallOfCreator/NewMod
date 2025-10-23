using System.Collections;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using MiraAPI.Networking;
using NewMod.Options.Roles;
using Reactor.Utilities;
using MiraAPI.Utilities;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using MiraAPI.Keybinds;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Buttons.Roles
{
    /// <summary>
    /// Custom button for the Pulseblade role to perform a high-speed strike on the closest player in aim direction.
    /// The strike teleports the user toward the target and executes a stealthy instant kill.
    /// </summary>
    public class StrikeButton : CustomActionButton
    {
        /// <summary>
        /// Display name for the button (not shown by default).
        /// </summary>
        public override string Name => "Strike";

        /// <summary>
        /// Cooldown between strikes, pulled from <see cref="PulseBladeOptions.StrikeCooldown"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<PulseBladeOptions>.Instance.StrikeCooldown;

        /// <summary>
        /// Maximum number of strike uses, from <see cref="PulseBladeOptions.MaxStrikeUses"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<PulseBladeOptions>.Instance.MaxStrikeUses;

        /// <summary>
        /// Effect duration (not used for this button).
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Placement of the button on the HUD.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// Default keybind for Pulseblade's Strike ability.  
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
        /// <summary>
        /// Sprite used for the button — set to empty;
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.StrikeButton;

        /// <summary>
        /// Determines whether the button is active for a given role.
        /// </summary>
        /// <param name="role">The current player's role.</param>
        /// <returns>True only for Pulseblade role.</returns>
        public override bool Enabled(RoleBehaviour role) => role is PulseBlade;

        /// <summary>
        /// Called when the button is pressed by the player.
        /// Searches for a valid target and executes the strike.
        /// </summary>
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;

            var target = PlayerControl.AllPlayerControls
               .ToArray()
               .Where(p => p != player && !p.Data.IsDead && !p.Data.Disconnected && !p.inVent)
               .OrderBy(p => Vector2.Distance(player.GetTruePosition(), p.GetTruePosition()))
               .FirstOrDefault(p => Vector2.Distance(player.GetTruePosition(), p.GetTruePosition()) <= OptionGroupSingleton<PulseBladeOptions>.Instance.StrikeRange);

            RpcPulseStrike(player, target);
        }

        /// <summary>
        /// RPC method to perform a Pulseblade strike on a target.
        /// </summary>
        /// <param name="source">The player performing the strike.</param>
        /// <param name="target">The victim of the strike.</param>
        [MethodRpc((uint)CustomRPC.Dash)]
        public static void RpcPulseStrike(PlayerControl source, PlayerControl target)
        {
            Coroutines.Start(DoPulseStrike(source, target));
        }

        /// <summary>
        /// Executes the strike: dashes to the target and performs a kill.
        /// Hides the body for a short time.
        /// </summary>
        /// <param name="killer">The Pulseblade player.</param>
        /// <param name="target">The struck victim.</param>
        /// <returns>IEnumerator.</returns>
        public static IEnumerator DoPulseStrike(PlayerControl killer, PlayerControl target)
        {
            var sound = NewModAsset.StrikeSound.LoadAsset();
            float originalSpeed = killer.MyPhysics.Speed;
            float dashSpeed = OptionGroupSingleton<PulseBladeOptions>.Instance.DashSpeed;

            killer.moveable = false;
            killer.MyPhysics.inputHandler.enabled = false;
            killer.MyPhysics.Speed = dashSpeed;

            while (Vector2.Distance(killer.GetTruePosition(), target.GetTruePosition()) > 0.1f)
            {
                Vector2 dir = target.GetTruePosition() - killer.GetTruePosition();
                killer.MyPhysics.SetNormalizedVelocity(dir.normalized);

                float step = killer.MyPhysics.TrueSpeed * Time.fixedDeltaTime;
                if (step >= dir.magnitude) break;

                yield return new WaitForFixedUpdate();
            }

            killer.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            killer.MyPhysics.Speed = originalSpeed;
            killer.MyPhysics.inputHandler.enabled = true;
            killer.moveable = true;

            SoundManager.Instance.PlaySound(sound, false, 1f);

            killer.RpcCustomMurder(
                target,
                didSucceed: true,
                resetKillTimer: false,
                createDeadBody: true,
                teleportMurderer: false,
                showKillAnim: false,
                playKillSound: false
            );

            Utils.RegisterStrikeKill(killer, target);

            var notif = Helpers.CreateAndShowNotification($"Perfect kill {target.Data.PlayerName} eliminated", new(1f, 0.25f, 0.25f), spr: NewModAsset.StrikeIcon.LoadAsset());
            notif.Text.SetOutlineThickness(0.30f);

            var bodies = Helpers.GetNearestDeadBodies(target.GetTruePosition(), 0.5f, Helpers.CreateFilter(Constants.NotShipMask));
            if (bodies != null && bodies.Count > 0)
            {
                foreach (var b in bodies) if (b) b.gameObject.SetActive(false);
                yield return new WaitForSeconds(OptionGroupSingleton<PulseBladeOptions>.Instance.HideBodyDuration);
                foreach (var b in bodies) if (b) b.gameObject.SetActive(true);
            }
        }
    }
}
