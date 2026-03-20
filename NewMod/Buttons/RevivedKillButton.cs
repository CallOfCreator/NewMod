using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.ImpostorRoles;
using Reactor.Utilities;
using AmongUs.GameOptions;
using System.Linq;
using UnityEngine;
using MiraAPI.Networking;
using MiraAPI.Utilities;

namespace NewMod.Buttons
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
        public override bool Enabled(RoleBehaviour role)
        {
            return NecromancerRole.RevivedPlayers.ContainsKey(PlayerControl.LocalPlayer.PlayerId);
        }
        public override PlayerControl GetTarget()
        {
            return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
        }
        public override bool IsTargetValid(PlayerControl target)
        {
            return target.PlayerId != PlayerControl.LocalPlayer.PlayerId;
        }
        public override void SetOutline(bool active)
        {
            Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.ImpostorRed));
        }

        public override bool CanUse()
        {
            if (!NecromancerRole.RevivedPlayers.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return false;
            return true;
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
        }
    }
}