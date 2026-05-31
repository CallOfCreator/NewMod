using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.ImpostorRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1
{
    [MiraIgnore]
    public class MirrorReflectButton : CustomActionButton
    {
        public override string Name => "Reflect";
        public override float Cooldown => OptionGroupSingleton<MirrorBladeOptions>.Instance.ReflectCooldown;
        public override int MaxUses => (int)OptionGroupSingleton<MirrorBladeOptions>.Instance.MaxReflectUses;
        public override float EffectDuration => OptionGroupSingleton<MirrorBladeOptions>.Instance.ReflectWindow;
        public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        public override LoadableAsset<Sprite> Sprite => NewModAsset.Slash;
        public override bool Enabled(RoleBehaviour role) => role is MirrorBladeRole;

        public override bool CanUse()
        {
            return base.CanUse() && !MirrorBladeRole.ArmedReflections.Contains(PlayerControl.LocalPlayer.PlayerId);
        }

        protected override void OnClick()
        {
            MirrorBladeRole.RpcArmReflection(PlayerControl.LocalPlayer);
        }
    }
}