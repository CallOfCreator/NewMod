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

/// <summary>
/// Defines the Call Wraith ability button for the Wraith Caller role.
/// </summary>
public class CallWraithButton : CustomActionButton, IEnergyAbility
{
    private static CustomPlayerMenu _activeMenu;

    public EnergyCategory Category => EnergyCategory.Control;
    public bool CaptureOnClick => false;

    /// <summary>
    /// The name displayed on the button.
    /// </summary>
    public override string Name => "Call Wraith";

    /// <summary>
    /// The cooldown time for the Call Wraith ability, as set in <see cref="WraithCallerOptions"/>.
    /// </summary>
    public override float Cooldown => OptionGroupSingleton<WraithCallerOptions>.Instance.CallWraithCooldown;

    /// <summary>
    /// The maximum uses for the Call Wraith ability, as set in <see cref="WraithCallerOptions"/>.
    /// </summary>
    public override int MaxUses => (int)OptionGroupSingleton<WraithCallerOptions>.Instance.CallWraithMaxUses;

    /// <summary>
    /// Location on the screen for the Call Wraith button.
    /// </summary>
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    /// <summary>
    /// Default keybind for the Call Wraith ability.
    /// </summary>
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

    /// <summary>
    /// The duration of any effect triggered by this ability.
    /// </summary>
    public override float EffectDuration => 0f;

    /// <summary>
    /// The icon for the Call Wraith button.
    /// </summary>
    public override LoadableAsset<Sprite> Sprite => NewModAsset.CallWraith;

    /// <summary>
    /// Enables the button for the Wraith Caller role only.
    /// </summary>
    /// <param name="role">Current player's role</param>
    /// <returns>True if role is Wraith Caller, otherwise false</returns>
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
        _activeMenu = menu;
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

            DecreaseUses();
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

    private static void CloseActiveMenu()
    {
        var menu = _activeMenu;
        _activeMenu = null;

        if (menu)
            menu.ForceClose();
    }
}