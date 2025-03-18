using System.Collections.Generic;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;
public class Revenant : ImpostorRole, ICustomRole
{
    public string RoleName => "Revenant";
    public string RoleDescription => "Cheat death—exactly once per match. Time it wisely.";
    public string RoleLongDescription => "As the Revenant, activate your ghostly form once per game to evade death for 10 seconds.\nIf a meeting is called during this time, your protection is lost permanently—time it wisely!";
    public Color RoleColor => new(0.3f, 0f, 0.5f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 2,
        OptionsScreenshot = MiraAssets.Empty,
        Icon = MiraAssets.Empty,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = true,
        TasksCountForProgress = false,
        CanUseSabotage = true,
        DefaultChance = 50,
        DefaultRoleCount = 1,
        CanModifyChance = true,
        GhostRole = AmongUs.GameOptions.RoleTypes.Crewmate, //Indeed
        RoleHintType = RoleHintType.RoleTab
    };
    public static Dictionary<byte, FeignDeathInfo> FeignDeathStates = new Dictionary<byte, FeignDeathInfo>();
    public static bool HasUsedFeignDeath = false;    
    public static Dictionary<byte, bool> StalkingStates = new Dictionary<byte, bool>();
    public class FeignDeathInfo
    {
        public float Timer;
        public DeadBody DeadBody;
        public bool Reported;
    }
}
