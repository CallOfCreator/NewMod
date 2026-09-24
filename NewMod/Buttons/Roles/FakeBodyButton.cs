using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class FakeBodyButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override string Name => "Prank";
    public override float Cooldown => OptionGroupSingleton<PranksterOptions>.Instance.PrankCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadBodySprite;

    public override bool CanUse()
    {
        return base.CanUse() && !PranksterUtilities.FindAllPranksterBodies().Any(body => body.ParentId == PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        PranksterUtilities.CreatePranksterDeadBody(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.PlayerId);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Prankster;
    }
}