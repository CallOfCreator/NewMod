using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1;

[MiraIgnore]
public class Voidwalker : ImpostorRole, INewModRole
{
    public string RoleName => "Voidwalker";
    public string RoleDescription => "Slip through closed doors and choose where to reappear.";
    public string RoleLongDescription => "Become invisible and pass through closed doors.\nYou cannot kill while phased. Once you return, you can attack as soon as your normal kill cooldown is ready.";

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
            MaxRoleCount = 1,
            RoleHintType = RoleHintType.RoleTab
        };

    public Color RoleColor => new(0.3f, 0f, 0.5f, 1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Rift;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var options = OptionGroupSingleton<VoidwalkerOptions>.Instance;

        tabText.AppendLine();
        tabText.AppendLine($"<size=65%>Void Duration: <color=#B388FF>{options.VoidTime:0.#}s</color>  •  Cooldown: <color=#9575CD>{options.EnterVoidCooldown:0.#}s</color></size>");

        tabText.AppendLine("<size=65%><color=#C8A2FF>While in the Void:</color> Invisible  •  Pass through doors  •  Cannot kill</size>");

        tabText.AppendLine("<size=65%>After returning: <color=#B388FF>No extra attack delay.</color> Your normal kill cooldown still applies.</size>");

        return tabText;
    }
}