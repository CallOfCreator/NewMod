using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using NewMod.Options.Roles.EdgeveilOptions;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles
{
    public class Edgeveil : ImpostorRole, INewModRole
    {
        public string RoleName => "Edgeveil";
        public string RoleDescription => "Draw. Cleave. Sheathe.";
        public string RoleLongDescription => "Perform a fast iaijutsu slash in a short cone. Anyone caught in the arc is killed.";
        public Color RoleColor => new(0.90f, 0.20f, 0.35f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
        public NewModFaction Faction => NewModFaction.Apex;
        public CustomRoleConfiguration Configuration => new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = NewModAsset.SlashIcon
        };
        public override bool DidWin(GameOverReason gameOverReason)
        {
            return gameOverReason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage;
        }
    }
}
