using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameModes;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Buttons;
using NewMod.Roles.ImpostorRoles;
using TMPro;

namespace NewMod.CustomGameModes
{
    [RegisterGameMode]
    public class RevivalRoyale : CustomGameMode 
    {
        public TextMeshPro ReviveCounter;
        public Dictionary<PlayerControl, int> playerReviveCounts = new Dictionary<PlayerControl, int>();
        public int ReviveCount = 0;
        private Dictionary<PlayerControl, bool> playerStates = new Dictionary<PlayerControl, bool>();
        public override string Name => "Revival Royale";
        public override string Description => "Everyone is a Necromancer. Revive as many bodies as you can to outlast your opponents.\n<b>The one with the most revivals wins the Revival Royale</b>";
        public override int Id => 1;

        public override void Initialize()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
             player.RpcSetRole((RoleTypes)RoleId.Get<NecromancerRole>());
             playerReviveCounts[player] = ReviveCount;
             playerStates[player] = player.Data.IsDead; 
             CustomButtonSingleton<NecromancerButton>.Instance.IncreaseUses(3);
             NewMod.Instance.Log.LogMessage("Initialize RevivalRoyale GameMode done!");
            }
        }

        public override void HudUpdate(HudManager instance)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                bool isDead = player.Data.IsDead;

                if (playerStates.ContainsKey(player) && playerStates[player] && !isDead)
                {
                    if (playerReviveCounts.ContainsKey(PlayerControl.LocalPlayer))
                    {
                        playerReviveCounts[PlayerControl.LocalPlayer]++;
                        ReviveCount = playerReviveCounts[PlayerControl.LocalPlayer];
                    }
                    ReviveCounter.text = $"Revive Count: {ReviveCount}";
                    if (ReviveCount >= 6)
                    {
                        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, true);
                        break;
                    }
                }
                playerStates[player] = isDead;
            }
        }

        public override void HudStart(HudManager instance)
        {
            ReviveCounter = Helpers.CreateTextLabel("RR", instance.transform, AspectPosition.EdgeAlignments.Top, new UnityEngine.Vector3(0f, 0.20f, 0f), 2f, TextAlignmentOptions.MidlineRight);
            int localReviveCount = playerReviveCounts.ContainsKey(PlayerControl.LocalPlayer) ? playerReviveCounts[PlayerControl.LocalPlayer] : 0;
            ReviveCounter.text = $"Revive Count: {localReviveCount}";
        }

        public override List<NetworkedPlayerInfo> CalculateWinners()
        {
            var winner = playerReviveCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
            return winner.Key != null ? new List<NetworkedPlayerInfo> { winner.Key.Data } : new List<NetworkedPlayerInfo>();
        }

        public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
        {
            runOriginal = false;
        }
        public override bool AreGameSettingsEnabled()
        {
            return false;
        }
        public override bool AreRoleSettingsEnabled()
        {
            return true;
        }
        public override bool ShowGameModeIntroCutscene()
        {
            return true;
        }
        public override bool CanReport(DeadBody body)
        {
            return false;
        }
        public override bool ShouldShowSabotageMap(MapBehaviour map)
        {
            return false;
        }
    }
}
