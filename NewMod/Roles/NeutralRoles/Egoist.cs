using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Options.Roles.EgoistOptions;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles
{
    public class EgoistRole : CrewmateRole, ICustomRole
    {
        public string RoleName => "Egoist";
        public string RoleDescription => "Crave attention. Earn revenge.";

        public string RoleLongDescription => "You are the Egoist, a chaotic neutral entity.\n\n" + "Your goal is to be ejected — if you are, and enough players vote for you, they die and you win.";

        public Color RoleColor => new Color(0.8f, 0.3f, 0.6f, 1f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
        public RoleOptionsGroup RoleOptionsGroup => RoleOptionsGroup.Neutral;

        public CustomRoleConfiguration Configuration =>
            new(this)
            {
                AffectedByLightOnAirship = false,
                CanGetKilled = true,
                UseVanillaKillButton = false,
                CanUseVent = false,
                CanUseSabotage = false,
                TasksCountForProgress = false,
                ShowInFreeplay = true,
                HideSettings = false,
                MaxRoleCount = 1,
                OptionsScreenshot = null,
                Icon = null,
            };

        [RegisterEvent]
        public static void OnEjection(EjectionEvent evt)
        {
            if (!AmongUsClient.Instance.AmHost)
                return;

            var egoist = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player && player.Data != null && player.Data.Role is EgoistRole);
            if (!egoist)
                return;

            var ejected = evt.ExileController.initData?.networkedPlayer?.Object;
            if (ejected != egoist)
                return;

            int minVotes = OptionGroupSingleton<EgoistRoleOptions>.Instance.MinimumVotesToWin;

            var voters = PlayerControl.AllPlayerControls.ToArray().Where(p =>
            {
                var voteData = p.GetVoteData();
                return voteData != null && voteData.VotedFor(egoist.PlayerId);
            }).ToList();

            if (voters.Count >= minVotes)
            {
                foreach (var p in voters)
                {
                    egoist.RpcCustomMurder(p, didSucceed: true, resetKillTimer: false, createDeadBody: true, teleportMurderer: false, showKillAnim: false, playKillSound: true);
                }

                CustomGameOver.Trigger<EgoistGameOver>([egoist.Data]);
            }
        }

        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason == CustomGameOver.GameOverReason<EgoistGameOver>();
        }
    }
}