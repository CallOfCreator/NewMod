using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles.VisionaryOptions
{
    public class VisionaryOptions : AbstractOptionGroup
    {
        public override string GroupName => "The Visionary";
        public override Type AdvancedRole => typeof(TheVisionary);

        [ModdedNumberOption("Screenshot Cooldown", min:5, max:30)]
        public float ScreenshotCooldown { get; set; } = 15f;

        [ModdedNumberOption("Max Screenshots", min:1, max:5)]
        public float MaxScreenshots { get; set; } = 3f;

        [ModdedNumberOption("Max Display Duration", min:5, max:10)]
        public float MaxDisplayDuration { get; set; } = 5f;
    }
}