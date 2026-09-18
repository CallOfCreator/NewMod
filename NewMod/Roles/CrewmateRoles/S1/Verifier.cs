using NewMod.Utilities;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using NewMod.Options.Roles.S1;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles.S1;

public class VerifierRole : CrewmateRole, INewModRole
{
    public string RoleName => "Verifier";
    public string RoleDescription => "Check whether a player did what they claim.";

    public string RoleLongDescription => "Once per meeting, choose a player and check something they did last round.\nYou can check tasks, venting, ability use, or whether they were near a body when it was reported.\nOnly you see the result.";

    public Color RoleColor => new Color32(88, 232, 190, 255);
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
            Icon = NewModAsset.VerifyIcon,
            MaxRoleCount = 1,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var radius = OptionGroupSingleton<VerifierOptions>.Instance.NearBodyRadius;
        return tabText.Append($"\n<size=65%>{RoleColor.ToTextColor()}One check per meeting.</color>\nChecks cover actions from the last round.\nBody proximity: within {radius:0.#} units at report time.\n<color=#FFCF70>Unknown</color> means no body proximity check was recorded.</size>");
    }
}