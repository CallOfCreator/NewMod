using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.GameEnd;

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

    public static bool CaptureWinners<TRole>(NetworkedPlayerInfo[] winners, out byte[] winnerIds)
        where TRole : RoleBehaviour, ICustomRole
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
            if (player != null)
            {
                EndGameResult.CachedWinners.Add(new CachedPlayerData(player));
            }
        }

        return true;
    }

    public static void SetPresentation<TRole>(EndGameManager manager, string text)
        where TRole : RoleBehaviour, ICustomRole
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

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<EnergyThief>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<EnergyThief>(manager, "Energy Thief Wins!");
}

public class DoubleAgentGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<DoubleAgent>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<DoubleAgent>(manager, "Double Agent Wins!");
}

public class PranksterGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<Prankster>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<Prankster>(manager, "Prankster Wins!");
}

public class SpecialAgentGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<SpecialAgent>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<SpecialAgent>(manager, "Special Agent Victory");
}

public class OverloadGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<OverloadRole>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<OverloadRole>(manager, "Overload Wins!");
}

public class EgoistGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<EgoistRole>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<EgoistRole>(manager, "Egoist Wins!");
}

public class InjectorGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<InjectorRole>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<InjectorRole>(manager, "Injector Victory");
}

public class PulseBladeGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<PulseBlade>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<PulseBlade>(manager, "PulseBlade Victory");
}

public class TyrantGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<Tyrant>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<Tyrant>(manager, "Tyrant Victory");
}

public class WraithCallerGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<WraithCaller>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<WraithCaller>(manager, "NPC Invasion Completed\nWraith Caller Wins!");
}

public class ShadeGameOver : CustomGameOver
{
    private byte[] _winnerIds = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        NewModGameOver.CaptureWinners<Shade>(winners, out _winnerIds);

    public override bool BeforeEndGameSetup(EndGameManager manager) => NewModGameOver.SetWinners(_winnerIds);

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<Shade>(manager, "Darkness Consumes All");
}