using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public sealed class TerminateButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Terminate";
    public override float Cooldown => 0f;
    public override int MaxUses => 1;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.TerminateButton;
    public override float Distance => OptionGroupSingleton<TerminatorOptions>.Instance.CounterAttackRange;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is not TerminatorRole;
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        base.SetActive(visible && TerminatorRole.FinalCountdownActive, role);
    }

    public override PlayerControl GetTarget()
    {
        var terminator = TerminatorRole.GetTerminator();
        return terminator && !terminator.Data.IsDead && !terminator.Data.Disconnected && Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), terminator.GetTruePosition()) <= Distance ? terminator : null;
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(Color.red));
    }

    protected override void OnClick()
    {
        TerminatorRole.RpcRequestTerminate(PlayerControl.LocalPlayer);
    }
}
