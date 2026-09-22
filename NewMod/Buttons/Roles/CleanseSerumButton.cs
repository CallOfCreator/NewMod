using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class CleanseSerumButton : CustomActionButton
{
    public override string Name => "Cleanse";
    public override float Cooldown => 0f;
    public override float EffectDuration => OptionGroupSingleton<InjectorOptions>.Instance.CleanseDuration;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.InjectIcon;
    public override bool Enabled(RoleBehaviour role) =>
        Button && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data &&
        !PlayerControl.LocalPlayer.Data.IsDead && InjectorUtilities.Experiments.Values.Any(sample => sample.TargetId == PlayerControl.LocalPlayer.PlayerId);
    protected override void OnClick() { }
    public override void OnEffectEnd()
    {
        if (!MeetingHud.Instance && !ExileController.Instance)
            InjectorUtilities.RpcCleanse(PlayerControl.LocalPlayer);
    }
}
