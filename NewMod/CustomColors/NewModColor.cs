using MiraAPI.Colors;
using UnityEngine;

namespace NewMod.CustomColors;

[RegisterCustomColors]
public static class NewModColors
{
    // NewMod v1.0.0
    public static CustomColor OceanColor { get; } = new("OceaBlue", new Color32(0, 105, 148, 255), new Color32(0, 73, 103, 255));
    public static CustomColor Gold { get; } = new("Gold", new Color(1.0f, 0.84f, 0.0f)); // Thanks to : https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExampleColors.cs#L13
    public static CustomColor BloodRed { get; } = new("BloodRed", new Color32(138, 3, 3, 255), new Color32(104, 2, 2, 255));
    public static CustomColor CrimsonTide { get; } = new("CrimsonTide", new Color32(220, 20, 60, 255), new Color32(176, 16, 48, 255));
    public static CustomColor MidnightBlue { get; } = new("MidNight", new Color32(25, 25, 112, 255), new Color32(15, 15, 80, 255));
    public static CustomColor NeonGreen { get; } = new("NeonGreen", new Color32(57, 255, 20, 255), new Color32(34, 139, 34, 255));
    public static CustomColor ElectricPurple { get; } = new("ElectricPurple", new Color32(191, 0, 255, 255), new Color32(128, 0, 170, 255));
    public static CustomColor PastelPink { get; } = new("PastelPink", new Color32(255, 182, 193, 255), new Color32(255, 105, 180, 255));
    public static CustomColor JadeGreen { get; } = new("JadeGreen", new Color32(0, 168, 107, 255), new Color32(0, 134, 85, 255));
    public static CustomColor CobaltBlue { get; } = new("CobaltBlue", new Color32(0, 71, 171, 255), new Color32(0, 57, 137, 255));
    public static CustomColor BurntSienna { get; } = new("BurntSienna", new Color32(233, 116, 81, 255), new Color32(187, 93, 65, 255));
    public static CustomColor TropicalYellow { get; } = new("TropicalYellow", new Color32(255, 255, 102, 255), new Color32(230, 230, 90, 255));
    public static CustomColor VelvetMaroon { get; } = new("VelvetMaroon", new Color32(128, 0, 0, 255), new Color32(105, 0, 0, 255));
    public static CustomColor DesertRose { get; } = new("DesertRose", new Color32(201, 76, 76, 255), new Color32(175, 60, 60, 255));
    public static CustomColor AtomicTangerine { get; } = new("AtomicTangerine", new Color32(255, 153, 102, 255), new Color32(230, 140, 95, 255));
    public static CustomColor Olive { get; } = new("Olive", new Color32(128, 128, 0, 255));

    // NewMod v1.1.0
    public static CustomColor SkyBlue { get; } = new("SkyBlue", new Color32(135, 206, 235, 255), new Color32(70, 130, 180, 255));
    public static CustomColor Salmon { get; } = new("Salmon", new Color32(250, 128, 114, 255), new Color32(233, 150, 122, 255));
    public static CustomColor Teal { get; } = new("Teal", new Color32(0, 128, 128, 255), new Color32(0, 100, 100, 255));
    public static CustomColor Amber { get; } = new("Amber", new Color32(255, 191, 0, 255), new Color32(255, 165, 0, 255));
    public static CustomColor Turquoise { get; } = new("Turquoise", new Color32(64, 224, 208, 255), new Color32(72, 209, 204, 255));
    public static CustomColor SlateGray { get; } = new("SlateGray", new Color32(112, 128, 144, 255), new Color32(47, 79, 79, 255));
    public static CustomColor Periwinkle { get; } = new("Periwinkle", new Color32(204, 204, 255, 255), new Color32(196, 196, 255, 255));
    public static CustomColor LimeGreen { get; } = new("LimeGreen", new Color32(50, 205, 50, 255), new Color32(34, 139, 34, 255));
    public static CustomColor Indigo { get; } = new("Indigo", new Color32(75, 0, 130, 255), new Color32(54, 0, 102, 255));
    public static CustomColor Apricot { get; } = new("Apricot", new Color32(251, 206, 177, 255), new Color32(255, 160, 122, 255));
    public static CustomColor Charcoal { get; } = new("Charcoal", new Color32(54, 69, 79, 255), new Color32(70, 70, 70, 255));
    public static CustomColor Burgundy { get; } = new("Burgundy", new Color32(128, 0, 32, 255), new Color32(100, 0, 20, 255));
    public static CustomColor Mustard { get; } = new("Mustard", new Color32(255, 219, 88, 255), new Color32(255, 215, 0, 255));
    public static CustomColor Emerald { get; } = new("Emerald", new Color32(80, 200, 120, 255), new Color32(0, 201, 87, 255));
    public static CustomColor Fuchsia { get; } = new("Fuchsia", new Color32(255, 119, 255, 255), new Color32(255, 0, 255, 255));
    public static CustomColor NavyBlue { get; } = new("NavyBlue", new Color32(0, 0, 128, 255), new Color32(0, 0, 102, 255));

    // NewMod v1.2.4
    public static CustomColor CyberPink { get; } = new("CyberPink", new Color32(255, 20, 147, 255), new Color32(199, 0, 110, 255));
    public static CustomColor PlasmaBlue { get; } = new("PlasmaBlue", new Color32(0, 191, 255, 255), new Color32(0, 128, 192, 255));
    public static CustomColor RadiantOrange { get; } = new("RadiantOrange", new Color32(255, 94, 19, 255), new Color32(204, 75, 15, 255));
    public static CustomColor ToxicLime { get; } = new("ToxicLime", new Color32(173, 255, 47, 255), new Color32(139, 204, 38, 255));
    public static CustomColor VoidBlack { get; } = new("VoidBlack", new Color32(10, 10, 10, 255), new Color32(0, 0, 0, 255));
    public static CustomColor SolarFlare { get; } = new("SolarFlare", new Color32(255, 140, 0, 255), new Color32(204, 112, 0, 255));
    public static CustomColor ArcticWhite { get; } = new("ArcticWhite", new Color32(245, 245, 245, 255), new Color32(220, 220, 220, 255));
    public static CustomColor MysticPurple { get; } = new("MysticPurple", new Color32(147, 112, 219, 255), new Color32(118, 90, 175, 255));
    public static CustomColor InfernoRed { get; } = new("InfernoRed", new Color32(255, 48, 48, 255), new Color32(204, 38, 38, 255));
    public static CustomColor AquaWave { get; } = new("AquaWave", new Color32(64, 224, 208, 255), new Color32(54, 189, 176, 255));
    public static CustomColor RoseGold { get; } = new("RoseGold", new Color32(183, 110, 121, 255), new Color32(150, 90, 100, 255));
    public static CustomColor StealthGray { get; } = new("StealthGray", new Color32(84, 88, 94, 255), new Color32(64, 66, 70, 255));
    public static CustomColor NeonYellow { get; } = new("NeonYellow", new Color32(255, 255, 0, 255), new Color32(204, 204, 0, 255));
    public static CustomColor EmberOrange { get; } = new("EmberOrange", new Color32(255, 97, 0, 255), new Color32(204, 77, 0, 255));
    public static CustomColor DeepSeaTeal { get; } = new("DeepSeaTeal", new Color32(0, 128, 128, 255), new Color32(0, 102, 102, 255));
}