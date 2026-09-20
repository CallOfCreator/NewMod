using System.Collections;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
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

public class DoubleAgent : CrewmateRole, INewModRole
{
    public static bool CounterfeitActive;
    public static float CooldownUntil;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.DoubleAgent");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.DoubleAgent.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.DoubleAgent.TabDescription");
    public NewModFaction Faction => NewModFaction.Sentinel;
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

    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    public System.Text.StringBuilder SetTabText()
    {
        var options = OptionGroupSingleton<DoubleAgentOptions>.Instance;
        return INewModRole.GetRoleTabText(this).Append(string.Format(MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.DoubleAgent.Tab.SabotageCommunicationsOnlyDurationS"), RoleColor.ToTextColor(), options.CounterfeitDuration, options.CounterfeitCooldown));
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;
        CounterfeitActive = false;
        CooldownUntil = 0f;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        CounterfeitActive = false;
    }

    public static void BeginCounterfeit(PlayerControl source, byte sabotageId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not DoubleAgent || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || CounterfeitActive || Time.time < CooldownUntil || (SystemTypes)sabotageId != SystemTypes.Comms)
            return;

        var sabotage = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
        if (sabotage.AnyActive)
            return;

        var duration = OptionGroupSingleton<DoubleAgentOptions>.Instance.CounterfeitDuration;
        RpcStartCounterfeit(PlayerControl.LocalPlayer, duration);
        ShipStatus.Instance.UpdateSystem(SystemTypes.Comms, source, 128);
        Coroutines.Start(CoFinishCounterfeit(duration));
    }

    [MethodRpc((uint)CustomRPC.DoubleAgentStartCounterfeit, LocalHandling = RpcLocalHandling.After)]
    public static void RpcStartCounterfeit(PlayerControl source, float duration)
    {
        if (!source.IsHost())
            return;

        CounterfeitActive = true;
        CooldownUntil = Time.time + OptionGroupSingleton<DoubleAgentOptions>.Instance.CounterfeitCooldown;

        if (PlayerControl.LocalPlayer.Data.Role is DoubleAgent)
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF4B4B>Counterfeit deployed.</color> It will collapse in {duration:0}s."));
    }

    public static IEnumerator CoFinishCounterfeit(float duration)
    {
        var ship = ShipStatus.Instance;
        var endsAt = Time.time + duration;
        while (ShipStatus.Instance == ship && CounterfeitActive && Time.time < endsAt && Utils.IsActive(SystemTypes.Comms))
            yield return null;

        if (ShipStatus.Instance == ship && CounterfeitActive)
            RpcFinishCounterfeit(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)CustomRPC.DoubleAgentFinishCounterfeit, LocalHandling = RpcLocalHandling.After)]
    public static void RpcFinishCounterfeit(PlayerControl source)
    {
        if (!source.IsHost())
            return;

        if (AmongUsClient.Instance.AmHost && CounterfeitActive)
        {
            if (ShipStatus.Instance.Type is ShipStatus.MapType.Hq or ShipStatus.MapType.Fungle)
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16);
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 17);
            }
            else
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 0);
            }
        }

        CounterfeitActive = false;
    }
}