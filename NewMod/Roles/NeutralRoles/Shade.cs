using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Options.Roles.ShadeOptions;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles
{
    public class Shade : ImpostorRole, INewModRole
    {
        public static readonly Dictionary<byte, int> ShadeKills = new();
        public string RoleName => "Shade";
        public string RoleDescription => "Lurk. Fade. Kill unseen.";
        public string RoleLongDescription => "Deploy a shadow field that grants invisibility and lethal power within its darkness.";
        public Color RoleColor => new(0.45f, 0f, 0.8f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
        public NewModFaction Faction => NewModFaction.Entropy;
        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
        };

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tabText = INewModRole.GetRoleTabText(this);

            int zonesActive = ShadowZone.zones.Count;
            int playersInZones = Helpers.GetAlivePlayers().Count(p => ShadowZone.IsInsideAny(p.GetTruePosition()));

            var mode = OptionGroupSingleton<ShadeOptions>.Instance.Behavior;

            tabText.AppendLine($"<size=65%><color=#{ColorUtility.ToHtmlStringRGBA(Color.magenta)}>Within the dark you are unseen.</color></size>");
            tabText.AppendLine("\n");
            tabText.AppendLine($"<size=65%>Active Shadow Zones: <color=#{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{zonesActive}</color></size>");
            tabText.AppendLine($"<size=65%>Players inside zones: <color=#{ColorUtility.ToHtmlStringRGBA(Color.gray)}>{playersInZones}</color></size>");

            string effectText = mode switch
            {
                ShadeOptions.ShadowMode.Invisible => "Enter a shadow zone to become invisible.",
                ShadeOptions.ShadowMode.KillEnabled => "Enter a shadow zone to gain the power to kill once.",
                ShadeOptions.ShadowMode.Both => "Enter a shadow zone to become invisible and gain the power to kill once.",
                _ => "Enter a shadow zone to embrace the darkness."
            };

            tabText.AppendLine($"\n<size=65%><color=#B39DDB>{effectText}</color></size>");

            return tabText;
        }
        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason == (GameOverReason)NewModEndReasons.ShadeWin;
        }

        [RegisterEvent]
        public static void OnAfterMurder(AfterMurderEvent evt)
        {
            var killer = evt.Source;
            var victim = evt.Target;

            Utils.RecordOnKill(killer, victim);

            if (killer.Data.Role is not Shade shadeRole)
                return;

            if (!ShadowZone.IsInsideAny(victim.GetTruePosition()))
                return;

            byte id = killer.PlayerId;
            ShadeKills[id] = ShadeKills.GetValueOrDefault(id) + 1;

            if (killer.AmOwner)
            {
                int required = (int)OptionGroupSingleton<ShadeOptions>.Instance.RequiredKills;
                Coroutines.Start(CoroutinesHelper.CoNotify(
                    $"<color=#8E44AD>Shadow Harvest</color>\nKills: {ShadeKills[id]}/{required}"
                ));
            }
        }
    }
}
