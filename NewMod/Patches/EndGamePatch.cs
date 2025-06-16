using UnityEngine;
using HarmonyLib;
using System.Linq;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using AmongUs.GameOptions;
using Object = UnityEngine.Object;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using NewMod.Options.Roles.SpecialAgentOptions;
using MiraAPI.GameOptions;
using MiraAPI.Events;

namespace NewMod.Patches
{
    public static class EndGamePatch
    {
        [RegisterEvent]
        public static void OnGameEnd(GameEndEvent evt)
        {
            EndGameManager endGameManager = evt?.EndGameManager;

            foreach (var playerObj in endGameManager.GetComponentsInChildren<PoolablePlayer>())
            {
                GameObject.Destroy(playerObj.gameObject);
            }

            var winningPlayers = EndGameResult.CachedWinners.ToArray()
                .OrderByDescending(p => !p.IsYou)
                .ToList();
            int num = winningPlayers.Count;

            for (int i = 0; i < num; i++)
            {
                var playerData = winningPlayers[i];

                int num2 = (i % 2 == 0 ? -1 : 1);
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (i == 0) ? -8f : -1f;

                PoolablePlayer poolablePlayer = Object.Instantiate(endGameManager.PlayerPrefab, endGameManager.transform);

                float xPos = 1f * num2 * num3 * num5 * 0.9f;
                float yPos = FloatRange.SpreadToEdges(-1.125f, 0f, num3, num) * 0.9f;
                float zPos = (num6 + num3 * 0.01f) * 0.9f;

                poolablePlayer.transform.localPosition = new Vector3(xPos, yPos, zPos);
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
                    endGameManager.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;
                case (GameOverReason)NewModEndReasons.DoubleAgentWin:
                    customWinText = "Double Agent Win!";
                    customWinColor = GetRoleColor(GetRoleType<DoubleAgent>());
                    endGameManager.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;
                case (GameOverReason)NewModEndReasons.PranksterWin:
                    customWinText = "Prankster Win!";
                    customWinColor = GetRoleColor(GetRoleType<Prankster>());
                    endGameManager.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;
                case (GameOverReason)NewModEndReasons.SpecialAgentWin:
                    customWinText = "Special Agent Victory";
                    customWinColor = GetRoleColor(GetRoleType<SpecialAgent>());
                    endGameManager.BackgroundBar.material.SetColor("_Color", customWinColor);
                    break;
                default:
                    customWinText = string.Empty;
                    customWinColor = Color.white;
                    break;
            }

            if (!string.IsNullOrEmpty(customWinText))
            {
                var customWinTextObject = Object.Instantiate(endGameManager.WinText.gameObject, endGameManager.transform);
                customWinTextObject.transform.localPosition = new Vector3(
                    endGameManager.WinText.transform.position.x,
                    endGameManager.WinText.transform.position.y - 0.5f,
                    endGameManager.WinText.transform.position.z);
                customWinTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                var customWinTextComponent = customWinTextObject.GetComponent<TMPro.TMP_Text>();
                customWinTextComponent.text = customWinText;
                customWinTextComponent.color = customWinColor;
                customWinTextComponent.fontSize = 4f;
            }
        }

        private static string GetRoleName(CachedPlayerData playerData, out Color roleColor)
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

        private static RoleTypes GetRoleType<T>() where T : RoleBehaviour
        {
            ushort roleId = RoleId.Get<T>();
            return (RoleTypes)roleId;
        }

        private static Color GetRoleColor(RoleTypes roleType)
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
            if (CheckEndGameForRole<DoubleAgent>(__instance, (GameOverReason)NewModEndReasons.DoubleAgentWin)) return false;
            if (CheckEndGameForRole<SpecialAgent>(__instance, (GameOverReason)NewModEndReasons.SpecialAgentWin)) return false;
            if (CheckEndGameForRole<Prankster>(__instance, (GameOverReason)NewModEndReasons.PranksterWin, 3)) return false;
            if (CheckEndGameForRole<EnergyThief>(__instance, (GameOverReason)NewModEndReasons.EnergyThiefWin)) return false;
            return true;
        }
        public static bool CheckEndGameForRole<T>(ShipStatus __instance, GameOverReason winReason, int maxCount = 1) where T : RoleBehaviour
        {
            var rolePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p.Data.Role is T)
            .Take(maxCount)
            .ToList();

            foreach (var player in rolePlayers)
            {
                bool shouldEndGame = false;

                if (typeof(T) == typeof(DoubleAgent))
                {
                    bool tasksCompleted = player.AllTasksCompleted();
                    bool isSabotageActive = Utils.IsSabotage();
                    shouldEndGame = tasksCompleted && isSabotageActive;
                }
                if (typeof(T) == typeof(EnergyThief))
                {
                    int WinReportCount = 2;
                    int currentReportCount = PranksterUtilities.GetReportCount(player.PlayerId);
                    shouldEndGame = currentReportCount >= WinReportCount;
                }
                if (typeof(T) == typeof(SpecialAgent))
                {
                    int missionSuccessCount = Utils.GetMissionSuccessCount(player.PlayerId);
                    int missionFailureCount = Utils.GetMissionFailureCount(player.PlayerId);
                    int netScore = missionSuccessCount - missionFailureCount;
                    shouldEndGame = netScore >= OptionGroupSingleton<SpecialAgentOptions>.Instance.RequiredMissionsToWin;
                }
                if (shouldEndGame)
                {
                    GameManager.Instance.RpcEndGame(winReason, false);
                    CustomStatsManager.IncrementRoleWin((ICustomRole)player.Data.Role);
                    return true;
                }
            }
            return false;
        }
    }
}
