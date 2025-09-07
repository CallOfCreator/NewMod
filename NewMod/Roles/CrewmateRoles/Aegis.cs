using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using NewMod.Components;
using NewMod.Options.Roles.AegisOptions;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles
{
    public class Aegis : CrewmateRole, INewModRole
    {
        public string RoleName => "Aegis";
        public string RoleDescription => "Project. Protect. Punish.";
        public string RoleLongDescription => "Deploy a protective zone that reacts to hostile abilities.";
        public Color RoleColor => new(0.227f, 0.651f, 1f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
        public NewModFaction Faction => NewModFaction.Sentinel;
        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = true,
            Icon = NewModAsset.ShieldIcon
        };

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tab = INewModRole.GetRoleTabText(this);

            var opts = OptionGroupSingleton<AegisOptions>.Instance;
            var mode = opts.Behavior;
            var cd = opts.AegisCooldown;
            var dur = opts.DurationSeconds;
            var radius = opts.Radius;
            var uses = opts.MaxCharges;

            var cyan = ColorUtility.ToHtmlStringRGB(Color.cyan);
            var yellow = ColorUtility.ToHtmlStringRGB(Color.yellow);
            var green = ColorUtility.ToHtmlStringRGB(Palette.AcceptedGreen);
            var title = ColorUtility.ToHtmlStringRGB(RoleColor);

            tab.AppendLine($"<size=70%><color=#{title}>Defensive Support</color></size>");
            tab.AppendLine();

            tab.AppendLine($"<size=65%>Mode: <b><color=#{green}>{mode}</color></b></size>");
            tab.AppendLine($"<size=65%>Radius: <color=#{cyan}>{radius:F1}u</color> • Duration: <color=#{cyan}>{dur:F0}s</color></size>");
            tab.AppendLine($"<size=65%>Cooldown: <color=#{yellow}>{cd:F0}s</color> • Charges: <color=#{yellow}>{uses}</color></size>");
            tab.AppendLine();

            tab.AppendLine("<size=65%><color=#FFD54F>Tip:</color> Place wards on choke points or common kill paths.</size>");

            return tab;
        }

        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason is GameOverReason.CrewmatesByTask or GameOverReason.CrewmatesByVote;
        }

        [RegisterEvent]
        public static void OnAnyButtonClick(MiraButtonClickEvent evt)
        {
            var mode = ShieldArea.Mode;
            if (mode == AegisOptions.AegisMode.WarnOnly) return;

            var lp = PlayerControl.LocalPlayer;

            bool block = ShieldArea.IsInsideOthersWard(lp);

            if (!block && evt.Button is CustomActionButton<PlayerControl> tbtn && tbtn.Target)
            {
                var targetPos = tbtn.Target.GetTruePosition();
                block = ShieldArea.IsInsideOthersWardAt(targetPos, lp.PlayerId);
            }

            if (!block) return;

            evt.Cancel();
            NewMod.Instance.Log.LogError("Role Ability Canceled");

            Coroutines.Start(CoroutinesHelper.CoNotify(
              "<color=#3A9EFF>Aegis</color> blocks your ability here"));
        }
        [RegisterEvent]
        public static void OnBeforeMurder(BeforeMurderEvent evt)
        {
            if (ShieldArea.Mode == AegisOptions.AegisMode.WarnOnly) return;
            if (MeetingHud.Instance || ExileController.Instance) return;
            if (!ShieldArea.IsInsideOthersWard(evt.Target)) return;

            evt.Cancel();
            NewMod.Instance.Log.LogError("Role Ability Canceled Before Murder");

            if (evt.Source.AmOwner)
            {
                Coroutines.Start(CoroutinesHelper.CoNotify(
                    "<color=#3A9EFF>Aegis</color> blocks your kill here"));
            }
            foreach (var area in ShieldArea.AreasAt(evt.Target.GetTruePosition()))
            {
                var aegis = Utils.PlayerById(area.ownerId);
                if (aegis.AmOwner)
                {
                    Coroutines.Start(CoroutinesHelper.CoNotify(
                    $"<color=#3A9EFF>Aegis Ward Alert:</color> Kill attempt blocked inside your ward!"));
                }
            }
        }
        [RegisterEvent]
        public static void OnAfterMurder(AfterMurderEvent evt)
        {
            var pos = evt.DeadBody ? (Vector2)evt.DeadBody.transform.position : evt.Target.GetTruePosition();

            foreach (var area in ShieldArea.AreasAt(pos))
            {
                var owner = Utils.PlayerById(area.ownerId);
                if (!owner) continue;

                switch (ShieldArea.Mode)
                {
                    case AegisOptions.AegisMode.WarnOnly:
                        if (owner.AmOwner)
                            Coroutines.Start(CoroutinesHelper.CoNotify(
                                "<color=#3A9EFF>A kill happened in your ward</color>"));
                        break;

                    case AegisOptions.AegisMode.BlockAndReveal:
                        if (owner.AmOwner)
                        {
                            var killerName = evt.Source.Data.PlayerName;
                            Coroutines.Start(CoroutinesHelper.CoNotify(
                                $"<color=#3A9EFF>Aegis Ward Alert:</color> A player was killed inside your ward by <color=#FF4444><b>{killerName}</b></color>!"));
                        }
                        break;
                }
            }
        }
    }
}
