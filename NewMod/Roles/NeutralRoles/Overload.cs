using System.Collections;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using NewMod.Buttons.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

[MiraIgnore]
public class OverloadRole : ImpostorRole, ICustomRole
{
    public static int AbsorbedAbilityCount;
    public static PlayerControl chosenPrey;
    public static List<CustomActionButton> CachedButtons = new();
    public string RoleName => "Overload";
    public string RoleDescription => "Hunt chosen prey to absorb their power.";
    public string RoleLongDescription => "Choose prey and kill them yourself to gain a charge.\nReach the required charge and use OVERLOAD to win.";
    public Color RoleColor => new(0.6f, 0.1f, 0.3f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = false,
            CanGetKilled = true,
            UseVanillaKillButton = true,
            CanUseVent = true,
            CanUseSabotage = false,
            TasksCountForProgress = false,
            ShowInFreeplay = true,
            HideSettings = false,
            MaxRoleCount = 1,
            OptionsScreenshot = null,
            Icon = null
        };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<OverloadGameOver>();
    }

    public static void ResetState()
    {
        AbsorbedAbilityCount = 0;
        chosenPrey = null;
        CachedButtons.Clear();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not OverloadRole) return;

        if (evt.TriggeredByIntro) Coroutines.Start(CoShowMenu(1f));
    }

    public static IEnumerator CoShowMenu(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PlayerControl.LocalPlayer.AmOwner && PlayerControl.LocalPlayer.Data.Role is OverloadRole && chosenPrey == null)
        {
            var menu = CustomPlayerMenu.Create();

            menu.Begin(player => !player.Data.IsDead && !player.Data.Disconnected && player.PlayerId != PlayerControl.LocalPlayer.PlayerId, prey =>
            {
                chosenPrey = prey;
                menu.Close();
                Coroutines.Start(CoroutinesHelper.CoNotify($"<color=yellow>Chosen prey: {prey?.Data.PlayerName}</color>"));
            });
        }

        yield return null;
    }
}