using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using NewMod.Components;
using SH = NewMod.Roles.NeutralRoles.Shade;
using NewMod.Options.Roles.ShadeOptions;

namespace NewMod.Buttons.Shade
{
    /// <summary>
    /// Custom action button for the Shade role.
    /// Deploys a Shadow Zone where the Shade becomes invisible and gains kill power.
    /// </summary>
    public class ShadeButton : CustomActionButton
    {
        /// <summary>
        /// Display name of the button.
        /// </summary>
        public override string Name => "Deploy Shadow";

        /// <summary>
        /// Cooldown duration between zone deployments.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<ShadeOptions>.Instance.Cooldown;

        /// <summary>
        /// Maximum number of uses.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<ShadeOptions>.Instance.MaxUses;

        /// <summary>
        /// Button HUD placement.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// Default keybind.
        /// </summary>
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

        /// <summary>
        /// Button icon.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.DeployZone;

        /// <summary>
        /// Button enabled only for the Shade role.
        /// </summary>
        public override bool Enabled(RoleBehaviour role) => role is SH;

        /// <summary>
        /// Deploys a shadow zone at the Shade's position.
        /// </summary>
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;

            Vector2 pos = player.GetTruePosition();
    
            float radius = OptionGroupSingleton<ShadeOptions>.Instance.Radius;
            float dur = OptionGroupSingleton<ShadeOptions>.Instance.Duration;

            ShadowZone.RpcDeployZone(player, pos, radius, dur);
        }
    }
}
