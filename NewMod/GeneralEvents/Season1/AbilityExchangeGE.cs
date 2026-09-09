using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
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
    public LoadableAsset<Sprite> Icon => NewModAsset.AbilityExchangeIcon;
    public Color AccentColor => new(0.55f, 0.24f, 1f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<Options.GEOptions>.Instance.AbilityExchangeFrequency.Value;
    public float Duration => 18f;

    public bool CanOccur()
    {
        return Helpers.GetAlivePlayers().Count(player => !player.HasModifier<InVoid>() && Utils.RoleToButtonsMap.TryGetValue(player.Data.Role.GetType(), out var buttons) && buttons.Count > 0) >= 2;
    }

    public void OnEventStart()
    {
        Active = true;
        BorrowedButtonTypeName = null;

        if (!PlayerControl.LocalPlayer.HasModifier<InVoid>())
            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);

        if (!AmongUsClient.Instance.AmHost)
            return;

        var players = Helpers.GetAlivePlayers().Where(player => !player.HasModifier<InVoid>() && Utils.RoleToButtonsMap.TryGetValue(player.Data.Role.GetType(), out var buttons) && buttons.Count > 0).OrderBy(player => player.PlayerId).ToArray();

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

        if (!PlayerControl.LocalPlayer.HasModifier<InVoid>())
            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);
    }

    public void Tick()
    {
        if (!Active || BorrowedButtonTypeName == null || PlayerControl.LocalPlayer.HasModifier<InVoid>())
            return;

        foreach (var button in CustomButtonManager.Buttons)
        {
            if (!IsRoleButton(button.GetType()))
                continue;

            var borrowed = button.GetType().FullName == BorrowedButtonTypeName;
            button.Button?.ToggleVisible(borrowed);

            if (borrowed)
                button.KeybindIcon?.SetActive(false);
        }
    }

    [RegisterEvent]
    public static void OnMiraButtonClick(MiraButtonClickEvent evt)
    {
        if (Active && BorrowedButtonTypeName != null && !PlayerControl.LocalPlayer.HasModifier<InVoid>() && IsRoleButton(evt.Button.GetType()) && evt.Button.GetType().FullName != BorrowedButtonTypeName)
            evt.Cancel();
    }

    private static bool IsRoleButton(System.Type buttonType)
    {
        foreach (var buttonTypes in Utils.RoleToButtonsMap.Values)
            if (buttonTypes.Contains(buttonType))
                return true;

        return false;
    }

    [MethodRpc((uint)CustomRPC.AbilityExchangeAssignment)]
    public static void RpcAssignAbility(PlayerControl source, byte playerId, string buttonTypeName)
    {
        if (PlayerControl.LocalPlayer.PlayerId != playerId || PlayerControl.LocalPlayer.HasModifier<InVoid>())
            return;

        BorrowedButtonTypeName = buttonTypeName;

        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !MeetingHud.Instance);
    }
}