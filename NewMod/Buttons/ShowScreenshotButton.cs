using MiraAPI.Utilities.Assets;
using MiraAPI.Hud;
using MiraAPI.GameOptions;
using NewMod.Options.Roles.VisionaryOptions;
using NewMod.Roles.CrewmateRoles;
using UnityEngine;
using NewMod.Utilities;
using Reactor.Utilities;
using System.Linq;

namespace NewMod.Buttons
{
    public class ShowScreenshotButton : CustomActionButton
    {
        public override string Name => "ShowScreenshot";
        public override float Cooldown => OptionGroupSingleton<VisionaryOptions>.Instance.ScreenshotCooldown;
        public override float EffectDuration => 0;
        public override int MaxUses => (int)OptionGroupSingleton<VisionaryOptions>.Instance.MaxScreenshots;
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;
        public override ButtonLocation Location => ButtonLocation.BottomRight;
        public override bool CanUse()
        {
            return base.CanUse() && VisionaryUtilities.CapturedScreenshotPaths.Any();
        }
        protected override void OnClick()
        {
            Coroutines.Start(VisionaryUtilities.ShowScreenshots(OptionGroupSingleton<VisionaryOptions>.Instance.MaxDisplayDuration));
        }
        public override bool Enabled(RoleBehaviour role)
        {
            return role is TheVisionary;
        }
    }
}
