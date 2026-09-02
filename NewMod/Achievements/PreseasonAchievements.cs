using System.Linq;
using AchievementsAPI.API;
using NewMod.Seasons;
using UnityEngine;

namespace NewMod.Achievements;

public sealed class PreseasonAchievementsTab : AchievementsTab
{
    public override string Name => "NewMod Preseason ALPHA";

    public override bool IsSelectable => SeasonManager.AvailableAchievementTabTypes.Contains(typeof(PreseasonAchievementsTab));

    public static BaseAchievement WelcomeToNewMod { get; } = new("Welcome to NewMod", "Start a Freeplay or online NewMod match.", NewModAsset.NMIcon.LoadAsset());

    public static CountAchievement ThreeInARow { get; } = new("Three in a Row", "Complete three online NewMod matches in one session.", NewModAsset.OG_NewModHat.LoadAsset(), 0, 3, AchPersistence.ThroughoutSessions, 3);

    public static BaseAchievement EventHorizonDenied { get; } = new("Event Horizon Denied", "Enter the Crimson Vortex and successfully escape it.", NewModAsset.CrismonIcon.LoadAsset(), 2);

    public static BaseAchievement TrustButVerify { get; } = new("Trust, But Verify", "As Verifier, receive a Confirmed or Denied result.", NewModAsset.VerifyIcon.LoadAsset(), 1);

    public override Color GetTabColor()
    {
        return new Color32(69, 38, 105, 255);
    }

    public override Sprite GetIcon()
    {
        return NewModAsset.NMIcon.LoadAsset();
    }
}