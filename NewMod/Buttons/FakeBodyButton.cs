using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.PranksterOptions;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using NewMod.Utilities;

namespace NewMod.Buttons
{
    [RegisterButton]
    public class FakeBodyButton : CustomActionButton
    {
       public override string Name => "Prank";
       public override float Cooldown => OptionGroupSingleton<PranksterOptions>.Instance.PrankCooldown;
       public override int MaxUses => (int)OptionGroupSingleton<PranksterOptions>.Instance.PrankMaxUses;
       public override ButtonLocation Location => ButtonLocation.BottomRight;
       public override float EffectDuration => 0f;
       public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadBodySprite;
       protected override void OnClick()
       {
          PranksterUtilities.CreatePranksterDeadBody();
       }
        public override bool Enabled(RoleBehaviour role)
        {
            return role is Prankster;
        }
    }
}