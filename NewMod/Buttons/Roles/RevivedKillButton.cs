using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.ImpostorRoles;
using AmongUs.GameOptions;
using UnityEngine;
using MiraAPI.Networking;
using MiraAPI.Utilities;

namespace NewMod.Buttons.Roles
{
    public class RevivedKillButton : CustomActionButton<PlayerControl>
    {
        public override string Name => "KILL";
        public override float Cooldown => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        public override int MaxUses => 0;
        public override float EffectDuration => 0f;
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
        public override ButtonLocation Location => ButtonLocation.BottomRight;
        public override LoadableAsset<Sprite> Sprite => NewModAsset.VanillaKillButton;

        private static bool CanUseRevivedKillButton()
        {
            var local = PlayerControl.LocalPlayer;
            return local != null && NecromancerRole.RevivedPlayers.ContainsKey(local.PlayerId);
        }

        public override bool Enabled(RoleBehaviour role)
        {
            return CanUseRevivedKillButton();
        }

        protected override void FixedUpdate(PlayerControl playerControl)
        {
            Button?.ToggleVisible(CanUseRevivedKillButton());
        }

        public override PlayerControl GetTarget()
        {
            return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
        }

        public override bool IsTargetValid(PlayerControl target)
        {
            return CanUseRevivedKillButton() &&
                   target != null &&
                   target.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
                   !target.Data.IsDead && !target.Data.Disconnected;
        }

        public override void SetOutline(bool active)
        {
            Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.ImpostorRed));
        }

        public override bool CanUse()
        {
            return base.CanUse() && CanUseRevivedKillButton();
        }

        protected override void OnClick()
        {
            var local = PlayerControl.LocalPlayer;

            local.RpcCustomMurder(
                Target,
                didSucceed: true,
                resetKillTimer: true,
                createDeadBody: true,
                teleportMurderer: true,
                showKillAnim: true,
                playKillSound: true
            );

            NecromancerRole.RevivedPlayers.Remove(local.PlayerId);
            ResetTarget();
            Button?.ToggleVisible(false);
        }
    }
}