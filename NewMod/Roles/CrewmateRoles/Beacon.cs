using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Options.Roles.BeaconOptions;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles
{
    public class Beacon : CrewmateRole, INewModRole
    {
        public string RoleName => "Beacon";
        public string RoleDescription => "Scan. Locate. Coordinate.";
        public string RoleLongDescription => "Send out a map-wide pulse that briefly reveals the position of all players.";
        public Color RoleColor => new(0.494f, 0.341f, 0.761f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
        public NewModFaction Faction => NewModFaction.Sentinel;
        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = true,
            Icon = NewModAsset.RadarIcon
        };

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tab = INewModRole.GetRoleTabText(this);
            var opts = OptionGroupSingleton<BeaconOptions>.Instance;

            var pulseDur = opts.PulseDuration;
            var cd = opts.PulseCooldown;
            var taskPerCh = opts.TasksPerCharge;
            var maxCharges = opts.MaxCharges;

            int completedTasks = GetCompletedTasks();
            int chargesFromTasks = (int)(completedTasks / taskPerCh);

            tab.AppendLine($"<size=65%><color=#{ColorUtility.ToHtmlStringRGB(RoleColor)}>Recon Support</color></size>");
            tab.AppendLine();
            tab.AppendLine($"<size=65%>Charges: <b><color=#{ColorUtility.ToHtmlStringRGB(Color.cyan)}>{chargesFromTasks}</color></b> / {maxCharges}  (+1 per {taskPerCh} tasks)</size>");
            tab.AppendLine($"<size=65%>Pulse Duration: <color=#{ColorUtility.ToHtmlStringRGB(Color.cyan)}>{pulseDur:F0}s</color> • Cooldown: <color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{cd:F0}s</color></size>");
            tab.AppendLine();
            tab.AppendLine("<size=65%><color=#FFD54F>Tip:</color> Use pulses after lights or suspected kills to catch rotations.</size>");

            return tab;
        }
        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason is GameOverReason.CrewmatesByVote or GameOverReason.CrewmatesByTask;
        }
        public static int charges;
        public static int grantedFromTasks;
        public static int lastCompletedTasks;
        public static float cooldownUntil;
        public static float pulseUntil;

        [RegisterEvent]
        public static void OnRoundStart(RoundStartEvent evt)
        {
            if (PlayerControl.LocalPlayer.Data.Role is not Beacon) return;
        }
        [RegisterEvent]
        public static void OnTaskComplete(CompleteTaskEvent evt)
        {
            if (PlayerControl.LocalPlayer.Data.Role is not Beacon) return;
            UpdateChargesFromTasks();
        }
        public static void UpdateChargesFromTasks()
        {
            var settings = OptionGroupSingleton<BeaconOptions>.Instance;
            int completed = GetCompletedTasks();
            if (completed == lastCompletedTasks) return;

            lastCompletedTasks = completed;
            int per = (int)settings.TasksPerCharge;
            int earned = Mathf.Min(completed / per, (int)settings.MaxCharges);
            int delta = earned - grantedFromTasks;

            if (delta > 0)
            {
                charges = Mathf.Clamp(charges + delta, 0, (int)settings.MaxCharges);
                grantedFromTasks = earned;
                Helpers.CreateAndShowNotification(
                    $"+{delta} Beacon {(delta > 1 ? "charges" : "charge")} (tasks)",
                    new Color(0.75f, 0.65f, 1f), spr:NewModAsset.RadarIcon.LoadAsset());
            }
        }
        public static int GetCompletedTasks()
        {
            var lp = PlayerControl.LocalPlayer;
            int done = 0;
            foreach (var t in lp.myTasks)
                if (t && t.IsComplete) done++;
            return done;
        }
    }
}
