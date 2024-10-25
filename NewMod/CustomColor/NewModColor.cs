using MiraAPI.Colors;
using UnityEngine;

namespace NewMod.Colors
{
    [RegisterCustomColors]
    public static class NewModColors
    {
        public static CustomColor OceanColor {get;} = new CustomColor("OceaBlue", new Color32(0, 105, 148, 255), new Color32(0, 73, 103, 255));
        public static CustomColor Gold {get;} = new CustomColor("Gold", new Color(1.0f, 0.84f, 0.0f)); // Thanks to : https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExampleColors.cs#L13
        public static CustomColor BloodRed { get; } = new CustomColor("BloodRed", new Color32(138, 3, 3, 255), new Color32(104, 2, 2, 255));
        public static CustomColor CrimsonTide { get; } = new CustomColor("CrimsonTide", new Color32(220, 20, 60, 255), new Color32(176, 16, 48, 255));
        public static CustomColor MidnightBlue {get;} = new CustomColor("MidNight",new Color32(25, 25, 112, 255), new Color32(15, 15, 80, 255));
        public static CustomColor NeonGreen {get;} = new CustomColor("NeonGreen", new Color32(57, 255, 20, 255), new Color32(34, 139, 34, 255));
        public static CustomColor ElectricPurple {get;} = new CustomColor("ElectricPurple", new Color32(191, 0, 255, 255), new Color32(128, 0, 170, 255));
        public static CustomColor PastelPink {get;} = new CustomColor("PastelPink", new Color32(255, 182, 193, 255), new Color32(255, 105, 180, 255));
        public static CustomColor JadeGreen { get; } = new CustomColor("JadeGreen", new Color32(0, 168, 107, 255), new Color32(0, 134, 85, 255));
        public static CustomColor CobaltBlue { get; } = new CustomColor("CobaltBlue", new Color32(0, 71, 171, 255), new Color32(0, 57, 137, 255));
        public static CustomColor BurntSienna { get; } = new CustomColor("BurntSienna", new Color32(233, 116, 81, 255), new Color32(187, 93, 65, 255));
        public static CustomColor TropicalYellow { get; } = new CustomColor("TropicalYellow", new Color32(255, 255, 102, 255), new Color32(230, 230, 90, 255));
        public static CustomColor VelvetMaroon { get; } = new CustomColor("VelvetMaroon", new Color32(128, 0, 0, 255), new Color32(105, 0, 0, 255));
        public static CustomColor DesertRose { get; } = new CustomColor("DesertRose", new Color32(201, 76, 76, 255), new Color32(175, 60, 60, 255));
        public static CustomColor AtomicTangerine { get; } = new CustomColor("AtomicTangerine", new Color32(255, 153, 102, 255), new Color32(230, 140, 95, 255));
        public static CustomColor Olive {get;} = new CustomColor("Olive", new Color32(128, 128, 0, 255));
    }
}
