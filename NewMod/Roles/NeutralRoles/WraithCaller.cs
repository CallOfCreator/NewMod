using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Options.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class WraithCaller : ImpostorRole, INewModRole
{
    public string RoleName => "Wraith Caller";
    public string RoleDescription => "Send Wraiths through walls to hunt targets.";
    public string RoleLongDescription => "Summon spectral NPCs that slip through walls and hunt your target. Reach the required number of Wraith kills and stay alive to win.";
    public Color RoleColor => new(0.58f, 0.20f, 0.90f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            MaxRoleCount = 1,
            Icon = NewModAsset.WraithIcon
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tab = INewModRole.GetRoleTabText(this);
        var playerId = PlayerControl.LocalPlayer.PlayerId;

        var sent = WraithCallerUtilities.GetSentNPC(playerId);
        var kills = WraithCallerUtilities.GetKillsNPC(playerId);

        var required = (int)OptionGroupSingleton<WraithCallerOptions>.Instance.RequiredNPCsToSend;
        var showWarn = OptionGroupSingleton<WraithCallerOptions>.Instance.ShowSummonWarnings;

        var cyan = ColorUtility.ToHtmlStringRGBA(Color.cyan);
        var yellow = ColorUtility.ToHtmlStringRGBA(Color.yellow);
        var green = ColorUtility.ToHtmlStringRGBA(Palette.AcceptedGreen);

        tab.AppendLine();

        tab.AppendLine($"<size=70%>Sent: <b><color=#{cyan}>{sent}</color></b></size>");
        tab.AppendLine($"<size=70%>Kills: <b><color=#{(kills >= required ? green : cyan)}>{kills}</color></b>/<color=#{yellow}>{required}</color></size>");

        if (kills < required)
        {
            var left = required - kills;
            tab.AppendLine($"<size=65%><color=#{yellow}>{left} more successful kill{(left == 1 ? "" : "s")} to win.</color></size>");
        }
        else
        {
            tab.AppendLine($"<size=65%><b><color=#{green}>Kill goal reached. You must be alive when victory is checked.</color></b></size>");
        }

        if (showWarn)
        {
            tab.AppendLine();
            tab.AppendLine($"<size=60%><color=#{yellow}>Tip:</color> Time your summons. Meetings cancel hunts.</size>");
        }

        return tab;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<WraithCallerGameOver>();
    }
}