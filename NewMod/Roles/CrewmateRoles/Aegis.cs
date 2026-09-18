using System.Text;
using Il2CppInterop.Runtime.Attributes;
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
    public string RoleName => "Aegis";
    public string RoleDescription => "Protect nearby players with a shield.";
    public string RoleLongDescription => "Place a shield around you. It blocks the first kill attempt against anyone inside, then breaks.\nThe shield stays where you placed it until it breaks or runs out.";
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
        return INewModRole.GetRoleTabText(this).Append($"\n<size=65%>{RoleColor.ToTextColor()}Ward: {options.DurationSeconds:0.#}s | Radius: {options.Radius:0.#}</color>\nCooldown: {options.AegisCooldown:0.#}s | Uses: {options.MaxCharges:0}\nBlocks one kill attempt against anyone inside.\n<color=#FFCF70>The shield breaks after blocking a kill.</color> Other abilities still work.</size>");
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