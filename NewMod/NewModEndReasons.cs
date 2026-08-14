using MiraAPI.GameEnd;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.GameEnd;

internal static class NewModGameOver
{
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
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: EnergyThief }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<EnergyThief>(manager, "Energy Thief Wins!");
}

public class DoubleAgentGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: DoubleAgent }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<DoubleAgent>(manager, "Double Agent Wins!");
}

public class PranksterGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: Prankster }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<Prankster>(manager, "Prankster Wins!");
}

public class SpecialAgentGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: SpecialAgent }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<SpecialAgent>(manager, "Special Agent Victory");
}

public class OverloadGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: OverloadRole }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<OverloadRole>(manager, "Overload Wins!");
}

public class EgoistGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: EgoistRole }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<EgoistRole>(manager, "Egoist Wins!");
}

public class InjectorGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: InjectorRole }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<InjectorRole>(manager, "Injector Victory");
}

public class PulseBladeGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: PulseBlade }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<PulseBlade>(manager, "PulseBlade Victory");
}

public class TyrantGameOver : CustomGameOver
{
    private NetworkedPlayerInfo[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners.Length == 0 || winners[0].Role is not Tyrant)
            return false;

        _winners = winners;
        return true;
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        for (var i = 1; i < _winners.Length; i++)
            EndGameResult.CachedWinners.Add(new CachedPlayerData(_winners[i]));

        return true;
    }

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<Tyrant>(manager, "Tyrant Victory");
}

public class WraithCallerGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: WraithCaller }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<WraithCaller>(manager, "NPC Invasion Completed\nWraith Caller Wins!");
}

public class ShadeGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: Shade }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<Shade>(manager, "Darkness Consumes All");
}

public class TerminatorGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners) =>
        winners is [{ Role: TerminatorRole }];

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<TerminatorRole>(manager, "Terminator Victory");
}

public class RevivalRoyaleGameOver : CustomGameOver
{
    private NetworkedPlayerInfo[] _winners = [];

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners.Length == 0)
            return false;

        _winners = winners;
        return true;
    }

    public override bool BeforeEndGameSetup(EndGameManager manager)
    {
        EndGameResult.CachedWinners.Clear();

        foreach (var winner in _winners)
            EndGameResult.CachedWinners.Add(new CachedPlayerData(winner));

        return true;
    }

    public override void AfterEndGameSetup(EndGameManager manager) =>
        NewModGameOver.SetPresentation<NecromancerRole>(manager, "Revival Royale Winner!");
}