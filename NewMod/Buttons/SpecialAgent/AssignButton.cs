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
    public class AssignButton : CustomActionButton
    {
        public override string Name => "Assign Mission";
        public override float Cooldown => OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignCooldown;
        public override int MaxUses => (int)OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignMaxUses;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        public override float EffectDuration => 0f;
        public override LoadableAsset<Sprite> Sprite => NewModAsset.SpecialAgentButton;
        public override bool Enabled(RoleBehaviour role)
        {
            return role is SA;
        }
        public override bool CanUse()
        {
            return base.CanUse() && SA.AssignedPlayer == null;
        }
        protected override void OnClick()
        {
            CustomPlayerMenu menu = CustomPlayerMenu.Create();

            SetTimerPaused(true);

            menu.Begin(player => !player.Data.IsDead && !player.Data.Disconnected && player.PlayerId != PlayerControl.LocalPlayer.PlayerId, (player) =>
            {
                SA.AssignedPlayer = player;
                Utils.AssignMission(SA.AssignedPlayer);

                if (OptionGroupSingleton<SpecialAgentOptions>.Instance.TargetCameraTracking)
                {
                    var cam = Camera.main.GetComponent<FollowerCamera>();
                    cam?.SetTarget(player);
                    Coroutines.Start(CoResetCamera(cam, OptionGroupSingleton<SpecialAgentOptions>.Instance.CameraTrackingDuration));
                }
                menu.Close();
                SetTimerPaused(false);
            });
        }
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
