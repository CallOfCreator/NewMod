using System;
using System.Reflection;
using System.Linq;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events;
using MiraAPI.Hud;
using MiraAPI.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.Events;

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
    public static void OnPlayerExiled(EjectionEvent evt)
    {
        ExileController exileController = @evt.ExileController;

        var exiled = exileController.initData.networkedPlayer;
        var exiledPlayer = Utils.PlayerById(exiled.PlayerId);
        var player = PlayerControl.LocalPlayer;
        
        if (exiledPlayer == null)
            return;

        if (exiledPlayer.Data.Role is not ICustomRole)
        {
            if (!player.AmOwner) return;

            if (player.Data.Role is OverloadRole overload)
            {
                if (exiledPlayer.Data.Role.Ability == null)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=orange>No ability to absorb from this player.</color>"));
                }
                if (AbsorbedAbilityCount >= 3)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Maximum abilities absorbed.</color>"));
                    return;
                }
                AbsorbedAbilityCount++;

                var abilityButton = HudManager.Instance.AbilityButton;
                var absorbedButton = Instantiate(abilityButton, abilityButton.transform.parent);
                absorbedButton.SetFromSettings(exiledPlayer.Data.Role.Ability);

                var pb = absorbedButton.GetComponent<PassiveButton>();
                pb.OnClick.RemoveAllListeners();
                pb.OnClick.AddListener((UnityAction)exiledPlayer.Data.Role.UseAbility);

                Coroutines.Start(CoroutinesHelper.CoNotify(
                $"<color=green>Ability absorbed from {exiledPlayer.Data.PlayerName}. Total absorbed: {OverloadRole.AbsorbedAbilityCount}</color>"));
            }
        }
        else
        {
            if (player.Data.Role is OverloadRole)
            {
                if (AbsorbedAbilityCount >= 3)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify("<color=red>Maximum abilities absorbed.</color>"));
                    return;
                }

                var customRole = (ICustomRole)exiledPlayer.Data.Role;
                var parentMod = customRole.ParentMod;
                var buttons = parentMod?.GetButtons();

                if (buttons.Count == 0)
                {
                    return;
                }
                var exiledButton = buttons.FirstOrDefault();
                var newButton = Activator.CreateInstance(exiledButton.GetType()) as CustomActionButton;

                newButton.CreateButton(HudManager.Instance.AbilityButton.transform.parent);
                newButton.OverrideName(exiledButton.Name);
                newButton.OverrideSprite(exiledButton.Sprite.LoadAsset());
                
                var passive = newButton.Button.GetComponent<PassiveButton>();
                passive.OnClick.RemoveAllListeners();
                passive.OnClick.AddListener((UnityAction)newButton.ClickHandler);
            }
        }
    }
}
