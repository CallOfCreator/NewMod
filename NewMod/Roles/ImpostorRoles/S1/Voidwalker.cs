using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1;

[MiraIgnore]
public class Voidwalker : ImpostorRole, INewModRole
{
    public string RoleName => "Voidwalker";
    public string RoleDescription => "Enter the void and kill once you leave it";
    public string RoleLongDescription => "As the Voidwalker, you can enter the void to become invisible and go through closed doors.\nWhile in the void, you can't kill and leave faint distortion trails that remain visible for only 1 second.\nAfter leaving the void, you have 4 seconds to perform a kill with a reduced cooldown.\nIf no kill is performed during those 4 seconds, the ability goes on its full cooldown.";

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            OptionsScreenshot = MiraAssets.Empty,
            Icon = NewModAsset.VoidwalkerIcon,
            CanGetKilled = true,
            UseVanillaKillButton = true,
            CanUseVent = true,
            TasksCountForProgress = false,
            CanUseSabotage = true,
        }; // maybe change later

    public Color RoleColor => new(0.3f, 0f, 0.5f, 1f); // change later
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Rift;
}