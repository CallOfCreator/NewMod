using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using NewMod.Options.Roles.WraithCallerOptions;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles
{
    [MiraIgnore]
    public class WraithCaller : ImpostorRole, INewModRole
    {
        public string RoleName => "Wraith Caller";
        public string RoleDescription => "Summon. Lurk. Reap.";
        public string RoleLongDescription => "Summon spectral NPCs that slip through walls and hunt down your marked target.";
        public Color RoleColor => new(0.58f, 0.20f, 0.90f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
        public NewModFaction Faction => NewModFaction.Entropy;
        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = NewModAsset.WraithIcon
        };

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tab = INewModRole.GetRoleTabText(this);
            var playerId = PlayerControl.LocalPlayer.PlayerId;

            int sent = WraithCallerUtilities.GetSentNPC(playerId);
            int kills = WraithCallerUtilities.GetKillsNPC(playerId);

            int required = (int)OptionGroupSingleton<WraithCallerOptions>.Instance.RequiredNPCsToSend;
            bool showWarn = OptionGroupSingleton<WraithCallerOptions>.Instance.ShowSummonWarnings;

            string cyan = ColorUtility.ToHtmlStringRGBA(Color.cyan);
            string yellow = ColorUtility.ToHtmlStringRGBA(Color.yellow);
            string green = ColorUtility.ToHtmlStringRGBA(Palette.AcceptedGreen);

            tab.AppendLine();

            tab.AppendLine($"<size=70%>Sent: <b><color=#{cyan}>{sent}</color></b></size>");
            tab.AppendLine($"<size=70%>Kills: <b><color=#{(kills >= required ? green : cyan)}>{kills}</color></b>/<color=#{yellow}>{required}</color></size>");

            if (kills < required)
            {
                int left = required - kills;
                tab.AppendLine($"<size=65%><color=#{yellow}>{left} more successful kill{(left == 1 ? "" : "s")} to win.</color></size>");
            }
            else
            {
                tab.AppendLine($"<size=65%><b><color=#{green}>Win condition armed. Survive to claim victory.</color></size></b>");
            }

            if (showWarn)
            {
                tab.AppendLine();
                tab.AppendLine($"<size=60%><color=#{yellow}>Tip:</color> Time your summons. Meetings cancel hunts.</size>");
            }
            return tab;
        }
    }
}
