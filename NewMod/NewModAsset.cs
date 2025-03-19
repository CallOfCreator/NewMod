using UnityEngine;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;

namespace NewMod;

public static class NewModAsset
{
   public static LoadableResourceAsset Banner { get; } = new("NewMod.Resources.optionImage.png");
   public static LoadableResourceAsset DeadBodySprite { get; } = new("NewMod.Resources.deadbody.png");
   public static LoadableResourceAsset NecromancerButton { get; } = new("NewMod.Resources.Revive2.png");
   public static LoadableResourceAsset Arrow { get; } = new("NewMod.Resources.Arrow.png");
   public static LoadableResourceAsset ModLogo { get; } = new("NewMod.Resources.Logo.png");
   public static LoadableResourceAsset Camera { get; } = new("NewMod.Resources.cam.png");
   public static LoadableResourceAsset SpecialAgentButton { get; } = new("NewMod.Resources.givemission.png");
   public static LoadableAudioResourceAsset ReviveSound { get; } = new("NewMod.Resources.Sounds.revive.wav");
}