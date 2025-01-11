using MiraAPI.Utilities.Assets;
using MiraAPI.Hud;
using MiraAPI.GameOptions;
using NewMod.Options.Roles.VisionaryOptions;
using NewMod.Roles.CrewmateRoles;
using UnityEngine;
using Reactor.Utilities;
using NewMod.Utilities;

namespace NewMod.Buttons
{
    [RegisterButton]
    public class CaptureButton : CustomActionButton
    {
        public override string Name => "Capture";
        public override float Cooldown => OptionGroupSingleton<VisionaryOptions>.Instance.ScreenshotCooldown;
        public override float EffectDuration => 0;
        public override int MaxUses => (int)OptionGroupSingleton<VisionaryOptions>.Instance.MaxScreenshots;
        public override LoadableAsset<Sprite> Sprite => NewModAsset.Camera;
        public override ButtonLocation Location => ButtonLocation.BottomLeft;
        protected override void OnClick()
        {
            Coroutines.Start(Utils.CaptureScreenshot());
        }
        public override bool Enabled(RoleBehaviour role)
        {
            return role is TheVisionary;
        }
    }
}
