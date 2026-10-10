using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class SubmitResearchButton : CustomActionButton
{
    public override string Name => "Submit Research";
    public override float Cooldown => OptionGroupSingleton<InjectorOptions>.Instance.SubmissionDuration;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.InjectIcon;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is InjectorRole;
    }

    public override bool CanUse()
    {
        return base.CanUse() && PlayerControl.LocalPlayer.CanMove && !InjectorUtilities.Submitting.Contains(PlayerControl.LocalPlayer.PlayerId) && !InjectorUtilities.Submitted.Contains(PlayerControl.LocalPlayer.PlayerId) && InjectorUtilities.SampleCount(PlayerControl.LocalPlayer.PlayerId) >= OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount;
    }

    protected override void OnClick()
    {
        InjectorUtilities.RpcSubmit(PlayerControl.LocalPlayer);
    }
}
