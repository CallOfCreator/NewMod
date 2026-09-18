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
        return new StringBuilder($"{role.RoleColor.ToTextColor()}You are <b>{role.RoleName}</b></color>\n<size=65%>Faction: {Utils.GetFactionDisplay((INewModRole)role)}</size>\n<size=70%>{role.RoleLongDescription}</size>\n");
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return GetRoleTabText(this);
    }
}