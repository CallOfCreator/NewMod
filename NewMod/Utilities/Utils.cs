using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Buttons.Roles;
using NewMod.Buttons.Roles.S1;
using NewMod.Modifiers;
using NewMod.Options.Roles;
using NewMod.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NewMod.Utilities;

/// <summary>
///     Provides various utility methods and fields for the mod.
/// </summary>
public static class Utils
{
    /// <summary>
    ///     Represents the different types of serums that the Injector role can apply to players.
    ///     Each serum causes a unique effect that alters gameplay.
    /// </summary>
    public enum SerumType
    {
        /// <summary>
        ///     Grants the target a burst of speed for a limited duration.
        /// </summary>
        Adrenaline,

        /// <summary>
        ///     Immobilizes the target, preventing them from moving for a short time.
        /// </summary>
        Paralysis,

        /// <summary>
        ///     Causes nearby players to be gently pushed away from the target for several seconds,
        ///     as if repelled by a magnetic force.
        /// </summary>
        RepelSerum,

        /// <summary>
        ///     Causes the target to bounce erratically for a few seconds.
        /// </summary>
        BounceSerum

        // More Coming Soon!
    }

    /// <summary>
    ///     Tracks the number of drains performed by each Energy Thief, keyed by player ID.
    /// </summary>
    public static Dictionary<byte, int> EnergyThiefDrainCounts = new();

    /// <summary>
    ///     Maps a victim player to its killer.
    /// </summary>
    public static Dictionary<byte, byte> PlayerKiller = new();

    /// <summary>
    ///     Stores the number of successful missions per player, keyed by their ID.
    /// </summary>
    public static Dictionary<byte, int> MissionSuccessCount = new();

    /// <summary>
    ///     Stores the number of failed missions per player, keyed by their ID.
    /// </summary>
    public static Dictionary<byte, int> MissionFailureCount = new();

    /// <summary>
    ///     Stores the player IDs of all players who have been injected by the Injector role.
    ///     Used to track injection progress for win condition.
    /// </summary>
    public static readonly HashSet<byte> InjectedPlayerIds = new();

    /// <summary>
    ///     Holds a set of players who are currently waiting for an event or action.
    /// </summary>
    public static HashSet<PlayerControl> waitingPlayers = new();

    /// <summary>
    ///     Maintains saved roles for players, keyed by their ID.
    /// </summary>
    public static Dictionary<byte, List<RoleBehaviour>> savedPlayerRoles = new();

    /// <summary>
    ///     Maps a player ID to a TextMeshPro timer display for missions.
    /// </summary>
    public static Dictionary<byte, TextMeshPro> MissionTimer = new();

    /// <summary>
    ///     A dictionary holding the strike kill counts for each player, indexed by their player ID.
    /// </summary>
    public static readonly Dictionary<byte, int> StrikeKills = new();

    public static Material _circleMat;

    /// <summary>
    ///     Stores tasks that have been saved for a given player, allowing restoration after missions.
    /// </summary>
    public static Il2CppSystem.Collections.Generic.Dictionary<PlayerControl, Il2CppSystem.Collections.Generic.List<PlayerTask>> savedTasks = new();

    /// <summary>
    ///     Maps each role to its associated list of custom action button types.
    ///     Used by Overload to absorb abilities based on the prey's role.
    /// </summary>
    public static readonly Dictionary<Type, List<Type>> RoleToButtonsMap = new()
    {
        { typeof(EnergyThief), new List<Type> { typeof(DrainButton) } },
        { typeof(NecromancerRole), new List<Type> { typeof(ReviveButton) } },
        { typeof(Prankster), new List<Type> { typeof(FakeBodyButton) } },
        { typeof(Revenant), new List<Type> { typeof(FeignDeathButton), typeof(DoomAwakening) } },
        { typeof(SpecialAgent), new List<Type> { typeof(AssignButton) } },
        { typeof(TheVisionary), new List<Type> { typeof(CaptureButton), typeof(ShowScreenshotButton) } },
        { typeof(PulseBlade), new List<Type> { typeof(StrikeButton) } },
        { typeof(WraithCaller), new List<Type> { typeof(CallWraithButton) } },
        { typeof(Edgeveil), new List<Type> { typeof(ArcButton) } },
        { typeof(Aegis), new List<Type> { typeof(AegisButton) } },
        { typeof(WardenRole), new List<Type> { typeof(WardenSealButton), typeof(InspectResidualButton) } },
        { typeof(Voidwalker), new List<Type> { typeof(EnterVoid) } },
        { typeof(TerminatorRole), new List<Type> { typeof(ObjectiveButton) } },
        { typeof(ArbitratorRole), new List<Type> { typeof(ArbitratorLeverageButton) } },
        { typeof(MirrorBladeRole), new List<Type> { typeof(MirrorReflectButton) } },
        { typeof(Shade), new List<Type> { typeof(DeployShadow) } },
        // Verifier is excluded since it uses a meeting ability.
        // I hate this system, I gotta replace it
        // TODO: Add Launchpad roles and their associated buttons here
    };

    /// <summary>
    ///     Retrieves a PlayerControl instance by its player ID.
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
    ///     Records a kill event by mapping a victim to its killer.
    /// </summary>
    /// <param name="killer">The player who performed the kill.</param>
    /// <param name="victim">The player who was killed.</param>
    public static void RecordOnKill(PlayerControl killer, PlayerControl victim)
    {
        PlayerKiller[victim.PlayerId] = killer.PlayerId;
    }

    /// <summary>
    ///     Retrieves the killer of the specified victim.
    /// </summary>
    /// <param name="victim">The player who was killed.</param>
    /// <returns>The player who killed the victim, or null if not found.</returns>
    public static PlayerControl GetKiller(PlayerControl victim)
    {
        return PlayerKiller.TryGetValue(victim.PlayerId, out var killerId) ? PlayerById(killerId) : null;
    }

    public static void ResetKillTracking()
    {
        PlayerKiller.Clear();
    }

    /// <summary>
    ///     Finds the closest dead body to the local player within their kill distance.
    /// </summary>
    /// <returns>The closest DeadBody instance, or null if none are found.</returns>
    public static DeadBody GetClosestBody()
    {
        var allocs = Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, Constants.PlayersOnlyMask);

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

// Thanks to: https://github.com/Rabek009/MoreGamemodes/blob/master/Modules/Utils.cs#L66
    /// <summary>
    ///     Checks if a particular system type is active on the current map.
    /// </summary>
    /// <param name="type">The SystemTypes to check.</param>
    /// <returns>True if the system type is active, otherwise false.</returns>
    public static bool IsActive(SystemTypes type)
    {
        int mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;

        if (!ShipStatus.Instance.Systems.ContainsKey(type)) return false;

        switch (type)
        {
            case SystemTypes.Electrical:
                if (mapId == 5) return false;
                var SwitchSystem = ShipStatus.Instance.Systems[type].TryCast<SwitchSystem>();
                return SwitchSystem != null && SwitchSystem.IsActive;
            case SystemTypes.Reactor:
                if (mapId == 2) return false;
                var ReactorSystemType = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                return ReactorSystemType != null && ReactorSystemType.IsActive;
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

                var HudOverrideSystemType = ShipStatus.Instance.Systems[type].TryCast<HudOverrideSystemType>();
                return HudOverrideSystemType != null && HudOverrideSystemType.IsActive;
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
    ///     Checks if any sabotage system is currently active.
    /// </summary>
    /// <returns>True if a sabotage system is active, otherwise false.</returns>
    public static bool IsSabotage()
    {
        return IsActive(SystemTypes.LifeSupp) || IsActive(SystemTypes.Reactor) || IsActive(SystemTypes.Laboratory) || IsActive(SystemTypes.Electrical) || IsActive(SystemTypes.Comms) || IsActive(SystemTypes.MushroomMixupSabotage) || IsActive(SystemTypes.HeliSabotage);
    }

    /// <summary>
    ///     Records a drain count for the specified player.
    /// </summary>
    /// <param name="energyThief">The player representing the energy thief.</param>
    public static void RecordDrainCount(PlayerControl energyThief)
    {
        var playerId = energyThief.PlayerId;
        EnergyThiefDrainCounts[playerId] = GetDrainCount(playerId) + 1;
        NewMod.Instance.Log.LogInfo($"Player {playerId} drain count: {GetDrainCount(playerId)}");
    }

    /// <summary>
    ///     Retrieves the drain count for a specific player.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <returns>The drain count for the player.</returns>
    public static int GetDrainCount(byte playerId)
    {
        return EnergyThiefDrainCounts.TryGetValue(playerId, out var count) ? count : 0;
    }

    /// <summary>
    ///     Resets all drain counts.
    /// </summary>
    public static void ResetDrainCount()
    {
        EnergyThiefDrainCounts.Clear();
    }

    /// <summary>
    ///     Records a successful mission for the given Special Agent player.
    /// </summary>
    /// <param name="specialAgent">The player who successfully completed the mission.</param>
    public static void RecordMissionSuccess(PlayerControl specialAgent)
    {
        var playerId = specialAgent.PlayerId;
        MissionSuccessCount[playerId] = GetMissionSuccessCount(playerId) + 1;
    }

    /// <summary>
    ///     Retrieves the number of successful missions for a given player.
    /// </summary>
    /// <param name="playerId">The player's ID.</param>
    /// <returns>The count of successful missions.</returns>
    public static int GetMissionSuccessCount(byte playerId)
    {
        return MissionSuccessCount.TryGetValue(playerId, out var count) ? count : 0;
    }

    /// <summary>
    ///     Resets the count of successful missions for all players.
    /// </summary>
    public static void ResetMissionSuccessCount()
    {
        MissionSuccessCount.Clear();
    }

    /// <summary>
    ///     Records a failed mission for the given Special Agent player.
    /// </summary>
    /// <param name="specialAgent">The player who failed the mission.</param>
    public static void RecordMissionFailure(PlayerControl specialAgent)
    {
        var playerId = specialAgent.PlayerId;
        var currentFailureCount = GetMissionFailureCount(playerId);

        if (currentFailureCount >= 0) MissionFailureCount[playerId] = currentFailureCount + 1;
    }

    /// <summary>
    ///     Retrieves the number of failed missions for a given player.
    /// </summary>
    /// <param name="playerId">The player's ID.</param>
    /// <returns>The count of failed missions.</returns>
    public static int GetMissionFailureCount(byte playerId)
    {
        return MissionFailureCount.TryGetValue(playerId, out var count) ? count : 0;
    }

    /// <summary>
    ///     Resets the count of failed missions for all players.
    /// </summary>
    public static void ResetMissionFailureCount()
    {
        MissionFailureCount.Clear();
    }

    /// <summary>
    ///     Resets the strike count for all players.
    ///     Clears the stored strike kill counts for all players.
    /// </summary>
    public static void ResetStrikeCount()
    {
        StrikeKills.Clear();
    }

    /// <summary>
    ///     Registers a strike kill for a specific player.
    ///     Increments the strike kill count for the given killer player.
    /// </summary>
    /// <param name="killer">The player who made the strike kill.</param>
    /// <param name="victim">The player who was struck (victim).</param>
    public static void RegisterStrikeKill(PlayerControl killer, PlayerControl victim)
    {
        var playerId = killer.PlayerId;
        StrikeKills[playerId] = StrikeKills.GetValueOrDefault(playerId) + 1;
    }

    /// <summary>
    ///     Retrieves the total number of strike kills for a specific player.
    /// </summary>
    /// <param name="playerId">The unique ID of the player whose strike count is being queried.</param>
    /// <returns>The number of strike kills for the specified player.</returns>
    public static int GetStrikes(byte playerId)
    {
        return StrikeKills.GetValueOrDefault(playerId);
    }

    /// <summary>
    ///     Registers a player as having been injected by the Injector.
    ///     Adds the player's ID to the injected players tracking list.
    /// </summary>
    /// <param name="target">The player who was injected.</param>
    public static void RegisterPlayerInjection(PlayerControl target)
    {
        InjectedPlayerIds.Add(target.PlayerId);
    }

    /// <summary>
    ///     Gets the number of unique players that have been injected by the Injector.
    ///     Used to evaluate the Injector's win condition.
    /// </summary>
    /// <returns>The total number of unique injected players.</returns>
    public static int GetInjectedCount()
    {
        return InjectedPlayerIds.Count;
    }

    /// <summary>
    ///     Clear's InjectedPlayerIds at end of the game
    /// </summary>
    public static void ResetInjections()
    {
        InjectedPlayerIds.Clear();
    }
// Inspired By: https://github.com/AU-Avengers/TOU-Mira/blob/dev/TownOfUs/Modules/ReviveUtilities.cs#L40

    [MethodRpc((uint)CustomRPC.HandleRevive)]
    public static IEnumerator HandleRevive(PlayerControl source, byte revivedId, RoleTypes roleToSet, float reviveX, float reviveY)
    {
        var revived = PlayerById(revivedId);

        if (revived.Data.Disconnected)
            yield break;

        yield return new WaitForSeconds(0.15f);

        if (revived.Data.Disconnected || !revived.Data.IsDead)
            yield break;

        var revivePos = new Vector2(reviveX, reviveY);
        var inMeetingOrExile = MeetingHud.Instance || ExileController.Instance;

        if (revived.Data.Role is NoisemakerRole noisemaker && noisemaker.deathArrowPrefab != null) Object.Destroy(noisemaker.deathArrowPrefab.gameObject);

        revived.Revive();
        revived.RemainingEmergencies = 0;
        RoleManager.Instance.SetRole(revived, roleToSet);
        revived.Data.Role.SpawnTaskHeader(revived);
        PlayerNameColor.Set(revived);

        if (AmongUsClient.Instance.AmHost) revived.RpcSetRole(roleToSet, true);

        if (!inMeetingOrExile)
        {
            revived.transform.position = revivePos;
            revived.MyPhysics.body.position = revivePos;
            Physics2D.SyncTransforms();

            if (revived.AmOwner) revived.NetTransform.RpcSnapTo(revivePos);
        }

        foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
            if (deadBody.ParentId == revived.PlayerId)
                Object.Destroy(deadBody.gameObject);

        var elapsed = 0f;
        while (elapsed < 1f)
        {
            foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
                if (deadBody.ParentId == revived.PlayerId)
                    Object.Destroy(deadBody.gameObject);

            elapsed += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }

// Thanks to: https://github.com/yanpla/yanplaRoles/blob/master/Utils.cs#L55
    /// <summary>
    ///     Records a player's role in their role history.
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="role">The RoleBehaviour to save.</param>
    public static void SavePlayerRole(byte playerId, RoleBehaviour role)
    {
        if (!savedPlayerRoles.ContainsKey(playerId)) savedPlayerRoles[playerId] = new List<RoleBehaviour>();

        savedPlayerRoles[playerId].Add(role);
    }

// Thanks to: https://github.com/yanpla/yanplaRoles/blob/master/Utils.cs#L64
    /// <summary>
    ///     Retrieves the role history for a specific player.
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <returns>A list of RoleBehaviour representing the player's role history.</returns>
    public static List<RoleBehaviour> GetPlayerRolesHistory(byte playerId)
    {
        if (savedPlayerRoles.ContainsKey(playerId)) return savedPlayerRoles[playerId];

        return new List<RoleBehaviour>();
    }

    /// <summary>
    ///     Retrieves a random player from the game who meets a specified condition.
    /// </summary>
    /// <param name="match">A predicate to filter eligible players.</param>
    /// <returns>A random PlayerControl instance, or null if none are valid.</returns>
    public static PlayerControl GetRandomPlayer(Predicate<PlayerControl> match)
    {
        var players = PlayerControl.AllPlayerControls.ToArray().Where(p => match(p)).ToList();

        if (players.Count > 0) return players[Random.RandomRange(0, players.Count)];

        return null;
    }

    public static string GetModifierFactionDisplay(INewModModifier modifier)
    {
        return modifier.Faction switch
        {
            ModifierFaction.Crew => $"<b><color=#00B7C7>Crew</color></b>",
            ModifierFaction.Murder => $"<b><color=#FF4C4C>Murder</color></b>",
            _ => $"Unknown"
        };
    }

    /// <summary>
    ///     Checks if there is at least one dead player in the game.
    /// </summary>
    /// <returns>A PlayerControl who is dead, or null if none.</returns>
    public static PlayerControl AnyDeadPlayer()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.IsDead)
                return player;

        return null;
    }

    /// <summary>
    ///     Performs a random draining action on a target player as part of a custom RPC.
    /// </summary>
    /// <param name="source">The player who initiates the drain.</param>
    /// <param name="target">The player who is the target of the drain.</param>
    [MethodRpc((uint)CustomRPC.Drain)]
    public static void RpcRandomDrainActions(PlayerControl source, PlayerControl target)
    {
        List<Action> actions = new()
        {
            () =>
            {
                target.MyPhysics.Speed *= 0.5f;
                if (source.AmOwner)
                    HudManager.Instance.ShowPopUp($"<color=purple>{target.Data.PlayerName} speed was reduced by 50%!</color>");
            },
            () =>
            {
                if (target.AmOwner)
                {
                    HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.black, 0.5f));
                    target.NetTransform.Halt();
                }

                if (source.AmOwner)
                    HudManager.Instance.ShowPopUp($"<color=blue>Movement is disabled for {target.Data.PlayerName}, and their screen is black!</color>");
            },
            () =>
            {
                target.myTasks.Clear();
                if (source.AmOwner)
                    HudManager.Instance.ShowPopUp($"<color=green>{target.Data.PlayerName} had all of their tasks cleared!</color>");
            },
            () =>
            {
                target.RemainingEmergencies = 0;
                if (source.AmOwner)
                    HudManager.Instance.ShowPopUp($"<color=orange>{target.Data.PlayerName} can no longer call emergency meetings!</color>");
            },
            () =>
            {
                var randomPlayer = GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected);
                if (randomPlayer != null)
                {
                    target.NetTransform.RpcSnapTo(randomPlayer.GetTruePosition());
                    if (source.AmOwner)
                        HudManager.Instance.ShowPopUp($"<color=red>{target.Data.PlayerName} has been teleported!</color>");
                }
            }
        };
        var randomIndex = Random.Range(0, actions.Count);
        actions[randomIndex].Invoke();
    }

    /// <summary>
    /// Selects and processes a mission for the specified target player based on the provided MissionType.
    /// </summary>
    /// <param name="target">The target player receiving the mission.</param>
    /// <param name="mission">The type of mission assigned.</param>
    /// <returns>A formatted string describing the selected mission.</returns>
    public static string GetMission(PlayerControl specialAgent, PlayerControl target, MissionType mission, byte mostWantedId)
    {
        switch (mission)
        {
            case MissionType.KillMostWanted:
            {
                var mostWanted = PlayerById(mostWantedId);
                var arrowObject = new GameObject("SpecialAgent_MostWantedArrow") { layer = 5 };
                var renderer = arrowObject.AddComponent<SpriteRenderer>();
                renderer.sprite = NewModAsset.Arrow.LoadAsset();

                var arrow = arrowObject.AddComponent<ArrowBehaviour>();
                arrow.image = renderer;
                arrow.target = mostWanted.transform.position;

                Coroutines.Start(CoroutinesHelper.CoHandleWantedTarget(specialAgent, arrow, mostWanted, target));

                return $"Kill the Most Wanted Target: {mostWanted.Data.PlayerName}";
            }
            case MissionType.CreateFakeBodies:
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#32CD32><i><b>Press F5 to create two fake bodies</b></i></color>"));
                Coroutines.Start(CoroutinesHelper.UsePranksterAbilities(specialAgent, target));
                return "Create two fake dead bodies using Prankster abilities";
            }
            case MissionType.DrainEnergy:
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#00FA9A><i><b>Press F5 to drain two nearby players</b></i></color>"));
                Coroutines.Start(CoroutinesHelper.UseEnergyThiefAbilities(specialAgent, target));
                return "Drain two nearby players using Energy Thief abilities";
            }
            case MissionType.ReviveAndKill:
            {
                Coroutines.Start(CoroutinesHelper.CoReviveAndKill(specialAgent, target));
                return "Revive a dead player and kill them again";
            }
            default:
                return "Unknown mission";
        }
    }

    [MethodRpc((uint)CustomRPC.MissionSuccess, LocalHandling = RpcLocalHandling.After)]
    public static void RpcMissionSuccess(PlayerControl requester, PlayerControl specialAgent, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || requester != target || specialAgent.Data.Role is not SpecialAgent || SpecialAgent.AssignedPlayer != target)
            return;

        RpcApplyMissionResult(PlayerControl.LocalPlayer, specialAgent, target, true);
    }

    [MethodRpc((uint)CustomRPC.MissionFails, LocalHandling = RpcLocalHandling.After)]
    public static void RpcMissionFails(PlayerControl requester, PlayerControl specialAgent, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || requester != target || specialAgent.Data.Role is not SpecialAgent || SpecialAgent.AssignedPlayer != target)
            return;

        RpcApplyMissionResult(PlayerControl.LocalPlayer, specialAgent, target, false);
    }

    [MethodRpc((uint)CustomRPC.ApplyMissionResult, LocalHandling = RpcLocalHandling.After)]
    public static void RpcApplyMissionResult(PlayerControl source, PlayerControl specialAgent, PlayerControl target, bool succeeded)
    {
        if (succeeded)
            RecordMissionSuccess(specialAgent);
        else
            RecordMissionFailure(specialAgent);

        if (specialAgent.AmOwner)
        {
            var score = GetMissionSuccessCount(specialAgent.PlayerId) - GetMissionFailureCount(specialAgent.PlayerId);
            var color = succeeded ? "#FFD700" : "#FF0000";
            var outcome = succeeded ? "completed" : "failed";
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color={color}>Target {target.Data.PlayerName} {outcome} their mission!\nNet score: {score}/3</color>"));
        }

        if (target.AmOwner)
        {
            Coroutines.Start(CoroutinesHelper.CoNotify(succeeded ? "<color=#32CD32>Mission completed. You are free to go!</color>" : "<color=#FF0000>Mission failed. You will face the consequences!</color>"));

            if (savedTasks.TryGetValue(target, out var tasks))
            {
                target.myTasks = tasks;
                savedTasks.Remove(target);
            }

            if (target.Data.Role is ICustomRole role && RoleToButtonsMap.TryGetValue(role.GetType(), out var buttonTypes))
            {
                foreach (var buttonType in buttonTypes)
                {
                    var button = CustomButtonManager.Buttons.FirstOrDefault(candidate => candidate.GetType() == buttonType);
                    if (button != null && button.Button)
                        button.Button.SetEnabled();
                }
            }
        }

        if (!succeeded && AmongUsClient.Instance.AmHost && !target.Data.IsDead)
        {
            specialAgent.RpcCustomMurder(target, createDeadBody: false, didSucceed: true, showKillAnim: false, playKillSound: true, teleportMurderer: false);
        }

        if (SpecialAgent.AssignedPlayer == target)
            SpecialAgent.AssignedPlayer = null;
    }

    public static string GetFactionDisplay(INewModRole role)
    {
        return role.Faction switch
        {
            NewModFaction.Apex => $"<b><color=#FF5A5A>Apex</color></b>",
            NewModFaction.Entropy => $"<b><color=#EAAA3E>Entropy</color></b>",
            NewModFaction.Sentinel => $"<b><color=#3AA6FF>Sentinel</color></b>",
            _ => $"Unknown"
        };
    }

    /// <summary>
    /// Assigns a random mission to the target player as a custom RPC.
    /// </summary>
    /// <param name="source">The player initiating the assignment (Special Agent).</param>
    /// <param name="target">The player who will receive the mission.</param>
    [MethodRpc((uint)CustomRPC.AssignMission)]
    public static void RpcAssignMission(PlayerControl source, PlayerControl target, MissionType mission, byte mostWantedId)
    {
        SpecialAgent.AssignedPlayer = target;

        if (!target.AmOwner)
            return;

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

        // Add the mission message to the player's tasks
        ImportantTextTask missionMessage = new GameObject("MissionMessage").AddComponent<ImportantTextTask>();
        missionMessage.transform.SetParent(AmongUsClient.Instance.transform, false);
        missionMessage.Text = $"<color=red>Special Agent</color> has given you a mission!\n" + $"<b><color=blue>Mission:</color></b> {GetMission(source, target, mission, mostWantedId)}\n" + $"<i><color=green>Complete it or face the consequences!</color></i>";

        target.myTasks.Insert(0, missionMessage);
        // Disable the Role Player's Ability
        if (target.Data.Role is ICustomRole role)
        {
            if (RoleToButtonsMap.TryGetValue(role.GetType(), out var buttonTypes))
            {
                foreach (var btnType in buttonTypes)
                {
                    var btn = CustomButtonManager.Buttons.FirstOrDefault(b => b.GetType() == btnType);

                    if (btn != null && btn.Button)
                        btn.Button.SetDisabled();
                }
            }
        }

        Coroutines.Start(CoroutinesHelper.CoMissionTimer(source, target, 30f));
    }

    /// <summary>
    /// Captures a screenshot of the current game screen, hides the HUD, and then reactivates it.
    /// </summary>
    /// <param name="filePath">The path to save the screenshot file.</param>
    /// <returns>An IEnumerator for coroutine control.</returns>
    public static IEnumerator CaptureScreenshot(string filePath)
    {
        if (VisionaryUtilities.IsCapturing)
            yield break;

        VisionaryUtilities.IsCapturing = true;
        var clip = NewModAsset.VisionarySound.LoadAsset();

        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, false);
        SoundManager.Instance.PlaySound(clip, false, 1f, null);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(filePath, 1);
        VisionaryUtilities.HasScreenshots = true;
        NewMod.Instance.Log.LogInfo($"Capturing screenshot at {Path.GetFileName(filePath)}.");

        yield return new WaitForEndOfFrame();

        SoundManager.Instance.StopSound(clip);
        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, true);
        VisionaryUtilities.IsCapturing = false;
    }

    /// <summary>
    ///     Causes the player to feign death, creating a body. If unreported, the player is revived after 10 seconds.
    /// </summary>
    /// <param name="player">The player feigning death.</param>
    /// <returns>An IEnumerator for coroutine control.</returns>
    public static IEnumerator StartFeignDeath(PlayerControl player)
    {
        var clip = NewModAsset.FeignDeathSound.LoadAsset();

        SavePlayerRole(player.PlayerId, player.Data.Role);

        player.RpcCustomMurder(player, true, false, true, false, false, false);

        SoundManager.Instance.PlaySound(clip, false);

        yield return new WaitForSeconds(0.5f);

        var body = player.GetNearestDeadBody(15f);

        var info = new Revenant.FeignDeathInfo { Timer = 10f, DeadBody = body, Reported = false };
        Revenant.FeignDeathStates[player.PlayerId] = info;

        Coroutines.Start(CoroutinesHelper.CoNotify("<color=green>You are now feigning death.\nYou will be revived in 10 seconds if unreported.</color>"));

        if (player.AmOwner) HudManager.Instance.SetHudActive(player, player.Data.Role, false);

        var timer = 10f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            info.Timer = timer;
            yield return null;

            if (info.Reported)
            {
                yield return CoroutinesHelper.CoNotify("<color=red>Your feign death has been reported. You remain dead.</color>");
                SoundManager.Instance.StopSound(clip);
                Revenant.FeignDeathStates.Remove(player.PlayerId);
                yield break;
            }
        }

        Revenant.HasUsedFeignDeath = true;
        Revenant.StalkingStates[player.PlayerId] = true;

        var roleHistory = GetPlayerRolesHistory(player.PlayerId);
        var roleToRestore = roleHistory.Count > 0 ? roleHistory[^1].Role : (RoleTypes)RoleId.Get<Revenant>();

        HandleRevive(player, player.PlayerId, roleToRestore, body.transform.position.x, body.transform.position.y);
        yield return new WaitForSeconds(0.2f);
        player.RpcShapeshift(GetRandomPlayer(p => !p.Data.IsDead && !p.Data.Disconnected), false);
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=green>You have been revived in a new body!</color>"));
        Revenant.FeignDeathStates.Remove(player.PlayerId);

        if (player.AmOwner) HudManager.Instance.SetHudActive(player, player.Data.Role, true);

        SoundManager.Instance.StopSound(clip);
    }

    /// <summary>
    ///     Gradually fades out the provided ghost object and then destroys it.
    /// </summary>
    /// <param name="ghost">The GameObject representing the ghost.</param>
    /// <param name="fadeDuration">The duration of the fade effect.</param>
    /// <returns>An IEnumerator for coroutine control.</returns>
    public static IEnumerator FadeAndDestroy(GameObject ghost, float fadeDuration)
    {
        var ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        var alpha = 0.5f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / fadeDuration * 0.5f;
            if (ghostRenderer != null) ghostRenderer.color = new Color(1f, 0f, 0f, alpha);

            yield return null;
        }

        Object.Destroy(ghost);
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
                var boostPercent = OptionGroupSingleton<InjectorOptions>.Instance.AdrenalineSpeedBoost;
                var multiplier = 1f + boostPercent / 100f;
                var originalSpeed = target.MyPhysics.Speed;

                target.MyPhysics.Speed *= multiplier;

                Coroutines.Start(CoroutinesHelper.ResetSpeedAfterDelay(target, originalSpeed, 10f));
                break;
            }

            case SerumType.Paralysis:
            {
                var duration = OptionGroupSingleton<InjectorOptions>.Instance.ParalysisDuration;

                target.moveable = false;
                target.MyPhysics.inputHandler.enabled = false;

                Coroutines.Start(CoroutinesHelper.EnableMovementAfterDelay(target, duration));
                break;
            }
            case SerumType.BounceSerum:
            {
                var bounceDuration = OptionGroupSingleton<InjectorOptions>.Instance.BounceDuration;
                var h = OptionGroupSingleton<InjectorOptions>.Instance.BounceForceHorizontal;
                //float v = OptionGroupSingleton<InjectorOptions>.Instance.BounceForceVertical;
                var maxRotate = OptionGroupSingleton<InjectorOptions>.Instance.BounceRotateEffect.Value;

                //Vector2 force = new(Random.Range(-h, h), Random.Range(-v, v));

                //target.MyPhysics.body.AddForce(force);

                Effects.Bounce(target.transform, bounceDuration, h);

                if (OptionGroupSingleton<InjectorOptions>.Instance.EnableBounceVariants)
                {
                    if (Helpers.CheckChance(OptionGroupSingleton<InjectorOptions>.Instance.BounceRotateEffect)) target.transform.Rotate(0, 0, Random.Range(-maxRotate, maxRotate));

                    Coroutines.Start(CoroutinesHelper.ResetRotationAfterDelay(target, bounceDuration));
                }
            }
                break;
            case SerumType.RepelSerum:
            {
                var RepelDuration = OptionGroupSingleton<InjectorOptions>.Instance.RepelDuration;
                var RepelRange = OptionGroupSingleton<InjectorOptions>.Instance.RepelRange;
                var RepelForce = OptionGroupSingleton<InjectorOptions>.Instance.RepelForce;

                foreach (var other in PlayerControl.AllPlayerControls)
                {
                    if (other == target || other.Data.IsDead || other.Data.Disconnected) continue;

                    var dist = Vector2.Distance(other.GetTruePosition(), target.GetTruePosition());

                    if (dist < RepelRange)
                    {
                        var dir = (other.GetTruePosition() - target.GetTruePosition()).normalized;
                        other.MyPhysics.body.velocity += dir * RepelForce;
                    }
                }

                Coroutines.Start(CoroutinesHelper.ResetRepelEffect(target, RepelDuration));
            }
                break;
        }

        RegisterPlayerInjection(target);

        if (source.AmOwner)
            Helpers.CreateAndShowNotification($"Injected {target.Data.PlayerName} with {serumType}", new Color(0.9f, 0.3f, 0.1f), spr: NewModAsset.InjectIcon.LoadAsset());
    }

    /// <summary>
    ///     Tracks the camera on its current target for a given duration,
    ///     then restores its position to the original state.
    ///     Optionally applies a shake effect during the final moments.
    /// </summary>
    /// <param name="cam">The <see cref="FollowerCamera" /> instance to adjust.</param>
    /// <param name="duration">The total duration, in seconds, to keep tracking before resetting.</param>
    /// <returns>
    ///     An <see cref="IEnumerator" /> coroutine that handles timing and the optional shake effect.
    /// </returns>
    public static IEnumerator CoShakeCamera(FollowerCamera cam, float duration)
    {
        var timeElapsed = 0f;
        var originalPos = cam.transform.position;
        var shakeThreshold = 1.5f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            if (duration - timeElapsed <= shakeThreshold)
            {
                var shakeMagnitude = 0.3f;
                var shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                cam.transform.localPosition = originalPos + shakeOffset;
            }
            else
            {
                cam.transform.localPosition = originalPos;
            }

            yield return null;
        }

        cam.transform.localPosition = originalPos;
    }

    /// <summary>
    ///     Formats a <see cref="System.TimeSpan" /> into a string with the format:
    ///     <c>dd:hh:mm:ss</c>.
    /// </summary>
    /// <param name="t">The <see cref="System.TimeSpan" /> to format.</param>
    public static string FormatSpan(TimeSpan t)
    {
        var dd = Mathf.Max(0, t.Days);
        var hh = Mathf.Clamp(t.Hours, 0, 99);
        var mm = Mathf.Clamp(t.Minutes, 0, 59);
        var ss = Mathf.Clamp(t.Seconds, 0, 59);
        return $"{dd:D1}:{hh:D2}:{mm:D2}:{ss:D2}";
    }

    /// <summary>
    ///     Finds the surveillance console on the current ship.
    /// </summary>
    /// <returns>
    ///     The first <see cref="SystemConsole" /> instance representing the surveillance console,
    /// </returns>
    public static SystemConsole FindSurveillanceConsole()
    {
        var all = ShipStatus.Instance?.AllConsoles;
        var sys = all.OfType<SystemConsole>().FirstOrDefault(c => c && c.MinigamePrefab && c.MinigamePrefab is SurveillanceMinigame);

        return all.OfType<SystemConsole>().FirstOrDefault(c =>
        {
            var n = c.name;
            return n.Contains("Surv", StringComparison.OrdinalIgnoreCase) || n.Contains("Lookout", StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    ///     Retrieves or creates a material used for drawing circles.
    /// </summary>
    /// <returns>
    ///     A <see cref="Material" /> instance with the "Sprites/Default" shader
    /// </returns>
    public static Material GetCircleMat()
    {
        if (_circleMat) return _circleMat;
        _circleMat = new Material(Shader.Find("Sprites/Default")) { renderQueue = 3000 };
        return _circleMat;
    }

    /// <summary>
    ///     Creates a filled circle mesh in the scene at a given position.
    /// </summary>
    /// <param name="name">The name of the created GameObject.</param>
    /// <param name="pos">The position where the circle will be created.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color to apply to the circle material.</param>
    /// <param name="duration">How long the circle should remain before being despawned.</param>
    /// <param name="segments">Number of segments for the circle geometry. Minimum of 12.</param>
    /// <returns>
    ///     The created <see cref="GameObject" /> representing the circle.
    /// </returns>
    public static GameObject CreateCircle(string name, Vector3 pos, float radius, Color color, float duration, int segments = 64)
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        HudManager.Instance.StartCoroutine(Effects.ScaleIn(go.transform, 0f, 1f, 0.5f));

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();

        var mat = new Material(GetCircleMat()) { color = color };
        mr.sharedMaterial = mat;

        var visualRadius = radius;

        segments = Mathf.Max(12, segments);
        var verts = new Vector3[segments + 1];
        var tris = new int[segments * 3];

        verts[0] = Vector3.zero;
        for (var i = 0; i < segments; i++)
        {
            var a = i / (float)segments * Mathf.PI * 2f;
            verts[i + 1] = new Vector3(Mathf.Cos(a) * visualRadius, Mathf.Sin(a) * visualRadius, 0f);
            tris[i * 3 + 0] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i == segments - 1 ? 1 : i + 2;
        }

        var mesh = new Mesh { name = $"{name}_Fill" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0, true);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        Coroutines.Start(CoroutinesHelper.DespawnCircle(go, duration));
        return go;
    }

    public static bool IsRoleActive(string roleName)
    {
        foreach (var roles in RoleManager.Instance.AllRoles)
        {
            CustomRoleManager.GetCustomRoleBehaviour(roles.Role, out var customRole);

            if (customRole != null && customRole.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                return customRole.GetChance() > 0 && customRole.GetCount() > 0;
        }

        return false;
    }
}