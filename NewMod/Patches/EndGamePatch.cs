using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;

namespace NewMod.Patches;

public static class CustomEndGame
{
    internal static bool IsMatchReady { get; set; }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro) IsMatchReady = true;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        IsMatchReady = false;
    }

    public static bool TryEndGame()
    {
        if (!IsMatchReady || !AmongUsClient.Instance.AmHost || !GameManager.Instance.ShouldCheckForGameEnd)
            return false;

        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Where(player => !player.Data.IsDead && !player.Data.Disconnected).ToArray();

        var wraithRequired = (int)OptionGroupSingleton<WraithCallerOptions>.Instance.RequiredNPCsToSend;
        var wraithCaller = alivePlayers.FirstOrDefault(player => player.Data.Role is WraithCaller && WraithCallerUtilities.GetKillsNPC(player.PlayerId) >= wraithRequired);

        if (wraithCaller)
        {
            CustomGameOver.Trigger<WraithCallerGameOver>([wraithCaller.Data]);
            return true;
        }

        var shadeRequired = (int)OptionGroupSingleton<ShadeOptions>.Instance.RequiredKills;
        var shade = alivePlayers.FirstOrDefault(player =>
        {
            if (player.Data.Role is not Shade) return false;

            Shade.ShadeKills.TryGetValue(player.PlayerId, out var kills);
            return kills >= shadeRequired;
        });

        if (shade)
        {
            CustomGameOver.Trigger<ShadeGameOver>([shade.Data]);
            return true;
        }

        var pulseBladeOptions = OptionGroupSingleton<PulseBladeOptions>.Instance;

        if (alivePlayers.Length <= pulseBladeOptions.PlayersThreshold)
        {
            var pulseBlade = alivePlayers.FirstOrDefault(player => player.Data.Role is PulseBlade && Utils.GetStrikes(player.PlayerId) >= pulseBladeOptions.RequiredStrikes);

            if (pulseBlade)
            {
                CustomGameOver.Trigger<PulseBladeGameOver>([pulseBlade.Data]);
                return true;
            }
        }

        if (Tyrant.ApexThroneReady && Tyrant.ApexThroneOutcomeSet)
        {
            var tyrant = alivePlayers.FirstOrDefault(player => player.Data.Role is Tyrant);

            if (tyrant)
            {
                var winners = new List<NetworkedPlayerInfo> { tyrant.Data };

                if (Tyrant.Outcome == Tyrant.ThroneOutcome.ChampionSideWin)
                {
                    var champion = Utils.PlayerById(Tyrant.ChampionId);

                    if (champion && !champion.Data.Disconnected) winners.Add(champion.Data);
                }

                CustomGameOver.Trigger<TyrantGameOver>(winners);
                return true;
            }
        }

        var doubleAgent = alivePlayers.FirstOrDefault(player => player.Data.Role is DoubleAgent && player.AllTasksCompleted() && Utils.IsSabotage());

        if (doubleAgent)
        {
            CustomGameOver.Trigger<DoubleAgentGameOver>([doubleAgent.Data]);
            return true;
        }

        var specialAgentRequired = OptionGroupSingleton<SpecialAgentOptions>.Instance.RequiredMissionsToWin;
        var specialAgent = alivePlayers.FirstOrDefault(player => player.Data.Role is SpecialAgent && Utils.GetMissionSuccessCount(player.PlayerId) - Utils.GetMissionFailureCount(player.PlayerId) >= specialAgentRequired);

        if (specialAgent)
        {
            CustomGameOver.Trigger<SpecialAgentGameOver>([specialAgent.Data]);
            return true;
        }

        var prankster = alivePlayers.FirstOrDefault(player => player.Data.Role is Prankster && PranksterUtilities.GetReportCount(player.PlayerId) >= 2);

        if (prankster)
        {
            CustomGameOver.Trigger<PranksterGameOver>([prankster.Data]);
            return true;
        }

        var energyThiefRequired = (int)OptionGroupSingleton<EnergyThiefOptions>.Instance.RequiredDrainCount;
        var energyThief = alivePlayers.FirstOrDefault(player => player.Data.Role is EnergyThief && Utils.GetDrainCount(player.PlayerId) >= energyThiefRequired);

        if (energyThief)
        {
            CustomGameOver.Trigger<EnergyThiefGameOver>([energyThief.Data]);
            return true;
        }

        var injectorRequired = (int)OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount;
        var injector = alivePlayers.FirstOrDefault(player => player.Data.Role is InjectorRole && Utils.GetInjectedCount() >= injectorRequired);

        if (injector)
        {
            CustomGameOver.Trigger<InjectorGameOver>([injector.Data]);
            return true;
        }

        return false;
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
public static class GameStartPatch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        CustomEndGame.IsMatchReady = false;
        NewModEventHandler.ResetMatchState();
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CustomEndGameCheckPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (!CustomEndGame.IsMatchReady || !AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists || MeetingHud.Instance || ExileController.Instance || !GameManager.Instance.ShouldCheckForGameEnd)
            return;

        CustomEndGame.TryEndGame();
    }
}