using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;
using NewMod.Buttons.EnergyThief;
using NewMod.Buttons.Necromancer;
using NewMod.Buttons.Prankster;
using NewMod.Buttons.Revenant;
using NewMod.Buttons.SpecialAgent;
using NewMod.Buttons.Visionary;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using MiraAPI.GameOptions;
using NewMod.Options.Roles.InjectorOptions;

namespace NewMod.Utilities
{
    /// <summary>
    /// Provides various utility methods and fields for the mod.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Tracks the number of drains performed by each Energy Thief, keyed by player ID.
        /// </summary>
        public static Dictionary<byte, int> EnergyThiefDrainCounts = new Dictionary<byte, int>();

        /// <summary>
        /// Maps a victim player to its killer.
        /// </summary>
        public static Dictionary<PlayerControl, PlayerControl> PlayerKiller = new Dictionary<PlayerControl, PlayerControl>();

        /// <summary>
        /// Stores the number of successful missions per player, keyed by their ID.
        /// </summary>
        public static Dictionary<byte, int> MissionSuccessCount = new Dictionary<byte, int>();

        /// <summary>
        /// Stores the number of failed missions per player, keyed by their ID.
        /// </summary>
        public static Dictionary<byte, int> MissionFailureCount = new Dictionary<byte, int>();

        /// <summary>
        /// Stores the player IDs of all players who have been injected by the Injector role.
        /// Used to track injection progress for win condition.
        /// </summary>
        public static readonly HashSet<byte> InjectedPlayerIds = new();

        /// <summary>
        /// Holds a set of players who are currently waiting for an event or action.
        /// </summary>
        public static HashSet<PlayerControl> waitingPlayers = new();

        /// <summary>
        /// Maintains saved roles for players, keyed by their ID.
        /// </summary>
        public static Dictionary<byte, List<RoleBehaviour>> savedPlayerRoles = new Dictionary<byte, List<RoleBehaviour>>();

        /// <summary>
        /// Maps a player ID to a TextMeshPro timer display for missions.
        /// </summary>
        public static Dictionary<byte, TMPro.TextMeshPro> MissionTimer = new Dictionary<byte, TMPro.TextMeshPro>();

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
                player.RpcSetRole(RoleTypes.Impostor, true);
            }
        }

        // Inspired By : https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/CrewmateRoles/AltruistMod/Coroutine.cs#L57
        public static void ReviveV2(DeadBody body)
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
                player.RpcSetRole((RoleTypes)RoleId.Get<Revenant>(), true);
                NewMod.Instance.Log.LogError($"---------------^^^^^^SETTING ROLE TO REVENANT-------------^^^^ NEW ROLE: {player.Data.Role.NiceName}");
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
        /// Records a successful mission for the given Special Agent player.
        /// </summary>
        /// <param name="specialAgent">The player who successfully completed the mission.</param>
        public static void RecordMissionSuccess(PlayerControl specialAgent)
        {
            var playerId = specialAgent.PlayerId;
            MissionSuccessCount[playerId] = GetMissionSuccessCount(playerId) + 1;
        }

        /// <summary>
        /// Retrieves the number of successful missions for a given player.
        /// </summary>
        /// <param name="playerId">The player's ID.</param>
        /// <returns>The count of successful missions.</returns>
        public static int GetMissionSuccessCount(byte playerId)
        {
            return MissionSuccessCount.TryGetValue(playerId, out var count) ? count : 0;
        }

        /// <summary>
        /// Resets the count of successful missions for all players.
        /// </summary>
        public static void ResetMissionSuccessCount()
        {
            MissionSuccessCount.Clear();
        }

        /// <summary>
        /// Records a failed mission for the given Special Agent player.
        /// </summary>
        /// <param name="specialAgent">The player who failed the mission.</param>
        public static void RecordMissionFailure(PlayerControl specialAgent)
        {
            var playerId = specialAgent.PlayerId;
            MissionFailureCount[playerId] = GetMissionFailureCount(playerId) + 1;
        }

        /// <summary>
        /// Retrieves the number of failed missions for a given player.
        /// </summary>
        /// <param name="playerId">The player's ID.</param>
        /// <returns>The count of failed missions.</returns>
        public static int GetMissionFailureCount(byte playerId)
        {
            return MissionFailureCount.TryGetValue(playerId, out var count) ? count : 0;
        }

        /// <summary>
        /// Resets the count of failed missions for all players.
        /// </summary>
        public static void ResetMissionFailureCount()
        {
            MissionFailureCount.Clear();
        }

        /// <summary>
        /// Registers a player as having been injected by the Injector.
        /// Adds the player's ID to the injected players tracking list.
        /// </summary>
        /// <param name="target">The player who was injected.</param>
        public static void RegisterPlayerInjection(PlayerControl target)
        {
            InjectedPlayerIds.Add(target.PlayerId);
        }

        /// <summary>
        /// Gets the number of unique players that have been injected by the Injector.
        /// Used to evaluate the Injector's win condition.
        /// </summary>
        /// <returns>The total number of unique injected players.</returns>
        public static int GetInjectedCount()
        {
            return InjectedPlayerIds.Count;
        }


        /// <summary>
        /// Clear's InjectedPlayerIds at end of the game
        /// </summary>
        public static void ClearInjections()
        {
            InjectedPlayerIds.Clear();
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
        /// Sends an RPC to revive a player from a dead body and set their role to Revenant.
        /// </summary>
        /// <param name="body">The DeadBody instance to revive from.</param>
        public static void RpcReviveV2(DeadBody body)
        {
            ReviveV2(body);
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.Revive,
                SendOption.Reliable
            );
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(body.ParentId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        // Thanks to: https://github.com/yanpla/yanplaRoles/blob/master/Utils.cs#L55
        /// <summary>
        /// Records a player's role in their role history.
        /// </summary>
        /// <param name="playerId">The ID of the player</param>
        /// <param name="role">The RoleBehaviour to save.</param>
        public static void SavePlayerRole(byte playerId, RoleBehaviour role)
        {
            if (!savedPlayerRoles.ContainsKey(playerId))
            {
                savedPlayerRoles[playerId] = new List<RoleBehaviour>();
            }
            savedPlayerRoles[playerId].Add(role);
        }

        // Thanks to: https://github.com/yanpla/yanplaRoles/blob/master/Utils.cs#L64
        /// <summary>
        /// Retrieves the role history for a specific player.
        /// </summary>
        /// <param name="playerId">The ID of the player</param>
        /// <returns>A list of RoleBehaviour representing the player's role history.</returns>
        public static List<RoleBehaviour> GetPlayerRolesHistory(byte playerId)
        {
            if (savedPlayerRoles.ContainsKey(playerId))
            {
                return savedPlayerRoles[playerId];
            }
            return new List<RoleBehaviour>();
        }

        /// <summary>
        /// Retrieves a random player from the game who meets a specified condition.
        /// </summary>
        /// <param name="match">A predicate to filter eligible players.</param>
        /// <returns>A random PlayerControl instance, or null if none are valid.</returns>
        public static PlayerControl GetRandomPlayer(System.Predicate<PlayerControl> match)
        {
            var players = PlayerControl.AllPlayerControls.ToArray().Where(p => match(p)).ToList();

            if (players.Count > 0)
            {
                return players[Random.RandomRange(0, players.Count)];
            }
            return null;
        }

        /// <summary>
        /// Checks if there is at least one dead player in the game.
        /// </summary>
        /// <returns>A PlayerControl who is dead, or null if none.</returns>
        public static PlayerControl AnyDeadPlayer()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead)
                {
                    return player;
                }
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
            List<System.Action> actions = new()
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
                    var randomPlayer = GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);
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

        /// <summary>
        /// Selects and processes a mission for the specified target player based on the provided MissionType.
        /// </summary>
        /// <param name="target">The target player receiving the mission.</param>
        /// <param name="mission">The type of mission assigned.</param>
        /// <returns>A formatted string describing the selected mission.</returns>
        public static string GetMission(PlayerControl target, MissionType mission)
        {
            var mostwantedTarget = GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);

            string selectedMission = mission switch
            {
                MissionType.KillMostWanted => $"Kill the Most Wanted Target: {mostwantedTarget.Data.PlayerName}",
                MissionType.DrainEnergy => "Drain one player using Energy Thief abilities",
                MissionType.CreateFakeBodies => "Disguise yourself as a random player and create fake dead bodies around the map using Prankster abilities!",
                MissionType.ReviveAndKill => "Revive a dead player using Necromancer powers and kill them again",
                _ => "Unknown mission."
            };
            switch (mission)
            {
                case MissionType.KillMostWanted:
                    var gameObj = new GameObject();
                    var arrow = gameObj.AddComponent<ArrowBehaviour>();
                    gameObj.transform.parent = mostwantedTarget.gameObject.transform;
                    gameObj.layer = 5;
                    var renderer = gameObj.AddComponent<SpriteRenderer>();
                    renderer.sprite = NewModAsset.Arrow.LoadAsset();
                    arrow.target = mostwantedTarget.transform.position;
                    arrow.image = renderer;

                    SavePlayerRole(target.PlayerId, target.Data.Role);

                    if (!target.Data.Role.IsImpostor)
                    {
                        target.RpcSetRole(RoleTypes.Impostor, true);
                    }
                    Coroutines.Start(CoroutinesHelper.CoHandleWantedTarget(arrow, mostwantedTarget, target));

                    var rolesHistory = GetPlayerRolesHistory(target.PlayerId);
                    if (rolesHistory.Count > 0)
                    {
                        var lastIndex = rolesHistory.Count - 1;
                        var originalRole = rolesHistory[lastIndex];
                        rolesHistory.RemoveAt(lastIndex);
                        target.RpcSetRole(originalRole.Role, true);
                    }
                    break;

                case MissionType.CreateFakeBodies:
                    if (target.AmOwner)
                    {
                        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#32CD32><i><b>Press F5 to Create Dead Bodies</b></i></color>"));
                    }
                    Coroutines.Start(CoroutinesHelper.UsePranksterAbilities(target));
                    break;

                case MissionType.DrainEnergy:
                    if (target.AmOwner)
                    {
                        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#00FA9A><i><b>Press F5 to drain nearby players'energy</b></i></color>"));
                    }
                    Coroutines.Start(CoroutinesHelper.UseEnergyThiefAbilities(target));
                    break;

                case MissionType.ReviveAndKill:
                    Coroutines.Start(CoroutinesHelper.CoReviveAndKill(target));
                    break;
            }
            return selectedMission;
        }

        [MethodRpc((uint)CustomRPC.MissionSuccess)]
        public static void RpcMissionSuccess(PlayerControl source, PlayerControl target)
        {
            RecordMissionSuccess(source);

            if (source.AmOwner)
            {
                int currentSuccessCount = GetMissionSuccessCount(source.PlayerId);
                int netScore = currentSuccessCount - GetMissionFailureCount(source.PlayerId);
                Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FFD700>Target {target.Data.PlayerName} has completed their mission!\nCurrent net score: {netScore}/3</color>"));
            }
            else
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#32CD32>Mission Completed! You are free to go!</color>"));
            }
            if (savedTasks.ContainsKey(target))
            {
                target.myTasks = savedTasks[target];
                savedTasks.Remove(target);
            }
            if (SpecialAgent.AssignedPlayer == target)
            {
                SpecialAgent.AssignedPlayer = null;
            }
            target.Data.Role.buttonManager.SetEnabled();
        }

        [MethodRpc((uint)CustomRPC.MissionFails)]
        public static void RpcMissionFails(PlayerControl source, PlayerControl target)
        {
            RecordMissionFailure(source);

            if (source.AmOwner)
            {
                int currentFailureCount = GetMissionFailureCount(source.PlayerId);
                int netScore = GetMissionSuccessCount(source.PlayerId) - currentFailureCount;
                Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF0000>Target {target.Data.PlayerName} has failed their mission! <b>Current net score: {netScore}/3</b></color>"));
            }
            else
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF0000>Mission Failed! You will face the consequences!</color>"));
            }
            source.RpcCustomMurder(target, createDeadBody: false, didSucceed: true, showKillAnim: false, playKillSound: true, teleportMurderer: false);

            if (savedTasks.ContainsKey(target))
            {
                target.myTasks = savedTasks[target];
                savedTasks.Remove(target);
            }
            if (SpecialAgent.AssignedPlayer == target)
            {
                SpecialAgent.AssignedPlayer = null;
            }
            target.Data.Role.buttonManager.SetEnabled();
        }

        /// <summary>
        /// Stores tasks that have been saved for a given player, allowing restoration after missions.
        /// </summary>
        public static Il2CppSystem.Collections.Generic.Dictionary<PlayerControl, Il2CppSystem.Collections.Generic.List<PlayerTask>> savedTasks = new();

        /// <summary>
        /// Assigns a random mission to the target player as a custom RPC.
        /// </summary>
        /// <param name="source">The player initiating the assignment (Special Agent).</param>
        /// <param name="target">The player who will receive the mission.</param>
        [MethodRpc((uint)CustomRPC.AssignMission)]
        public static void RpcAssignMission(PlayerControl source, PlayerControl target)
        {
            // Save the target's tasks
            if (!savedTasks.ContainsKey(target))
            {
                var newTaskList = new Il2CppSystem.Collections.Generic.List<PlayerTask>();

                foreach (var task in target.myTasks)
                {
                    newTaskList.Add(task);
                }
                savedTasks[target] = newTaskList;
            }

            // Clear all assigned tasks for the specified target player
            target.myTasks.Clear();

            // Get all values of the MissionType enum
            MissionType[] missions = (MissionType[])System.Enum.GetValues(typeof(MissionType));
            // Pick a random mission
            MissionType randomMission = missions[Random.Range(0, missions.Length)];

            // Add the mission message to the player's tasks
            ImportantTextTask Missionmessage = new GameObject("MissionMessage").AddComponent<ImportantTextTask>();
            Missionmessage.transform.SetParent(AmongUsClient.Instance.transform, false);
            Missionmessage.Text = $"<color=red>Special Agent</color> has given you a mission!\n" +
                       $"<b><color=blue>Mission:</color></b> {GetMission(target, randomMission)}\n" +
                       $"<i><color=green>Complete it or face the consequences!</color></i>";

            target.myTasks.Insert(0, Missionmessage);
            // Disable the Role Player's Ability
            target.Data.Role.buttonManager.SetDisabled();

            Coroutines.Start(CoroutinesHelper.CoMissionTimer(target, 60f));
        }

        /// <summary>
        /// Captures a screenshot of the current game screen, hides the HUD, and then reactivates it.
        /// </summary>
        /// <param name="filePath">The path to save the screenshot file.</param>
        /// <returns>An IEnumerator for coroutine control.</returns>
        public static IEnumerator CaptureScreenshot(string filePath)
        {
            var clip = NewModAsset.VisionarySound.LoadAsset();

            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, false);
            SoundManager.Instance.PlaySound(clip, false, 1f, null);
            ScreenCapture.CaptureScreenshot(filePath, 4);
            VisionaryUtilities.CapturedScreenshotPaths.Add(filePath);
            NewMod.Instance.Log.LogInfo($"Capturing screenshot at {System.IO.Path.GetFileName(filePath)}.");

            yield return new WaitForSeconds(0.2f);

            SoundManager.Instance.StopSound(clip);
            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, true);
        }

        /// <summary>
        /// Causes the player to feign death, creating a body. If unreported, the player is revived after 10 seconds.
        /// </summary>
        /// <param name="player">The player feigning death.</param>
        /// <returns>An IEnumerator for coroutine control.</returns>
        public static IEnumerator StartFeignDeath(PlayerControl player)
        {
            var clip = NewModAsset.FeignDeathSound.LoadAsset();

            player.RpcCustomMurder(player,
                didSucceed: true,
                resetKillTimer: false,
                createDeadBody: true,
                teleportMurderer: false,
                showKillAnim: false,
                playKillSound: false);


            SoundManager.Instance.PlaySound(clip, false, 1f, null);

            if (player.AmOwner)
            {
                HudManager.Instance.SetHudActive(false);
            }
            yield return new WaitForSeconds(0.5f);

            var body = player.GetNearestDeadBody(15f);

            var info = new Revenant.FeignDeathInfo
            {
                Timer = 10f,
                DeadBody = body,
                Reported = false,
            };
            Revenant.FeignDeathStates[player.PlayerId] = info;

            Coroutines.Start(CoroutinesHelper.CoNotify("<color=green>You are now feigning death.\nYou will be revived in 10 seconds if unreported.</color>"));

            float timer = 10f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                info.Timer = timer;
                yield return null;

                if (info.Reported)
                {
                    yield return CoroutinesHelper.CoNotify("<color=red>Your feign death has been reported. You remain dead.</color>");
                    SoundManager.Instance.StopSound(clip);
                    yield break;
                }
            }
            RpcReviveV2(body);
            player.transform.position = body.transform.position;
            player.RpcShapeshift(GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected), false);
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=green>You have been revived in a new body!</color>"));
            Revenant.HasUsedFeignDeath = true;
            Revenant.StalkingStates[player.PlayerId] = true;
            Revenant.FeignDeathStates.Remove(player.PlayerId);

            if (player.AmOwner)
            {
                HudManager.Instance.SetHudActive(player, player.Data.Role, true);
            }
            SoundManager.Instance.StopSound(clip);
        }

        /// <summary>
        /// Gradually fades out the provided ghost object and then destroys it.
        /// </summary>
        /// <param name="ghost">The GameObject representing the ghost.</param>
        /// <param name="fadeDuration">The duration of the fade effect.</param>
        /// <returns>An IEnumerator for coroutine control.</returns>
        public static IEnumerator FadeAndDestroy(GameObject ghost, float fadeDuration)
        {
            SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
            float alpha = 0.5f;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime / fadeDuration * 0.5f;
                if (ghostRenderer != null)
                {
                    ghostRenderer.color = new Color(1f, 0f, 0f, alpha);
                }
                yield return null;
            }
            Object.Destroy(ghost);
        }

        /// <summary>
        /// Maps each role to its associated list of custom action button types.
        /// Used by Overload to absorb abilities based on the prey's role.
        /// </summary>
        public static readonly Dictionary<System.Type, List<System.Type>> RoleToButtonsMap = new()
        {
            { typeof(EnergyThief),     new() { typeof(DrainButton) } },
            { typeof(NecromancerRole), new() { typeof(ReviveButton) } },
            { typeof(Prankster),       new() { typeof(FakeBodyButton) } },
            { typeof(Revenant),        new() { typeof(FeignDeathButton), typeof(DoomAwakening) } },
            { typeof(SpecialAgent),    new() { typeof(AssignButton) } },
            { typeof(TheVisionary),    new() { typeof(CaptureButton), typeof(ShowScreenshotButton) } }
            // TODO: Add Launchpad roles and their associated buttons here
        };

        /// <summary>
        /// Represents the different types of serums that the Injector role can apply to players.
        /// Each serum causes a unique effect that alters gameplay.
        /// </summary>
        public enum SerumType
        {
            /// <summary>
            /// Grants the target a burst of speed for a limited duration.
            /// </summary>
            Adrenaline,

            /// <summary>
            /// Immobilizes the target, preventing them from moving for a short time.
            /// </summary>
            Paralysis,

            /// <summary>
            /// Causes nearby players to be gently pushed away from the target for several seconds,
            /// as if repelled by a magnetic force.
            /// </summary>
            RepelSerum,

            /// <summary>
            /// Causes the target to bounce erratically for a few seconds.
            /// </summary>
            BounceSerum

            // More Coming Soon!
        }
        [MethodRpc((uint)CustomRPC.ApplySerum)]

        /// <summary>
        /// Handles applying serum effects to target players for the Injector role.
        /// </summary>
        public static void RpcApplySerum(PlayerControl source, PlayerControl target, SerumType serumType)
        {
            switch (serumType)
            {
                case SerumType.Adrenaline:
                    {
                        float boostPercent = OptionGroupSingleton<InjectorOptions>.Instance.AdrenalineSpeedBoost;
                        float multiplier = 1f + (boostPercent / 100f);
                        float originalSpeed = target.MyPhysics.Speed;

                        target.MyPhysics.Speed *= multiplier;

                        Coroutines.Start(CoroutinesHelper.ResetSpeedAfterDelay(target, originalSpeed, 10f));
                        break;
                    }

                case SerumType.Paralysis:
                    {
                        float duration = OptionGroupSingleton<InjectorOptions>.Instance.ParalysisDuration;

                        target.MyPhysics.body.velocity = Vector2.zero;

                        Coroutines.Start(CoroutinesHelper.EnableMovementAfterDelay(target, duration));
                        break;
                    }
                case SerumType.BounceSerum:
                    {
                        float bounceDuration = OptionGroupSingleton<InjectorOptions>.Instance.BounceDuration;
                        float h = OptionGroupSingleton<InjectorOptions>.Instance.BounceForceHorizontal;
                        float v = OptionGroupSingleton<InjectorOptions>.Instance.BounceForceVertical;
                        float maxRotate = OptionGroupSingleton<InjectorOptions>.Instance.BounceRotateEffect.Value;

                        Vector2 force = new(Random.Range(-h, h), Random.Range(-v, v));

                        target.MyPhysics.body.AddForce(force);

                        if (OptionGroupSingleton<InjectorOptions>.Instance.EnableBounceVariants)
                        {
                            if (Helpers.CheckChance(OptionGroupSingleton<InjectorOptions>.Instance.BounceRotateEffect))
                            {
                                target.transform.Rotate(0, 0, Random.Range(-maxRotate, maxRotate));
                            }
                            Coroutines.Start(CoroutinesHelper.ResetRotationAfterDelay(target, bounceDuration));
                        }
                    }
                    break;
                case SerumType.RepelSerum:
                    {
                        float RepelDuration = OptionGroupSingleton<InjectorOptions>.Instance.RepelDuration;
                        float RepelRange = OptionGroupSingleton<InjectorOptions>.Instance.RepelRange;
                        float RepelForce = OptionGroupSingleton<InjectorOptions>.Instance.RepelForce;

                        foreach (var other in PlayerControl.AllPlayerControls)
                        {
                            if (other == target || other.Data.IsDead && other.Data.Disconnected) continue;

                            float dist = Vector2.Distance(other.GetTruePosition(), target.GetTruePosition());

                            if (dist < RepelRange)
                            {
                                Vector2 dir = (other.GetTruePosition() - target.GetTruePosition()).normalized;
                                other.MyPhysics.body.velocity += dir * RepelForce;
                            }
                        }
                        Coroutines.Start(CoroutinesHelper.ResetRepelEffect(target, RepelDuration));
                    }
                    break;
            }
            RegisterPlayerInjection(target);
        }
    }
}

