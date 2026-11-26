using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Buttons.Roles.S1;
using NewMod.Components;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.UI;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.NewBot;

public sealed class NewBotLessons(NewBot bot)
{
    public IEnumerator CoHarvestFragments(Collector.FragmentKind kind, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var position = bot.Level >= 3
                ? bot.Map.RoamingPositions[Random.Range(0, bot.Map.RoamingPositions.Count)]
                : bot.Map.Positions[bot.Map.Dummies.Count + (i + (int)kind) % (bot.Map.Positions.Count - bot.Map.Dummies.Count)];
            var target = Collector.Inventories[bot.Player.PlayerId][(int)kind] + 1;
            Collector.RpcSpawnFragment(bot.Player, ++Collector._nextFragmentId, (byte)kind, position.x, position.y);
            if (bot.Level < 4)
                bot.ShowGoal(position);
            yield return bot.WaitFor($"Harvest the {kind} fragment.", () => Collector.Inventories[bot.Player.PlayerId][(int)kind] >= target, 90f);
            if (bot.GoalMarker)
                Object.Destroy(bot.GoalMarker.gameObject);
        }
    }

    public IEnumerator CoQuestion(string question, string answer, params string[] alternatives)
    {
        var correct = new NewBotMenu.Choice(answer, Color.white);
        var choices = alternatives.Distinct().Where(text => text != answer).Select(text => new NewBotMenu.Choice(text, Color.white)).Append(correct).OrderBy(_ => Random.value).ToArray();
        while (true)
        {
            bot.Menu = NewBotMenu.Open(question, choices);
            while (bot.Menu.Component)
                yield return null;
            if (bot.Menu.Selected == correct)
                yield break;
            if (bot.Menu.Selected == null)
            {
                bot.LessonSucceeded = false;
                Object.Destroy(bot);
                yield return null;
                yield break;
            }
            if (!bot.Progressive)
            {
                yield return bot.Say($"Hold on, that doesn't match what happened. The answer is '{answer}'. Read the choices once more; we can stay here and try again.");
                continue;
            }
            bot.Failure = $"Ah, we missed that one. The answer was '{answer}'. Let's repeat the exercise and watch for it this time.";
            bot.LessonSucceeded = false;
            yield break;
        }
    }

    public IEnumerator CoJudgments()
    {
        var id = bot.Player.PlayerId;
        var options = OptionGroupSingleton<ArbitratorOptions>.Instance;
        yield return bot.Say("Accuse scores when your target is exiled. Defend scores when they receive enough votes but survive. You must earn each token on a different player.");
        for (var round = 0; round < options.JudgmentTokensToWin; round++)
        {
            foreach (var dummy in bot.Map.Dummies)
            {
                if (dummy.Data.IsDead)
                    dummy.Revive();
                dummy.GetComponent<DummyBehaviour>().PlayerIdToVoteFor = 253;
            }

            var target = bot.Map.Dummies.Where(dummy => !ArbitratorRole.ScoredTargets.Contains(dummy.PlayerId)).OrderBy(_ => bot.Level >= 3 ? Random.value : 0f).First();
            var defend = bot.Level >= 3 ? Random.value > 0.5f : round % 2 == 0;
            var voters = bot.Map.Dummies.Where(dummy => dummy != target).ToArray();
            var votes = defend ? (int)options.DefendVotesRequired : voters.Length;
            for (var voter = 0; voter < votes; voter++)
                voters[voter].GetComponent<DummyBehaviour>().PlayerIdToVoteFor = target.PlayerId;
            yield return bot.Say(defend ? $"{target.Data.PlayerName} will receive {votes} votes. The rest will skip. Choose the judgment that scores if they survive, and vote Skip." : $"Everyone else plans to vote for {target.Data.PlayerName}. Choose the judgment that scores if they are exiled, and vote for them.");
            var mode = defend ? ArbitratorJudgmentMode.Defend : ArbitratorJudgmentMode.Accuse;
            yield return Meeting("Choose your judgment and target, then cast the planned vote.", () => ArbitratorRole._localLocked && ArbitratorRole._hostTarget == target.PlayerId && ArbitratorRole._hostMode == mode);
            if (!bot.LessonSucceeded || ArbitratorRole.JudgmentTokens.GetValueOrDefault(id) != round + 1)
            {
                bot.Failure = "That judgment did not score. Check who receives votes and whether that player will survive.";
                yield break;
            }

            if (round == 0)
            {
                bot.Observation = null;
                yield return bot.Say("Use Leverage on a surviving player. It tells you whether their last vote matched yours.");
                yield return bot.WaitFor("Use Leverage and read its result.", () => bot.Observation != null, options.LeverageCooldown + 60f);
                yield return CoQuestion("Did their vote match yours?", bot.Observation, "Same vote", "Different vote");
                if (!bot.LessonSucceeded)
                    yield break;
            }
        }
    }

    public IEnumerator CoPractice(RoleBehaviour role)
    {
        bot.PracticeRole = role;
        bot.Events.SelectedEvent = null;
        var repeat = new NewBotMenu.Choice("Try again", Color.white);
        var harder = new NewBotMenu.Choice("Next difficulty", new Color(1f, 0.75f, 0.3f));
        var roles = new NewBotMenu.Choice("Choose another role", Color.white);
        while (true)
        {
            var count = role switch
            {
                Tyrant => 5,
                EnergyThief => 6,
                Prankster => (int)OptionGroupSingleton<PranksterOptions>.Instance.ReportsRequiredToWin,
                Revenant => bot.Level >= 3 ? 2 : 1,
                Shade => (int)OptionGroupSingleton<ShadeOptions>.Instance.RequiredKills,
                Aegis => 2,
                Edgeveil or PulseBlade => bot.Level + 1,
                EgoistRole => bot.Level >= 3 ? 5 : 3,
                VerifierRole => 3,
                ArbitratorRole => Mathf.Max((int)OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin + 1, (int)OptionGroupSingleton<ArbitratorOptions>.Instance.DefendVotesRequired * 2),
                TheVisionary => bot.Level >= 3 ? 3 : 2,
                Beacon or Specialist => bot.Level + 1,
                NecromancerRole => bot.Level >= 3 ? 3 : 2,
                WraithCaller or WardenRole or Collector or Usurper => 2,
                Voidwalker => bot.Level > 1 ? 2 : 1,
                InjectorRole => Mathf.Max(2, OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount),
                DoubleAgent => bot.Level > 1 ? 1 : 0,
                Nomad => 2,
                TerminatorRole => bot.Level == 1 ? 1 : (int)OptionGroupSingleton<TerminatorOptions>.Instance.ArmorSegments + bot.Level - 2,
                _ => 1,
            };
            yield return bot.Map.CoPrepare(role, count, chooseRoom: true);
            if (bot.Map.Positions.Count == 0)
            {
                yield return bot.Say("I couldn't find enough clear space for this trial.");
                yield break;
            }

            bot.LessonSucceeded = true;
            bot.Victory = false;
            yield return bot.CoSupervise(bot.Player.Data.Role);
            bot.Map.StopMovements();

            while (MeetingHud.Instance || ExileController.Instance)
                yield return null;
            var succeeded = bot.LessonSucceeded && !bot.Player.Data.IsDead;
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
                yield return null;
            }
            bot.Player.RpcSetRole(AmongUs.GameOptions.RoleTypes.Crewmate);
            bot.Map.ResetPractice();
            bot.Player.Revive();
            if (!succeeded)
            {
                yield return bot.Say(bot.Failure ?? "That attempt didn't finish. Want another go? Choose Try again and I'll set it up.");
            }
            else
            {
                var recap = role switch
                {
                    Aegis => "That ward took the hit. In a match, put it around whoever needs protecting; it breaks after blocking one attack.",
                    MirrorBladeRole => "You caught the attack with Reflect. Keep that timing: activating too early leaves you exposed when it wears off.",
                    Beacon => "You read the scan. Those markers show where people were, so check again before treating an old position as current.",
                    Specialist => "You read all three scans. Presence counts players, Forensics finds bodies, and Disturbance checks recent violence. None of them names the killer.",
                    DoubleAgent => "Your Comms worked and ended on its own. You're still helping the crew, so choose a moment that helps them rather than interrupting them.",
                    TheVisionary => "You brought the photo into the meeting. Keep track of which camera captured the evidence before you show it.",
                    WardenRole => "You read the room's trace. Use it to check someone's story. The seal blocks vent entry; it doesn't stop a kill.",
                    VerifierRole => "You matched the claims to the evidence. Confirmed means your statement was correct, even when that statement was 'they didn't do it'.",
                    Edgeveil => "You landed Arc without hitting the others. Face the right way before charging; you can't steer it afterward.",
                    PulseBlade => "You landed Strike on the right target. Their outline tells you who you're aiming at, but the dash won't turn to follow them.",
                    NecromancerRole => "They're back! Revive works on someone else's victim. A body from your own kill won't be a valid target.",
                    Revenant => "You survived the fake death and caught your target with Doom. Keep your fake body away from witnesses, and save Doom until you're close enough.",
                    Voidwalker => "You made the crossing and attacked after returning. You can't attack while phased, so choose your exit before leaving the Void.",
                    Deadwire => "You've tried all five records. Watch what you captured before using Override; the category decides what it does.",
                    EnergyThief => "You finished the breach. Both energy and distinct categories matter. Once they're ready, leave Siphon and get to the node.",
                    InjectorRole => "Samples submitted. Each one came from a different player, and you stayed still for the finish. That's the whole experiment.",
                    Prankster => "They fell for it. Each different reporter gave you a point; inspecting a fake would give you nothing. I'm checking every body you leave near my lab.",
                    Shade => "Those kills counted because both you and the target were inside your active shadow. Set up the zone before you commit to the chase.",
                    WraithCaller => "Your Wraith landed a kill. You'll need a new trace for the next hunt. Watch for anyone who can reflect it back at you.",
                    Tyrant => "Your Champion accepted and you both survived the meeting. Keep them alive; losing either of you breaks that shared win.",
                    Bounty => "Contract collected. Keep contact during the escort, then move fast once the head start ends. Cash Out belongs in the collection window.",
                    Nomad => "Routes scored and meeting survived. Count different rooms, let each visit register, then Wander home. Replacing an unfinished Anchor starts that route over.",
                    Collector => "You built the set and survived the meeting. Pairs buy powers; one of each arms the win. Spend carefully so you keep what you need.",
                    Usurper => "You kept the crown and survived the meeting. An early meeting only pauses the hold timer, so wait until it's finished.",
                    TerminatorRole => "You survived the countdown. Starting Terminate is only the beginning; keep enough distance that attackers can't strip your armor.",
                    ArbitratorRole => "Your judgments scored. Accuse needs an exile; Defend needs enough votes and a survivor. Pick someone new for each token.",
                    EgoistRole => "You built Ego, locked your opponent, and got them exiled. Locking the challenge doesn't cast your normal vote; you still need both.",
                    _ => "You finished the exercise. Want another go?",
                };
                yield return bot.Say(recap);
            }

            var choices = new List<NewBotMenu.Choice>
            {
                repeat,
                roles,
            };
            if (succeeded && bot.Progressive && bot.Level < 4)
                choices.Insert(1, harder);
            var title = bot.Level switch
            {
                1 => "Practice",
                2 => "Challenge",
                3 => "Hard",
                _ => "Extreme",
            };
            bot.Menu = NewBotMenu.Open(title, choices);
            while (bot.Menu.Component)
                yield return null;
            if (bot.Menu.Selected == null || bot.Menu.Selected == roles)
                yield break;
            if (bot.Menu.Selected == harder)
                bot.Level++;
        }
    }

    public IEnumerator CoRunLesson(RoleBehaviour role)
    {
        var id = bot.Player.PlayerId;
        switch (role)
        {
            case Aegis:
            case MirrorBladeRole:
                yield return bot.Challenges.CoDefense(role);
                break;
            case Beacon:
                Beacon.charges = Mathf.Max(1, Beacon.charges);
                bot.Map.StartRoaming();
                yield return bot.Say("I'll get everyone moving. Use Scan and count the purple markers on your map. Remember where they were: those dots won't follow them.");
                for (var scan = 0; scan < (bot.Level >= 3 ? 2 : 1); scan++)
                {
                    if (scan > 0)
                    {
                        Beacon.charges = Mathf.Max(1, Beacon.charges);
                        yield return bot.Say($"I've supplied another practice charge. In a match, you earn one every {OptionGroupSingleton<BeaconOptions>.Instance.TasksPerCharge:0} completed tasks. Scan again and compare the new positions with your first snapshot.");
                    }

                    var previousPulse = Beacon.pulseUntil;
                    yield return bot.WaitFor("Use Scan and count the other players.", () => Beacon.pulseUntil > previousPulse);
                    var visiblePlayers = bot.Map.Dummies.Count(dummy => !dummy.Data.IsDead && !dummy.inVent);
                    while (MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                        yield return null;
                    yield return CoQuestion("How many players did Scan show?", visiblePlayers.ToString(CultureInfo.InvariantCulture), (visiblePlayers - 1).ToString(CultureInfo.InvariantCulture), (visiblePlayers + 1).ToString(CultureInfo.InvariantCulture), (visiblePlayers + 2).ToString(CultureInfo.InvariantCulture));
                    if (!bot.LessonSucceeded)
                        yield break;
                    if (bot.Level >= 3)
                    {
                        yield return CoQuestion("What does that snapshot tell you now?", "Where players were when I scanned", "Where players are now", "Which players are impostors");
                        if (!bot.LessonSucceeded)
                            yield break;
                    }
                }

                break;
            case Specialist:
                yield return bot.Say("I'll lend you scan charges. You normally earn them from tasks. Pick a mode, press Scan, and hold still for 1.5 seconds. Move too soon and you'll cancel it.");
                var modes = System.Enum.GetValues<Specialist.ScanMode>().OrderBy(mode => bot.Level >= 3 ? Random.value : (float)mode).ToArray();
                foreach (var mode in modes)
                {
                    var explanation = mode switch
                    {
                        Specialist.ScanMode.Presence => "Presence counts other living players nearby, within 5 units. It does not reveal names or teams, and players in vents are excluded.",
                        Specialist.ScanMode.Forensics => "Forensics finds nearby bodies. The result tells you whether a body is very close (within 2 units), farther away (2-5 units), or absent. It does not identify the killer.",
                        _ => "Disturbance detects a nearby attack from the last 15 seconds, within 5 units. A body can remain after that trace expires. This scan does not identify the attacker.",
                    };
                    if (bot.Level < 3)
                        yield return bot.Say(explanation);
                    Specialist.EarnScan(id);
                    yield return bot.WaitFor($"Press Scan Mode until it shows {mode}.", () => Specialist.ScanStates[id].Mode == mode, 45f);
                    if (!bot.LessonSucceeded)
                        yield break;
                    if (mode != Specialist.ScanMode.Presence)
                    {
                        foreach (var body in Object.FindObjectsOfType<DeadBody>())
                            Object.Destroy(body.gameObject);
                        bot.Map.Dummies[1].Revive();
                        bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                        if (bot.Level == 4 && mode == Specialist.ScanMode.Disturbance)
                        {
                            yield return bot.Say("Watch this body. I'll leave it here for a while, then ask you to check for recent violence. Read the result carefully.");
                            yield return new WaitForSeconds(16f);
                        }
                    }

                    bot.Observation = null;
                    var charges = Specialist.ScanStates[id].Charges;
                    yield return bot.WaitFor($"{mode}: press Scan, then stand still for 1.5 seconds.", () => Specialist.ScanStates[id].Charges < charges, 60f);
                    if (!bot.LessonSucceeded)
                        yield break;
                    if (Specialist.ScanStates[id].Mode != mode || bot.Observation == null)
                    {
                        bot.Failure = $"That used the wrong mode. Select {mode} before scanning.";
                        yield break;
                    }

                    yield return bot.CoDelay(3f);
                    if (bot.Level < 3)
                    {
                        var result = mode == Specialist.ScanMode.Presence ? $"{bot.Observation} other living player(s) nearby" : bot.Observation;
                        yield return bot.Say($"You found {result}. That tells us about this area; it doesn't name a killer.");
                        continue;
                    }
                    if (mode == Specialist.ScanMode.Presence)
                        yield return CoQuestion("How many living players were nearby?", bot.Observation, "0", "1", "2", "3", "4", "5");
                    else if (mode == Specialist.ScanMode.Forensics)
                        yield return CoQuestion("What did Forensics find?", bot.Observation, "No bodies", "A body within 2 units", "A body 2 to 5 units away");
                    else
                        yield return CoQuestion("What did Disturbance find?", bot.Observation, "Recent violence", "No recent violence");
                    if (!bot.LessonSucceeded)
                        yield break;
                }

                break;
            case DoubleAgent:
                DoubleAgent.CooldownUntil = 0f;
                yield return bot.Say("You're still on the crew's side. Open Sabotage and try Communications. I'll wait with you while it runs out. The other sabotages aren't yours to use.");
                if (bot.Level > 1)
                {
                    bot.Map.StartRoaming();
                    bot.ShowGoal(bot.Map.RoamingPositions[0]);
                    yield return bot.Say("Time this one. Use Comms when the dummy reaches the arrow. The later rounds give you a smaller window.");
                }

                for (var attempt = 0; attempt < bot.Level && bot.LessonSucceeded; attempt++)
                {
                    yield return bot.WaitFor("Wait for Comms to recharge, then time the next pass.", () => Time.time >= DoubleAgent.CooldownUntil, OptionGroupSingleton<DoubleAgentOptions>.Instance.CounterfeitCooldown + 5f);
                    yield return bot.WaitFor("Sabotage Communications.", () => DoubleAgent.CounterfeitActive, 90f);
                    if (bot.Level > 1 && Vector2.Distance(bot.Map.Dummies[0].GetTruePosition(), bot.Map.RoamingPositions[0]) > 1.5f - bot.Level * 0.2f)
                    {
                        bot.Failure = "The dummy wasn't at the arrow when Comms started. Watch its route, then time the sabotage.";
                        yield break;
                    }

                    if (MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                        MapBehaviour.Instance.Close();
                    yield return bot.WaitFor("Wait for Counterfeit Comms to end.", () => !DoubleAgent.CounterfeitActive, 45f);
                    var comms = bot.Map.Ship.Systems[SystemTypes.Comms];
                    var hqComms = comms.TryCast<HqHudSystemType>();
                    bot.LessonSucceeded = hqComms != null ? !hqComms.IsActive : !comms.Cast<HudOverrideSystemType>().IsActive;
                }

                break;
            case TheVisionary:
                bot.EvidenceCamera = -1;
                bot.ShowGoal(bot.Map.Dummies[1]);
                yield return bot.Say("Put Practice 2 in your camera's frame. I'll stage an attack while it charges. Bring the photo back and we'll show it in a meeting.");
                if (bot.Level >= 3)
                    yield return bot.Say("Practice 3 will try to disable your camera. Stay close enough to collect it when it's ready.");
                yield return bot.WaitFor("Place a camera aimed at the marked scene.", () => VisionaryUtilities.Cameras.Count > 0);
                var cameraId = VisionaryUtilities.Cameras.Keys.Max();
                yield return new WaitForSeconds(OptionGroupSingleton<VisionaryOptions>.Instance.CaptureDelay * 0.5f);
                bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                if (bot.Level >= 3)
                    bot.Map.StartMovement(bot.Challenges.CoDisableCamera(bot.Map.Dummies[2], cameraId));
                yield return bot.WaitFor("Collect the camera with the body in frame.", () => VisionaryUtilities.HasScreenshots || VisionaryUtilities.Cameras.Values.All(camera => !camera.Object), OptionGroupSingleton<VisionaryOptions>.Instance.CaptureDelay + 45f);
                if (!VisionaryUtilities.Cameras.TryGetValue(bot.EvidenceCamera, out var photo) || !photo.Collected)
                {
                    bot.Failure = "The collected photo missed the body. Place the camera so the marked scene stays in frame.";
                    yield break;
                }

                yield return Meeting("Broadcast the photo with the body, then vote Skip.", () => VisionaryUtilities.BroadcastOwners.Contains(id));
                break;
            case WardenRole:
                WardenRole.ResetState();
                yield return bot.Say("Seal this room and watch what happens inside. I'll stage an attack. Your Residual Trace will give us clues; it won't hand us the killer's name.");
                yield return bot.WaitFor("Use Seal in this room.", () => WardenRole.SealActive);
                if (bot.Level >= 3 && Random.value > 0.5f)
                    WardenRole.RpcTrackAbilityUse(bot.Map.Dummies[0]);
                bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                yield return null;
                if (WardenRole.ResidualMarks.Count == 0)
                {
                    bot.Failure = "The attack left no trace in your seal. Keep the seal in this room and try again.";
                    yield break;
                }

                bot.Observation = null;
                yield return bot.WaitFor("Inspect the Residual Trace near your seal.", () => bot.Observation != null || WardenRole.ResidualMarks.Count == 0);
                if (bot.Observation == null)
                {
                    bot.Failure = "The trace expired before you inspected it. Move to the seal as soon as the attack happens.";
                    yield break;
                }

                if (bot.Level >= 3)
                    yield return CoQuestion("What did the trace tell you?", bot.Observation, "Entered after the seal", "Already inside", "Used an ability", "No recent ability", "The attacker is innocent");
                yield return CoQuestion("What does the seal prevent?", "Vent entry in the sealed room", "All kills in the room", "Players leaving on foot");
                break;
            case VerifierRole:
                yield return bot.Say("I'll give you evidence about one player. In the meeting, choose VERIFY, then that player and the matching row. Green says they did it; red says they didn't. We're checking an action, not clearing their name.");
                var claims = System.Enum.GetValues<VerifierClaimType>().Where(claim => claim != VerifierClaimType.None);
                foreach (var claim in claims)
                {
                    bot.VerificationClaim = claim;
                    bot.VerificationExpected = bot.Level < 3 ? claim != VerifierClaimType.EnteredVent : Random.value > 0.5f;
                    var targets = bot.Map.Dummies.Where(dummy => !dummy.Data.IsDead).ToArray();
                    var verified = targets[Random.Range(0, bot.Level >= 3 ? targets.Length : 1)];
                    bot.VerificationTarget = verified.PlayerId;
                    if (bot.VerificationExpected)
                    {
                        if (claim == VerifierClaimType.NearBody)
                        {
                            var victim = targets.Last(dummy => dummy != verified);
                            bot.ShowGoal(verified);
                            victim.NetTransform.SnapTo(verified.GetTruePosition() - victim.Collider.offset);
                            verified.RpcCustomMurder(victim, MeetingCheck.OutsideMeeting, teleportMurderer: false);
                            yield return bot.WaitFor("Watch where the body falls beside the marked player.", () => Helpers.GetBodyById(victim.PlayerId), 10f);
                            if (!bot.LessonSucceeded)
                                yield break;
                        }
                        else
                        {
                            VerifierUtilities.RegisterFact(verified.PlayerId, claim);
                        }
                    }

                    var action = claim switch
                    {
                        VerifierClaimType.DidTask => "completed a task",
                        VerifierClaimType.EnteredVent => "entered a vent",
                        VerifierClaimType.UsedAbility => "used an ability",
                        _ => "was near a body",
                    };
                    var row = claim switch
                    {
                        VerifierClaimType.DidTask => "Completed a task last round",
                        VerifierClaimType.EnteredVent => "Entered a vent last round",
                        VerifierClaimType.UsedAbility => "Used an ability last round",
                        _ => "Near a body at report time",
                    };
                    if (claim == VerifierClaimType.NearBody)
                        VerifierUtilities.RegisterNearBody();
                    bot.VerificationExpected = (VerifierUtilities.RoundFacts.GetValueOrDefault(verified.PlayerId) & VerifierUtilities.GetFlag(claim)) != 0;
                    var evidence = bot.VerificationExpected ? $"Evidence confirms that {verified.Data.PlayerName} {action}." : $"There is no record that {verified.Data.PlayerName} {action}.";
                    yield return bot.Say($"Practice evidence: {evidence}\nI'll open a meeting. Press VERIFY, select {verified.Data.PlayerName}, and find the row '{row}'.");
                    yield return Meeting(string.Empty, () => VerifierUtilities.UsedThisMeeting, () =>
                    {
                        bot.VerificationExpected = (VerifierUtilities.RoundFacts.GetValueOrDefault(verified.PlayerId) & VerifierUtilities.GetFlag(claim)) != 0;
                        if (bot.Level >= 3)
                            return $"Verify {verified.Data.PlayerName}: {row}.\nChoose the answer from the evidence, then Skip.";
                        var answer = bot.VerificationExpected ? "green check (did)" : "red cross (did not)";
                        return $"VERIFY > {verified.Data.PlayerName}\n{row}: {answer}. Then Skip.";
                    });
                    if (!bot.LessonSucceeded)
                        yield break;
                    yield return bot.Say(bot.VerificationExpected
                        ? $"You confirmed that {verified.Data.PlayerName} {action}. That action alone does not prove their team."
                        : $"You denied that {verified.Data.PlayerName} {action}. 'Confirmed' means your denial matched the record. It does not mean they performed the action.");
                    if (claim == VerifierClaimType.NearBody)
                        yield return bot.Say("Near a body is only an observation at report time, not proof of a kill. Unknown means that body observation was unavailable; it is not a yes or a no.");
                }

                break;
            case Edgeveil:
            case PulseBlade:
                yield return bot.Say(role is Edgeveil ? "Face left or right toward the marked player, then press Arc. You can't turn it once the charge starts. Keep the others out of your line of fire." : "Get the marked player outlined, then press Strike. Your dash follows the direction chosen when you clicked, so watch their movement. Leave the others alive.");
                if (bot.Level > 1)
                {
                    foreach (var dummy in bot.Map.Dummies)
                        bot.Map.StartMovement(bot.Challenges.CoEvade(dummy));
                }

                for (var strike = 0; strike < bot.Level; strike++)
                {
                    var target = bot.Map.Dummies[strike];
                    bot.ExpectedVictim = target.PlayerId;
                    bot.ShowGoal(target);
                    yield return bot.WaitFor($"Land your ability on {target.Data.PlayerName}. Keep the others alive.", () => target.Data.IsDead, 120f);
                }

                break;
            case NecromancerRole:
                bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                yield return bot.WaitFor("Watch Practice 2 fall.", () => bot.Map.Dummies[1].Data.IsDead, 10f);
                bot.ShowGoal(bot.Map.Dummies[1]);
                yield return bot.Say(bot.Level >= 3
                    ? "Bring Practice 2 back. Practice 3 will chase you as you approach, so take a route that gives you time to reach the body."
                    : "Practice 2 needs a second chance. Someone else killed them, so you can help: walk up to the body and press Revive. You can't undo your own kills.");
                if (bot.Level >= 3)
                    bot.Map.StartMovement(bot.Challenges.CoAttack(bot.Map.Dummies[2], bot.Player));
                yield return bot.WaitFor("Revive the fallen dummy.", () => !bot.Map.Dummies[1].Data.IsDead);
                break;
            case Revenant:
                if (bot.Level > 1)
                    bot.Map.StartMovement(bot.Challenges.CoEvade(bot.Map.Dummies[0]));
                if (bot.Level >= 3)
                {
                    bot.Map.StartMovement(bot.Challenges.CoWatchFeign(bot.Map.Dummies[1]));
                    yield return bot.Say("A witness is patrolling. Feign away from them: if they reach your fake corpse before you revive, the trick is over.");
                }

                bot.ShowGoal(bot.Map.Dummies[0]);
                yield return bot.Say(bot.Level == 4
                    ? "Feign out of the witness's sight. After you revive, follow the arrow and get close before using Doom Awakening. Your target runs between rooms. Catch them with Doom's speed boost."
                    : "Use Feign Death and wait for your revival. A reported fake corpse ends the trick. After reviving, activate Doom Awakening and touch the marked dummy.");
                yield return bot.WaitFor("Feign death, revive, then catch the dummy during Doom.", () => Revenant.Phases.TryGetValue(id, out var phase) && phase == Revenant.Phase.DoomSpent);
                bot.LessonSucceeded = bot.Map.Dummies[0].Data.IsDead;
                if (!bot.LessonSucceeded)
                    bot.Failure = "Doom ran out before you caught the target. Get closer after reviving, then activate it and make contact.";
                break;
            case Voidwalker:
                yield return bot.Say(bot.Level == 1 ? "Enter the Void and move to the arrow. Return before attacking the dummy. You cannot attack while phased." : "Follow each arrow while phased. A hunter will chase you when you return. Finish the route, leave the Void beside your target, and kill them before the hunter catches you.");
                if (bot.Level > 1)
                    bot.Map.StartMovement(bot.Challenges.CoPursue(bot.Map.Dummies[1]));
                yield return bot.WaitFor("Enter the Void.", () => bot.Player.HasModifier<InVoid>(), 45f);
                for (var checkpoint = 0; checkpoint < bot.Level; checkpoint++)
                {
                    var point = bot.Map.Positions[checkpoint % bot.Map.Positions.Count];
                    bot.ShowGoal(point);
                    yield return bot.WaitFor("Reach the arrow while phased.", () => Vector2.Distance(bot.Player.GetTruePosition(), point) < 0.65f && bot.Player.HasModifier<InVoid>(), 30f);
                }

                bot.ShowGoal(bot.Map.Dummies[0]);
                yield return bot.WaitFor("Return from the Void beside your target.", () => !bot.Player.HasModifier<InVoid>() && Vector2.Distance(bot.Player.GetTruePosition(), bot.Map.Dummies[0].GetTruePosition()) < 1.5f, 30f);
                bot.AllowKills = true;
                bot.ExpectedVictim = bot.Map.Dummies[0].PlayerId;
                bot.Player.killTimer = 0f;
                yield return bot.WaitFor(bot.Level == 1 ? "Kill Practice 1 after leaving the Void." : "Kill Practice 1 before the hunter reaches you.", () => bot.Map.Dummies[0].Data.IsDead, 20f);
                break;
            case Deadwire:
                Deadwire.ResetState();
                yield return bot.Say("Mark Practice 1 with Deadlock. I'll stage an ability for you to record, then we'll try Override. I'll refill Deadlock between examples so we can cover all five types.");
                if (bot.Level > 1)
                    bot.Map.StartRoaming();
                var categories = System.Enum.GetValues<EnergyCategory>().OrderBy(category => bot.Level >= 3 ? Random.value : (float)category).ToArray();
                foreach (var category in categories)
                {
                    var explanation = category switch
                    {
                        EnergyCategory.Aggression => "Aggression cuts your kill cooldown. Watch the timer after Override.",
                        EnergyCategory.Intelligence => "Intelligence tracks the player who supplied the record.",
                        EnergyCategory.Mobility => "Mobility blinks in your movement direction. Face clear floor before Override.",
                        EnergyCategory.Control => "Control jams the recorded player's abilities for a short time.",
                        _ => "Protection gives you a temporary barrier.",
                    };
                    if (bot.Level < 3)
                        yield return bot.Say(explanation);
                    CustomButtonSingleton<DeadlockButton>.Instance.SetUses(1);
                    CustomButtonSingleton<DeadlockButton>.Instance.SetTimer(0f);
                    CustomButtonSingleton<OverrideButton>.Instance.SetTimer(0f);
                    yield return bot.WaitFor($"Mark the dummy to capture {category}.", () => Deadwire.MarkedPlayers.ContainsKey(id), 75f);
                    Deadwire.RpcRequestCapture(bot.Map.Dummies[0], (byte)category);
                    yield return bot.WaitFor($"Capture the {category} record.", () => Deadwire.Records.ContainsKey(id), 15f);
                    if (bot.Level >= 3)
                    {
                        var response = category switch
                        {
                            EnergyCategory.Aggression => "Reduce my kill cooldown",
                            EnergyCategory.Intelligence => "Track the recorded player",
                            EnergyCategory.Mobility => "Blink in my movement direction",
                            EnergyCategory.Control => "Jam the recorded player's abilities",
                            _ => "Gain a temporary barrier",
                        };
                        yield return CoQuestion(
                            $"You recorded {category}. What will Override do?",
                            response,
                            "Reduce my kill cooldown",
                            "Track the recorded player",
                            "Blink in my movement direction",
                            "Jam the recorded player's abilities",
                            "Gain a temporary barrier");
                        if (!bot.LessonSucceeded)
                            yield break;
                    }
                    if (category == EnergyCategory.Aggression)
                        bot.Player.killTimer = 20f;
                    yield return bot.WaitFor($"Use Override with the {category} record.", () => !Deadwire.Records.ContainsKey(id), 60f);
                }

                break;
            case EnergyThief:
                EnergyThief.ResetState();
                yield return bot.Say("Let's steal some energy. Siphon a player, then watch what they do. An ability gives you its energy type; letting the tether expire gives raw energy. I'll stage the ability uses for this exercise.");
                if (bot.Level > 1)
                    bot.Map.StartRoaming();
                var rawBefore = EnergyThief.Energy.GetValueOrDefault(id);
                yield return bot.Say("First, Siphon Practice 6 and let the tether expire. That gives raw energy without a category.");
                yield return bot.WaitFor("Siphon Practice 6 without an ability capture.", () => EnergyThief.TetherTargets.ContainsKey(id));
                if (!bot.LessonSucceeded)
                    yield break;
                if (EnergyThief.TetherTargets[id] != bot.Map.Dummies[5].PlayerId)
                {
                    bot.Failure = "Wrong volunteer. Use Practice 6 for the raw-energy example.";
                    yield break;
                }
                yield return bot.WaitFor("Wait for the raw energy.", () => EnergyThief.Energy.GetValueOrDefault(id) > rawBefore, OptionGroupSingleton<EnergyThiefOptions>.Instance.SiphonDuration + 10f);
                for (var categoryIndex = 0; categoryIndex < 5 && !EnergyThief.IsReady(id); categoryIndex++)
                {
                    var category = (EnergyCategory)categoryIndex;
                    var target = bot.Map.Dummies[categoryIndex];
                    target.SetName(category.ToString());
                    bot.ShowGoal(target);
                    yield return bot.WaitFor($"Siphon {category}.", () => EnergyThief.TetherTargets.ContainsKey(id), 90f);
                    if (!bot.LessonSucceeded)
                        yield break;
                    if (EnergyThief.TetherTargets[id] != target.PlayerId)
                    {
                        bot.Failure = $"That was the wrong target. Look for {category} before attaching Siphon.";
                        yield break;
                    }

                    EnergyThief.RpcRequestCapture(target, (byte)category, false);
                    yield return bot.WaitFor($"Collect {category} energy.", () => EnergyThief.Categories.TryGetValue(id, out var captured) && captured.Contains(category));
                }

                yield return bot.WaitFor("Find the Power Node.", () => EnergyThief.NodePositionIndex >= 0);
                bot.ShowGoal(EnergyThief.GetNodePosition());
                yield return bot.Say("You've filled the energy and category requirements. Follow the arrow to the Power Node and start Grid Breach. Stay inside its circle until it finishes; leaving or a meeting interrupts it.");
                if (bot.Level >= 3)
                {
                    bot.Map.StopMovements();
                    bot.Map.StartMovement(bot.Challenges.CoGroundNode(bot.Map.Dummies[5]));
                    yield return bot.Say("A defender will try to ground the node when you begin. Watch their distance and start when you have time to finish.");
                }

                yield return bot.WaitFor("Reach the Power Node and finish Grid Breach.", () => bot.Victory, 240f);
                break;
            case InjectorRole:
                if (bot.Level > 1)
                    bot.Map.StartRoaming();
                yield return bot.Say("Adrenaline speeds someone up; Sedative slows them down. I'll name the serum for each sample. Inject a fresh player, stay with them, and collect when observation finishes.");
                if (bot.Level >= 3)
                    yield return bot.Say($"These volunteers won't wait around. You'll have {(bot.Level == 4 ? 2 : 5)} seconds to collect after observation, then they'll cleanse the injection. Keep up with them.");
                for (var sample = 0; sample < OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount; sample++)
                {
                    var required = sample + 1;
                    var adrenaline = bot.Level >= 3 ? Random.value > 0.5f : sample % 2 == 0;
                    var serum = adrenaline ? InjectorRole.SerumType.Adrenaline : InjectorRole.SerumType.Sedative;
                    yield return bot.WaitFor($"Select {serum} before injecting.", () => InjectorUtilities.SelectedSerum == serum);
                    yield return bot.WaitFor($"Inject a new target with {serum}.", () => InjectorUtilities.Experiments.ContainsKey(id), 90f);
                    if (InjectorUtilities.Experiments[id].Serum != serum)
                    {
                        bot.Failure = $"That injection used the wrong serum. Select {serum} before injecting.";
                        yield break;
                    }

                    if (bot.Level >= 3)
                        bot.Map.StartMovement(bot.Challenges.CoCleanseExperiment(Utils.PlayerById(InjectorUtilities.Experiments[id].TargetId), InjectorUtilities.Experiments[id].ReadyAt));
                    yield return bot.WaitFor($"Collect sample {required} from the injected target.", () => InjectorUtilities.SampleCount(id) >= required, 150f);
                }

                yield return bot.Say("That's enough samples. Find a spot to stand still, then press Submit. Don't move until it finishes.");
                yield return bot.WaitFor("Submit your samples without moving.", () => InjectorUtilities.Submitted.Contains(id), 120f);
                break;
            case Prankster:
                yield return bot.Say($"Let's fool {bot.Map.Dummies.Count} {(bot.Map.Dummies.Count == 1 ? "volunteer" : "volunteers")}. Prank drops a fake body at your feet. Get someone to report it and you score. No meeting, no murder. You can only have one fake out at a time.");
                yield return bot.Say(bot.Level switch
                {
                    1 => "Practice 1 will stay put. Follow the arrow, drop a fake nearby, and watch them report it.",
                    2 => "They're walking a route this time. Drop your fake where they'll find it. Follow the arrow if you lose them.",
                    3 => "They're patrolling and checking for fakes. Wait for the distracted label before placing one near them. If they inspect it, you get no point.",
                    _ => "They'll inspect most fakes. You get a brief distracted window, and no arrow to follow. Watch their names and movement. Those labels are training hints; real players won't have them.",
                });
                foreach (var witness in bot.Map.Dummies)
                {
                    var name = witness.Data.PlayerName;
                    if (bot.Level < 4)
                        bot.ShowGoal(witness);
                    bot.Map.StartMovement(bot.Challenges.CoPrankWitness(witness));
                    yield return bot.WaitFor(
                        $"Place a fake near {name} and get them to report it.",
                        () => PranksterUtilities.FooledPlayers.Contains((id, witness.PlayerId)),
                        OptionGroupSingleton<PranksterOptions>.Instance.PrankCooldown + 60f);
                    bot.Map.StopMovements();
                    witness.SetName(name);
                    if (!bot.LessonSucceeded)
                        yield break;
                    var reports = PranksterUtilities.GetReportCount(id);
                    if (reports < bot.Map.Dummies.Count)
                        yield return bot.Say($"{name} fell for it! That's {reports} of {bot.Map.Dummies.Count}. They can't score twice. Next is {bot.Map.Dummies[reports].Data.PlayerName}; let Prank recharge before you try them.");
                }
                break;
            case Shade:
                yield return bot.Say($"Earn {bot.Map.Dummies.Count} kills inside active shadow zones. Both you and the target must be inside. The targets will try to escape on harder settings.");
                if (bot.Level > 1)
                {
                    foreach (var dummy in bot.Map.Dummies)
                        bot.Map.StartMovement(bot.Challenges.CoEvade(dummy));
                }

                for (var victim = 0; victim < bot.Map.Dummies.Count; victim++)
                {
                    var target = bot.Map.Dummies[victim];
                    bot.ExpectedVictim = target.PlayerId;
                    bot.ShowGoal(target);
                    yield return bot.WaitFor("Deploy a shadow zone.", () => ShadowZone.zones.Any(zone => zone && zone.shadeId == id), OptionGroupSingleton<ShadeOptions>.Instance.Cooldown + 60f);
                    if (bot.Level == 1)
                        yield return bot.Map.CoWalk(target, bot.Player.GetTruePosition());
                    var kills = victim + 1;
                    yield return bot.WaitFor("Catch the marked target inside your active zone.", () => target.Data.IsDead, OptionGroupSingleton<ShadeOptions>.Instance.Cooldown + OptionGroupSingleton<ShadeOptions>.Instance.Duration + 60f);
                    if (Shade.ShadeKills.GetValueOrDefault(id) < kills)
                    {
                        bot.Failure = "That kill was outside an active shadow zone. Both players must be inside when the hit lands.";
                        yield break;
                    }
                }

                break;
            case WraithCaller:
                bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                yield return bot.Say("There's your trace on the body. Collect it, then send a Wraith after the living player. If the hunt fails, you get the trace back for another try.");
                if (bot.Level > 1)
                {
                    bot.Map.Dummies[0].RpcSetRole((AmongUs.GameOptions.RoleTypes)RoleId.Get<MirrorBladeRole>());
                    bot.Map.StartMovement(bot.Challenges.CoReflectionWindows(bot.Map.Dummies[0]));
                    yield return bot.Say("This target can reflect your Wraith back at you. Their name shows when Reflect is active. Get close, watch the window, and summon so the Wraith arrives while they're open.");
                    if (bot.Level > 2)
                        bot.Map.StartRoaming();
                }

                yield return bot.WaitFor("Collect a trace and land a Wraith hunt.", () => WraithCallerUtilities.GetKillsNPC(id) > 0, 90f);
                break;
            case Tyrant:
                if (bot.Level > 1)
                    bot.Map.StartRoaming();
                yield return bot.Say("One kill unlocks Fear Pulse. Try it on the others, then work toward four kills. Leave one volunteer alive: you'll need someone to accept your Champion offer.");
                yield return bot.WaitFor("Earn one kill, then use Fear Pulse.", () => ((Tyrant)bot.Player.Data.Role).NextPulse > Time.time);
                yield return bot.WaitFor("Reach four kills, then offer the last living dummy an alliance.", () => Tyrant.ChampionId != byte.MaxValue);
                yield return CoQuestion("What happens if the Champion refuses?", "The shared win is unavailable", "The Champion dies", "You win alone");
                if (!bot.LessonSucceeded)
                    yield break;
                Tyrant.RpcAnswerOffer(Utils.PlayerById(Tyrant.ChampionId), true);
                yield return bot.Say("Your Champion accepted. Both of you must survive this meeting. Vote Skip. Losing either ally breaks the shared win.");
                yield return Meeting("Vote Skip and keep your Champion alive.", () => true);
                bot.LessonSucceeded &= !bot.Player.Data.IsDead && !Utils.PlayerById(Tyrant.ChampionId).Data.IsDead && Tyrant.Outcome == Tyrant.ThroneOutcome.ChampionSideWin;
                break;
            case Bounty:
                Bounty.AssignContract(id);
                bot.Map.StartMovement(bot.Challenges.CoFleeContract(bot.Map.Dummies[0]));
                yield return bot.Say("Stay close to your contract target until Collection starts. They'll get a head start while you're held still. Then chase them down and use Cash Out before the timer runs out.");
                var bountyOptions = OptionGroupSingleton<BountyOptions>.Instance;
                yield return bot.WaitFor("Stay with your target, then Cash Out during Collection.", () => bot.Victory, (bountyOptions.EscortDuration + bountyOptions.HeadStart + bountyOptions.CollectionDuration + 25f) * 2.5f);
                break;
            case Nomad:
                var nomadOptions = OptionGroupSingleton<NomadOptions>.Instance;
                yield return bot.Say($"Place an Anchor in a room. That counts as your first stop. We need {nomadOptions.RoomsPerRoute:0} different rooms per route; stay at each new stop for {nomadOptions.VisitDuration:0.#} seconds and travel far enough for it to count.");
                yield return bot.WaitFor("Place your Anchor.", () => Nomad.Anchors.ContainsKey(id));
                yield return bot.Say($"Wander lights up when the route is ready. Use it to return and score, then place a new Anchor for the next route. We'll do {nomadOptions.ScoreGoal:0} routes, then survive a meeting. Replacing the Anchor early resets your unfinished route.");
                for (var route = 0; route < nomadOptions.ScoreGoal; route++)
                {
                    var score = route + 1;
                    if (route > 0)
                        yield return bot.WaitFor("Place a new Anchor to start your next route.", () => Nomad.Anchors.ContainsKey(id) || Nomad.Scores.GetValueOrDefault(id) >= score);
                    yield return bot.WaitFor(
                        $"Visit different rooms until Wander is ready. Route {score}/{nomadOptions.ScoreGoal:0}.",
                        () => Nomad.CanWander.Contains(id) || Nomad.Scores.GetValueOrDefault(id) >= score,
                        nomadOptions.RoomsPerRoute * (nomadOptions.VisitDuration + 30f) * (2.5f - bot.Level * 0.3f));
                    yield return bot.WaitFor("Use Wander to return to your Anchor and score.", () => Nomad.Scores.GetValueOrDefault(id) >= score, nomadOptions.WanderCooldown + 30f);
                }

                yield return bot.Say("Your routes are complete. Stay alive through this meeting to finish.");
                yield return Meeting("Vote Skip and survive.", () => true);
                bot.LessonSucceeded &= bot.Victory;
                break;
            case Collector:
                yield return bot.Say("I'll leave fragments for you to harvest. Violence comes from normal kills, Ability from ability kills, and Fate from exile or environmental deaths. Let's turn a pair into something useful.");
                if (bot.Level == 4)
                    yield return bot.Say("You'll need to find the samples yourself this time. Search this room and use their colors to tell them apart. I'll name the type you need, but won't mark its position.");
                yield return CoHarvestFragments(Collector.FragmentKind.Violence, 2);
                yield return bot.Say("Two Violence fragments can make Trace. Use Manifest and watch for a highlighted fragment. Trace helps you find your next harvest.");
                var tracePoint = bot.Map.Positions[^1];
                Collector.RpcSpawnFragment(bot.Player, ++Collector._nextFragmentId, (byte)Collector.FragmentKind.Fate, tracePoint.x, tracePoint.y);
                yield return bot.WaitFor("Use Manifest to spend the Violence pair on Trace.", () => Collector.Inventories[id][0] == 0);
                yield return bot.WaitFor("Harvest the Fate fragment revealed by Trace.", () => Collector.Inventories[id][2] > 0);
                yield return CoHarvestFragments(Collector.FragmentKind.Fate, 1);
                yield return bot.Say("A Fate pair gives Drift instead. Use Manifest, then move to the arrow and feel the speed boost.");
                yield return bot.WaitFor("Use Manifest to spend the Fate pair on Drift.", () => Collector.Inventories[id][2] == 0);
                bot.ShowGoal(bot.Map.Positions[^1]);
                yield return bot.WaitFor("Move to the arrow with Drift.", () => Vector2.Distance(bot.Player.GetTruePosition(), bot.Map.Positions[^1]) < 0.65f);
                Object.Destroy(bot.GoalMarker.gameObject);
                yield return bot.Say("Ability fragments come from ability kills. Harvest this pair and use Manifest. Like a Violence pair, it gives Trace.");
                yield return CoHarvestFragments(Collector.FragmentKind.Ability, 2);
                var samplePoint = bot.Map.Positions[bot.Map.Dummies.Count];
                Collector.RpcSpawnFragment(bot.Player, ++Collector._nextFragmentId, (byte)Collector.FragmentKind.Violence, samplePoint.x, samplePoint.y);
                yield return bot.WaitFor("Use Manifest with the Ability pair.", () => Collector.Inventories[id][1] == 0);
                yield return bot.WaitFor("Harvest the Violence fragment revealed by Trace.", () => Collector.Inventories[id][0] > 0);
                var conversionCost = (int)OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost;
                yield return bot.Say($"Convert spends {conversionCost} duplicates to fill a missing type. You must keep one fragment of the type you spend. Keep the Violence fragment you just found and collect {conversionCost} more.");
                yield return CoHarvestFragments(Collector.FragmentKind.Violence, conversionCost);
                yield return bot.WaitFor("Use Convert to get your missing Ability fragment.", () => Collector.Inventories[id][1] > 0);
                yield return bot.Say("You now have Violence and Ability. Collect Fate for the full set. Manifesting one of each arms your win, but you still have to survive the next meeting.");
                yield return CoHarvestFragments(Collector.FragmentKind.Fate, 1);
                yield return bot.WaitFor("Use Manifest with one of each fragment.", () => Collector.VictoryArmed.Contains(id));
                yield return Meeting("Vote Skip and survive this meeting.", () => true);
                bot.LessonSucceeded &= bot.Victory;
                break;
            case Usurper:
                yield return bot.Say("Choose a volunteer with Claim. I'll arrange their death so you can practice taking the crown. Stand still beside it, with nobody else close, until it's yours.");
                yield return bot.WaitFor("Claim a dummy.", () => Usurper.States.TryGetValue(id, out var state) && state.Phase == Usurper.CrownPhase.Claimed);
                var claimed = Utils.PlayerById(Usurper.States[id].TargetId);
                var attacker = bot.Map.Dummies.First(dummy => dummy != claimed);
                attacker.RpcCustomMurder(claimed, MeetingCheck.OutsideMeeting, teleportMurderer: false);
                yield return bot.Map.CoWalk(attacker, bot.Map.Positions.OrderByDescending(point => Vector2.Distance(point, claimed.GetTruePosition())).First());
                if (bot.Level >= 3)
                {
                    bot.Map.StartMovement(bot.Challenges.CoContestCrown(attacker));
                    yield return bot.Say("The rival will contest the crown, then patrol away. Use that opening to collect it. Once you hold it, keep away from the rival until the timer ends.");
                }

                yield return bot.WaitFor("Stand still beside the crown, alone, to pick it up.", () => Usurper.States[id].Phase == Usurper.CrownPhase.Held);
                yield return bot.Say("The crown reveals you. Hold it until its timer finishes, then survive a meeting. Calling a meeting early only pauses the timer.");
                yield return bot.WaitFor("Keep the crown until its hold timer finishes.", () => Usurper.HoldRemaining.GetValueOrDefault(id) <= 0f, OptionGroupSingleton<UsurperOptions>.Instance.HoldDuration * 4f);
                yield return Meeting("Vote Skip and keep the crown through this meeting.", () => true);
                bot.LessonSucceeded &= bot.Victory;
                break;
            case TerminatorRole:
                yield return bot.Say("First we need to get you through the required meetings. I'll arrange safe votes. Once your objective appears, go to it and start Terminate. Then you have to stay alive until it finishes.");
                var requiredMeetings = (int)OptionGroupSingleton<TerminatorOptions>.Instance.MeetingsBeforeObjective;
                for (var meeting = 0; meeting < requiredMeetings; meeting++)
                {
                    yield return Meeting($"Survive meeting {meeting + 1} of {requiredMeetings}. Vote Skip.", () => true);
                    if (!bot.LessonSucceeded || bot.Player.Data.IsDead)
                        yield break;
                }

                if (bot.Level > 1)
                    yield return bot.Say("Each attacker can break one armor segment. Use other rooms to escape once the countdown starts. The attackers will chase you. If they strip all your armor, the trial ends.");
                TerminatorRole.RpcSpawnObjective(bot.Player, bot.Map.Center.x, bot.Map.Center.y);
                yield return bot.WaitFor("Activate the objective with Terminate.", () => TerminatorRole.FinalCountdownActive);
                if (bot.Level > 1)
                {
                    foreach (var dummy in bot.Map.Dummies)
                        bot.Map.StartMovement(bot.Challenges.CoHuntTerminator(dummy));
                }

                yield return bot.WaitFor(bot.Level == 1 ? "Survive until the countdown ends." : "Evade the attackers until the countdown ends.", () => bot.Victory || !TerminatorRole.FinalCountdownActive);
                bot.LessonSucceeded &= bot.Victory;
                break;
            case ArbitratorRole:
                yield return CoJudgments();
                break;
            case EgoistRole:
                var opposingVotes = bot.Level >= 3 ? 2 : 1;
                yield return bot.Say($"Let's get your Challenge button ready. {(opposingVotes == 1 ? "Practice 1" : "Practice 1 and Practice 2")} will vote for you; the others will skip. You skip too. I'll show you what happens to your Ego afterward.");
                var requiredEgo = (int)OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired;
                while (EgoistRole.States[id].Ego < requiredEgo)
                {
                    var previousEgo = EgoistRole.States[id].Ego;
                    for (var voter = 0; voter < bot.Map.Dummies.Count; voter++)
                        bot.Map.Dummies[voter].GetComponent<DummyBehaviour>().PlayerIdToVoteFor = voter < opposingVotes ? id : 253;
                    yield return Meeting($"Vote Skip. Planned votes against you: {opposingVotes}.\nEgo: {previousEgo}/{requiredEgo}.", () => true);
                    if (EgoistRole.States[id].Ego <= previousEgo)
                    {
                        bot.LessonSucceeded = false;
                        bot.Failure = "Ego did not increase. You must survive votes from other players. Vote Skip in this practice step.";
                        yield break;
                    }
                    yield return bot.Say($"You survived. Ego: {EgoistRole.States[id].Ego}/{requiredEgo}. " +
                        (EgoistRole.States[id].Ego < requiredEgo ? "We'll repeat the same safe voting plan." : "Challenge is ready for the next meeting."));
                }

                var opponent = bot.Map.Dummies[bot.Level == 4 ? Random.Range(0, bot.Map.Dummies.Count) : 0];
                yield return bot.Say($"Let's challenge {opponent.Data.PlayerName}. Press CHALLENGE, select them, and wait for LOCKED. Then vote for them normally. I've arranged enough votes to exile them if you follow that plan.");
                for (var voter = 0; voter < bot.Map.Dummies.Count; voter++)
                    bot.Map.Dummies[voter].GetComponent<DummyBehaviour>().PlayerIdToVoteFor = bot.Level >= 3 && voter < 2 ? id : opponent.PlayerId;
                yield return Meeting($"CHALLENGE > {opponent.Data.PlayerName} > LOCKED.\nThen vote for {opponent.Data.PlayerName}.", () => EgoistRole.States[id].OpponentId == opponent.PlayerId);
                bot.LessonSucceeded &= opponent.Data.IsDead;
                break;
            default:
                bot.LessonSucceeded = false;
                Error($"NewBot has no exercise for {role.GetType().Name}.");
                break;
        }
    }

    public IEnumerator Meeting(string instruction, System.Func<bool> completed, System.Func<string> instructionAfterOpening = null)
    {
        bot.Player.CmdReportDeadBody(null);
        var opened = Time.time + 5f;
        while (!MeetingHud.Instance && Time.time < opened)
            yield return null;
        if (!MeetingHud.Instance)
        {
            bot.LessonSucceeded = false;
            yield break;
        }

        while (MeetingHud.Instance && MeetingHud.Instance.CurrentState == MeetingHud.MeetingStates.Animating)
            yield return null;
        var position = bot.Objective.GetComponent<AspectPosition>();
        var previousAlignment = position.Alignment;
        var previousOffset = position.DistanceFromEdge;
        var previousSize = bot.Objective.rectTransform.sizeDelta;
        position.Alignment = AspectPosition.EdgeAlignments.Bottom;
        position.DistanceFromEdge = new Vector3(0f, 1.25f, -40f);
        position.AdjustPosition();
        bot.Objective.rectTransform.sizeDelta = new Vector2(7f, 0.7f);
        bot.Objective.text = instructionAfterOpening?.Invoke() ?? instruction;
        var performed = false;
        try
        {
            while (MeetingHud.Instance)
            {
                performed |= completed();
                bot.Objective.enabled = MeetingHud.Instance.CurrentState != MeetingHud.MeetingStates.Results && !Minigame.Instance;
                yield return null;
            }

            bot.Objective.text = string.Empty;
            while (ExileController.Instance)
                yield return null;
            bot.LessonSucceeded &= performed;
        }
        finally
        {
            bot.Objective.text = string.Empty;
            bot.Objective.enabled = true;
            bot.Objective.rectTransform.sizeDelta = previousSize;
            position.Alignment = previousAlignment;
            position.DistanceFromEdge = previousOffset;
            position.AdjustPosition();
        }
    }
}
