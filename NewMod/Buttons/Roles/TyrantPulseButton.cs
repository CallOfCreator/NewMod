using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class TyrantPulseButton : CustomActionButton
{
    public override string Name => "Intimidate";
    public override float Cooldown => OptionGroupSingleton<TyrantOptions>.Instance.PulseCooldown;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.CrownIcon;
    public override bool Enabled(RoleBehaviour role) => role is Tyrant;
    public override bool CanUse() => base.CanUse() && ((Tyrant)PlayerControl.LocalPlayer.Data.Role).Kills >= 1;
    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        var position = player.GetTruePosition();
        Tyrant.RpcSpawnFearPulse(player, position.x, position.y);
    }
}
