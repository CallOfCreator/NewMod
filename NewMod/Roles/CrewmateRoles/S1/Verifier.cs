using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using NewMod.Options.Roles.S1;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles.S1;

[MiraIgnore]
public class VerifierRole : CrewmateRole, INewModRole
{
    public string RoleName => "Verifier";
    public string RoleDescription => "Check one recorded action each meeting.";

    public string RoleLongDescription => "Once per meeting, verify whether a player did a task, entered a vent, was near a body, or used an ability. Only you see the result: Confirmed, Denied, or Unknown.";

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
        var unknown = OptionGroupSingleton<VerifierOptions>.Instance.UnknownChance;
        var radius = OptionGroupSingleton<VerifierOptions>.Instance.NearBodyRadius;

        tabText.AppendLine();
        tabText.AppendLine("<size=65%>Meeting ability: <color=#58E8BE>once per meeting</color></size>");
        tabText.AppendLine($"<size=65%>Unknown chance: <color=#FFD166>{unknown}%</color></size>");
        tabText.AppendLine($"<size=65%>Near body radius: <color=#FFD166>{radius:F1}u</color></size>");
        tabText.AppendLine("<size=65%><color=#58E8BE>Use discussion to bait claims, then verify the claim type.</color></size>");

        return tabText;
    }
}