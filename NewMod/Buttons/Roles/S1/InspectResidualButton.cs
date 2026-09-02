using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class InspectResidualButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    private const float InspectRange = 1.25f;

    public override string Name => "Inspect";
    public override float Cooldown => 0f;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.WardenInvestigate;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is WardenRole;
    }

    public override bool CanUse()
    {
        return base.CanUse() && WardenRole.GetNearestResidualMark(PlayerControl.LocalPlayer.GetTruePosition(), InspectRange);
    }

    protected override void OnClick()
    {
        var mark = WardenRole.GetNearestResidualMark(PlayerControl.LocalPlayer.GetTruePosition(), InspectRange);

        if (mark)
            WardenRole.InspectResidual(mark);
    }
}