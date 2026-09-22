using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Options.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class WraithCaller : ImpostorRole, INewModRole
{
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller.TabDescription");
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

        var kills = WraithCallerUtilities.GetKillsNPC(playerId);

        var required = (int)OptionGroupSingleton<WraithCallerOptions>.Instance.RequiredNPCsToSend;
        tab.AppendLine(MiraLocaleManager.Get(WraithCallerUtilities.Traces.Contains(playerId) ? "NewMod.Roles.NeutralRoles.WraithCaller.Tab.TraceReady" : "NewMod.Roles.NeutralRoles.WraithCaller.Tab.NeedTrace"));

        var cyan = ColorUtility.ToHtmlStringRGBA(Color.cyan);
        var yellow = ColorUtility.ToHtmlStringRGBA(Color.yellow);
        var green = ColorUtility.ToHtmlStringRGBA(Palette.AcceptedGreen);

        tab.AppendLine();

        tab.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller.Tab.Kills"), kills >= required ? green : cyan, kills, yellow, required));

        if (kills < required)
        {
            var left = required - kills;
            tab.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller.Tab.KillsRemaining"), yellow, left, left == 1 ? "" : "s"));
        }
        else
        {
            tab.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller.Tab.GoalReached"), green));
        }

        tab.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.WraithCaller.Tab.HuntDuration"), OptionGroupSingleton<WraithCallerOptions>.Instance.HuntDuration));

        return tab;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<WraithCallerGameOver>();
    }
}