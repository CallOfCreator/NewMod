using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1
{
    [MiraIgnore]
    public class MirrorBladeRole : ImpostorRole, INewModRole
    {
        public string RoleName => "MirrorBlade";
        public string RoleDescription => "Reflect the strike meant for you.";
        public string RoleLongDescription => "Arm your mirror stance. The next murder targeting you during the reflect window is cancelled and reflected back into the attacker.";
        public Color RoleColor => new Color32(192, 220, 255, 255);
        public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
        public NewModFaction Faction => NewModFaction.Apex;

        public static readonly HashSet<byte> ArmedReflections = new();
        public static bool _reflecting;

        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = true,
            CanUseVent = true,
            UseVanillaKillButton = true,
            TasksCountForProgress = false,
            Icon = MiraAssets.Empty,
            OptionsScreenshot = MiraAssets.Empty,
            MaxRoleCount = 1,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tabText = INewModRole.GetRoleTabText(this);
            string state = ArmedReflections.Contains(PlayerControl.LocalPlayer.PlayerId) ? "armed" : "idle";

            tabText.AppendLine();
            tabText.AppendLine($"<size=65%>Reflect state: <color=#C0DCFF>{state}</color></size>");
            tabText.AppendLine("<size=65%><color=#B7D8FF>Use Reflect before danger hits. If someone strikes you, they suffer their own blade.</color></size>");

            return tabText;
        }

        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage;
        }

        [RegisterEvent]
        public static void OnBeforeMurder(BeforeMurderEvent evt)
        {
            if (_reflecting || !PlayerControl.LocalPlayer.IsHost())
                return;

            if (evt.Target.Data.Role is not MirrorBladeRole)
                return;

            if (!ArmedReflections.Remove(evt.Target.PlayerId))
                return;

            evt.Cancel();

            if (!evt.Source || evt.Source.Data.IsDead || evt.Source.Data.Disconnected)
                return;

            _reflecting = true;

            evt.Target.RpcCustomMurder(
                evt.Source,
                didSucceed: true,
                resetKillTimer: false,
                createDeadBody: true,
                teleportMurderer: false,
                showKillAnim: false,
                playKillSound: true
            );

            _reflecting = false;
        }

        [RegisterEvent]
        public static void OnRoundStart(RoundStartEvent evt)
        {
            if (evt.TriggeredByIntro)
                ArmedReflections.Clear();
        }

        [RegisterEvent]
        public static void OnGameEnd(GameEndEvent evt)
        {
            ArmedReflections.Clear();
        }

        [MethodRpc((uint)CustomRPC.MirrorBladeArm)]
        public static void RpcArmReflection(PlayerControl source)
        {
            ArmedReflections.Add(source.PlayerId);
            Coroutines.Start(CoDisarmReflection(source.PlayerId, OptionGroupSingleton<MirrorBladeOptions>.Instance.ReflectWindow));

            if (source.AmOwner)
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#C0DCFF>Mirror stance armed.</color>"));
        }

        static IEnumerator CoDisarmReflection(byte playerId, float delay)
        {
            yield return new WaitForSeconds(delay);
            ArmedReflections.Remove(playerId);
        }
    }
}