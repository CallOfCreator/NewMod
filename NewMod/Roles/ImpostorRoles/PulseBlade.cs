using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Options.Roles;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class PulseBlade : ImpostorRole, INewModRole
{
    public string RoleName => "PulseBlade";
    public string RoleDescription => "Commit to a dash and catch someone in your path.";
    public string RoleLongDescription => "Charge briefly, then dash in a fixed direction.\nHit an opponent to kill them, then recover before moving again. Win with the impostors.";
    public Color RoleColor => new(1f, 0.25f, 0.25f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Apex;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            MaxRoleCount = 1,
            Icon = NewModAsset.StrikeIcon
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var options = OptionGroupSingleton<PulseBladeOptions>.Instance;
        tabText.AppendLine($"<size=65%>Charge: <color=#FF8080>{options.ChargeDuration:0.#}s</color> | Recovery: <color=#FF8080>{options.RecoveryDuration:0.#}s</color></size>");
        tabText.AppendLine("<size=65%>Your direction locks when you press Strike. Walls stop the dash. Misses still use the cooldown.</size>");
        return tabText;
    }
}