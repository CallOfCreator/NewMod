using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.GameModes.WraithSiegeGamemode;
using NewMod.Options.Roles.S1;
using NewMod.RoleLogic;
using NewMod.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewMod;

internal static class NewModGameOver
{
    public static bool CaptureWinnerData(NetworkedPlayerInfo[] winners, out CachedPlayerData[] cachedWinners)
    {
        cachedWinners = [];
        if (winners is not { Length: > 0 } || winners.Any(player => !player || !player.Role))
            return false;

        cachedWinners = winners.DistinctBy(player => player.PlayerId).Select(player => new CachedPlayerData(player)).ToArray();
        return true;
    }

    public static bool CaptureWinners<TRole>(NetworkedPlayerInfo[] winners, out CachedPlayerData[] cachedWinners) where TRole : RoleBehaviour, ICustomRole
    {
        cachedWinners = [];
        if (winners is not { Length: > 0 } || !winners[0] || winners[0].Role is not TRole)
            return false;

        return CaptureWinnerData(winners, out cachedWinners);
    }

    public static bool SetWinners(CachedPlayerData[] winners)
    {
        EndGameResult.CachedWinners.Clear();

        foreach (var winner in winners)
            EndGameResult.CachedWinners.Add(winner);

        return true;
    }

    public static void SetPresentation<TRole>(EndGameManager manager, string text) where TRole : RoleBehaviour, ICustomRole
    {
        var color = CustomRoleSingleton<TRole>.Instance.RoleColor;
        manager.WinText.text = text;
        manager.WinText.color = color;
        manager.BackgroundBar.material.SetColor(ShaderID.Color, color);
    }
}

public class EnergyThiefGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<EnergyThief>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<EnergyThief>(manager, "Energy Thief Wins!");
    }
}

public class DoubleAgentGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<DoubleAgent>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<DoubleAgent>(manager, "Double Agent Wins!");
    }
}

public class PranksterGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Prankster>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Prankster>(manager, "Prankster Wins!");
    }
}

public class SpecialAgentGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<SpecialAgent>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<SpecialAgent>(manager, "Special Agent Victory");
    }
}

public class OverloadGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<OverloadRole>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<OverloadRole>(manager, "Overload Wins!");
    }
}

public class EgoistGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<EgoistRole>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<EgoistRole>(manager, "Egoist Wins!");
    }
}

public class InjectorGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<InjectorRole>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<InjectorRole>(manager, "Injector Victory");
    }
}

public class PulseBladeGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<PulseBlade>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<PulseBlade>(manager, "PulseBlade Victory");
    }
}

public class TyrantGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Tyrant>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Tyrant>(manager, "Tyrant Victory");
    }
}

public class WraithCallerGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<WraithCaller>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<WraithCaller>(manager, "NPC Invasion Completed\nWraith Caller Wins!");
    }
}

public class ShadeGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Shade>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Shade>(manager, "Darkness Consumes All");
    }
}

public class TerminatorGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<TerminatorRole>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<TerminatorRole>(manager, "Terminator Victory");
    }
}

public class TerminatorDefeatedGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners.Length > 0 && winners.All(player => player.Role is not TerminatorRole) && NewModGameOver.CaptureWinnerData(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        var color = new Color32(117, 230, 165, 255);
        manager.WinText.text = "Terminator Destroyed";
        manager.WinText.color = color;
        manager.BackgroundBar.material.SetColor(ShaderID.Color, color);
    }
}

public class ArbitratorGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners.Length != 1)
            return false;

        ArbitratorRole.JudgmentTokens.TryGetValue(winners[0].PlayerId, out var tokens);

        if (tokens < OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin) return false;

        return NewModGameOver.CaptureWinnerData(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<ArbitratorRole>(manager, "Judgment Has Been Passed\nArbitrator Wins!");
    }
}

public class WraithSiegeWraithGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners.Length == 0)
            return false;

        _winners = winners.Select(player => new CachedPlayerData(player)).ToArray();
        return true;
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        EndGameResult.CachedWinners.Clear();

        foreach (var winner in _winners)
            EndGameResult.CachedWinners.Add(winner);

        return true;
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        manager.WinText.text = "WRAITHS WIN";
        manager.WinText.color = WraithSiege.WraithColor;
        manager.WinText.fontSize = 4f;
        manager.WinText.enableAutoSizing = false;

        manager.BackgroundBar.material.SetColor(ShaderID.Color, WraithSiege.WraithColor);

        var subtitle = Object.Instantiate(manager.WinText, manager.WinText.transform.parent);
        subtitle.name = "WraithSiegeSubtitle";
        subtitle.text = "BREACHED THE FLAG";
        subtitle.color = WraithSiege.WraithColor;
        subtitle.fontSize = 1.8f;
        subtitle.enableAutoSizing = false;
        subtitle.transform.localPosition = manager.WinText.transform.localPosition + new Vector3(00f, 1.8668f, -14f);
    }
}

public class WraithSiegeReviverGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners.Length == 0)
            return false;

        _winners = winners.Select(player => new CachedPlayerData(player)).ToArray();
        return true;
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        EndGameResult.CachedWinners.Clear();

        foreach (var winner in _winners)
            EndGameResult.CachedWinners.Add(winner);

        return true;
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        manager.WinText.text = "REVIVERS WIN";
        manager.WinText.color = WraithSiege.ReviverColor;
        manager.WinText.fontSize = 4f;
        manager.WinText.enableAutoSizing = false;

        manager.BackgroundBar.material.SetColor(ShaderID.Color, WraithSiege.ReviverColor);

        var subtitle = Object.Instantiate(manager.WinText, manager.WinText.transform.parent);
        subtitle.name = "WraithSiegeSubtitle";
        subtitle.text = "HELD THE LINE";
        subtitle.color = WraithSiege.ReviverColor;
        subtitle.fontSize = 1.8f;
        subtitle.enableAutoSizing = false;
        subtitle.transform.localPosition = manager.WinText.transform.localPosition + new Vector3(00f, 1.8668f, -14f);
    }
}

public class NomadGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Nomad>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Nomad>(manager, "The Nomad Escaped the Pattern");
    }
}

public class CollectorGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Collector>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Collector>(manager, "The Collection Is Complete");
    }
}

public class BountyGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Bounty>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Bounty>(manager, "The Contract Was Cashed Out");
    }
}

public class UsurperGameOver : CustomGameOver
{
    private CachedPlayerData[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Usurper>(winners, out _winners);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winners);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Usurper>(manager, "The Usurper Stole the Ending");
    }
}

public sealed record SummaryPlayer(byte Id, string Name, string Role, string RoleColor, string Faction, string Modifiers, int CompletedTasks, int TotalTasks, string Status);

[HarmonyPatch]
public static class MatchSummaryTracker
{
    private static readonly Dictionary<byte, SummaryPlayer> Players = [];
    private static bool _tracking;
    public static IReadOnlyList<SummaryPlayer> Snapshot { get; private set; } = [];

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            Players.Clear();
            Snapshot = [];
            _tracking = true;
        }

        if (_tracking && GameData.Instance)
            foreach (var player in GameData.Instance.AllPlayers)
                Capture(player);
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent evt)
    {
        Capture(evt.Player?.Data);
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        Capture(evt.Player.Data);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void BeforeDisconnect([HarmonyArgument(0)] PlayerControl player)
    {
        if (player)
            Capture(player.Data, true);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void BeforeEndGame()
    {
        if (!_tracking)
            return;
        
        foreach (var player in GameData.Instance.AllPlayers)
            Capture(player);
        Snapshot = Players.Values.OrderBy(player => player.Id).ToArray();
        _tracking = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
    public static void OnDisconnected()
    {
        _tracking = false;
    }

    private static void Capture(NetworkedPlayerInfo player, bool disconnected = false)
    {
        if (!_tracking || !player)
            return;

        var character = player.Object;
        if (character && character.notRealPlayer)
            return;

        var previous = Players.GetValueOrDefault(player.PlayerId);
        if (previous?.Status == "Left")
            return;
        var role = player.Role;
        var roleType = role ? role.Role : RoleTypes.Crewmate;
        var roleName = roleType.ToString();
        var roleColor = role && role.IsImpostor ? "FF4D4D" : "58E8BE";
        var faction = string.Empty;
        if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var custom))
        {
            roleName = custom.RoleName;
            roleColor = ColorUtility.ToHtmlStringRGB(custom.RoleColor);
            if (custom is INewModRole newModRole)
                faction = newModRole.Faction.ToString();
        }

        if (previous != null)
        {
            var summaryRole = MatchSummaryRole.Select(new MatchSummaryRole(roleName, roleColor, faction), new MatchSummaryRole(previous.Role, previous.RoleColor, previous.Faction), player.IsDead);
            roleName = summaryRole.Name;
            roleColor = summaryRole.Color;
            faction = summaryRole.Faction;
        }

        var inSiege = CustomGameModeManager.ActiveMode is WraithSiege;
        if (CustomGameModeManager.ActiveMode is WraithSiege siege && character)
        {
            var wraith = siege.IsWraith(character);
            roleName = wraith ? "Wraith" : "Reviver";
            roleColor = wraith ? "9B6CFF" : "58E8BE";
            faction = "Wraith Siege";
        }

        var modifiers = previous?.Modifiers ?? string.Empty;
        if (character && !player.IsDead)
        {
            var component = character.GetComponent<ModifierComponent>();
            var names = new List<string>();
            if (component)
                foreach (var modifier in component.ActiveModifiers)
                    if (!modifier.HideOnUi)
                        names.Add(modifier.ModifierName);
            modifiers = string.Join(", ", names);
        }

        var total = 0;
        var completed = 0;
        if (!inSiege && player.Tasks != null)
            foreach (var task in player.Tasks)
            {
                if (task == null) continue;
                total++;
                if (task.Complete) completed++;
            }

        var status = disconnected || player.Disconnected ? "Left" : player.IsDead ? "Dead" : "Alive";
        Players[player.PlayerId] = new SummaryPlayer(player.PlayerId, CleanText(player.PlayerName), CleanText(roleName), roleColor, CleanText(faction), CleanText(modifiers), completed, total, status);
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    [HarmonyPostfix]
    public static void ShowSummary(EndGameManager __instance)
    {
        if (Snapshot.Count == 0)
            return;

        var summary = Object.Instantiate(__instance.WinText, __instance.transform);
        summary.gameObject.name = "NewModMatchSummary";
        var translator = summary.GetComponent<TextTranslatorTMP>();
        if (translator)
        {
            Object.Destroy(translator);
        }

        var aspect = summary.GetComponent<AspectPosition>();
        if (aspect) aspect.enabled = false;

        var camera = Camera.main;
        var position = AspectPosition.ComputeWorldPosition(camera, AspectPosition.EdgeAlignments.LeftTop, new Vector3(0.2f, 0.2f, 0f));
        position.z = __instance.WinText.transform.position.z;
        summary.transform.position = position;
        summary.transform.localScale = Vector3.one;
        summary.transform.localRotation = Quaternion.identity;
        summary.rectTransform.pivot = new Vector2(0f, 1f);
        summary.rectTransform.sizeDelta = new Vector2(camera.orthographicSize * Mathf.Min(camera.aspect, Screen.safeArea.width / Screen.safeArea.height) * 0.9f - 0.4f, camera.orthographicSize * 2f - 1.6f);
        summary.alignment = TextAlignmentOptions.TopLeft;
        summary.color = Color.white;
        summary.fontStyle = FontStyles.Normal;
        summary.enableVertexGradient = false;
        summary.fontSizeMin = 0.6f;
        summary.fontSizeMax = 1.3f;
        summary.fontSize = 1.3f;
        summary.enableWordWrapping = false;

        var text = new StringBuilder("End game summary:\n");
        foreach (var player in Snapshot)
        {
            text.Append($"{player.Name} - <color=#{player.RoleColor}>{player.Role}</color>");
            if (player.Modifiers.Length > 0)
                text.Append($" ({player.Modifiers})");
            if (player.TotalTasks > 0)
                text.Append($" | Tasks: {player.CompletedTasks}/{player.TotalTasks}");
            text.AppendLine($" | {player.Status}");
        }

        summary.text = text.ToString().TrimEnd();
        summary.gameObject.SetActive(true);
    }

    public static string CleanText(string text)
    {
        return (text ?? string.Empty).Replace('<', '(').Replace('>', ')').Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Trim();
    }
}