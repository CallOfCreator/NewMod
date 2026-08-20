using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

[MiraIgnore]
public class WardenSealButton : CustomActionButton
{
    public override string Name => "Seal";
    public override float Cooldown => OptionGroupSingleton<WardenOptions>.Instance.SealCooldown;
    public override float EffectDuration => OptionGroupSingleton<WardenOptions>.Instance.SealDuration;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.WardenSeal;
    public override bool Enabled(RoleBehaviour role) => role is WardenRole;

    public override bool CanUse()
    {
        if (!base.CanUse() || WardenRole.SealActive)
            return false;

        var room = WardenRole.GetRoom(PlayerControl.LocalPlayer.GetTruePosition());

        return room && room.RoomId != SystemTypes.Hallway;
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        var room = WardenRole.GetRoom(player.GetTruePosition());

        if (!room || room.RoomId == SystemTypes.Hallway)
            return;

        WardenRole.RpcStartSeal(player, (byte)room.RoomId, OptionGroupSingleton<WardenOptions>.Instance.SealDuration);
    }
}