using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class SpecialistScanButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    public override string Name => "Scan";
    public override float Cooldown => 1f;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Specialist;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Specialist.ScanStates.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var state) && state.Charges > 0;
    }

    protected override void OnClick()
    {
        Specialist.Scan();
    }
}