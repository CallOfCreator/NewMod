using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.SpecialAgentOptions;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using AmongUs.GameOptions;
using Object = UnityEngine.Object;
using NewMod.Utilities;
using Reactor.Utilities;
using System;

namespace NewMod.Buttons
{
    public class AssignButton : CustomActionButton
    {
        public override string Name => "Assign Mission";
        public override float Cooldown => OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignCooldown;
        public override int MaxUses => (int)OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignMaxUses;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        public override float EffectDuration => 0f;
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;
        public override bool Enabled(RoleBehaviour role)
        {
            return role is SpecialAgent;
        }
        public override bool CanUse()
        {
            return SpecialAgent.AssignedPlayer == null;
        }
        protected override void OnClick()
        {
            CustomPlayerMenu menu = CustomPlayerMenu.Create();

            menu.Begin(player => !player.Data.IsDead && !player.Data.Disconnected && player.PlayerId != PlayerControl.LocalPlayer.PlayerId, (player) =>
            {
                SpecialAgent.AssignedPlayer = player;
                Utils.AssignMission(SpecialAgent.AssignedPlayer);

                if (OptionGroupSingleton<SpecialAgentOptions>.Instance.TargetCameraTracking)
                {
                    var cam = Camera.main.GetComponent<FollowerCamera>();
                    cam?.SetTarget(player);
                    Coroutines.Start(CoResetCamera(cam, OptionGroupSingleton<SpecialAgentOptions>.Instance.CameraTrackingDuration));
                }
            });
        }
        public static IEnumerator CoResetCamera(FollowerCamera cam, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (cam != null)
            {
                cam.SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }
}
