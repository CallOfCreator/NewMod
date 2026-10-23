using System.Collections;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using NewMod.Buttons.Roles;
using NewMod.Buttons.Roles.S1;
using NewMod.Components;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using PathfindingAPI.Core;
using PathfindingAPI.Navigation;
using UnityEngine;

namespace NewMod.NewBot;

public sealed class NewBotChallenges(NewBot bot)
{
    public IEnumerator CoReflectionWindows(PlayerControl dummy)
    {
        while (bot.LessonRunning && dummy && !dummy.Data.IsDead)
        {
            dummy.SetName("Reflecting");
            MirrorBladeRole.ArmedReflections.Add(dummy.PlayerId);
            yield return new WaitForSeconds(3f);
            MirrorBladeRole.ArmedReflections.Remove(dummy.PlayerId);
            dummy.SetName("Open now");
            yield return new WaitForSeconds(bot.Level == 4 ? 1.5f : 4.5f - bot.Level * 0.5f);
        }
    }

    public IEnumerator CoPursue(PlayerControl dummy)
    {
        while (bot.LessonRunning && dummy && !dummy.Data.IsDead && !bot.Player.Data.IsDead)
        {
            if (bot.IsPaused)
            {
                dummy.MyPhysics.body.velocity = Vector2.zero;
                yield return null;
                continue;
            }

            yield return bot.Map.CoWalk(dummy, bot.Player.GetTruePosition(), bot.Player.MyPhysics.TrueSpeed * (bot.Level == 4 ? 1.05f : 0.8f), 0.3f);
            if (!bot.Player.HasModifier<InVoid>() && Vector2.Distance(dummy.GetTruePosition(), bot.Player.GetTruePosition()) < 0.7f)
                dummy.RpcCustomMurder(bot.Player, true, false, teleportMurderer: false);
            yield return null;
        }
    }

    public IEnumerator CoHuntTerminator(PlayerControl dummy)
    {
        yield return new WaitForSeconds(bot.Level == 4 ? 1f : 2.5f);
        var speedMultiplier = bot.Level switch
        {
            4 => 1.1f,
            3 => 0.95f,
            _ => 0.75f,
        };
        var probe = MapPathfinding.CreateRequest(dummy, bot.Player.GetTruePosition(), bot.Map.WalkingOptions);
        MapPathRequest search = null;
        Vector2[] points = [];
        var waypoint = 0;
        var nextSearch = 0f;
        var tick = new WaitForFixedUpdate();
        while (bot.LessonRunning && TerminatorRole.FinalCountdownActive && dummy && !dummy.Data.IsDead && !TerminatorRole.ArmorAttackers.Contains(dummy.PlayerId))
        {
            if (bot.IsPaused || !dummy.moveable)
            {
                dummy.MyPhysics.body.velocity = Vector2.zero;
                yield return tick;
                continue;
            }

            var position = dummy.GetTruePosition();
            var target = bot.Player.GetTruePosition();
            if (Vector2.Distance(position, target) <= OptionGroupSingleton<TerminatorOptions>.Instance.CounterAttackRange)
                TerminatorRole.RpcRequestTerminate(dummy);
            if (TerminatorRole.ArmorAttackers.Contains(dummy.PlayerId))
                break;

            var destination = position;
            if (probe.CanMove(position, target))
            {
                search?.Cancel();
                search = null;
                points = [];
                destination = target;
            }
            else
            {
                if (search == null && Time.time >= nextSearch)
                {
                    search = MapPathfinding.CreateRequest(dummy, target, bot.Map.WalkingOptions);
                    nextSearch = Time.time + (bot.Level == 4 ? 0.2f : 0.5f);
                }

                if (search != null)
                {
                    search.Step();
                    if (search.Status != PathStatus.Searching)
                    {
                        if (search.Status == PathStatus.Found)
                        {
                            points = search.Result.Points;
                            waypoint = 0;
                        }

                        search = null;
                    }
                }

                while (waypoint < points.Length && Vector2.Distance(position, points[waypoint]) < 0.15f)
                    waypoint++;
                while (waypoint + 1 < points.Length && probe.CanMove(position, points[waypoint + 1]))
                    waypoint++;
                if (waypoint < points.Length)
                    destination = points[waypoint];
            }

            var next = Vector2.MoveTowards(position, destination, bot.Player.MyPhysics.TrueSpeed * speedMultiplier * Time.fixedDeltaTime);
            dummy.MyPhysics.body.velocity = probe.CanMove(position, next) ? (next - position) / Time.fixedDeltaTime : Vector2.zero;
            yield return tick;
        }

        search?.Cancel();
        if (dummy)
            dummy.MyPhysics.body.velocity = Vector2.zero;
    }

    public IEnumerator CoEvade(PlayerControl dummy)
    {
        var crossRoom = bot.CanLeavePracticeRoom && bot.Map.EscapePositions.Count > 1;
        var speed = bot.Player.MyPhysics.TrueSpeed * (crossRoom ? 1.05f : 0.65f + bot.Level * 0.05f);
        var positions = crossRoom ? bot.Map.EscapePositions : bot.Map.RoamingPositions;
        var probe = MapPathfinding.CreateRequest(dummy, dummy.GetTruePosition(), bot.Map.WalkingOptions);
        MapPathRequest search = null;
        Vector2[] points = [];
        var waypoint = 0;
        var destination = dummy.GetTruePosition();
        var tick = new WaitForFixedUpdate();
        while (bot.LessonRunning && dummy && !dummy.Data.IsDead)
        {
            if (bot.IsPaused || !dummy.moveable)
            {
                dummy.MyPhysics.body.velocity = Vector2.zero;
                yield return tick;
                continue;
            }

            var position = dummy.GetTruePosition();
            while (waypoint < points.Length && Vector2.Distance(position, points[waypoint]) < 0.15f)
                waypoint++;
            while (waypoint + 1 < points.Length && probe.CanMove(position, points[waypoint + 1]))
                waypoint++;
            if (search == null && (waypoint >= points.Length || !probe.CanMove(position, points[waypoint]) || (waypoint == points.Length - 1 && Vector2.Distance(position, points[waypoint]) < 1.5f)))
            {
                var choices = positions
                    .Where(point => Vector2.Distance(point, position) > 1f && point != destination)
                    .OrderByDescending(point => Vector2.Distance(point, bot.Player.GetTruePosition()) - Vector2.Distance(point, position) * 0.5f)
                    .Take(2).ToArray();
                if (choices.Length > 0)
                {
                    destination = choices[Random.Range(0, choices.Length)];
                    search = MapPathfinding.CreateRequest(dummy, destination, bot.Map.WalkingOptions);
                }
            }

            if (search != null)
            {
                search.Step();
                if (search.Status == PathStatus.Searching && search.ExpandedNodes >= 2048)
                    search.Cancel();
                if (search.Status != PathStatus.Searching)
                {
                    if (search.Status == PathStatus.Found)
                    {
                        points = search.Result.Points;
                        waypoint = 0;
                    }

                    search = null;
                }
            }

            var next = waypoint < points.Length ? Vector2.MoveTowards(position, points[waypoint], speed * Time.fixedDeltaTime) : position;
            dummy.MyPhysics.body.velocity = probe.CanMove(position, next) ? (next - position) / Time.fixedDeltaTime : Vector2.zero;
            yield return tick;
        }

        search?.Cancel();
        if (dummy)
            dummy.MyPhysics.body.velocity = Vector2.zero;
    }

    public IEnumerator CoFleeContract(PlayerControl target)
    {
        while (bot.LessonRunning && !target.Data.IsDead)
        {
            if (bot.Level > 1 && Bounty.Contracts.TryGetValue(bot.Player.PlayerId, out var contract) && contract.Phase == Bounty.ContractPhase.Collection)
            {
                yield return CoEvade(target);
                yield break;
            }

            yield return bot.Map.CoWalk(target, bot.Map.RoamingPositions[Random.Range(0, bot.Map.RoamingPositions.Count)], replanAfter: 0.5f);
            yield return null;
        }
    }

    public IEnumerator CoAttack(PlayerControl attacker, PlayerControl victim)
    {
        while (bot.LessonRunning && attacker && victim && !attacker.Data.IsDead && !victim.Data.IsDead)
        {
            if (bot.IsPaused)
            {
                attacker.MyPhysics.body.velocity = Vector2.zero;
                yield return null;
                continue;
            }

            if (Vector2.Distance(attacker.GetTruePosition(), victim.GetTruePosition()) < 0.65f && !PhysicsHelpers.AnythingBetween(attacker.GetTruePosition(), victim.GetTruePosition(), Constants.ShipAndObjectsMask, false))
            {
                attacker.RpcCustomMurder(victim, MeetingCheck.OutsideMeeting, resetKillTimer: false, teleportMurderer: false);
                yield break;
            }

            yield return bot.Map.CoWalk(attacker, victim.GetTruePosition(), bot.Player.MyPhysics.TrueSpeed * (0.6f + bot.Level * 0.1f), 0.25f);
            yield return null;
        }
    }

    public IEnumerator CoDefense(RoleBehaviour role)
    {
        var ward = role is Aegis;
        var attacker = bot.Map.Dummies[0];
        var victim = ward && bot.Level > 1 ? bot.Map.Dummies[1] : bot.Player;
        var button = ward ? (CustomActionButton)CustomButtonSingleton<AegisButton>.Instance : CustomButtonSingleton<MirrorReflectButton>.Instance;
        var attempts = ward ? Mathf.Min(bot.Level, (int)OptionGroupSingleton<AegisOptions>.Instance.MaxCharges) : bot.Level;
        var instruction = "Reflect turns one hit back on the attacker. Watch their approach and time the window.";
        if (ward)
            instruction = bot.Level == 1 ? "Place Sentinel Ward and stay inside it. It blocks one hit, then breaks." : "Protect Practice 2 with Sentinel Ward. Each ward blocks one hit. Save your charges and stay close enough to cover your ally.";
        else if (bot.Level >= 3)
            instruction += " Watch for feints before the real approach.";
        yield return bot.Say(instruction);
        if (ward && bot.Level == 4)
            bot.Map.StartMovement(bot.Map.CoRoam(victim, 2));
        for (var attempt = 0; attempt < attempts && bot.LessonSucceeded; attempt++)
        {
            if (attacker.Data.IsDead)
            {
                attacker.Revive();
                foreach (var body in Object.FindObjectsOfType<DeadBody>())
                {
                    if (body.ParentId == attacker.PlayerId)
                        Object.Destroy(body.gameObject);
                }
            }

            yield return bot.WaitFor("Get ready for the next attack.", () => button.CanClick(), button.Cooldown + 15f);
            yield return bot.Map.CoWalk(attacker, bot.Map.Positions[(attempt + 2) % bot.Map.Positions.Count]);
            bot.ShowGoal(victim);
            if (!ward && bot.Level >= 3)
            {
                attacker.SetName("Feint");
                yield return bot.Map.CoWalk(attacker, bot.Map.Positions[(attempt + 1) % bot.Map.Positions.Count], replanAfter: 0.6f);
                yield return new WaitForSeconds(Random.Range(0.4f, 1.2f));
            }

            attacker.SetName("Attacking");
            if (bot.Level == 1)
            {
                if (ward)
                {
                    yield return bot.WaitFor("Place Sentinel Ward and stay inside it.", () => ShieldArea._active.Any(area => area && area.ownerId == bot.Player.PlayerId && area.Contains(victim.GetTruePosition())));
                }
                else
                {
                    yield return bot.Map.CoWalk(attacker, bot.Player.GetTruePosition());
                    yield return bot.WaitFor("Activate Reflect now.", () => MirrorBladeRole.ArmedReflections.Contains(bot.Player.PlayerId));
                }
            }

            bot.Instruction = ward ? "Block the attack on the marked player." : "Reflect the attack.";
            bot.Objective.text = bot.Instruction;
            bot.StepRemaining = 45f;
            yield return CoAttack(attacker, victim);
            if (victim.Data.IsDead || (!ward && !attacker.Data.IsDead))
            {
                bot.Failure = ward ? "The attacker reached the ally outside a ward. Place the next ward around the player you need to protect." : "The attack missed your Reflect window. Watch the approach and try again.";
                bot.LessonSucceeded = false;
                yield break;
            }
        }
    }

    public IEnumerator CoWatchFeign(PlayerControl witness)
    {
        while (bot.LessonRunning)
        {
            if (Revenant.Phases.TryGetValue(bot.Player.PlayerId, out var phase) && phase == Revenant.Phase.Feigning && Revenant.Bodies.TryGetValue(bot.Player.PlayerId, out var body))
            {
                yield return bot.Map.CoWalk(witness, body.TruePosition, bot.Player.MyPhysics.TrueSpeed * (bot.Level == 4 ? 0.9f : 0.65f), 0.4f);
                if (witness && body && Revenant.Phases.TryGetValue(bot.Player.PlayerId, out phase) && phase == Revenant.Phase.Feigning && Vector2.Distance(witness.GetTruePosition(), body.TruePosition) < 0.7f)
                {
                    Revenant.RpcRequestFeignReport(witness, bot.Player.PlayerId);
                    bot.Failure = "The witness reported your fake body. Feign farther from them so you can revive before they arrive.";
                    yield break;
                }
            }
            else
            {
                yield return bot.Map.CoWalk(witness, bot.Map.RoamingPositions[Random.Range(0, bot.Map.RoamingPositions.Count)], replanAfter: 0.5f);
            }

            yield return null;
        }
    }

    public IEnumerator CoCleanseExperiment(PlayerControl target, float readyAt)
    {
        var deadline = readyAt + (bot.Level == 4 ? 2f : 5f);
        while (bot.LessonRunning && InjectorUtilities.Experiments.TryGetValue(bot.Player.PlayerId, out var experiment) && experiment.TargetId == target.PlayerId)
        {
            if (Time.time >= deadline)
            {
                InjectorUtilities.RpcCleanse(target);
                bot.Failure = "The target cleansed the experiment. Stay close and collect as soon as observation finishes.";
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator CoContestCrown(PlayerControl rival)
    {
        while (bot.LessonRunning && Usurper.States.TryGetValue(bot.Player.PlayerId, out var state))
        {
            if (state.Phase == Usurper.CrownPhase.Held)
            {
                yield return CoAttack(rival, bot.Player);
                yield break;
            }

            if (state.Phase == Usurper.CrownPhase.Available && Usurper.CrownPositions.TryGetValue(bot.Player.PlayerId, out var crown))
            {
                rival.SetName("Contesting");
                yield return bot.Map.CoWalk(rival, crown, replanAfter: 1f);
                yield return bot.Map.CoWalk(rival, bot.Map.RoamingPositions.OrderByDescending(point => Vector2.Distance(point, crown)).First());
                rival.SetName("Patrolling");
                yield return new WaitForSeconds(OptionGroupSingleton<UsurperOptions>.Instance.PickupDuration + (bot.Level == 4 ? 0.5f : 2f));
            }

            yield return null;
        }
    }

    public IEnumerator CoGroundNode(PlayerControl rival)
    {
        while (bot.LessonRunning && !bot.Victory)
        {
            if (EnergyThief.BreachActive)
            {
                yield return bot.Map.CoWalk(rival, EnergyThief.GetNodePosition(), bot.Player.MyPhysics.TrueSpeed * (bot.Level == 4 ? 0.9f : 0.6f), 0.5f);
                if (EnergyThief.BreachActive && Vector2.Distance(rival.GetTruePosition(), EnergyThief.GetNodePosition()) <= OptionGroupSingleton<EnergyThiefOptions>.Instance.GridBreachRadius)
                {
                    EnergyThief.RpcRequestGround(rival);
                    bot.Failure = "The defender grounded your breach. Begin while they are far enough away for the countdown to finish.";
                    yield break;
                }
            }
            else
            {
                var destination = bot.Map.RoamingPositions[Random.Range(0, bot.Map.RoamingPositions.Count)];
                yield return bot.Map.CoWalk(rival, destination, replanAfter: 0.5f);
            }

            yield return null;
        }
    }

    public IEnumerator CoPrankWitness(PlayerControl witness)
    {
        while (bot.LessonRunning && !PranksterUtilities.FooledPlayers.Contains((bot.Player.PlayerId, witness.PlayerId)))
        {
            var distracted = bot.Level < 3 || (Time.time + witness.PlayerId) % (bot.Level == 4 ? 6f : 8f) < 2.5f;
            witness.SetName(distracted ? "Distracted" : "Inspecting");
            var body = PranksterUtilities.FindAllPranksterBodies().FirstOrDefault();
            if (body)
            {
                if (distracted && Vector2.Distance(witness.GetTruePosition(), body.TruePosition) <= witness.MaxReportDistance)
                {
                    PranksterUtilities.RpcRequestFakeReport(witness, bot.Player.PlayerId, body.TruePosition.x, body.TruePosition.y);
                    yield break;
                }

                if (!distracted && Vector2.Distance(witness.GetTruePosition(), body.TruePosition) <= OptionGroupSingleton<PranksterOptions>.Instance.InspectRange)
                {
                    yield return new WaitForSeconds(OptionGroupSingleton<PranksterOptions>.Instance.InspectDuration);
                    if (body)
                        PranksterUtilities.RpcInspectBody(witness, body.ParentId, body.TruePosition.x, body.TruePosition.y);
                }
                else
                {
                    yield return bot.Map.CoWalk(witness, body.TruePosition, replanAfter: 0.3f);
                }
            }

            yield return null;
        }
    }

    public IEnumerator CoDisableCamera(PlayerControl witness, int cameraId)
    {
        yield return new WaitForSeconds(bot.Level == 4 ? 1f : 3f);
        while (bot.LessonRunning && VisionaryUtilities.Cameras.TryGetValue(cameraId, out var camera) && camera.Object)
        {
            yield return bot.Map.CoWalk(witness, camera.Object.transform.position, replanAfter: 0.4f);
            if (camera.Object && Vector2.Distance(witness.GetTruePosition(), camera.Object.transform.position) <= 1.25f)
            {
                VisionaryUtilities.RpcInteractCamera(witness, cameraId);
                if (!camera.Collected && !camera.Object)
                    bot.Failure = "The witness disabled your camera. Stay close enough to collect it as soon as the photo is ready.";
            }

            yield return null;
        }
    }
}
