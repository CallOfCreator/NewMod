using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace NewMod.Utilities
{
    public static class Utils
    {
        public static Dictionary<byte, int> EnergyThiefDrainCounts = new Dictionary<byte, int>();
        public static Dictionary<PlayerControl, PlayerControl> PlayerKiller = new Dictionary<PlayerControl, PlayerControl>();
        public static Dictionary<PlayerControl, int> NecromancerReviveCount = new Dictionary<PlayerControl, int>();
        public static HashSet<PlayerControl> waitingPlayers = new();

        /// <summary>
        /// Retrieves a PlayerControl instance by its player ID.
        /// </summary>
        /// <param name="id">The player's ID.</param>
        /// <returns>The PlayerControl object or null if not found.</returns>
        //  Thanks to: https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/Utils.cs#L219
        public static PlayerControl PlayerById(byte id)
        {
             foreach (var player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;

            return null;
        }
        /// <summary>  
        /// Records a kill event by mapping a victim to its killer.
        /// </summary>
        /// <param name="killer">The player who performed the kill.</param>
        /// <param name="victim">The player who was killed.</param>
        public static void RecordOnKill(PlayerControl killer, PlayerControl victim)
        {
            if (PlayerKiller.ContainsKey(killer))
            {
                PlayerKiller[victim] = killer;
            }
            else
            {
                PlayerKiller.Add(victim, killer);
            }
        }

        /// <summary>
        /// Retrieves the killer of the specified victim.
        /// </summary>
        /// <param name="victim">The player who was killed.</param>
        /// <returns>The player who killed the victim, or null if not found.</returns>
        public static PlayerControl GetKiller(PlayerControl victim)
        {
            return PlayerKiller.TryGetValue(victim, out var killer) ? killer : null;
        }
        /// <summary>
        /// Finds the closest dead body to the local player within their kill distance.
        /// </summary>
        /// <returns>The closest DeadBody instance, or null if none are found.</returns>
        public static DeadBody GetClosestBody()
        {
            var allocs = Physics2D.OverlapCircleAll(
                PlayerControl.LocalPlayer.GetTruePosition(),
                GameOptionsManager.Instance.currentNormalGameOptions.KillDistance,
                Constants.PlayersOnlyMask
            );

            DeadBody closestBody = null;
            var closestDistance = float.MaxValue;

            foreach (var collider2D in allocs)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || collider2D.tag != "DeadBody") continue;

                var component = collider2D.GetComponent<DeadBody>();
                var distance = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), component.TruePosition);

                if (distance <= GameOptionsManager.Instance.currentNormalGameOptions.KillDistance && distance < closestDistance)
                {
                    closestBody = component;
                    closestDistance = distance;
                }
            }
            return closestBody;
        }
        // Inspired By : https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/CrewmateRoles/AltruistMod/Coroutine.cs#L57
        public static void Revive(DeadBody body)
        {
             if (body == null) return;

            var parentId = body.ParentId;
            var player = PlayerById(parentId);

            if (player != null)
            {
                foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
                {
                    if (deadBody.ParentId == body.ParentId)
                        Object.Destroy(deadBody.gameObject);
                }

                player.Revive();
                if (player.Data.Role is NoisemakerRole role)
                {
                    Object.Destroy(role.deathArrowPrefab.gameObject);
                }
                player.RpcSetRole(AmongUs.GameOptions.RoleTypes.Impostor, true);
            }
        }
        
        // Thanks to: https://github.com/Rabek009/MoreGamemodes/blob/master/Modules/Utils.cs#L66
        /// <summary>
        /// Checks if a particular system type is active on the current map.
        /// </summary>
        /// <param name="type">The SystemTypes to check.</param>
        /// <returns>True if the system type is active, otherwise false.</returns>
        public static bool IsActive(SystemTypes type)
        {
            int mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            
            if (!ShipStatus.Instance.Systems.ContainsKey(type))
            {
              return false;
            }   
            switch (type)
            {
                case SystemTypes.Electrical:
                    if (mapId == 5) return false;
                    var SwitchSystem = ShipStatus.Instance.Systems[type].TryCast<SwitchSystem>();
                    return SwitchSystem != null && SwitchSystem.IsActive;
                case SystemTypes.Reactor:
                    if (mapId == 2) return false; 
                    else
                    {
                        var ReactorSystemType = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                        return ReactorSystemType != null && ReactorSystemType.IsActive;
                    }
                case SystemTypes.Laboratory:
                    if (mapId != 2) return false;
                    var ReactorSystemType2 = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                    return ReactorSystemType2 != null && ReactorSystemType2.IsActive;
                case SystemTypes.LifeSupp:
                    if (mapId is 2 or 4 or 5) return false;
                    var LifeSuppSystemType = ShipStatus.Instance.Systems[type].TryCast<LifeSuppSystemType>();
                    return LifeSuppSystemType != null && LifeSuppSystemType.IsActive;
                case SystemTypes.HeliSabotage:
                    if (mapId != 4) return false;
                    var HeliSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<HeliSabotageSystem>();
                    return HeliSabotageSystem != null && HeliSabotageSystem.IsActive;
                case SystemTypes.Comms:
                    if (mapId is 1 or 5)
                    {
                        var HqHudSystemType = ShipStatus.Instance.Systems[type].TryCast<HqHudSystemType>();
                        return HqHudSystemType != null && HqHudSystemType.IsActive;
                    }
                    else
                    {
                        var HudOverrideSystemType = ShipStatus.Instance.Systems[type].TryCast<HudOverrideSystemType>();
                        return HudOverrideSystemType != null && HudOverrideSystemType.IsActive;
                    }
                case SystemTypes.MushroomMixupSabotage:
                    if (mapId != 5) return false;
                    var MushroomMixupSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<MushroomMixupSabotageSystem>();
                    return MushroomMixupSabotageSystem != null && MushroomMixupSabotageSystem.IsActive;
            default:
                return false;
        }
    }
        // Thanks to : https://github.com/Rabek009/MoreGamemodes/blob/master/Modules/Utils.cs#L118
        /// <summary>
        /// Checks if any sabotage system is currently active.
        /// </summary>
        /// <returns>True if a sabotage system is active, otherwise false.</returns>
        public static bool IsSabotage()
        {
            return IsActive(SystemTypes.LifeSupp) ||
                   IsActive(SystemTypes.Reactor) ||
                   IsActive(SystemTypes.Laboratory) ||
                   IsActive(SystemTypes.Electrical) ||
                   IsActive(SystemTypes.Comms) ||
                   IsActive(SystemTypes.MushroomMixupSabotage) ||
                   IsActive(SystemTypes.HeliSabotage);
        }
        /// <summary>
        /// Records a drain count for the specified player.
        /// </summary>
        /// <param name="energyThief">The player representing the energy thief.</param>
        public static void RecordDrainCount(PlayerControl energyThief)
        {
          /*  if (energyThief == null) return;

            var playerId = energyThief.PlayerId;

            if (EnergyThiefDrainCounts.ContainsKey(playerId))
            {
                EnergyThiefDrainCounts[playerId] += 1; 
            }
            else
            {
                EnergyThiefDrainCounts.Add(playerId, 1);
            }
            NewMod.Instance.Log.LogInfo(GetDrainCount(playerId));
            */
            var playerId = energyThief.PlayerId;
            EnergyThiefDrainCounts[playerId] = GetDrainCount(playerId) + 1;
            NewMod.Instance.Log.LogInfo($"Player {playerId} drain count: {GetDrainCount(playerId)}");
        }

        /// <summary>
        /// Retrieves the drain count for a specific player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The drain count for the player.</returns>
        public static int GetDrainCount(byte playerId)
        {
            return EnergyThiefDrainCounts.TryGetValue(playerId, out var count) ? count : 0;
        }
        /// <summary>
        /// Resets all drain counts.
        /// </summary>
        public static void ResetDrainCount()
        {
            EnergyThiefDrainCounts.Clear();
        }

        /// <summary>
        /// Sends an RPC to revive a player from a dead body.
        /// </summary>
        /// <param name="body">The DeadBody instance to revive from.</param>
        public static void RpcRevive(DeadBody body)
        {
            Revive(body);
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.Revive,
                SendOption.Reliable
            );
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(body.ParentId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        /// <summary>
        /// Retrieves a random player from the game who is alive and not disconnected.
        /// </summary>
        /// <returns>A random PlayerControl instance, or null if none are valid.</returns>
        public static PlayerControl GetRandomPlayer()
        {
            var players = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.Data.IsDead && !p.Data.Disconnected).ToList();

            if (players.Count > 0)
            {
                return players[UnityEngine.Random.RandomRange(0, players.Count)];
            }
            return null;
        }
        
        /// <summary>
        /// Performs a random draining action on a target player as part of a custom RPC.
        /// </summary>
        /// <param name="source">The player who initiates the drain.</param>
        /// <param name="target">The player who is the target of the drain.</param>
        [MethodRpc((uint)CustomRPC.Drain)]
        public static void RpcRandomDrainActions(PlayerControl source, PlayerControl target)
        {
            List<System.Action> actions = new List<System.Action>
            {
                () =>
                {
                    target.MyPhysics.Speed *= 0.5f;
                    if (source.AmOwner)
                    {
                        HudManager.Instance.ShowPopUp($"<color=purple>{target.Data.PlayerName} speed was reduced by 50%!</color>");
                    }
                },
                () =>
                {
                    if (target.AmOwner)
                    {
                        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.black, 0.5f, false));
                        target.NetTransform.Halt();
                    }
                    if (source.AmOwner)
                    {
                        HudManager.Instance.ShowPopUp($"<color=blue>Movement is disabled for {target.Data.PlayerName}, and their screen is black!</color>");
                    }
                },
                () =>
                {
                    target.myTasks.Clear();
                    if (source.AmOwner)
                    {
                        HudManager.Instance.ShowPopUp($"<color=green>{target.Data.PlayerName} had all of their tasks cleared!</color>");
                    }
                },
                () =>
                {
                    target.RemainingEmergencies = 0;
                    if (source.AmOwner)
                    {
                        HudManager.Instance.ShowPopUp($"<color=orange>{target.Data.PlayerName} can no longer call emergency meetings!</color>");
                    }
                },
                () =>
                {
                    var randomPlayer = GetRandomPlayer();
                    if (randomPlayer != null)
                    {
                        target.NetTransform.RpcSnapTo(randomPlayer.GetTruePosition());
                        if (source.AmOwner)
                        {
                            HudManager.Instance.ShowPopUp($"<color=red>{target.Data.PlayerName} has been teleported!</color>");
                        }
                    }
                }
            };

            int randomIndex = Random.Range(0, actions.Count);
            actions[randomIndex].Invoke();
        }
    }
}
