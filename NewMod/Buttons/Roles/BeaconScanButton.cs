using MiraAPI.Hud;
using MiraAPI.GameOptions;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Utilities;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Patches.Roles.Beacon;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class BeaconScanButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    public override string Name => "Scan";
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<BeaconOptions>.Instance.PulseCooldown);
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.RadarIcon;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Beacon;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Beacon.charges > 0 && !Utils.IsActive(SystemTypes.Comms);
    }

    protected override void OnClick()
    {
        Coroutines.Start(BeaconShowMapPatch.ShowSnapshot());
    }
}