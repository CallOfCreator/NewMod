using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Buttons.Roles;
using NewMod.Modifiers.S1;
using NewMod.Patches.Roles.Visionary;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

[HarmonyPatch]
public class AbilityExchangeGE : IGeneralEvent
{
    public static bool Active { get; private set; }
    public static readonly Dictionary<byte, Type> BorrowedRoles = [];
    public static readonly Dictionary<CustomActionButton, (int Uses, float Timer)> ButtonStates = [];

    public string Title => "Ability Exchange";
    public string Description => "YOUR ABILITY HAS BEEN EXCHANGED!";
    public LoadableAsset<Sprite> Icon => NewModAsset.AbilityExchangeIcon;
    public Color AccentColor => new(0.55f, 0.24f, 1f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<Options.GEOptions>.Instance.AbilityExchangeFrequency.Value;
    public float Duration => 18f;

    public bool CanOccur()
    {
        return Helpers.GetAlivePlayers().Count(player => !player.HasModifier<InVoid>() && Utils.RoleToButtonsMap.ContainsKey(player.Data.Role.GetType())) >= 2;
    }

    public void OnEventStart()
    {
        Active = true;
        BorrowedRoles.Clear();
        ButtonStates.Clear();

        if (!AmongUsClient.Instance.AmHost)
            return;

        var players = Helpers.GetAlivePlayers().Where(player => !player.HasModifier<InVoid>() && Utils.RoleToButtonsMap.ContainsKey(player.Data.Role.GetType())).OrderBy(player => player.PlayerId).ToArray();

        for (var i = 0; i < players.Length; i++)
        {
            var donor = players[(i + 1) % players.Length];
            RpcAssignAbility(PlayerControl.LocalPlayer, players[i].PlayerId, donor.PlayerId);
        }
    }

    public void OnEventEnd()
    {
        Active = false;

        foreach (var (playerId, roleType) in BorrowedRoles)
        {
            var player = Utils.PlayerById(playerId);
            if (player && player.Data.Role.GetType() == roleType)
                continue;

            if (roleType == typeof(EnergyThief) && (!EnergyThief.BreachActive || EnergyThief.NodeOwnerId != playerId))
                EnergyThief.RemoveBorrowedState(playerId);
            if (roleType == typeof(Revenant) && Revenant.Phases.TryGetValue(playerId, out var phase) && phase is not (Revenant.Phase.Feigning or Revenant.Phase.DoomActive))
                Revenant.Phases.Remove(playerId);
        }

        BorrowedRoles.Clear();

        RestoreButtons();
        CallWraithButton.CloseActiveMenu();
        if (TerminatorRole.ObjectiveSpawned)
            TerminatorRole.CreateObjectiveArrow(TerminatorRole.ObjectivePosition);

        var localPlayer = PlayerControl.LocalPlayer;
        VisionaryMeetingEvents.UpdateMeetingButton();
        ArbitratorRole.UpdateMeetingButton();
        if (!localPlayer.HasModifier<InVoid>() && (!Revenant.Phases.TryGetValue(localPlayer.PlayerId, out var localPhase) || localPhase != Revenant.Phase.Feigning))
            HudManager.Instance.SetHudActive(localPlayer, localPlayer.Data.Role, !localPlayer.Data.IsDead && !MeetingHud.Instance && !ExileController.Instance);
    }

    public void OnHudUpdate()
    {
        var player = PlayerControl.LocalPlayer;
        if (player.Data.Role is Shade && !HasRoleAbility<Shade>(player))
            HudManager.Instance.KillButton.Hide();
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool KillButtonPrefix()
    {
        var player = PlayerControl.LocalPlayer;
        return player.Data.Role is not Shade || HasRoleAbility<Shade>(player);
    }

    public static void RestoreButtons()
    {
        foreach (var (button, state) in ButtonStates)
        {
            button.ResetCooldownAndOrEffect();
            button.SetUses(state.Uses);
            button.SetTimer(state.Timer);
        }
        ButtonStates.Clear();
    }

    public static bool HasRoleAbility<T>(PlayerControl player) where T : RoleBehaviour
    {
        return HasRoleAbility<T>(player.Data.Role, player.PlayerId);
    }

    public static bool HasRoleAbility<T>(RoleBehaviour role, byte playerId) where T : RoleBehaviour
    {
        return Active && BorrowedRoles.TryGetValue(playerId, out var borrowedRole) ? borrowedRole == typeof(T) : role is T;
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (!BorrowedRoles.Remove(evt.Player.PlayerId, out var roleType))
            return;

        if (roleType == typeof(EnergyThief) && evt.Player.Data.Role is not EnergyThief)
            EnergyThief.RemoveBorrowedState(evt.Player.PlayerId);

        if (evt.Player.AmOwner)
        {
            RestoreButtons();
            VisionaryMeetingEvents.UpdateMeetingButton();
            ArbitratorRole.UpdateMeetingButton();
        }
    }

    [RegisterEvent]
    public static void OnMiraButtonClick(MiraButtonClickEvent evt)
    {
        if (!Active || !BorrowedRoles.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var borrowedRole))
            return;

        var buttonType = evt.Button.GetType();
        if (Utils.RoleToButtonsMap.Values.Any(buttons => buttons.Contains(buttonType)) && !Utils.RoleToButtonsMap[borrowedRole].Contains(buttonType))
            evt.Cancel();
    }

    [MethodRpc((uint)CustomRPC.AbilityExchangeAssignment)]
    public static void RpcAssignAbility(PlayerControl source, byte playerId, byte donorId)
    {
        if (!source.IsHost() || !Active || BorrowedRoles.ContainsKey(playerId))
            return;

        var player = Utils.PlayerById(playerId);
        var donor = Utils.PlayerById(donorId);
        if (!player || !donor || player.Data.IsDead || player.Data.Disconnected || player.HasModifier<InVoid>())
            return;

        var roleType = donor.Data.Role.GetType();
        if (!Utils.RoleToButtonsMap.TryGetValue(roleType, out var buttons))
            return;

        BorrowedRoles[playerId] = roleType;
        if (roleType == typeof(EnergyThief) && player.Data.Role is not EnergyThief)
        {
            EnergyThief.Energy.TryGetValue(donorId, out var energy);
            EnergyThief.Energy[playerId] = energy;
            EnergyThief.Categories[playerId] = EnergyThief.Categories.TryGetValue(donorId, out var categories) ? [.. categories] : [];
            EnergyThief.HarvestedTargets[playerId] = EnergyThief.HarvestedTargets.TryGetValue(donorId, out var harvested) ? [.. harvested] : [];
        }
        if (roleType == typeof(Revenant))
            Revenant.Phases.TryAdd(playerId, Revenant.Phase.Ready);

        if (!player.AmOwner)
            return;

        CallWraithButton.CloseActiveMenu();
        if (roleType == typeof(Shade) && !player.Data.Role.CanUseKillButton)
            player.killTimer = 0f;
        if (roleType != player.Data.Role.GetType())
        {
            foreach (var button in CustomButtonManager.Buttons)
            {
                if (!buttons.Contains(button.GetType()))
                    continue;

                ButtonStates[button] = (button.UsesLeft, button.Timer);
                button.SetUses(button.MaxUses);
                button.SetTimer(0f);
            }
        }

        if (roleType == typeof(TerminatorRole) && TerminatorRole.ObjectiveSpawned)
            TerminatorRole.CreateObjectiveArrow(TerminatorRole.ObjectivePosition);
        else
            TerminatorRole.DestroyObjectiveArrow();
        VisionaryMeetingEvents.UpdateMeetingButton();
        ArbitratorRole.UpdateMeetingButton();
        HudManager.Instance.SetHudActive(player, player.Data.Role, !MeetingHud.Instance && !ExileController.Instance && (!Revenant.Phases.TryGetValue(playerId, out var phase) || phase != Revenant.Phase.Feigning));
    }
}
