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
    public static LoadableResourceAsset Banner { get; } = new("NewMod.Resources.optionImage.png");
    public static LoadableResourceAsset Arrow { get; } = new("NewMod.Resources.Arrow.png");
    public static LoadableResourceAsset ModLogo { get; } = new("NewMod.Resources.Logo.png");
    public static LoadableResourceAsset NewModLogo { get; } = new("NewMod.Resources.NewModLogo.png");
    public static LoadableResourceAsset NormalLogo { get; } = new("NewMod.Resources.NormalLogo.png");
    public static LoadableResourceAsset NMIcon { get; } = new("NewMod.Resources.nm.png");
    public static LoadableResourceAsset CustomCursor { get; } = new("NewMod.Resources.cursor.png");
    public static LoadableAsset<GameObject> Toast { get; } = new LoadableBundleAsset<GameObject>("Toast", Bundle);
    public static LoadableAsset<GameObject> SlashTray { get; } = new LoadableBundleAsset<GameObject>("SlashTray", Bundle);
    public static LoadableAsset<Sprite> ConfirmIconHover { get; } = new LoadableBundleAsset<Sprite>("confirmOutline", Bundle);
    public static LoadableAsset<Sprite> DenyIconHover { get; } = new LoadableBundleAsset<Sprite>("deniedOutline", Bundle);
    public static LoadableResourceAsset ResidualTrace { get; } = new("NewMod.Resources.residualTrace.png");
    public static LoadableResourceAsset PowerNodeActive { get; } = new("NewMod.Resources.Active.png");
    public static LoadableResourceAsset PowerNodeOvercharged { get; } = new("NewMod.Resources.Overcharged.png");

    // Button icons
    public static LoadableResourceAsset SpecialAgentButton { get; } = new("NewMod.Resources.givemission.png");
    public static LoadableResourceAsset ShowScreenshotButton { get; } = new("NewMod.Resources.showscreenshot.png");
    public static LoadableResourceAsset DoomAwakeningButton { get; } = new("NewMod.Resources.doomawakening.png");
    public static LoadableResourceAsset NecromancerButton { get; } = new("NewMod.Resources.Revive2.png");
    public static LoadableResourceAsset InjectButton { get; } = new("NewMod.Resources.inject.png");
    public static LoadableResourceAsset DeadBodySprite { get; } = new("NewMod.Resources.deadbody.png");
    public static LoadableResourceAsset Camera { get; } = new("NewMod.Resources.cam.png");
    public static LoadableResourceAsset StrikeButton { get; } = new("NewMod.Resources.Strike.png");
    public static LoadableResourceAsset FinalButton { get; } = new("NewMod.Resources.final.png");
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
    public static LoadableResourceAsset AggressionEnergyIcon { get; } = new("NewMod.Resources.Aggression.png");
    public static LoadableResourceAsset ControlEnergyIcon { get; } = new("NewMod.Resources.Control.png");
    public static LoadableResourceAsset IntelligenceEnergyIcon { get; } = new("NewMod.Resources.Intelligence.png");
    public static LoadableResourceAsset MobilityEnergyIcon { get; } = new("NewMod.Resources.Mobility.png");
    public static LoadableResourceAsset ProtectionEnergyIcon { get; } = new("NewMod.Resources.Protection.png");


    // SFX
    public static LoadableAudioResourceAsset ReviveSound { get; } = new("NewMod.Resources.Sounds.revive.wav");
    public static LoadableAudioResourceAsset DoomAwakeningSound { get; } = new("NewMod.Resources.Sounds.gloomy_aura.wav");
    public static LoadableAudioResourceAsset DoomAwakeningEndSound { get; } = new("NewMod.Resources.Sounds.evil_laugh.wav");
    public static LoadableAudioResourceAsset FeignDeathSound { get; } = new("NewMod.Resources.Sounds.feign_death.wav");
    public static LoadableAudioResourceAsset VisionarySound { get; } = new("NewMod.Resources.Sounds.visionary_sound.wav");
    public static LoadableAudioResourceAsset StrikeSound { get; } = new("NewMod.Resources.Sounds.strike_sound.wav");
    public static LoadableAudioResourceAsset FearSound { get; } = new("NewMod.Resources.Sounds.fear_sound.wav");
    public static LoadableAudioResourceAsset HeartbeatSound { get; } = new("NewMod.Resources.Sounds.heartbeat_sound.wav");
    public static LoadableAudioResourceAsset GEEnterSound { get; } = new("NewMod.Resources.Sounds.ge_enter.wav");
    public static LoadableAudioResourceAsset GEExitSound { get; } = new("NewMod.Resources.Sounds.ge_exit.wav");
    public static LoadableAudioResourceAsset EnterVoidSFX { get; } = new("NewMod.Resources.Sounds.entervoid.wav");

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
    public static LoadableAsset<Shader> EarthquakeShader { get; } = new LoadableBundleAsset<Shader>("EarthquakeFullScreen.shader", Bundle);
    public static LoadableAsset<Shader> SlowPulseHueShader { get; } = new LoadableBundleAsset<Shader>("SlowPulseHue.shader", Bundle);
    public static LoadableAsset<Shader> DistorationWaveShader { get; } = new LoadableBundleAsset<Shader>("DistorationWave.shader", Bundle);
    public static LoadableAsset<Shader> ShadowFluxShader { get; } = new LoadableBundleAsset<Shader>("ShadowFlux.shader", Bundle);
    public static LoadableAsset<Shader> CrismonVortexShader { get; } = new LoadableBundleAsset<Shader>("CrismonVortexV3.shader", Bundle);
    public static LoadableAsset<Shader> VoidwalkerTransitionVoid { get; } = new LoadableBundleAsset<Shader>("VoidwalkerTransition.shader", Bundle);
    public static LoadableAsset<Shader> VoidwalkerVoidShader { get; } = new LoadableBundleAsset<Shader>("VoidwalkerVoid.shader", Bundle);
    public static LoadableAsset<Shader> NegativeRealityShader { get; } = new LoadableBundleAsset<Shader>("NegativeReality.shader", Bundle);
    public static LoadableAsset<Shader> ShatteredGlassShader { get; } = new LoadableBundleAsset<Shader>("ShatteredGlass.shader", Bundle);

    // Textures
    public static LoadableAsset<Texture2D> NoiseTex { get; } = new LoadableBundleAsset<Texture2D>("noise.png", Bundle);
    public static LoadableResourceAsset CrismonTexture { get; } = new("NewMod.Resources.turbulence8.png");
    public static LoadableAsset<Texture2D> ShatteredGlassTexture { get; } = new LoadableBundleAsset<Texture2D>("radialShatter.png", Bundle);

    //General Events
    public static LoadableAsset<GameObject> GeneralEventHud { get; } = new LoadableBundleAsset<GameObject>("GeneralEvent", Bundle);
    public static LoadableResourceAsset CrismonIcon { get; } = new("NewMod.Resources.GeneralEvents.crismon_ge_icon.png");
    public static LoadableResourceAsset NegativeRealityIcon { get; } = new("NewMod.Resources.GeneralEvents.NegativeIcon.png");
    public static LoadableResourceAsset IdentityCrisisIcon { get; } = new("NewMod.Resources.GeneralEvents.IdentityCrisis.png");
    public static LoadableResourceAsset RoleScrambleIcon { get; } = new("NewMod.Resources.GeneralEvents.RoleScramble.png");
    public static LoadableResourceAsset SystemOverrideIcon { get; } = new("NewMod.Resources.GeneralEvents.SystemOverride.png");
    public static LoadableResourceAsset AbilityExchangeIcon { get; } = new("NewMod.Resources.GeneralEvents.AbilityExchange.png");
    public static LoadableResourceAsset NoMansLandIcon { get; } = new("NewMod.Resources.GeneralEvents.NoMansLand.png");

    //Cosmetics
    public static LoadableResourceAsset OG_NewModHat { get; } = new("NewMod.Resources.Cosmetics.Hats.og_newmod.png");
    public static LoadableResourceAsset MintIceCreamHat { get; } = new("NewMod.Resources.Cosmetics.Hats.minticecream.png");
    public static LoadableResourceAsset StrawberryIceCreamHat { get; } = new("NewMod.Resources.Cosmetics.Hats.strawberryicecream.png");
    public static LoadableResourceAsset PizzaHat { get; } = new("NewMod.Resources.Cosmetics.Hats.pizza.png");
    public static LoadableResourceAsset SqueezeCapHat { get; } = new("NewMod.Resources.Cosmetics.Hats.squeezecap.png");

    public static LoadableResourceAsset ZrosHat { get; } = new("NewMod.Resources.Cosmetics.Hats.zros.png");

    public static LoadableResourceAsset IGotanIdeaHat { get; } = new("NewMod.Resources.Cosmetics.Hats.igotanidea.png");

    public static LoadableResourceAsset CottonMemoriesVisor { get; } = new("NewMod.Resources.Cosmetics.Visors.cottonmemories.png");
    public static LoadableResourceAsset MaliciousLook { get; } = new("NewMod.Resources.Cosmetics.Visors.maliciouslook.png");

    public static LoadableResourceAsset GlitchedRealityHat { get; } = new("NewMod.Resources.Cosmetics.Hats.glitchedReality.png");
    public static LoadableResourceAsset SunnyNameplate { get; } = new("NewMod.Resources.Cosmetics.Nameplates.sunnynameplate.png");
    public static LoadableResourceAsset NMraveNameplate { get; } = new("NewMod.Resources.Cosmetics.Nameplates.nmrave.png");

    //Minigames
    public static LoadableAsset<GameObject> VerifyMinigame { get; } = new LoadableBundleAsset<GameObject>("VerifyMinigame", Bundle);
    public static LoadableAsset<GameObject> WraithCallerMinigame { get; } = new LoadableBundleAsset<GameObject>("WraithCallerMinigame", Bundle);


    // GameModes
    public static LoadableResourceAsset WraithSiegeFlag { get; } = new("NewMod.Resources.flag.png");
    public static LoadableResourceAsset WraithSiegeTicket { get; } = new("NewMod.Resources.ticket.png");
    public static LoadableResourceAsset WraithSiegeWraith { get; } = new("NewMod.Resources.wraith.png");
    public static LoadableResourceAsset WraithSiegeBanish { get; } = new("NewMod.Resources.banish.png");
}