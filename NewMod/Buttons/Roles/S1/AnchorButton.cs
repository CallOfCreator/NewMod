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
public sealed class AnchorButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Mobility;
    private byte _roomId;

    public override string Name => "Anchor";
    public override float Cooldown => OptionGroupSingleton<NomadOptions>.Instance.AnchorCooldown;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.AnchorButton;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Nomad;
    }

    public override bool CanUse()
    {
        if (!base.CanUse())
            return false;

        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room.RoomId == SystemTypes.Hallway || !room.roomArea.OverlapPoint(PlayerControl.LocalPlayer.GetTruePosition()))
                continue;

            _roomId = (byte)room.RoomId;
            return true;
        }

        return false;
    }

    protected override void OnClick()
    {
        var position = PlayerControl.LocalPlayer.GetTruePosition();
        Nomad.RpcRequestAnchor(PlayerControl.LocalPlayer, _roomId, position.x, position.y);
    }
}