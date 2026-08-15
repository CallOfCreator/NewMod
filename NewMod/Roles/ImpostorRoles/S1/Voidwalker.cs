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
    public string RoleDescription => "Phase. Emerge. Strike.";
    public string RoleLongDescription => "Enter the Void to become invisible and pass through closed doors.\nYou cannot kill while phased. After returning, you have 4 seconds to strike with a reduced cooldown.";

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

        tabText.AppendLine("<size=65%><color=#E040FB>On Exit:</color> You have <color=#FFFFFF>4 seconds</color> to perform your empowered kill.</size>");

        return tabText;
    }
}