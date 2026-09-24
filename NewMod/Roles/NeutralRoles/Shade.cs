using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class Shade : ImpostorRole, INewModRole
{
    public static readonly Dictionary<byte, int> ShadeKills = new();
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.Shade");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.Shade.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.Shade.TabDescription");
    public Color RoleColor => new(0.45f, 0f, 0.8f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = true,
            TasksCountForProgress = false,
            MaxRoleCount = 1,
            Icon = NewModAsset.DeployZoneIcon
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);

        var zonesActive = ShadowZone.zones.Count;
        var playersInZones = Helpers.GetAlivePlayers().Count(p => ShadowZone.IsInsideAny(p.GetTruePosition()));

        tabText.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.Shade.Tab.ActiveShadowZones"), ColorUtility.ToHtmlStringRGBA(Color.cyan), zonesActive));
        tabText.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.Shade.Tab.PlayersInsideZones"), ColorUtility.ToHtmlStringRGBA(Color.gray), playersInZones));

        return tabText;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<ShadeGameOver>();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var killer = evt.Source;
        var victim = evt.Target;

        Utils.RecordOnKill(killer, victim);

        if (killer.Data.Role is not Shade)
            return;

        if (!victim.Data.IsDead || !ShadowZone.zones.Any(zone => zone && zone.shadeId == killer.PlayerId && zone.Contains(killer.GetTruePosition()) && zone.Contains(victim.GetTruePosition())))
            return;

        var id = killer.PlayerId;
        ShadeKills[id] = ShadeKills.GetValueOrDefault(id) + 1;

        if (killer.AmOwner)
        {
            var required = (int)OptionGroupSingleton<ShadeOptions>.Instance.RequiredKills;
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#8E44AD>Shadow Harvest</color>\nKills: {ShadeKills[id]}/{required}"));
        }
    }

    [RegisterEvent]
    public static void BeforeMurder(BeforeMurderEvent evt)
    {
        if (evt.Source.Data.Role is not Shade) return;
        if (!ShadowZone.zones.Any(zone => zone && zone.shadeId == evt.Source.PlayerId && zone.Contains(evt.Source.GetTruePosition()) && zone.Contains(evt.Target.GetTruePosition())))
            evt.Cancel();
    }

    [RegisterEvent]
    public static void OnShadeRoleAssigned(SetRoleEvent evt)
    {
        if (evt.Player.AmOwner && evt.Player.Data.Role is Shade)
        {
            var hud = HudManager.Instance;

            hud.KillButton.currentTarget = null;
            hud.KillButton.gameObject.SetActive(false);
        }
    }
}