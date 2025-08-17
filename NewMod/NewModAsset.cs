using MiraAPI.Utilities.Assets;

namespace NewMod;

public static class NewModAsset
{
    // Miscellaneous
    public static LoadableResourceAsset Banner { get; } = new("NewMod.Resources.optionImage.png");
    public static LoadableResourceAsset Arrow { get; } = new("NewMod.Resources.Arrow.png");
    public static LoadableResourceAsset ModLogo { get; } = new("NewMod.Resources.Logo.png");

    // Button icons
    public static LoadableResourceAsset SpecialAgentButton { get; } = new("NewMod.Resources.givemission.png");
    public static LoadableResourceAsset ShowScreenshotButton { get; } = new("NewMod.Resources.showscreenshot.png");
    public static LoadableResourceAsset DoomAwakeningButton { get; } = new("NewMod.Resources.doomawakening.png");
    public static LoadableResourceAsset NecromancerButton { get; } = new("NewMod.Resources.Revive2.png");
    public static LoadableResourceAsset InjectButton { get; } = new("NewMod.Resources.inject.png");
    public static LoadableResourceAsset DeadBodySprite { get; } = new("NewMod.Resources.deadbody.png");
    public static LoadableResourceAsset Camera { get; } = new("NewMod.Resources.cam.png");
    public static LoadableResourceAsset StrikeButton { get; } = new("NewMod.Resources.Strike.png");

    // SFX
    public static LoadableAudioResourceAsset ReviveSound { get; } = new("NewMod.Resources.Sounds.revive.wav");
    public static LoadableAudioResourceAsset DoomAwakeningSound { get; } = new("NewMod.Resources.Sounds.gloomy_aura.wav");
    public static LoadableAudioResourceAsset DoomAwakeningEndSound { get; } = new("NewMod.Resources.Sounds.evil_laugh.wav");
    public static LoadableAudioResourceAsset DrainSound { get; } = new("NewMod.Resources.Sounds.drain_sound.wav");
    public static LoadableAudioResourceAsset FeignDeathSound { get; } = new("NewMod.Resources.Sounds.feign_death.wav");
    public static LoadableAudioResourceAsset VisionarySound { get; } = new("NewMod.Resources.Sounds.visionary_sound.wav");
    public static LoadableAudioResourceAsset StrikeSound { get; } = new("NewMod.Resources.Sounds.strike_sound.wav");
    public static LoadableAudioResourceAsset FearSound { get; } = new("NewMod.Resources.Sounds.fear_sound.wav");
    public static LoadableAudioResourceAsset HeartbeatSound { get; } = new("NewMod.Resources.Sounds.heartbeat_sound.wav");

    // Role Icons
    public static LoadableResourceAsset StrikeIcon { get; } = new("NewMod.Resources.RoleIcons.StrikeIcon.png");
    public static LoadableResourceAsset InjectIcon { get; } = new("NewMod.Resources.RoleIcons.InjectIcon.png");
    public static LoadableResourceAsset CrownIcon { get; } = new("NewMod.Resources.RoleIcons.crown.png");

    // Notif Icons
    public static LoadableResourceAsset VisionDebuff { get; } = new("NewMod.Resources.NotifIcons.vision_debuff.png");
    public static LoadableResourceAsset SpeedDebuff { get; } = new("NewMod.Resources.NotifIcons.speed_debuff.png");
    public static LoadableResourceAsset Freeze { get; } = new("NewMod.Resources.NotifIcons.freeze.png");
}