using MiraAPI.Utilities.Assets;

namespace NewMod;

public static class NewModAsset
{
   public static LoadableResourceAsset Banner { get; } = new("NewMod.Resources.optionImage.png");
   public static LoadableResourceAsset DeadBodySprite { get; } = new("NewMod.Resources.deadbody.png");
   public static LoadableResourceAsset NecromancerButton { get; } = new("NewMod.Resources.Revive2.png");
}