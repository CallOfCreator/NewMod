using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Networking.Attributes;
using UnityEngine;
using Reactor.Utilities;
using AmongUs.GameOptions;
using MiraAPI.Networking;
using NewMod.Roles.NeutralRoles;
using System.Collections;

namespace NewMod.Utilities
{
    public static class Utils
    {
        public static Dictionary<byte, int> EnergyThiefDrainCounts = new Dictionary<byte, int>();
        public static Dictionary<PlayerControl, PlayerControl> PlayerKiller = new Dictionary<PlayerControl, PlayerControl>();
        public static Dictionary<byte, int> MissionSuccessCount = new Dictionary<byte, int>();
        public static Dictionary<byte, int> MissionFailureCount = new Dictionary<byte, int>();
        public static HashSet<PlayerControl> waitingPlayers = new();
        public static Dictionary<byte, List<RoleBehaviour>> savedPlayerRoles = new Dictionary<byte, List<RoleBehaviour>>();
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
            var reviveSound = NewModAsset.ReviveSound.LoadAsset();

            if (player != null)
            {
                foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
                {
                    if (deadBody.ParentId == body.ParentId)
                        Object.Destroy(deadBody.gameObject);
                }
                SoundManager.Instance.PlaySound(reviveSound, false, 1f, null);
                
                player.Revive();

                if (player.Data.Role is NoisemakerRole role)
                {
                    Object.Destroy(role.deathArrowPrefab.gameObject);
                }
                player.RpcSetRole(RoleTypes.Impostor, true);
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
        public static void RecordMissionSuccess(PlayerControl specialAgent)
        {
            var playerId = specialAgent.PlayerId;
            MissionSuccessCount[playerId] = GetMissionSuccessCount(playerId) + 1;
        }
        public static int GetMissionSuccessCount(byte playerId)
        {
            return MissionSuccessCount.TryGetValue(playerId, out var count) ? count : 0;
        }
        public static void ResetMissionSuccessCount()
        {
            MissionSuccessCount.Clear();
        }
        public static void RecordMissionFailure(PlayerControl specialAgent)
        {
            var playerId = specialAgent.PlayerId;
            MissionFailureCount[playerId] = GetMissionFailureCount(playerId) + 1;
        }
        public static int GetMissionFailureCount(byte playerId)
        {
            return MissionFailureCount.TryGetValue(playerId, out var count) ? count : 0;
        }
        public static void ResetMissionFailureCount()
        {
            MissionFailureCount.Clear();
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
        /// Retrieves a random player from the game meets a specified condition.
        /// </summary>
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
                    // Set up arrow for most wanted target
                    var gameObj = new GameObject();
                    var arrow = gameObj.AddComponent<ArrowBehaviour>();
                    gameObj.transform.parent = mostwantedTarget.gameObject.transform;
                    gameObj.layer = 5;
                    var renderer = gameObj.AddComponent<SpriteRenderer>();
                    renderer.sprite = NewModAsset.Arrow.LoadAsset();
                    arrow.target = mostwantedTarget.transform.position;
                    arrow.image = renderer;

                    // Save the current role of the target
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
                    // Disguise as a random player
                    //var randPlayer = GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);
                    //target.RpcShapeshift(randPlayer, false);
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
        public static void MissionSuccess(PlayerControl target, PlayerControl specialAgent)
        {
            RecordMissionSuccess(specialAgent);

            if (specialAgent.AmOwner)
            {
                int currentSuccessCount = GetMissionSuccessCount(specialAgent.PlayerId);
                int netScore = currentSuccessCount - GetMissionFailureCount(specialAgent.PlayerId);
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
        public static void MissionFails(PlayerControl target, PlayerControl specialAgent)
        {
            RecordMissionFailure(specialAgent);

            if (specialAgent.AmOwner)
            {
                int currentFailureCount = GetMissionFailureCount(specialAgent.PlayerId);
                int netScore = GetMissionSuccessCount(specialAgent.PlayerId) - currentFailureCount;
                Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FF0000>Target {target.Data.PlayerName} has failed their mission! <b>Current net score: {netScore}/3</b></color>"));
            }
            else
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FF0000>Mission Failed! You will face the consequences!</color>"));
            }
            specialAgent.RpcCustomMurder(target, createDeadBody: false, didSucceed: true, showKillAnim: false, playKillSound: true, teleportMurderer: false);

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
        public static Il2CppSystem.Collections.Generic.Dictionary<PlayerControl, Il2CppSystem.Collections.Generic.List<PlayerTask>> savedTasks = new();

        [MethodRpc((uint)CustomRPC.AssignMission)]
        public static void AssignMission(PlayerControl target)
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
        public static IEnumerator CaptureScreenshot(string filePath)
        {
            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, false);
            ScreenCapture.CaptureScreenshot(filePath, 4);
            VisionaryUtilities.CapturedScreenshotPaths.Add(filePath);
            NewMod.Instance.Log.LogInfo($"Capturing screenshot at {System.IO.Path.GetFileName(filePath)}.");

            yield return new WaitForSeconds(0.2f);

            HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, true);
        }
    }
}
