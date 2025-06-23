using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.EnergyThiefOptions;
using ET = NewMod.Roles.NeutralRoles.EnergyThief;
using UnityEngine;
using NewMod.Utilities;

namespace NewMod.Buttons.EnergyThief
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class DrainButton : CustomActionButton<PlayerControl>
    {
        /// <summary>
        /// Gets the display name for this button.
        /// </summary>
        public override string Name => "Drain";

        /// <summary>
        /// Gets the cooldown value for this button, based on EnergyThiefOptions.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<EnergyThiefOptions>.Instance.DrainCooldown;

        /// <summary>
        /// Gets the maximum number of uses for this button, based on EnergyThiefOptions.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<EnergyThiefOptions>.Instance.DrainMaxUses;

        /// <summary>
        /// The on-screen position of this button.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomRight;

        /// <summary>
        /// The duration of the effect applied by this button; in this case, zero.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// Gets the sprite asset for this button. Currently set to an empty sprite.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

        /// <summary>
        /// Determines the target for this button action.
        /// </summary>
        /// <returns>The closest valid PlayerControl, or null if none.</returns>
        public override PlayerControl GetTarget()
        {
              return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, false, p => !p.Data.IsDead && !p.Data.Disconnected);
        }

        /// <summary>
        /// Enables or disables an outline around the targeted player.
        /// </summary>
        /// <param name="active">Whether to enable or disable the outline.</param>
        public override void SetOutline(bool active)
        {
            Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.magenta));
        }

        /// <summary>
        /// Determines whether the targeted player is valid for draining.
        /// </summary>
        /// <param name="target">The candidate player.</param>
        /// <returns>Always true in this implementation.</returns>
        public override bool IsTargetValid(PlayerControl target)
        {
            return true;
        }

        /// <summary>
        /// Specifies whether the button is enabled for a given role.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <returns>True if the role is EnergyThief, otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is ET;
        }

        /// <summary>
        /// Invoked when the button is clicked. Records a drain count, queues a pending effect,
        /// notifies the local player, and locks the button from reuse until the meeting ends.
        /// </summary>
        protected override void OnClick()
        {
            PendingEffectManager.AddPendingEffect(Target);

            Utils.RecordDrainCount(PlayerControl.LocalPlayer);

            if (PlayerControl.LocalPlayer.AmOwner)
            {
                HudManager.Instance.Notifier.AddDisconnectMessage($"The Drain effect will be applied to {Target.Data.PlayerName} after the meeting ends.");
            }
            Utils.waitingPlayers.Add(PlayerControl.LocalPlayer);
        }

        /// <summary>
        /// Checks if the button can be used, preventing usage if the local player
        /// is already in the waitingPlayers set.
        /// </summary>
        /// <returns>True if usable, otherwise false.</returns>
        public override bool CanUse()
        {
            if (Utils.waitingPlayers.Contains(PlayerControl.LocalPlayer))
            {
                return false;
            }
            return base.CanUse();
        }
    }
}
