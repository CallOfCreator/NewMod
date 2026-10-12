using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using NewMod.Buttons.Roles;
using NewMod.Buttons.Roles.S1;
using NewMod.Components;
using NewMod.GeneralEvents.Season1;
using NewMod.Modifiers;
using NewMod.Modifiers.S1;
using NewMod.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NewMod.Utilities;

/// <summary>
/// Various utilities.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Killer IDs by victim ID.
    /// </summary>
    public static Dictionary<byte, byte> PlayerKiller = [];

    /// <summary>
    /// Role history by player ID.
    /// </summary>
    public static Dictionary<byte, List<RoleBehaviour>> savedPlayerRoles = [];

    public static Material _circleMat;

    /// <summary>
    /// Finds a player by ID.
    /// </summary>
    /// <param name="id">The player's ID.</param>
    /// <returns>The player, or null if not found.</returns>
    // Thanks to: https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/Utils.cs#L219
    public static PlayerControl PlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id)
                return player;
        }

        return null;
    }

    /// <summary>
    /// Ability button types for each role.
    /// </summary>
    public static readonly Dictionary<Type, List<Type>> RoleToButtonsMap = new()
    {
        { typeof(EnergyThief), new List<Type> { typeof(DrainButton) } },
        { typeof(NecromancerRole), new List<Type> { typeof(ReviveButton) } },
        { typeof(Prankster), new List<Type> { typeof(FakeBodyButton) } },
        { typeof(Revenant), new List<Type> { typeof(FeignDeathButton), typeof(DoomAwakening) } },
        { typeof(TheVisionary), new List<Type> { typeof(CaptureButton), typeof(ShowScreenshotButton) } },
        { typeof(PulseBlade), new List<Type> { typeof(StrikeButton) } },
        { typeof(WraithCaller), new List<Type> { typeof(CallWraithButton), typeof(CollectWraithTraceButton) } },
        { typeof(Edgeveil), new List<Type> { typeof(ArcButton) } },
        { typeof(Aegis), new List<Type> { typeof(AegisButton) } },
        { typeof(WardenRole), new List<Type> { typeof(WardenSealButton), typeof(InspectResidualButton), typeof(WardenClueButton) } },
        { typeof(Voidwalker), new List<Type> { typeof(EnterVoid) } },
        { typeof(TerminatorRole), new List<Type> { typeof(ObjectiveButton) } },
        { typeof(ArbitratorRole), new List<Type> { typeof(ArbitratorLeverageButton) } },
        { typeof(MirrorBladeRole), new List<Type> { typeof(MirrorReflectButton) } },
        { typeof(Shade), new List<Type> { typeof(DeployShadow) } },
        // Verifier is excluded since it uses a meeting ability.
    };

    /// <summary>
    /// Records who killed the victim.
    /// </summary>
    /// <param name="killer">The killer.</param>
    /// <param name="victim">The victim.</param>
    public static void RecordOnKill(PlayerControl killer, PlayerControl victim)
    {
        PlayerKiller[victim.PlayerId] = killer.PlayerId;
    }

    /// <summary>
    /// Gets the victim's killer.
    /// </summary>
    /// <param name="victim">The victim.</param>
    /// <returns>The killer, or null if not found.</returns>
    public static PlayerControl GetKiller(PlayerControl victim)
    {
        return PlayerKiller.TryGetValue(victim.PlayerId, out var killerId) ? PlayerById(killerId) : null;
    }

    public static void ResetKillTracking()
    {
        PlayerKiller.Clear();
    }

    // Thanks to: https://github.com/Rabek009/MoreGamemodes/blob/master/Modules/Utils.cs#L66

    /// <summary>
    /// Checks whether a sabotage system is active.
    /// </summary>
    /// <param name="type">The system to check.</param>
    /// <returns>True if the system is active.</returns>
    public static bool IsActive(SystemTypes type)
    {
        int mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;

        if (!ShipStatus.Instance.Systems.TryGetValue(type, out var system)) return false;

        switch (type)
        {
            case SystemTypes.Electrical:
                if (mapId == 5) return false;
                var switchSystem = system.TryCast<SwitchSystem>();
                return switchSystem != null && switchSystem.IsActive;
            case SystemTypes.Reactor:
                if (mapId == 2) return false;
                var reactorSystemType = system.TryCast<ReactorSystemType>();
                return reactorSystemType != null && reactorSystemType.IsActive;
            case SystemTypes.Laboratory:
                if (mapId != 2) return false;
                var reactorSystemType2 = system.TryCast<ReactorSystemType>();
                return reactorSystemType2 != null && reactorSystemType2.IsActive;
            case SystemTypes.LifeSupp:
                if (mapId is 2 or 4 or 5) return false;
                var lifeSuppSystemType = system.TryCast<LifeSuppSystemType>();
                return lifeSuppSystemType != null && lifeSuppSystemType.IsActive;
            case SystemTypes.HeliSabotage:
                if (mapId != 4) return false;
                var heliSabotageSystem = system.TryCast<HeliSabotageSystem>();
                return heliSabotageSystem != null && heliSabotageSystem.IsActive;
            case SystemTypes.Comms:
                if (mapId is 1 or 5)
                {
                    var hqHudSystemType = system.TryCast<HqHudSystemType>();
                    return hqHudSystemType != null && hqHudSystemType.IsActive;
                }

                var hudOverrideSystemType = system.TryCast<HudOverrideSystemType>();
                return hudOverrideSystemType != null && hudOverrideSystemType.IsActive;
            case SystemTypes.MushroomMixupSabotage:
                if (mapId != 5) return false;
                var mushroomMixupSabotageSystem = system.TryCast<MushroomMixupSabotageSystem>();
                return mushroomMixupSabotageSystem != null && mushroomMixupSabotageSystem.IsActive;
            default:
                return false;
        }
    }

    // Thanks to : https://github.com/Rabek009/MoreGamemodes/blob/master/Modules/Utils.cs#L118

    /// <summary>
    /// Checks whether any sabotage is active.
    /// </summary>
    /// <returns>True if a sabotage is active.</returns>
    public static bool IsSabotage()
    {
        return IsActive(SystemTypes.LifeSupp) || IsActive(SystemTypes.Reactor) || IsActive(SystemTypes.Laboratory) || IsActive(SystemTypes.Electrical) || IsActive(SystemTypes.Comms) || IsActive(SystemTypes.MushroomMixupSabotage) || IsActive(SystemTypes.HeliSabotage);
    }

    // Inspired By: https://github.com/AU-Avengers/TOU-Mira/blob/dev/TownOfUs/Modules/ReviveUtilities.cs#L40
    [MethodRpc((uint)CustomRPC.HandleRevive)]
    public static IEnumerator HandleRevive(PlayerControl source, byte revivedId, RoleTypes roleToSet, float reviveX, float reviveY)
    {
        var necromancer = AbilityExchangeGE.HasRoleAbility<NecromancerRole>(source);
        var revived = PlayerById(revivedId);

        if (!revived || revived.Data.Disconnected)
            yield break;

        yield return new WaitForSeconds(0.15f);

        if (!revived || revived.Data.Disconnected || !revived.Data.IsDead)
            yield break;

        var revivePos = new Vector2(reviveX, reviveY);
        var inMeetingOrExile = MeetingHud.Instance || ExileController.Instance;

        var noisemaker = revived.Data.Role.TryCast<NoisemakerRole>();
        if (noisemaker && noisemaker.deathArrow)
            Object.Destroy(noisemaker.deathArrow.gameObject);

        if (necromancer)
        {
            OverclockedModifier.Pulse(source);
            NecromancerRole.RevivedPlayers[revivedId] = source.PlayerId;
        }

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
        {
            if (deadBody.ParentId == revived.PlayerId)
                Object.Destroy(deadBody.gameObject);
        }

        var elapsed = 0f;
        while (elapsed < 1f)
        {
            foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == revived.PlayerId)
                    Object.Destroy(deadBody.gameObject);
            }

            elapsed += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Thanks to: https://github.com/yanpla/yanplaRoles/blob/master/Utils.cs#L55

    /// <summary>
    /// Adds a role to the player's history.
    /// </summary>
    /// <param name="playerId">The player's ID.</param>
    /// <param name="role">The role to save.</param>
    public static void SavePlayerRole(byte playerId, RoleBehaviour role)
    {
        if (!savedPlayerRoles.TryGetValue(playerId, out var roles))
            savedPlayerRoles[playerId] = roles = [];

        roles.Add(role);
    }

    // Thanks to: https://github.com/yanpla/yanplaRoles/blob/master/Utils.cs#L64

    /// <summary>
    /// Gets the player's role history.
    /// </summary>
    /// <param name="playerId">The player's ID.</param>
    /// <returns>Saved roles, or an empty list if none are recorded.</returns>
    public static List<RoleBehaviour> GetPlayerRolesHistory(byte playerId)
    {
        return savedPlayerRoles.TryGetValue(playerId, out var roles) ? roles : [];
    }

    /// <summary>
    /// Picks a random player that matches the filter.
    /// </summary>
    /// <param name="match">The player filter.</param>
    /// <returns>A matching player, or null if none match.</returns>
    public static PlayerControl GetRandomPlayer(Predicate<PlayerControl> match)
    {
        var players = new List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (match(player))
                players.Add(player);
        }

        return players.Random();
    }

    public static string GetModifierFactionDisplay(INewModModifier modifier)
    {
        return modifier.Faction switch
        {
            ModifierFaction.Crew => $"<b><color=#00B7C7>Crew</color></b>",
            ModifierFaction.Murder => $"<b><color=#FF4C4C>Murder</color></b>",
            _ => MiraLocaleManager.Get("NewMod.Faction.Unknown"),
        };
    }

    public static string GetFactionDisplay(INewModRole role)
    {
        return role.Faction switch
        {
            NewModFaction.Apex => $"<b><color=#FF5A5A>{MiraLocaleManager.Get("NewMod.Faction.Apex")}</color></b>",
            NewModFaction.Entropy => $"<b><color=#EAAA3E>{MiraLocaleManager.Get("NewMod.Faction.Entropy")}</color></b>",
            NewModFaction.Sentinel => $"<b><color=#3AA6FF>{MiraLocaleManager.Get("NewMod.Faction.Sentinel")}</color></b>",
            NewModFaction.Rift => $"<b><color=#301934>{MiraLocaleManager.Get("NewMod.Faction.Rift")}</color></b>",
            _ => MiraLocaleManager.Get("NewMod.Faction.Unknown"),
        };
    }

    /// <summary>
    /// Fades out the ghost, then destroys it.
    /// </summary>
    /// <param name="ghost">The ghost to fade out.</param>
    /// <param name="fadeDuration">Fade time in seconds.</param>
    /// <returns>The fade coroutine.</returns>
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

    /// <summary>
    /// Shakes the camera during the last 1.5 seconds, then restores its position.
    /// </summary>
    /// <param name="cam">The camera to shake.</param>
    /// <param name="duration">Total time in seconds, including the delay before shaking.</param>
    /// <returns>The camera shake coroutine.</returns>
    public static IEnumerator CoShakeCamera(FollowerCamera cam, float duration)
    {
        var timeElapsed = 0f;
        var originalPos = cam.transform.localPosition;
        var shakeThreshold = 1.5f;

        while (cam && timeElapsed < duration)
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

        if (cam)
            cam.transform.localPosition = originalPos;
    }

    public static string FormatSpan(TimeSpan t)
    {
        var dd = Mathf.Max(0, t.Days);
        var hh = Mathf.Clamp(t.Hours, 0, 99);
        var mm = Mathf.Clamp(t.Minutes, 0, 59);
        var ss = Mathf.Clamp(t.Seconds, 0, 59);
        return $"{dd:D1}:{hh:D2}:{mm:D2}:{ss:D2}";
    }

    /// <summary>
    /// Finds the surveillance console on the current ship.
    /// </summary>
    /// <returns>The surveillance console, or null if the map has none.</returns>
    public static SystemConsole FindSurveillanceConsole()
    {
        if (!ShipStatus.Instance)
            return null;

        return ShipStatus.Instance.GetComponentsInChildren<SystemConsole>().FirstOrDefault(console =>
            console.MinigamePrefab && (console.MinigamePrefab.TryCast<SurveillanceMinigame>() ||
                                      console.MinigamePrefab.TryCast<PlanetSurveillanceMinigame>() ||
                                      console.MinigamePrefab.TryCast<FungleSurveillanceMinigame>()));
    }

    /// <summary>
    /// Gets the shared circle material, creating it if needed.
    /// </summary>
    /// <returns>The circle material using the "Sprites/Default" shader.</returns>
    public static Material GetCircleMat()
    {
        if (_circleMat) return _circleMat;
        _circleMat = new Material(Shader.Find("Sprites/Default")) { renderQueue = 3000 };
        return _circleMat;
    }

    public static GameObject CreateSphere(string name, Vector3 position, float radius, Color color, float duration, bool filled = false)
    {
        var sphere = new GameObject(name);
        sphere.transform.position = position;
        sphere.transform.SetParent(ShipStatus.Instance.transform, true);
        var bubble = sphere.AddComponent<AreaBubble>();
        bubble.Init(radius, color, filled ? 0.25f : 0.04f);
        bubble.expiresAt = Time.time + duration;
        return sphere;
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
