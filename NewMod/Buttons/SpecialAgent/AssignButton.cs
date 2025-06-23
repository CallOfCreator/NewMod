using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.SpecialAgentOptions;
using SA = NewMod.Roles.NeutralRoles.SpecialAgent;
using UnityEngine;
using NewMod.Utilities;
using Reactor.Utilities;

namespace NewMod.Buttons.SpecialAgent
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class AssignButton : CustomActionButton
    {
        /// <summary>
        /// The name displayed on this button.
        /// </summary>
        public override string Name => "Assign Mission";

        /// <summary>
        /// The cooldown time before this button can be used again, based on <see cref="SpecialAgentOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignCooldown;

        /// <summary>
        /// The maximum number of times the assign action can be performed, based on <see cref="SpecialAgentOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignMaxUses;

        /// <summary>
        /// The on-screen location of this button.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// The duration of any effect triggered by this button; here, it's zero.
        /// </summary>
        public override float EffectDuration => 0f;

        /// <summary>
        /// The sprite icon representing this button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.SpecialAgentButton;

        /// <summary>
        /// Specifies whether this button is enabled for the specified role.
        /// </summary>
        /// <param name="role">The current role to check.</param>
        /// <returns>True if the role is <see cref="SA"/>, otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is SA;
        }

        /// <summary>
        /// Checks whether the player can use this button, ensuring no one has been assigned a mission yet.
        /// </summary>
        /// <returns>True if base conditions are met and no player is assigned, otherwise false.</returns>
        public override bool CanUse()
        {
            return base.CanUse() && SA.AssignedPlayer == null;
        }

        /// <summary>
        /// Invoked when the button is clicked. Opens a custom player menu to pick a mission target.
        /// </summary>
        protected override void OnClick()
        {
            CustomPlayerMenu menu = CustomPlayerMenu.Create();

            SetTimerPaused(true);

            menu.Begin(
                player => !player.Data.IsDead && 
                          !player.Data.Disconnected && 
                          player.PlayerId != PlayerControl.LocalPlayer.PlayerId,
                player =>
                {
                    SA.AssignedPlayer = player;
                    Utils.RpcAssignMission(PlayerControl.LocalPlayer, SA.AssignedPlayer);

                    if (OptionGroupSingleton<SpecialAgentOptions>.Instance.TargetCameraTracking)
                    {
                        var cam = Camera.main.GetComponent<FollowerCamera>();
                        cam?.SetTarget(player);
                        Coroutines.Start(CoResetCamera(cam, OptionGroupSingleton<SpecialAgentOptions>.Instance.CameraTrackingDuration));
                    }
                    menu.Close();
                    SetTimerPaused(false);
                }
            );
        }

        /// <summary>
        /// Resets the camera target to the local player after a specified duration, optionally shaking the camera at the end.
        /// </summary>
        /// <param name="cam">The <see cref="FollowerCamera"/> to reset.</param>
        /// <param name="duration">How long to track the target before resetting.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator CoResetCamera(FollowerCamera cam, float duration)
        {
            float timeElapsed = 0f;
            Vector3 originalPosition = cam.transform.position;
            float shakeThreshold = 1.5f;
            bool shouldShake = OptionGroupSingleton<SpecialAgentOptions>.Instance.ShouldShakeCamera;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;
                if (shouldShake && (duration - timeElapsed) <= shakeThreshold)
                {
                    float shakeMagnitude = 0.3f;
                    Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                    cam.transform.localPosition = originalPosition + shakeOffset;
                }
                else
                {
                    cam.transform.localPosition = originalPosition;
                }
                yield return null;
            }
            cam.transform.localPosition = originalPosition;
            cam?.SetTarget(PlayerControl.LocalPlayer);
        }
    }
}
