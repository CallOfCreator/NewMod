using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using NewMod.Utilities;

namespace NewMod.Roles
{
    public interface INewModRole : ICustomRole
    {
        public static StringBuilder GetRoleTabText(ICustomRole role)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{role.RoleColor.ToTextColor()}You are <b>{role.RoleName}</b></color>");
            sb.AppendLine($"<size=65%>Faction: {Utils.GetFactionDisplay()}</size>");
            sb.AppendLine($"<size=70%>{role.RoleLongDescription}</size>");
            return sb;
        }

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            return GetRoleTabText(this);
        }
    }
}