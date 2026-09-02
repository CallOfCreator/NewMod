using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class DoubleAgent : CrewmateRole, ICustomRole
{
    public static bool CounterfeitActive;

    public string RoleName => "Double Agent";
    public string RoleDescription => "Create short counterfeit sabotages to bait Impostors.";
    public string RoleLongDescription => "Use the sabotage map to start a convincing counterfeit that repairs itself quickly. Complete your tasks and survive a real sabotage to win.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Crewmate;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            OptionsScreenshot = MiraAssets.Empty,
            Icon = MiraAssets.Empty,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = true,
            CanUseSabotage = true,
            DefaultChance = 50,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<DoubleAgentGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
            CounterfeitActive = false;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        CounterfeitActive = false;
    }

    public static void BeginCounterfeit(PlayerControl source, byte sabotageId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not DoubleAgent || source.Data.IsDead || CounterfeitActive)
            return;

        var sabotage = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
        if (sabotage.AnyActive)
            return;

        var duration = OptionGroupSingleton<DoubleAgentOptions>.Instance.CounterfeitDuration;
        RpcStartCounterfeit(PlayerControl.LocalPlayer, sabotageId, duration);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, sabotageId);
        Coroutines.Start(CoFinishCounterfeit(sabotageId, duration));
    }

    [MethodRpc((uint)CustomRPC.DoubleAgentStartCounterfeit, LocalHandling = RpcLocalHandling.After)]
    public static void RpcStartCounterfeit(PlayerControl host, byte sabotageId, float duration)
    {
        if (!host.IsHost())
            return;

        CounterfeitActive = true;

        if (PlayerControl.LocalPlayer.Data.Role is DoubleAgent)
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF4B4B>Counterfeit deployed.</color> It will collapse in {duration:0}s."));
    }

    private static IEnumerator CoFinishCounterfeit(byte sabotageId, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (CounterfeitActive)
            RpcFinishCounterfeit(PlayerControl.LocalPlayer, sabotageId);
    }

    [MethodRpc((uint)CustomRPC.DoubleAgentFinishCounterfeit, LocalHandling = RpcLocalHandling.After)]
    public static void RpcFinishCounterfeit(PlayerControl host, byte sabotageId)
    {
        if (!host.IsHost())
            return;

        var system = (SystemTypes)sabotageId;

        if (AmongUsClient.Instance.AmHost)
            switch (system)
            {
                case SystemTypes.Comms when ShipStatus.Instance.Type is ShipStatus.MapType.Hq or ShipStatus.MapType.Fungle:
                    ShipStatus.Instance.RpcUpdateSystem(system, 16);
                    ShipStatus.Instance.RpcUpdateSystem(system, 17);
                    break;
                case SystemTypes.Comms:
                    ShipStatus.Instance.RpcUpdateSystem(system, 0);
                    break;
                case SystemTypes.HeliSabotage:
                    ShipStatus.Instance.RpcUpdateSystem(system, 16);
                    ShipStatus.Instance.RpcUpdateSystem(system, 17);
                    break;
                case SystemTypes.Reactor:
                case SystemTypes.Laboratory:
                case SystemTypes.LifeSupp:
                    ShipStatus.Instance.RpcUpdateSystem(system, 16);
                    break;
            }

        if (system == SystemTypes.Electrical)
        {
            var lights = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            lights.ActualSwitches = lights.ExpectedSwitches;
        }
        else if (system == SystemTypes.MushroomMixupSabotage)
        {
            var mushroom = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();
            mushroom.currentSecondsUntilHeal = 0.1f;
        }

        CounterfeitActive = false;
    }
}
