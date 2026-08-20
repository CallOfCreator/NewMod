using System.Linq;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class AbilityExchangeGE : IGeneralEvent
{
    public static bool Active { get; private set; }
    public static string BorrowedButtonTypeName { get; private set; }

    public string Title => "Ability Exchange";
    public string Description => "YOUR ABILITY HAS BEEN EXCHANGED!";
    public LoadableAsset<Sprite> Icon => MiraAssets.Empty;
    public Color AccentColor => new(0.55f, 0.24f, 1f);
    public int OccurrenceChance => 8;
    public float Duration => 18f;

    public bool CanOccur()
    {
        return Helpers.GetAlivePlayers().Count(player => Utils.RoleToButtonsMap.TryGetValue(player.Data.Role.GetType(), out var buttons) && buttons.Count > 0) >= 2;
    }

    public void OnEventStart()
    {
        Active = true;
        BorrowedButtonTypeName = null;

        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);

        if (!AmongUsClient.Instance.AmHost)
            return;

        var players = Helpers.GetAlivePlayers().Where(player => Utils.RoleToButtonsMap.TryGetValue(player.Data.Role.GetType(), out var buttons) && buttons.Count > 0).OrderBy(player => player.PlayerId).ToArray();

        if (players.Length < 2)
            return;

        for (var i = 0; i < players.Length; i++)
        {
            var donor = players[(i + 1) % players.Length];
            var donorButtons = Utils.RoleToButtonsMap[donor.Data.Role.GetType()];
            var buttonType = donorButtons[Random.Range(0, donorButtons.Count)];

            RpcAssignAbility(PlayerControl.LocalPlayer, players[i].PlayerId, buttonType.FullName);
        }
    }

    public void OnEventEnd()
    {
        Active = false;
        BorrowedButtonTypeName = null;

        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);
    }

    [MethodRpc((uint)CustomRPC.AbilityExchangeAssignment)]
    public static void RpcAssignAbility(PlayerControl source, byte playerId, string buttonTypeName)
    {
        if (PlayerControl.LocalPlayer.PlayerId != playerId)
            return;

        BorrowedButtonTypeName = buttonTypeName;

        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);
    }
}

[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.SetActive))]
public static class AbilityExchangeButtonVisibilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CustomActionButton __instance, bool visible)
    {
        if (!AbilityExchangeGE.Active || !Utils.RoleToButtonsMap.Values.Any(buttons => buttons.Contains(__instance.GetType())))
        {
            return true;
        }

        __instance.Button?.ToggleVisible(visible && __instance.GetType().FullName == AbilityExchangeGE.BorrowedButtonTypeName);

        return false;
    }
}

[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.ClickHandler))]
public static class AbilityExchangeButtonClickPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CustomActionButton __instance)
    {
        if (!AbilityExchangeGE.Active || !Utils.RoleToButtonsMap.Values.Any(buttons => buttons.Contains(__instance.GetType())))
        {
            return true;
        }

        return __instance.GetType().FullName == AbilityExchangeGE.BorrowedButtonTypeName;
    }
}

[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.FixedUpdateHandler))]
public static class AbilityExchangeKeybindVisibilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(CustomActionButton __instance)
    {
        if (AbilityExchangeGE.Active && __instance.GetType().FullName == AbilityExchangeGE.BorrowedButtonTypeName)
        {
            __instance.KeybindIcon?.SetActive(false);
        }
    }
}