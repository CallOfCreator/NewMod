using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Utilities;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles;

public class TheVisionary : CrewmateRole, INewModRole
{
    public RoleOptionsGroup RoleOptionGroup { get; } = RoleOptionsGroup.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;
    public string RoleName => "The Visionary";
    public string RoleDescription => "Set up cameras and share photos in meetings.";
    public string RoleLongDescription => "Place a camera and aim it. It takes one photo after a short delay.\nReturn to collect it, then choose a photo to show everyone in a meeting.\nOther players can disable your camera before you collect it.";
    public Color RoleColor => new(0.75f, 0.5f, 1.0f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            DefaultRoleCount = 1,
            DefaultChance = 50,
            MaxRoleCount = 1,
            AffectedByLightOnAirship = true,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            TasksCountForProgress = true,
            Icon = MiraAssets.Empty,
            OptionsScreenshot = MiraAssets.Empty,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var options = OptionGroupSingleton<VisionaryOptions>.Instance;
        return INewModRole.GetRoleTabText(this).Append($"\n<size=65%>{RoleColor.ToTextColor()}Cameras: {options.MaxScreenshots:0} | Capture delay: {options.CaptureDelay:0.#}s</color>\nRelease to place. Right-click or Esc cancels.\nReturn to collect the photo. Other players can disable the camera.\n<color=#FFCF70>Comms ruins the shot.</color> Show one photo per meeting.</size>");
    }
}