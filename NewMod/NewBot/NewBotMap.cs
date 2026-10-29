using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using PathfindingAPI.Core;
using PathfindingAPI.Navigation;
using PathfindingAPI.Options;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.NewBot;

public sealed class NewBotMap(NewBot bot)
{
    public ShipStatus Ship;
    public Vector2 Center;
    public PlainShipRoom Room;
    public readonly List<Vector2> Positions = [];
    public readonly List<Vector2> RoamingPositions = [];
    public readonly List<Vector2> EscapePositions = [];
    public readonly List<PlayerControl> Dummies = [];
    public readonly Dictionary<IEnumerator, IEnumerator> Movements = [];
    public SpriteRenderer Transition;
    public PathOptions WalkingOptions { get; } = new()
    {
        UseVents = false,
        UseLadders = false,
        UseZiplines = false,
        UseMovingPlatforms = false,
        UseDecontamination = false,
        UseElevators = false,
    };

    public IEnumerator CoFindArena(int requiredPositions)
    {
        Positions.Clear();
        RoamingPositions.Clear();
        EscapePositions.Clear();
        var probe = MapPathfinding.CreateRequest(bot.OriginalPosition, bot.OriginalPosition, WalkingOptions);
        var examined = 0;
        foreach (var room in Ship.AllRooms.OrderBy(room => room == Room || room.RoomId == SystemTypes.Cafeteria).ThenBy(_ => Random.value))
        {
            if (!room.roomArea || room.RoomId == SystemTypes.Hallway)
                continue;
            var bounds = room.roomArea.bounds;
            var candidates = new Dictionary<Vector2Int, Vector2>();
            for (var x = 0; x * 0.5f < bounds.size.x; x++)
            {
                for (var y = 0; y * 0.5f < bounds.size.y; y++)
                {
                    var point = new Vector2(bounds.min.x + 0.25f + x * 0.5f, bounds.min.y + 0.25f + y * 0.5f);
                    if (room.roomArea.OverlapPoint(point) && probe.IsClear(point, WalkingOptions.Radius + 0.2f))
                        candidates.Add(new Vector2Int(x, y), point);
                    if (++examined % 64 == 0)
                        yield return null;
                }
            }

            while (candidates.Count >= Mathf.Max(4, requiredPositions))
            {
                var start = candidates.OrderBy(pair => Vector2.SqrMagnitude(pair.Value - (Vector2)bounds.center)).First();
                var connected = new List<Vector2> { start.Value };
                var pending = new Queue<KeyValuePair<Vector2Int, Vector2>>();
                pending.Enqueue(start);
                candidates.Remove(start.Key);
                while (pending.TryDequeue(out var current))
                {
                    for (var x = -1; x <= 1; x++)
                    {
                        for (var y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0)
                                continue;
                            var key = current.Key + new Vector2Int(x, y);
                            if (!candidates.TryGetValue(key, out var point) || !probe.CanMove(current.Value, point))
                                continue;
                            candidates.Remove(key);
                            connected.Add(point);
                            pending.Enqueue(new KeyValuePair<Vector2Int, Vector2>(key, point));
                        }
                    }

                    if (++examined % 32 == 0)
                        yield return null;
                }

                Positions.Clear();
                foreach (var position in connected.OrderBy(point => Vector2.SqrMagnitude(point - start.Value)))
                {
                    if (Vector2.Distance(position, start.Value) < 1.5f || Positions.Any(other => Vector2.Distance(position, other) < 1.1f))
                        continue;
                    Positions.Add(position);
                    if (Positions.Count >= Mathf.Max(4, requiredPositions))
                        break;
                }

                if (Positions.Count < Mathf.Max(4, requiredPositions))
                    continue;
                var access = MapPathfinding.CreateRequest(bot.OriginalPosition, start.Value, WalkingOptions);
                yield return access.Run();
                if (access.Status != PathStatus.Found)
                    continue;
                Center = start.Value;
                Room = room;
                foreach (var position in connected.OrderBy(_ => Random.value))
                {
                    if (!RoamingPositions.Any(other => Vector2.Distance(position, other) < 1.5f))
                        RoamingPositions.Add(position);
                }

                yield break;
            }
        }

        Positions.Clear();
    }

    public IEnumerator CoFindEscapeRoutes()
    {
        var probe = MapPathfinding.CreateRequest(Center, Center, WalkingOptions);
        var rooms = Ship.AllRooms.Where(room => room != Room && room.roomArea && room.RoomId != SystemTypes.Hallway)
            .OrderBy(room => Vector2.SqrMagnitude((Vector2)room.roomArea.bounds.center - Center)).Take(6).ToArray();
        for (var index = 0; index < rooms.Length && EscapePositions.Count < 3; index++)
        {
            var room = rooms[index];
            DestroyableSingleton<LoadingBarManager>.Instance.SetLoadingPercent(50f + 40f * index / rooms.Length, StringNames.None);
            var bounds = room.roomArea.bounds;
            var candidates = new List<Vector2>();
            for (var x = bounds.min.x + 0.5f; x < bounds.max.x; x += 1f)
            {
                for (var y = bounds.min.y + 0.5f; y < bounds.max.y; y += 1f)
                {
                    var point = new Vector2(x, y);
                    if (room.roomArea.OverlapPoint(point) && probe.IsClear(point, WalkingOptions.Radius + 0.2f))
                        candidates.Add(point);
                }
            }

            foreach (var point in candidates.OrderBy(point => Vector2.SqrMagnitude(point - (Vector2)bounds.center)).Take(2))
            {
                var path = MapPathfinding.CreateRequest(Center, point, WalkingOptions);
                while (path.Status == PathStatus.Searching && path.ExpandedNodes < 2048)
                {
                    path.Step();
                    yield return null;
                }

                if (path.Status == PathStatus.Searching)
                    path.Cancel();
                if (path.Status != PathStatus.Found)
                    continue;
                EscapePositions.Add(point);
                break;
            }

            yield return null;
        }
    }

    public IEnumerator CoPrepare(RoleBehaviour role, int dummyCount, bool chooseRoom = false)
    {
        var loading = bot.Loading = DestroyableSingleton<LoadingBarManager>.Instance;
        loading.ToggleLoadingBar(true);
        loading.SetLoadingPercent(0f, StringNames.None);
        bot.Player.moveable = false;
        bot.Player.MyPhysics.body.velocity = Vector2.zero;
        try
        {
            if (chooseRoom)
                yield return CoFindArena(role is Collector ? dummyCount + 3 : dummyCount);
            if (Positions.Count == 0)
                yield break;
            loading.SetLoadingPercent(50f, StringNames.None);
            if (bot.Level == 4 && role is Revenant or Bounty)
            {
                EscapePositions.Clear();
                yield return CoFindEscapeRoutes();
            }

            bot.Practicing = true;
            Transition = Object.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.transform);
            Transition.gameObject.SetActive(true);
            Transition.transform.localPosition = new Vector3(0f, 0f, -80f);
            yield return CoFade(0f, 1f);
            ClearDummies();
            bot.Player.Revive();
            bot.Player.RpcSetRole(RoleTypes.Crewmate);
            ResetPractice();
            bot.Player.moveable = false;
            yield return null;
            bot.Player.NetTransform.RpcSnapTo(Center - bot.Player.Collider.offset);
            for (var i = 0; i < dummyCount; i++)
                SpawnDummy($"Practice {i + 1}", Positions[i]);
            bot.Player.RpcSetRole(role.Role);
            bot.Player.killTimer = 0f;
            foreach (var button in CustomButtonManager.Buttons)
            {
                button.ResetCooldownAndOrEffect();
                button.SetTimer(0f);
                if (button.LimitedUses)
                    button.SetUses(button.MaxUses);
            }

            bot.Player.moveable = false;
            loading.SetLoadingPercent(100f, StringNames.None);
            yield return CoFade(1f, 0f);
        }
        finally
        {
            loading.ToggleLoadingBar(false);
            bot.Loading = null;
            if (Transition)
                Object.Destroy(Transition.gameObject);
            Transition = null;
            if (bot.Player)
                bot.Player.moveable = true;
        }

        if (!bot.Objective)
            bot.Objective = Helpers.CreateTextLabel("NewbotObjective", HudManager.Instance.transform, AspectPosition.EdgeAlignments.Top, new Vector3(0f, 1.15f, -40f), 1.5f);
        bot.Objective.rectTransform.sizeDelta = new Vector2(5.5f, 0.7f);
    }

    public PlayerControl SpawnDummy(string label, Vector2 position)
    {
        var dummy = Object.Instantiate(TutorialManager.Instance.PlayerPrefab);
        dummy.PlayerId = (byte)GameData.Instance.GetAvailableId();
        var info = GameData.Instance.AddDummy(dummy);
        AmongUsClient.Instance.Spawn(info, -2, SpawnFlags.None);
        AmongUsClient.Instance.Spawn(dummy, -2, SpawnFlags.None);
        dummy.isDummy = true;
        var dummyBehaviour = dummy.GetComponent<DummyBehaviour>();
        dummyBehaviour.enabled = true;
        dummyBehaviour.PlayerIdToVoteFor = 253;
        dummy.NetTransform.enabled = false;
        dummy.NetTransform.SnapTo(position - dummy.Collider.offset);
        dummy.SetName(label);
        var color = (bot.Player.Data.DefaultOutfit.ColorId + Dummies.Count + 1) % Palette.PlayerColors.Length;
        dummy.SetColor(color);
        dummy.SetHat(string.Empty, color);
        dummy.SetSkin(string.Empty, color);
        dummy.SetPet(string.Empty);
        dummy.SetVisor(string.Empty, color);
        dummy.SetNamePlate(string.Empty);
        dummy.SetLevel(0);
        info.RpcSetTasks(System.Array.Empty<byte>());
        Dummies.Add(dummy);
        return dummy;
    }

    public IEnumerator CoFade(float from, float to)
    {
        for (var elapsed = 0f; elapsed < 0.25f; elapsed += Time.unscaledDeltaTime)
        {
            Transition.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, elapsed / 0.25f));
            yield return null;
        }

        Transition.color = new Color(0f, 0f, 0f, to);
    }

    public IEnumerator CoWalk(PlayerControl dummy, Vector2 destination, float speed = 0f, float replanAfter = float.PositiveInfinity)
    {
        if (!dummy || dummy.Data.IsDead)
            yield break;

        var request = MapPathfinding.CreateRequest(dummy, destination, WalkingOptions);
        yield return request.Run();
        if (!dummy || dummy.Data.IsDead || request.Status != PathStatus.Found)
            yield break;
        var elapsed = 0f;
        var tick = new WaitForFixedUpdate();
        if (speed <= 0f)
            speed = 1f + (bot.Level - 1) * 0.4f;
        foreach (var point in request.Result.Points)
        {
            var remaining = 10f;
            while (dummy && !dummy.Data.IsDead && Vector2.Distance(dummy.GetTruePosition(), point) > 0.08f && remaining > 0f)
            {
                if (bot.IsPaused || !dummy.moveable)
                {
                    dummy.MyPhysics.body.velocity = Vector2.zero;
                    yield return null;
                    continue;
                }

                if (elapsed >= replanAfter)
                {
                    dummy.MyPhysics.body.velocity = Vector2.zero;
                    yield break;
                }

                var position = dummy.GetTruePosition();
                var movementSpeed = speed;
                foreach (var experiment in InjectorUtilities.Experiments.Values)
                {
                    if (experiment.TargetId != dummy.PlayerId || Time.time >= experiment.ReadyAt)
                        continue;
                    var options = OptionGroupSingleton<InjectorOptions>.Instance;
                    var change = experiment.Serum == InjectorRole.SerumType.Adrenaline ? options.AdrenalineSpeedBoost : -options.SedativeSlowdown;
                    movementSpeed *= 1f + change / 100f;
                    break;
                }

                var next = Vector2.MoveTowards(position, point, movementSpeed * Time.fixedDeltaTime);
                if (!request.CanMove(position, next))
                    break;
                dummy.MyPhysics.body.velocity = (next - position) / Time.fixedDeltaTime;
                remaining -= Time.fixedDeltaTime;
                elapsed += Time.fixedDeltaTime;
                yield return tick;
            }

            if (!dummy || dummy.Data.IsDead)
                yield break;
            dummy.MyPhysics.body.velocity = Vector2.zero;
            if (Vector2.Distance(dummy.GetTruePosition(), point) > 0.1f)
                yield break;
        }
    }

    public IEnumerator CoRoam(PlayerControl dummy, int offset)
    {
        var index = offset % RoamingPositions.Count;
        var pause = new WaitForSeconds(1f / bot.Level);
        while (bot.LessonRunning && dummy && !dummy.Data.IsDead)
        {
            yield return CoWalk(dummy, RoamingPositions[index]);
            index = (index + 1) % RoamingPositions.Count;
            yield return pause;
        }
    }

    public void StartRoaming()
    {
        for (var i = 0; i < Dummies.Count; i++)
        {
            if (!Dummies[i].Data.IsDead)
                StartMovement(CoRoam(Dummies[i], i + 1));
        }
    }

    public void StartMovement(IEnumerator action)
    {
        var movement = CoMovement(action);
        Movements.Add(action, movement);
        Coroutines.Start(movement);
    }

    public IEnumerator CoMovement(IEnumerator action)
    {
        yield return null;
        yield return action;
        Movements.Remove(action);
    }

    public void StopMovements()
    {
        var movements = Movements.Values.ToArray();
        Movements.Clear();
        foreach (var movement in movements)
            Coroutines.Stop(movement);
        foreach (var dummy in Dummies)
        {
            if (dummy)
                dummy.MyPhysics.body.velocity = Vector2.zero;
        }
    }

    public void ResetPractice()
    {
        bot.Player.RpcRemoveModifier<InVoid>();
        foreach (var body in Object.FindObjectsOfType<DeadBody>())
            Object.Destroy(body.gameObject);
        if (DoubleAgent.CounterfeitActive)
            DoubleAgent.RpcFinishCounterfeit(bot.Player);
        WardenRole.ResetState();
        Deadwire.ResetState();
        Usurper.Reset();
        ArbitratorRole.ResetState();
        VisionaryUtilities.DeleteAllScreenshots();
        foreach (var fragment in Collector.Fragments.Values)
            Object.Destroy(fragment.Object);
        Collector.Fragments.Clear();
        Collector.VictoryArmed.Clear();
        Collector.EnvironmentalDeaths.Clear();
        foreach (var zone in ShadowZone.zones.ToArray())
        {
            if (zone)
                Object.Destroy(zone.gameObject);
        }

        foreach (var button in CustomButtonManager.Buttons)
            button.ResetCooldownAndOrEffect();
        NewModEventHandler.ResetMatchState();
        bot.Player.Visible = true;
        bot.Player.moveable = true;
        bot.Player.Collider.enabled = true;
        bot.Player.MyPhysics.inputHandler.enabled = true;
        bot.Player.MyPhysics.body.velocity = Vector2.zero;
    }

    public void ClearDummies()
    {
        StopMovements();
        foreach (var dummy in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!dummy.isDummy)
                continue;
            var info = dummy.Data;
            foreach (var netObject in dummy.GetComponentsInChildren<InnerNetObject>())
            {
                if (netObject.NetId != 0)
                    AmongUsClient.Instance.Despawn(netObject);
            }

            AmongUsClient.Instance.Despawn(info);
            GameData.Instance.RemovePlayer(dummy.PlayerId);
            Object.Destroy(dummy.gameObject);
        }

        Dummies.Clear();
    }
}
