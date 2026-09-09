using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class SpecialistModeButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    public bool CaptureOnClick => false;
    public override string Name => "Scan Mode";
    public override float Cooldown => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Specialist;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Specialist.ScanStates.ContainsKey(PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        Specialist.CycleMode();
    }
}