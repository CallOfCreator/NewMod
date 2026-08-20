using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.GameModes.WraithSiegeGamemode;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod;

internal static class NewModGameOver
{
    public static bool CaptureWinnerIds(NetworkedPlayerInfo[] winners, out byte[] winnerIds)
    {
        if (winners.Length == 0)
        {
            winnerIds = [];
            return false;
        }

        winnerIds = winners.Select(player => player.PlayerId).Distinct().ToArray();
        return true;
    }

    public static bool CaptureWinners<TRole>(NetworkedPlayerInfo[] winners, out byte[] winnerIds) where TRole : RoleBehaviour, ICustomRole
    {
        if (winners.Length == 0 || winners[0].Role is not TRole)
        {
            winnerIds = [];
            return false;
        }

        return CaptureWinnerIds(winners, out winnerIds);
    }

    public static bool SetWinners(byte[] winnerIds)
    {
        EndGameResult.CachedWinners.Clear();

        foreach (var playerId in winnerIds)
        {
            var player = GameData.Instance.GetPlayerById(playerId);
            if (player != null) EndGameResult.CachedWinners.Add(new CachedPlayerData(player));
        }

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
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<EnergyThief>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<EnergyThief>(manager, "Energy Thief Wins!");
    }
}

public class DoubleAgentGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<DoubleAgent>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<DoubleAgent>(manager, "Double Agent Wins!");
    }
}

public class PranksterGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Prankster>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Prankster>(manager, "Prankster Wins!");
    }
}

public class SpecialAgentGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<SpecialAgent>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<SpecialAgent>(manager, "Special Agent Victory");
    }
}

public class OverloadGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<OverloadRole>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<OverloadRole>(manager, "Overload Wins!");
    }
}

public class EgoistGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<EgoistRole>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<EgoistRole>(manager, "Egoist Wins!");
    }
}

public class InjectorGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<InjectorRole>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<InjectorRole>(manager, "Injector Victory");
    }
}

public class PulseBladeGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<PulseBlade>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<PulseBlade>(manager, "PulseBlade Victory");
    }
}

public class TyrantGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Tyrant>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Tyrant>(manager, "Tyrant Victory");
    }
}

public class WraithCallerGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<WraithCaller>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<WraithCaller>(manager, "NPC Invasion Completed\nWraith Caller Wins!");
    }
}

public class ShadeGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return NewModGameOver.CaptureWinners<Shade>(winners, out _winnerIds);
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        return NewModGameOver.SetWinners(_winnerIds);
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<Shade>(manager, "Darkness Consumes All");
    }
}

public class TerminatorGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners is [{ Role: TerminatorRole }];
    }

    public override void AfterEndGameSetup(EndGameManager manager)
    {
        NewModGameOver.SetPresentation<TerminatorRole>(manager, "Terminator Victory");
    }
}

public sealed class ArbitratorGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners.Length != 1)
            return false;

        ArbitratorRole.JudgmentTokens.TryGetValue(winners[0].PlayerId, out var tokens);

        if (tokens < OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin)
        {
            return false;
        }

        _winnerIds = [winners[0].PlayerId];
        return true;
    }

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) => NewModGameOver.SetPresentation<ArbitratorRole>(manager, "Judgment Has Been Passed\nArbitrator Wins!");
}

public sealed class WraithSiegeWraithGameOver : CustomGameOver
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

public sealed class WraithSiegeReviverGameOver : CustomGameOver
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