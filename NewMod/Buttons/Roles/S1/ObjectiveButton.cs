using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class ObjectiveButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Aggression;
    public override string Name => "Terminate";
    public override float Cooldown => 0f;
    public override int MaxUses => 1;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.TerminateButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is TerminatorRole;
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || !TerminatorRole.ObjectiveSpawned || TerminatorRole.FinalCountdownActive)
            return false;

        var radius = OptionGroupSingleton<TerminatorOptions>.Instance.FinalObjectiveRadius;
        return Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), TerminatorRole.ObjectivePosition) <= radius;
    }

    protected override void OnClick()
    {
        TerminatorRole.RpcStartFinalCountdown(PlayerControl.LocalPlayer);
    }
}