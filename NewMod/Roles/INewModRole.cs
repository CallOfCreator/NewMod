using MiraAPI.Translation;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using NewMod.Utilities;

namespace NewMod.Roles;
#pragma warning disable CS0108
public interface INewModRole : ICustomRole
{
    /// <summary>
    ///     The faction associated with the current role.
    /// </summary>
    public NewModFaction Faction { get; }

    public static StringBuilder GetRoleTabText(ICustomRole role)
    {
        return new StringBuilder(string.Format(MiraLocaleManager.Get("NewMod.RoleTab.Header"), role.RoleColor.ToTextColor(), role.RoleName, Utils.GetFactionDisplay((INewModRole)role), role.RoleLongDescription));
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return GetRoleTabText(this);
    }
}