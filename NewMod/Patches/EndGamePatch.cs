using UnityEngine;
using HarmonyLib;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using System.Linq;
using MiraAPI.Roles;
using AmongUs.GameOptions;
using Object = UnityEngine.Object;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static class SetEverythingUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            foreach (var playerObj in __instance.GetComponentsInChildren<PoolablePlayer>())
            {
                GameObject.Destroy(playerObj.gameObject);
            }

            var winningPlayers = EndGameResult.CachedWinners.ToArray().OrderByDescending(p => !p.IsYou).ToList();

            int num = winningPlayers.Count;

            for (int i = 0; i < num; i++)
            {
                var playerData = winningPlayers[i];

                int num2 = ((i % 2 == 0) ? (-1) : 1);
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (i == 0) ? -8f : -1f;

                PoolablePlayer poolablePlayer = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);

                float xPos = 1f * num2 * num3 * num5 * 0.9f;
                float yPos = FloatRange.SpreadToEdges(-1.125f, 0f, num3, num) * 0.9f;
                float zPos = (num6 + num3 * 0.01f) * 0.9f;

                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                poolablePlayer.transform.localScale = Vector3.one * num5;

                if (playerData.IsDead)
                {
                    poolablePlayer.SetBodyAsGhost();
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }
                 poolablePlayer.UpdateFromPlayerOutfit(
                    playerData.Outfit,
                    PlayerMaterial.MaskType.None,
                    playerData.IsDead,
                    true,
                    null, 
                    false
                );

                string roleName = GetRoleName(playerData, out Color roleColor);

                string playerNameWithRole = $"{playerData.PlayerName}\n{roleName}";

                var nameText = poolablePlayer.cosmetics.nameText;

                nameText.transform.localPosition = new Vector3(0f, -1.5f, -15f);

                nameText.text = playerNameWithRole;
                nameText.color = roleColor;
                nameText.alignment = TMPro.TextAlignmentOptions.Center;
            }

            string customWinText;
            Color customWinColor;

            switch (EndGameResult.CachedGameOverReason)
            {
                case (GameOverReason)NewModEndReasons.EnergyThiefWin:
                    customWinText = "Energy Thief Win!";
                    customWinColor = GetRoleColor(GetRoleType<EnergyThief>());
                    __instance.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;

                case (GameOverReason)NewModEndReasons.DoubleAgentWin:
                    customWinText = "Double Agent Win!";
                    customWinColor = GetRoleColor(GetRoleType<DoubleAgent>());
                    __instance.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;
                case (GameOverReason)NewModEndReasons.PranksterWin:
                    customWinText = "Prankster Win!";
                    customWinColor = GetRoleColor(GetRoleType<Prankster>());
                    __instance.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;
                default:
                    customWinText = string.Empty;
                    customWinColor = Color.white;
                    break;
            }

            if (!string.IsNullOrEmpty(customWinText))
            {
                var customWinTextObject = GameObject.Instantiate(__instance.WinText.gameObject, __instance.transform);
                customWinTextObject.transform.localPosition = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
                customWinTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                var customWinTextComponent = customWinTextObject.GetComponent<TMPro.TMP_Text>();
                customWinTextComponent.text = customWinText;
                customWinTextComponent.color = customWinColor;
                customWinTextComponent.fontSize = 4f;
            }
        } 

        public static string GetRoleName(CachedPlayerData playerData, out Color roleColor)
        {
            RoleTypes roleType = playerData.RoleWhenAlive;
            RoleBehaviour roleBehaviour = RoleManager.Instance.GetRole(roleType);

            if (roleBehaviour != null)
            {
                if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var customRole))
                {
                    roleColor = customRole.RoleColor;
                    return customRole.RoleName;
                }
                else
                {
                    roleColor = roleBehaviour.NameColor;
                    return roleBehaviour.NiceName;
                }
            }
            else
            {
                roleColor = Color.white;
                return null;
            }
        }

        public static RoleTypes GetRoleType<T>() where T : RoleBehaviour
        {
            ushort roleId = RoleId.Get<T>();
            return (RoleTypes)roleId;
        }

        public static Color GetRoleColor(RoleTypes roleType)
        {
            RoleBehaviour roleBehaviour = RoleManager.Instance.GetRole(roleType);

            if (roleBehaviour != null)
            {
                if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var customRole))
                {
                    return customRole.RoleColor;
                }
                else
                {
                    return roleBehaviour.NameColor;
                }
            }
            else
            {
                return Color.white;
            }
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static class CheckGameEndPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
            if (CheckEndGameForEnergyThief(__instance)) return false;
            if (CheckEndGameForDoubleAgent(__instance)) return false;
            if (CheckEndGameForPrankster(__instance)) return false;
            return true;
        } 

        public static bool CheckEndGameForEnergyThief(ShipStatus __instance)
        {
                if (PlayerControl.LocalPlayer != null  && PlayerControl.LocalPlayer.Data.Role is EnergyThief)
                {
                    int drainCount = Utils.GetDrainCount(PlayerControl.LocalPlayer.PlayerId);
                    if (drainCount > 3)
                    {
                        GameManager.Instance.RpcEndGame((GameOverReason)NewModEndReasons.EnergyThiefWin, false);
                        StatsManager.Instance.AddWinReason((GameOverReason)NewModEndReasons.DoubleAgentWin, (int)GameManager.Instance.LogicOptions.MapId, (RoleTypes)RoleId.Get<EnergyThief>());
                        return true;
                    }
                }
            return false; 
        }

        public static bool CheckEndGameForDoubleAgent(ShipStatus __instance)
        {
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role is DoubleAgent)
                {
                    bool tasksCompleted = PlayerControl.LocalPlayer.AllTasksCompleted();
                    bool isSabotageActive = Utils.IsSabotage();
                    if (tasksCompleted && isSabotageActive)
                    {
                        GameManager.Instance.RpcEndGame((GameOverReason)NewModEndReasons.DoubleAgentWin, false);
                        StatsManager.Instance.AddWinReason((GameOverReason)NewModEndReasons.DoubleAgentWin, (int)GameManager.Instance.LogicOptions.MapId, (RoleTypes)RoleId.Get<DoubleAgent>());
                        return true;
                    }
                }
            return false;
        }
        public static bool CheckEndGameForPrankster(ShipStatus __instance)
        {
             if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role is Prankster)
             {
                int WinReportCount = 2;
                int currentReportCount = PranksterUtilities.GetReportCount(PlayerControl.LocalPlayer.PlayerId);
                if (currentReportCount >= WinReportCount)
                {
                   GameManager.Instance.RpcEndGame((GameOverReason)NewModEndReasons.PranksterWin, false);
                   StatsManager.Instance.AddWinReason((GameOverReason)NewModEndReasons.PranksterWin, (int)GameManager.Instance.LogicOptions.MapId, (RoleTypes)RoleId.Get<Prankster>());
                   return true;
                }
             }
             return false;
        }
    }
}