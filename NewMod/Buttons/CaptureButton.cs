using System;
using System.IO;
using System.Collections;
using MiraAPI.Utilities.Assets;
using MiraAPI.Hud;
using MiraAPI.GameOptions;
using NewMod.Options.Roles.ImageCollectorOptions;
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
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filePath = Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");
            ScreenCapture.CaptureScreenshot(filePath, 1);
            NewMod.Instance.Log.LogInfo($"Capturing screenshot at {filePath}.");
            Coroutines.Start(VisionaryUtilities.ShowScreenshots(OptionGroupSingleton<VisionaryOptions>.Instance.MaxDisplayDuration));
        }
        public override bool Enabled(RoleBehaviour role)
        {
            return role is TheVisionary;
        }
    }
}
