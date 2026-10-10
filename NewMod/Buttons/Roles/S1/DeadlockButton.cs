using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using DeadwireRole = NewMod.Roles.ImpostorRoles.S1.Deadwire;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class DeadlockButton : CustomActionButton<PlayerControl>, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override float Distance => OptionGroupSingleton<DeadwireOptions>.Instance.DeadlockRange;

    public override string Name => "Deadlock";
    public override float InitialCooldown => 0f;
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<DeadwireOptions>.Instance.DeadlockCooldown);
    public override int MaxUses => (int)OptionGroupSingleton<DeadwireOptions>.Instance.DeadlockUses;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadlockButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is DeadwireRole;
    }

    public override PlayerControl GetTarget()
    {
        return DeadwireRole.Records.ContainsKey(PlayerControl.LocalPlayer.PlayerId) ? null : PlayerControl.LocalPlayer.GetClosestPlayer(false, Distance, predicate: player => !player.inVent);
    }

    public override void SetOutline(bool active)
    {
        if (Target)
            Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.red));
    }

    protected override void OnClick()
    {
        DeadwireRole.RpcRequestDeadlock(PlayerControl.LocalPlayer, Target);
    }
}
