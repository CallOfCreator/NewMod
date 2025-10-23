using System.Collections;
using MiraAPI.Events;
using MiraAPI.Roles;
using Reactor.Utilities;
using UnityEngine;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using NewMod.Buttons.Roles;
using NewMod.Utilities;
using System.Collections.Generic;

namespace NewMod.Roles.NeutralRoles;

public class OverloadRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Overload";
    public string RoleDescription => "Absorb, Consume, Devour, Overload.";
    public string RoleLongDescription => "You are the Overload, an impostor who thrives on the abilities of the fallen. Each ejected player fuels your chaos, granting you their power";
    public Color RoleColor => new Color(0.6f, 0.1f, 0.3f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;
    public static int AbsorbedAbilityCount = 0;
    public static PlayerControl chosenPrey;
    public static List<CustomActionButton> CachedButtons = new();
    public CustomRoleConfiguration Configuration => new(this)
    {
        AffectedByLightOnAirship = false,
        CanGetKilled = true,
        UseVanillaKillButton = true,
        CanUseVent = true,
        CanUseSabotage = false,
        TasksCountForProgress = false,
        ShowInFreeplay = true,
        HideSettings = false,
        OptionsScreenshot = null,
        Icon = null,
    };
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not OverloadRole) return;

        if (evt.TriggeredByIntro)
        {
            CustomButtonSingleton<OverloadButton>.Instance.absorbed = null;
            AbsorbedAbilityCount = 0;
            chosenPrey = null;

            Coroutines.Start(CoShowMenu(1f));
        }
    }
    public static IEnumerator CoShowMenu(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PlayerControl.LocalPlayer.AmOwner && PlayerControl.LocalPlayer.Data.Role is OverloadRole && chosenPrey == null)
        {
            CustomPlayerMenu menu = CustomPlayerMenu.Create();

            menu.Begin(
                player => !player.Data.IsDead && !player.Data.Disconnected && player.PlayerId != PlayerControl.LocalPlayer.PlayerId,
                prey =>
                {
                    chosenPrey = prey;
                    menu.Close();
                    Coroutines.Start(CoroutinesHelper.CoNotify($"<color=yellow>Chosen prey: {prey?.Data.PlayerName}</color>"));
                });
        }
        yield return null;
    }
}
