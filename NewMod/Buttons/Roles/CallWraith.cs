using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using Wraith = NewMod.Roles.NeutralRoles.WraithCaller;
using UnityEngine;
using NewMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Keybinds;
using Reactor.Utilities;

namespace NewMod.Buttons.Roles;

public class CallWraithButton : CustomActionButton, IEnergyAbility
{
    public static CustomPlayerMenu ActiveMenu;

    public EnergyCategory Category => EnergyCategory.Control;
    public bool CaptureOnClick => false;
    public override string Name => "Call Wraith";
    public override float Cooldown => OptionGroupSingleton<WraithCallerOptions>.Instance.CallWraithCooldown;

    public override bool CanUse()
    {
        var ownerId = PlayerControl.LocalPlayer.PlayerId;
        return base.CanUse() && WraithCallerUtilities.Traces.Contains(ownerId) && !WraithCallerUtilities.ActiveNpcs.Values.Any(npc => npc && npc.isActive && npc.Owner.PlayerId == ownerId);
    }

    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override float EffectDuration => 0f;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.CallWraith;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Wraith;
    }

    public override void ClickHandler()
    {
        if (CanClick())
            OnClick();
    }

    protected override void OnClick()
    {
        CloseActiveMenu();

        var menu = CustomPlayerMenu.Create();
        ActiveMenu = menu;
        var allowedPlayers = new HashSet<byte>();

        foreach (var info in GameData.Instance.AllPlayers)
        {
            if (info.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
            if (info.IsDead || info.Disconnected) continue;

            allowedPlayers.Add(info.PlayerId);
        }

        menu.Begin(player => allowedPlayers.Contains(player.PlayerId) && !player.notRealPlayer, player =>
        {
            if (!player)
            {
                CloseActiveMenu();
                return;
            }

            CloseActiveMenu();

            ResetCooldownAndOrEffect();

            WraithCallerUtilities.RequestSummonNPC(PlayerControl.LocalPlayer, player);
            EnergyThief.ReportAbilityUse(Category);
        });

        foreach (var panel in menu.potentialVictims)
        {
            var icon = panel.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(renderer => renderer.name == "ShapeshifterIcon");

            if (icon)
                icon.sprite = NewModAsset.WraithIcon.LoadAsset();
        }
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        CloseActiveMenu();
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        CloseActiveMenu();
    }

    public static void CloseActiveMenu()
    {
        var menu = ActiveMenu;
        ActiveMenu = null;

        if (menu)
            menu.ForceClose();
    }
}