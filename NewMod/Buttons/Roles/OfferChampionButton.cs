using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.ImpostorRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class OfferChampionButton : CustomActionButton
{
    public static CustomPlayerMenu ActiveMenu;
    public override string Name => "Offer Alliance";
    public override float Cooldown => 0f;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.CrownIcon;
    public override bool Enabled(RoleBehaviour role) => role is Tyrant;
    public override bool CanUse() => base.CanUse() && !ActiveMenu && Tyrant.ApexThroneReady && Tyrant.ChampionId == byte.MaxValue;
    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        if (ActiveMenu) ActiveMenu.ForceClose();
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        if (ActiveMenu) ActiveMenu.ForceClose();
    }

    protected override void OnClick()
    {
        var menu = CustomPlayerMenu.Create();
        ActiveMenu = menu;
        menu.Begin(player => player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.Data.Disconnected,
            player =>
            {
                if (player) Tyrant.RpcNotifyChampion(PlayerControl.LocalPlayer, player);
                menu.ForceClose();
            });
    }
}
