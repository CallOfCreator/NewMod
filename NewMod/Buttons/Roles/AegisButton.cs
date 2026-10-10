using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Components;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

/// <summary>
/// Places an Aegis shield zone.
/// </summary>
public class AegisButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Protection;

    public override string Name => "Sentinel Ward";

    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<AegisOptions>.Instance.AegisCooldown);

    public override int MaxUses => (int)OptionGroupSingleton<AegisOptions>.Instance.MaxCharges;

    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

    public override float EffectDuration => 0f;

    public override LoadableAsset<Sprite> Sprite => NewModAsset.Shield;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Aegis;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !ShieldArea._active.Exists(area => area.ownerId == PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;

        AegisUtilities.RpcRequestWard(player);
    }
}
