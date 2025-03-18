using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.RevenantOptions;
using Rev = NewMod.Roles.ImpostorRoles.Revenant;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Revenant
{
    public class FeignDeathButton : CustomActionButton
    {
        public override string Name => "Feign Death";
        public override float Cooldown => OptionGroupSingleton<RevenantOptions>.Instance.FeignDeathCooldown;
        public override int MaxUses => (int)OptionGroupSingleton<RevenantOptions>.Instance.FeignDeathMaxUses;
        public override ButtonLocation Location => ButtonLocation.BottomRight;
        public override float EffectDuration => 0f;
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;
        public override bool Enabled(RoleBehaviour role)
        {
            return role is Rev && !Rev.HasUsedFeignDeath;
        }
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;
            Coroutines.Start(Utils.StartFeignDeath(player));
        }
    }
}
