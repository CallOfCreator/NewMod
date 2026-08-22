using System.Collections;
using System;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using SA = NewMod.Roles.NeutralRoles.SpecialAgent;
using UnityEngine;
using NewMod.Utilities;
using Reactor.Utilities;
using MiraAPI.Keybinds;
using Random = UnityEngine.Random;

namespace NewMod.Buttons.Roles
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
        /// Default keybind for Special Agent's Assign ability.
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

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

        public override void ClickHandler()
        {
            if (CanClick())
                OnClick();
        }

        protected override void OnClick()
        {
            var menu = CustomPlayerMenu.Create();

            menu.Begin(player => !player.Data.IsDead && !player.Data.Disconnected && player.PlayerId != PlayerControl.LocalPlayer.PlayerId, player =>
            {
                menu.Close();

                DecreaseUses();
                ResetCooldownAndOrEffect();

                var missions = (MissionType[])Enum.GetValues(typeof(MissionType));
                var mission = missions[Random.Range(0, missions.Length)];
                var mostWantedId = byte.MaxValue;

                if (mission == MissionType.KillMostWanted)
                {
                    var candidates = PlayerControl.AllPlayerControls.ToArray().Where(candidate => candidate && candidate.Data != null && !candidate.Data.IsDead && !candidate.Data.Disconnected && candidate != player && candidate != PlayerControl.LocalPlayer).ToArray();

                    if (candidates.Length == 0)
                    {
                        mission = MissionType.DrainEnergy;
                    }
                    else
                    {
                        mostWantedId = candidates[Random.Range(0, candidates.Length)].PlayerId;
                    }
                }

                SA.AssignedPlayer = player;

                Utils.RpcAssignMission(PlayerControl.LocalPlayer, player, mission, mostWantedId);

                if (OptionGroupSingleton<SpecialAgentOptions>.Instance.TargetCameraTracking)
                {
                    var camera = Camera.main.GetComponent<FollowerCamera>();

                    if (camera)
                    {
                        camera.SetTarget(player);

                        Coroutines.Start(CoResetCamera(camera, OptionGroupSingleton<SpecialAgentOptions>.Instance.CameraTrackingDuration));
                    }
                }
            });
        }

        /// <summary>
        /// Resets the camera target to the local player after a specified duration, optionally shaking the camera at the end.
        /// </summary>
        /// <param name="cam">The <see cref="FollowerCamera"/> to reset.</param>
        /// <param name="duration">How long to track the target before resetting.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator CoResetCamera(FollowerCamera cam, float duration)
        {
            if (!cam)
                yield break;

            float timeElapsed = 0f;
            Vector3 originalPosition = cam.transform.localPosition;
            float shakeThreshold = 1.5f;
            bool shouldShake = OptionGroupSingleton<SpecialAgentOptions>.Instance.ShouldShakeCamera;

            while (timeElapsed < duration && cam)
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

            if (!cam)
                yield break;

            cam.transform.localPosition = originalPosition;
            cam.SetTarget(PlayerControl.LocalPlayer);
        }
    }
}