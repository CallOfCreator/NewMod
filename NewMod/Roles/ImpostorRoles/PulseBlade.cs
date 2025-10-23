using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Options.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles
{
    public class PulseBlade : ImpostorRole, INewModRole
    {
        public string RoleName => "PulseBlade";
        public string RoleDescription => "Dash. Strike. Clean.";
        public string RoleLongDescription => "Dash to eliminate a target with precision. Victim’s body disappears temporarily";
        public Color RoleColor => new(1f, 0.25f, 0.25f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
        public NewModFaction Faction => NewModFaction.Apex;
        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = NewModAsset.StrikeIcon
        };
        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tabText = INewModRole.GetRoleTabText(this);
            var strikes = Utils.GetStrikes(PlayerControl.LocalPlayer.PlayerId);
            int alive = Helpers.GetAlivePlayers().Count;
            int threshold = (int)OptionGroupSingleton<PulseBladeOptions>.Instance.PlayersThreshold;
            int req = (int)OptionGroupSingleton<PulseBladeOptions>.Instance.RequiredStrikes;

            tabText.AppendLine($"<size=65%><color=#{ColorUtility.ToHtmlStringRGBA(Color.yellow)}>Warning: If your target is far beyond strike range and you strike, you will lose one use.</color></size>");
            tabText.AppendLine("\n");
            tabText.AppendLine($"<size=65%>Win: <color=#{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{strikes}</color>/<color=#{ColorUtility.ToHtmlStringRGBA(Color.gray)}>{req}</color> strikes</size>");

            if (strikes >= req)
            {
                if (alive <= threshold)
                {
                    tabText.AppendLine($"<size=65%><color=#{ColorUtility.ToHtmlStringRGBA(Color.green)}>Condition met, players ≤ {threshold}. Victory will trigger.</color></size>");
                }
                else
                {
                    tabText.AppendLine($"<size=65%><color=#7CB342>Armed: stay alive until players ≤ {threshold} to win.</color></size>");
                }
            }
            else
            {
                int left = req - strikes;
                tabText.AppendLine($"<size=65%><color=#{ColorUtility.ToHtmlStringRGBA(Color.yellow)}>{left} more strike{(left == 1 ? "" : "s")} needed to arm your win.</color></size>");
            }

            string aliveHex = alive <= threshold ? ColorUtility.ToHtmlStringRGBA(Palette.AcceptedGreen) : ColorUtility.ToHtmlStringRGBA(Color.yellow);
            tabText.AppendLine($"<size=65%>Current Alive: <color=#{aliveHex}>{alive}</color>  •  Threshold: <color=#B39DDB>{threshold}</color></size>");

            return tabText;
        }
        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason == (GameOverReason)NewModEndReasons.PulseBladeWin;
        }
    }
}