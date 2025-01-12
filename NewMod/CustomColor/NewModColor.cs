using MiraAPI.Colors;
using UnityEngine;

namespace NewMod.Colors
{
    [RegisterCustomColors]
    public static class NewModColors
    {
        // NewMod v1.0.0
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

        // NewMod v1.1.0
        public static CustomColor SkyBlue { get; } = new CustomColor("SkyBlue", new Color32(135, 206, 235, 255), new Color32(70, 130, 180, 255));
        public static CustomColor Salmon { get; } = new CustomColor("Salmon", new Color32(250, 128, 114, 255), new Color32(233, 150, 122, 255));
        public static CustomColor Teal { get; } = new CustomColor("Teal", new Color32(0, 128, 128, 255), new Color32(0, 100, 100, 255));
        public static CustomColor Amber { get; } = new CustomColor("Amber", new Color32(255, 191, 0, 255), new Color32(255, 165, 0, 255));
        public static CustomColor Turquoise { get; } = new CustomColor("Turquoise", new Color32(64, 224, 208, 255), new Color32(72, 209, 204, 255));
        public static CustomColor SlateGray { get; } = new CustomColor("SlateGray", new Color32(112, 128, 144, 255), new Color32(47, 79, 79, 255));
        public static CustomColor Periwinkle { get; } = new CustomColor("Periwinkle", new Color32(204, 204, 255, 255), new Color32(196, 196, 255, 255));
        public static CustomColor LimeGreen { get; } = new CustomColor("LimeGreen", new Color32(50, 205, 50, 255), new Color32(34, 139, 34, 255));
        public static CustomColor Indigo { get; } = new CustomColor("Indigo", new Color32(75, 0, 130, 255), new Color32(54, 0, 102, 255));
        public static CustomColor Apricot { get; } = new CustomColor("Apricot", new Color32(251, 206, 177, 255), new Color32(255, 160, 122, 255));
        public static CustomColor Charcoal { get; } = new CustomColor("Charcoal", new Color32(54, 69, 79, 255), new Color32(70, 70, 70, 255));
        public static CustomColor Burgundy { get; } = new CustomColor("Burgundy", new Color32(128, 0, 32, 255), new Color32(100, 0, 20, 255));
        public static CustomColor Mustard { get; } = new CustomColor("Mustard", new Color32(255, 219, 88, 255), new Color32(255, 215, 0, 255));
        public static CustomColor Emerald { get; } = new CustomColor("Emerald", new Color32(80, 200, 120, 255), new Color32(0, 201, 87, 255));
        public static CustomColor Fuchsia { get; } = new CustomColor("Fuchsia", new Color32(255, 119, 255, 255), new Color32(255, 0, 255, 255));
        public static CustomColor NavyBlue { get; } = new CustomColor("NavyBlue", new Color32(0, 0, 128, 255), new Color32(0, 0, 102, 255));
    }
}
