using System.Collections.Generic;
using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class InjectButton : CustomActionButton<PlayerControl>, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Control;
    public override string Name => "Inject";
    public override float Cooldown => 0.25f;
    public override float Distance => OptionGroupSingleton<InjectorOptions>.Instance.InjectionRange;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.InjectButton;
    public override bool Enabled(RoleBehaviour role) => role is InjectorRole;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Button.graphic.SetCooldownNormalizedUvs();
    }

    public override PlayerControl GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (InjectorUtilities.Experiments.TryGetValue(player.PlayerId, out var sample))
        {
            var target = Utils.PlayerById(sample.TargetId);
            return target && !target.Data.IsDead && !target.Data.Disconnected && !target.inVent && Time.time >= sample.ReadyAt &&
                Vector2.Distance(player.GetTruePosition(), target.GetTruePosition()) <= Distance &&
                !PhysicsHelpers.AnythingBetween(player.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false) ? target : null;
        }
        if (Time.time < InjectorUtilities.NextInjection.GetValueOrDefault(player.PlayerId)) return null;
        return player.GetClosestPlayer(false, Distance, predicate: target => !target.inVent &&
            !InjectorUtilities.Samples.Contains((player.PlayerId, target.PlayerId)));
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(Palette.AcceptedGreen));
    }

    protected override void FixedUpdate(PlayerControl player)
    {
        OverrideName(InjectorUtilities.Experiments.ContainsKey(player.PlayerId) ? "Collect Sample" : Name);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (InjectorUtilities.Experiments.ContainsKey(player.PlayerId))
            InjectorUtilities.RpcCollectSample(player);
        else
            InjectorUtilities.RpcApplySerum(player, Target, InjectorUtilities.SelectedSerum);
    }
}
