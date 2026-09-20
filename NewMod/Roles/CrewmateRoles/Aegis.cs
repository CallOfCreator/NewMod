using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Components;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class Aegis : CrewmateRole, INewModRole
{
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Aegis");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Aegis.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Aegis.TabDescription");
    public Color RoleColor => new(0.227f, 0.651f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = true,
            MaxRoleCount = 1,
            Icon = NewModAsset.ShieldIcon
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var options = OptionGroupSingleton<AegisOptions>.Instance;
        return INewModRole.GetRoleTabText(this).Append(string.Format(MiraLocaleManager.Get("NewMod.Roles.CrewmateRoles.Aegis.Tab.Details"), RoleColor.ToTextColor(), options.DurationSeconds, options.Radius, options.AegisCooldown, options.MaxCharges));
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (evt.IsCancelled || MeetingHud.Instance || ExileController.Instance)
            return;

        foreach (var area in ShieldArea._active)
        {
            if (!area || !area.Contains(evt.Target.GetTruePosition()))
                continue;

            evt.Cancel();
            if (AmongUsClient.Instance.AmHost)
                AegisUtilities.RpcBreakWard(PlayerControl.LocalPlayer, area.ownerId);
            break;
        }
    }
}