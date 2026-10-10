using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Translation;
using NewMod.Utilities;

namespace NewMod.Roles;
public interface INewModRole : ICustomRole
{
    /// <summary>
    /// Gets the role faction.
    /// </summary>
    public NewModFaction Faction { get; }

    public static StringBuilder GetRoleTabText(ICustomRole role)
    {
        return new StringBuilder(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.RoleTab.Header"), role.RoleColor.ToTextColor(), role.RoleName, Utils.GetFactionDisplay((INewModRole)role), role.RoleLongDescription));
    }

    [HideFromIl2Cpp]
    public new StringBuilder SetTabText()
    {
        return GetRoleTabText(this);
    }
}
