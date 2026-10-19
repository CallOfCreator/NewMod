using AchievementsAPI.API;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace NewMod.Achievements;

public class NewModAchievementsTab : AchievementsTab
{
    public override StringNames Name { get; } = CustomStringName.CreateAndRegister("NewMod");

    public override Sprite GetIcon()
    {
        return NewModAsset.NMIcon.LoadAsset();
    }

    public static BaseAchievement WelcomeToNewMod { get; } = new(
        "Welcome to NewMod",
        "Start an online NewMod match.",
        NewModAsset.NMIcon.LoadAsset());

    public static CountAchievement ThreeInARow { get; } = new(
        "Three in a Row",
        "Complete three online NewMod matches in one session.",
        NewModAsset.OG_NewModHat.LoadAsset(),
        0,
        3,
        AchPersistence.ThroughoutSessions,
        rarity: 3);

    public static BaseAchievement TraceEvidence { get; } = new(
        "Trace Evidence",
        "As Warden, seal a location and investigate its Residual Trace.",
        NewModAsset.ResidualTrace.LoadAsset(),
        rarity: 2,
        tags: ["Warden"]) { Category = AchievementCategories.Crewmate };

    public static BaseAchievement EventHorizonDenied { get; } = new(
        "Event Horizon Denied",
        "Enter the Crimson Vortex and successfully escape it.",
        NewModAsset.CrismonIcon.LoadAsset(),
        rarity: 2,
        tags: ["Crimson Vortex", "General Events"]);

    public static BaseAchievement TrustButVerify { get; } = new(
        "Trust, But Verify",
        "As Verifier, receive a Confirmed or Denied result.",
        NewModAsset.VerifyIcon.LoadAsset(),
        rarity: 1,
        tags: ["Verifier"]) { Category = AchievementCategories.Crewmate };
}

