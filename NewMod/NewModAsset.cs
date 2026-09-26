using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod;

public static class NewModAsset
{
#pragma warning disable CA2211
    public static AssetBundle Bundle = AssetBundleManager.Load("newmod");
#pragma warning restore CA2211

    // Miscellaneous
    public static LoadableAsset<Sprite> Arrow { get; } = new LoadableBundleAsset<Sprite>("arrow.png", Bundle);
    public static LoadableResourceAsset NewModLogo { get; } = new("NewMod.Resources.NewModLogo.png");
    public static LoadableAsset<Sprite> NMIcon { get; } = new LoadableBundleAsset<Sprite>("nm", Bundle);
    public static LoadableAsset<Sprite> CustomCursor { get; } = new LoadableBundleAsset<Sprite>("cursor.png", Bundle);
    public static LoadableAsset<GameObject> Toast { get; } = new LoadableBundleAsset<GameObject>("Toast", Bundle);
    public static LoadableAsset<GameObject> SlashTray { get; } = new LoadableBundleAsset<GameObject>("SlashTray", Bundle);
    public static LoadableAsset<Sprite> ConfirmIconHover { get; } = new LoadableBundleAsset<Sprite>("confirmOutline", Bundle);
    public static LoadableAsset<Sprite> DenyIconHover { get; } = new LoadableBundleAsset<Sprite>("deniedOutline", Bundle);
    public static LoadableResourceAsset ResidualTrace { get; } = new("NewMod.Resources.residualTrace.png");
    public static LoadableAsset<Sprite> PowerNodeActive { get; } = new LoadableBundleAsset<Sprite>("active.png", Bundle);
    public static LoadableAsset<Sprite> PowerNodeOvercharged { get; } = new LoadableBundleAsset<Sprite>("overcharged.png", Bundle);

    // Button icons
    public static LoadableResourceAsset ShowScreenshotButton { get; } = new("NewMod.Resources.showscreenshot.png");
    public static LoadableResourceAsset DoomAwakeningButton { get; } = new("NewMod.Resources.doomawakening.png");
    public static LoadableResourceAsset NecromancerButton { get; } = new("NewMod.Resources.Revive2.png");
    public static LoadableResourceAsset InjectButton { get; } = new("NewMod.Resources.inject.png");
    public static LoadableResourceAsset DeadBodySprite { get; } = new("NewMod.Resources.deadbody.png");
    public static LoadableResourceAsset Camera { get; } = new("NewMod.Resources.cam.png");
    public static LoadableAsset<Sprite> CameraOff { get; } = new LoadableBundleAsset<Sprite>("cam_off.png", Bundle);
    
    public static LoadableAsset<Sprite> CameraOffOutline { get; } = new LoadableBundleAsset<Sprite>("cam_off.png", Bundle);
    public static LoadableAsset<Sprite> CameraEnabled { get; } = new LoadableBundleAsset<Sprite>("cam_enabled.png", Bundle);
    public static LoadableAsset<Sprite> CameraEnabledOutline { get; } = new LoadableBundleAsset<Sprite>("cam_enabled.png", Bundle);
    public static LoadableAsset<Sprite> CameraDisabled { get; } = new LoadableBundleAsset<Sprite>("cam_disabled.png", Bundle);
    public static LoadableAsset<Sprite> CameraDisabledOutline { get; } = new LoadableBundleAsset<Sprite>("cam_disabled.png", Bundle);
    public static LoadableAsset<Sprite> TyrantAlliance { get; } = new LoadableBundleAsset<Sprite>("tyrantAlliance.png", Bundle);
    public static LoadableAsset<Sprite> TyrantIntimidate { get; } = new LoadableBundleAsset<Sprite>("tyrantIntimidate.png", Bundle);
    public static LoadableResourceAsset StrikeButton { get; } = new("NewMod.Resources.Strike.png");
    public static LoadableResourceAsset CallWraith { get; } = new("NewMod.Resources.callwraith.png");
    public static LoadableResourceAsset Shield { get; } = new("NewMod.Resources.Shield.png");
    public static LoadableResourceAsset Slash { get; } = new("NewMod.Resources.Slash.png");
    public static LoadableResourceAsset DeployZone { get; } = new("NewMod.Resources.deployzone.png");
    public static LoadableResourceAsset VerifyButton { get; } = new("NewMod.Resources.verifier.png");
    public static LoadableResourceAsset VanillaKillButton { get; } = new("NewMod.Resources.killbutton.png");
    public static LoadableResourceAsset EnterVoid { get; } = new("NewMod.Resources.EnterVoid.png");
    public static LoadableResourceAsset WardenSeal { get; } = new("NewMod.Resources.Seal.png");
    public static LoadableResourceAsset WardenInvestigate { get; } = new("NewMod.Resources.investigate.png");
    public static LoadableResourceAsset AccuseButton { get; } = new("NewMod.Resources.Accuse.png");
    public static LoadableResourceAsset DefendButton { get; } = new("NewMod.Resources.Defend.png");
    public static LoadableResourceAsset LeverageButton { get; } = new("NewMod.Resources.leverage.png");
    public static LoadableResourceAsset TerminateButton { get; } = new("NewMod.Resources.RoleIcons.Terminate.png");
    public static LoadableResourceAsset ReflectButton { get; } = new("NewMod.Resources.RoleIcons.Reflect.png");
    public static LoadableResourceAsset DeadlockButton { get; } = new("NewMod.Resources.deadlock.png");
    public static LoadableResourceAsset OverrideButton { get; } = new("NewMod.Resources.override.png");
    public static LoadableResourceAsset AnchorButton { get; } = new("NewMod.Resources.anchor.png");
    public static LoadableResourceAsset WanderButton { get; } = new("NewMod.Resources.wander.png");
    public static LoadableResourceAsset GroundAbilityButton { get; } = new("NewMod.Resources.GroundAbility.png");


    // Energy category icons
    public static LoadableAsset<Sprite> AggressionEnergyIcon { get; } = new LoadableBundleAsset<Sprite>("aggression.png", Bundle);
    public static LoadableAsset<Sprite> ControlEnergyIcon { get; } = new LoadableBundleAsset<Sprite>("control.png", Bundle);
    public static LoadableAsset<Sprite> IntelligenceEnergyIcon { get; } = new LoadableBundleAsset<Sprite>("intelligence.png", Bundle);
    public static LoadableAsset<Sprite> MobilityEnergyIcon { get; } = new LoadableBundleAsset<Sprite>("mobility.png", Bundle);
    public static LoadableAsset<Sprite> ProtectionEnergyIcon { get; } = new LoadableBundleAsset<Sprite>("protection.png", Bundle);


    // SFX
    public static LoadableAsset<AudioClip> ReviveSound { get; } = new LoadableBundleAsset<AudioClip>("revive.wav", Bundle);
    public static LoadableAsset<AudioClip> DoomAwakeningSound { get; } = new LoadableBundleAsset<AudioClip>("gloomy_aura.wav", Bundle);
    public static LoadableAsset<AudioClip> DoomAwakeningEndSound { get; } = new LoadableBundleAsset<AudioClip>("evil_laugh.wav", Bundle);
    public static LoadableAsset<AudioClip> FeignDeathSound { get; } = new LoadableBundleAsset<AudioClip>("feign_death.wav", Bundle);
    public static LoadableAsset<AudioClip> VisionarySound { get; } = new LoadableBundleAsset<AudioClip>("visionary_sound.wav", Bundle);
    public static LoadableAsset<AudioClip> StrikeSound { get; } = new LoadableBundleAsset<AudioClip>("strike_sound.wav", Bundle);
    public static LoadableAsset<AudioClip> FearSound { get; } = new LoadableBundleAsset<AudioClip>("fear_sound.wav", Bundle);
    public static LoadableAsset<AudioClip> HeartbeatSound { get; } = new LoadableBundleAsset<AudioClip>("heartbeat_sound.wav", Bundle);
    public static LoadableAsset<AudioClip> GEEnterSound { get; } = new LoadableBundleAsset<AudioClip>("ge_enter.wav", Bundle);
    public static LoadableAsset<AudioClip> GEExitSound { get; } = new LoadableBundleAsset<AudioClip>("ge_exit.wav", Bundle);
    public static LoadableAsset<AudioClip> EnterVoidSFX { get; } = new LoadableBundleAsset<AudioClip>("entervoid.wav", Bundle);

    // Role Icons
    public static LoadableResourceAsset StrikeIcon { get; } = new("NewMod.Resources.RoleIcons.StrikeIcon.png");
    public static LoadableResourceAsset ReflectIcon { get; } = new("NewMod.Resources.RoleIcons.Reflect.png");
    public static LoadableResourceAsset TerminatorIcon { get; } = new("NewMod.Resources.RoleIcons.Terminate.png");
    public static LoadableResourceAsset InjectIcon { get; } = new("NewMod.Resources.RoleIcons.InjectIcon.png");
    public static LoadableResourceAsset CrownIcon { get; } = new("NewMod.Resources.RoleIcons.CrownIcon.png");
    public static LoadableResourceAsset WraithIcon { get; } = new("NewMod.Resources.RoleIcons.WraithIcon.png");
    public static LoadableResourceAsset ShieldIcon { get; } = new("NewMod.Resources.RoleIcons.ShieldIcon.png");
    public static LoadableResourceAsset RadarIcon { get; } = new("NewMod.Resources.RoleIcons.RadarIcon.png");
    public static LoadableResourceAsset SlashIcon { get; } = new("NewMod.Resources.RoleIcons.SlashIcon.png");
    public static LoadableResourceAsset DeployZoneIcon { get; } = new("NewMod.Resources.RoleIcons.DeployzoneIcon.png");
    public static LoadableResourceAsset ReviveIcon { get; } = new("NewMod.Resources.RoleIcons.ReviveIcon.png");
    public static LoadableResourceAsset VerifyIcon { get; } = new("NewMod.Resources.RoleIcons.VerifyIcon.png");
    public static LoadableResourceAsset VoidwalkerIcon { get; } = new("NewMod.Resources.RoleIcons.VoidwalkerIcon.png");
    public static LoadableResourceAsset WardenIcon { get; } = new("NewMod.Resources.RoleIcons.WardenRoleIcon.png");
    public static LoadableResourceAsset EnergyThiefIcon { get; } = new("NewMod.Resources.GridBreachAbilityIcon.png");

    // Notif Icons
    public static LoadableResourceAsset VisionDebuff { get; } = new("NewMod.Resources.NotifIcons.vision_debuff.png");
    public static LoadableResourceAsset SpeedDebuff { get; } = new("NewMod.Resources.NotifIcons.speed_debuff.png");
    public static LoadableResourceAsset Freeze { get; } = new("NewMod.Resources.NotifIcons.freeze.png");

    // Shaders
    public static LoadableAsset<Shader> GlitchShader { get; } = new LoadableBundleAsset<Shader>("GlitchFullScreen.shader", Bundle);
    public static LoadableAsset<Shader> GlitchScreenV2 { get; } = new LoadableBundleAsset<Shader>("GlitchFullScreenV2.shader", Bundle);
    public static LoadableAsset<Shader> EarthquakeShader { get; } = new LoadableBundleAsset<Shader>("EarthquakeFullScreen.shader", Bundle);
    public static LoadableAsset<Shader> SlowPulseHueShader { get; } = new LoadableBundleAsset<Shader>("SlowPulseHue.shader", Bundle);
    public static LoadableAsset<Shader> DistorationWaveShader { get; } = new LoadableBundleAsset<Shader>("DistorationWave.shader", Bundle);
    public static LoadableAsset<Shader> ShadowFluxShader { get; } = new LoadableBundleAsset<Shader>("ShadowFlux.shader", Bundle);
    public static LoadableAsset<Shader> CrismonVortexShader { get; } = new LoadableBundleAsset<Shader>("CrismonVortexV3.shader", Bundle);
    public static LoadableAsset<Shader> VoidwalkerTransitionVoid { get; } = new LoadableBundleAsset<Shader>("VoidwalkerTransition.shader", Bundle);
    public static LoadableAsset<Shader> VoidwalkerVoidShader { get; } = new LoadableBundleAsset<Shader>("VoidwalkerVoid.shader", Bundle);
    public static LoadableAsset<Shader> NegativeRealityShader { get; } = new LoadableBundleAsset<Shader>("NegativeReality.shader", Bundle);
    public static LoadableAsset<Shader> ShatteredGlassShader { get; } = new LoadableBundleAsset<Shader>("ShatteredGlass.shader", Bundle);
    public static LoadableAsset<Shader> EnergyThiefBreak { get; } = new LoadableBundleAsset<Shader>("EnergyThiefBreak.shader", Bundle);

    // Textures
    public static LoadableAsset<Texture2D> NoiseTex { get; } = new LoadableBundleAsset<Texture2D>("noise.png", Bundle);
    public static LoadableAsset<Texture2D> CrismonTexture { get; } = new LoadableBundleAsset<Texture2D>("turbulence8.png", Bundle);
    public static LoadableAsset<Texture2D> ShatteredGlassTexture { get; } = new LoadableBundleAsset<Texture2D>("radialShatter.png", Bundle);

    //General Events
    public static LoadableAsset<GameObject> GeneralEventHud { get; } = new LoadableBundleAsset<GameObject>("GeneralEvent", Bundle);
    public static LoadableAsset<Sprite> CrismonIcon { get; } = new LoadableBundleAsset<Sprite>("crismon_ge_icon.png", Bundle);
    public static LoadableAsset<Sprite> NegativeRealityIcon { get; } = new LoadableBundleAsset<Sprite>("negativeicon.png", Bundle);
    public static LoadableAsset<Sprite> IdentityCrisisIcon { get; } = new LoadableBundleAsset<Sprite>("identitycrisis.png", Bundle);
    public static LoadableAsset<Sprite> RoleScrambleIcon { get; } = new LoadableBundleAsset<Sprite>("rolescramble.png", Bundle);
    public static LoadableAsset<Sprite> SystemOverrideIcon { get; } = new LoadableBundleAsset<Sprite>("systemoverride.png", Bundle);
    public static LoadableAsset<Sprite> AbilityExchangeIcon { get; } = new LoadableBundleAsset<Sprite>("abilityexchange.png", Bundle);
    public static LoadableAsset<Sprite> NoMansLandIcon { get; } = new LoadableBundleAsset<Sprite>("nomansland.png", Bundle);
    public static LoadableAsset<Sprite> ScrDesyncIcon { get; } = new LoadableBundleAsset<Sprite>("scrdesyncicon.png", Bundle);

    //Cosmetics
    public static LoadableAsset<Sprite> OG_NewModHat { get; } = new LoadableBundleAsset<Sprite>("og_newmod.png", Bundle);
    public static LoadableAsset<Sprite> MintIceCreamHat { get; } = new LoadableBundleAsset<Sprite>("minticecream.png", Bundle);
    public static LoadableAsset<Sprite> StrawberryIceCreamHat { get; } = new LoadableBundleAsset<Sprite>("strawberryicecream.png", Bundle);
    public static LoadableAsset<Sprite> PizzaHat { get; } = new LoadableBundleAsset<Sprite>("pizza.png", Bundle);
    public static LoadableAsset<Sprite> SqueezeCapHat { get; } = new LoadableBundleAsset<Sprite>("squeezecap.png", Bundle);

    public static LoadableAsset<Sprite> ZrosHat { get; } = new LoadableBundleAsset<Sprite>("zros.png", Bundle);

    public static LoadableAsset<Sprite> IGotanIdeaHat { get; } = new LoadableBundleAsset<Sprite>("igotanidea.png", Bundle);

    public static LoadableAsset<Sprite> CottonMemoriesVisor { get; } = new LoadableBundleAsset<Sprite>("cottonmemories.png", Bundle);
    public static LoadableAsset<Sprite> MaliciousLook { get; } = new LoadableBundleAsset<Sprite>("maliciouslook.png", Bundle);

    public static LoadableAsset<Sprite> GlitchedRealityHat { get; } = new LoadableBundleAsset<Sprite>("glitchedreality.png", Bundle);
    public static LoadableAsset<Sprite> SunnyNameplate { get; } = new LoadableBundleAsset<Sprite>("sunnynameplate.png", Bundle);
    public static LoadableAsset<Sprite> NMraveNameplate { get; } = new LoadableBundleAsset<Sprite>("nmrave.png", Bundle);

    //Minigames
    public static LoadableAsset<GameObject> VerifyMinigame { get; } = new LoadableBundleAsset<GameObject>("VerifyMinigame", Bundle);
    public static LoadableAsset<GameObject> WraithCallerMinigame { get; } = new LoadableBundleAsset<GameObject>("WraithCallerMinigame", Bundle);
    public static LoadableAsset<GameObject> PhotoPanelMinigame { get; } = new LoadableBundleAsset<GameObject>("PhotoPanelMinigame", Bundle);


    // GameModes
    public static LoadableAsset<Sprite> WraithSiegeFlag { get; } = new LoadableBundleAsset<Sprite>("flag.png", Bundle);
    public static LoadableAsset<Sprite> WraithSiegeTicket { get; } = new LoadableBundleAsset<Sprite>("ticket.png", Bundle);
    public static LoadableAsset<Sprite> WraithSiegeWraith { get; } = new LoadableBundleAsset<Sprite>("wraith.png", Bundle);
    public static LoadableResourceAsset WraithSiegeBanish { get; } = new("NewMod.Resources.banish.png");
}