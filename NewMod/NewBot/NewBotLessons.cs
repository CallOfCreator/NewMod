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
            var position = bot.Map.Positions[bot.Map.Dummies.Count + (i + (int)kind) % (bot.Map.Positions.Count - bot.Map.Dummies.Count)];
            var target = Collector.Inventories[bot.Player.PlayerId][(int)kind] + 1;
            Collector.RpcSpawnFragment(bot.Player, ++Collector._nextFragmentId, (byte)kind, position.x, position.y);
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
        bot.Menu = NewBotMenu.Open(question, choices);
        while (bot.Menu.Component)
            yield return null;
        if (bot.Menu.Selected != correct)
        {
            bot.Failure = $"The answer was {answer}. Let's repeat the exercise and check the evidence.";
            bot.LessonSucceeded = false;
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
            var defend = round % 2 == 0;
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
        var description = (ICustomRole)role;
        yield return bot.Say($"Let's try {description.RoleName}.\n{description.RoleLongDescription}");
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
                NecromancerRole or WraithCaller or WardenRole or Collector or Usurper => 2,
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
            if (!succeeded)
            {
                if (Minigame.Instance)
                {
                    Minigame.Instance.ForceClose();
                    yield return null;
                }

                yield return bot.Map.CoPrepare(role, count);
                yield return bot.Say(bot.Failure ?? "That didn't work. I've reset the trial so you can try again.");
            }
            else
            {
                yield return bot.Say("Nicely done. Want another go, or a different role?");
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
                yield return bot.Say("Scan shows a snapshot of everyone else. Count the purple markers, then close the map. They will not follow players after the scan.");
                for (var scan = 0; scan < (bot.Level >= 3 ? 2 : 1); scan++)
                {
                    if (scan > 0)
                    {
                        Beacon.charges = 0;
                        yield return bot.Say($"Earn another charge by completing tasks. It takes {OptionGroupSingleton<BeaconOptions>.Instance.TasksPerCharge:0} tasks per charge. Return here when it is ready.");
                        yield return bot.WaitFor("Complete tasks until Scan has a charge.", () => Beacon.charges > 0, 600f);
                        yield return bot.WaitFor("Return to the practice area.", () => Vector2.Distance(bot.Player.GetTruePosition(), bot.Map.Center) < 2f, 180f);
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
                yield return bot.Say("Read each scan result. Presence counts living players within 5 units. Forensics separates close and distant bodies. Disturbance only detects violence from the last 15 seconds.");
                var modes = System.Enum.GetValues<Specialist.ScanMode>().OrderBy(_ => bot.Level >= 3 ? Random.value : 0f).ToArray();
                foreach (var mode in modes)
                {
                    Specialist.EarnScan(id);
                    yield return bot.WaitFor($"Choose {mode} mode.", () => Specialist.ScanStates[id].Mode == mode, 45f);
                    if (mode != Specialist.ScanMode.Presence)
                    {
                        foreach (var body in Object.FindObjectsOfType<DeadBody>())
                            Object.Destroy(body.gameObject);
                        bot.Map.Dummies[1].Revive();
                        bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                        if (bot.Level == 4 && mode == Specialist.ScanMode.Disturbance)
                        {
                            yield return bot.Say("That attack is old news now. Wait here until its trace expires, then check Disturbance.");
                            yield return new WaitForSeconds(16f);
                        }
                    }

                    bot.Observation = null;
                    var charges = Specialist.ScanStates[id].Charges;
                    yield return bot.WaitFor($"Use {mode} Scan and read the result.", () => Specialist.ScanStates[id].Charges < charges, 60f);
                    if (Specialist.ScanStates[id].Mode != mode || bot.Observation == null)
                    {
                        bot.Failure = $"That used the wrong mode. Select {mode} before scanning.";
                        yield break;
                    }

                    yield return new WaitForSeconds(bot.Level == 4 ? 1f : 3f);
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
                yield return bot.Say("You still play for the crew. Open Sabotage and choose Communications. Counterfeit Comms ends on its own; you cannot use the other sabotages.");
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
                yield return bot.Say("Place a camera covering Practice 2. I'll stage an attack there while it charges. Collect the photo with the body in frame, then show that evidence in a meeting.");
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
                yield return bot.Say("Seal this room first. I'll stage an attack inside it. Then inspect the Residual Trace to learn what the room recorded.");
                yield return bot.WaitFor("Use Seal in this room.", () => WardenRole.SealActive);
                bot.Map.Dummies[0].RpcCustomMurder(bot.Map.Dummies[1], MeetingCheck.OutsideMeeting, teleportMurderer: false);
                yield return null;
                if (WardenRole.ResidualMarks.Count == 0)
                {
                    bot.LessonSucceeded = false;
                    break;
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
                var claims = System.Enum.GetValues<VerifierClaimType>().Where(claim => claim != VerifierClaimType.None);
                foreach (var claim in claims.OrderBy(_ => bot.Level >= 3 ? Random.value : 0f))
                {
                    bot.VerificationClaim = claim;
                    bot.VerificationExpected = bot.Level < 3 || Random.value > 0.5f;
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
                    if (claim == VerifierClaimType.NearBody)
                        VerifierUtilities.RegisterNearBody();
                    bot.VerificationExpected = (VerifierUtilities.RoundFacts.GetValueOrDefault(verified.PlayerId) & VerifierUtilities.GetFlag(claim)) != 0;
                    var evidence = bot.VerificationExpected ? $"Evidence confirms that {verified.Data.PlayerName} {action}." : $"There is no record that {verified.Data.PlayerName} {action}.";
                    yield return bot.Say($"{evidence} Verify that claim in the next meeting, then skip.");
                    yield return Meeting(string.Empty, () => VerifierUtilities.UsedThisMeeting, () =>
                    {
                        bot.VerificationExpected = (VerifierUtilities.RoundFacts.GetValueOrDefault(verified.PlayerId) & VerifierUtilities.GetFlag(claim)) != 0;
                        return $"{verified.Data.PlayerName}: {action}. Select the {(bot.VerificationExpected ? "green check" : "red cross")}, then vote Skip.";
                    });
                    if (!bot.LessonSucceeded)
                        yield break;
                }

                break;
            case Edgeveil:
            case PulseBlade:
                yield return bot.Say(role is Edgeveil ? "Use Arc on the marked target. Your direction locks during the charge. Leave the other players alive." : "Use Strike on the marked target. Aim for where they are going; the dash direction locks during the charge. Leave the other players alive.");
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
                yield return bot.Say("This dummy was killed by someone else. Approach the body and use Revive. You cannot revive a player you killed yourself.");
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
                yield return bot.Say("Deadlock records a marked player's next ability. Override spends that record. I'll simulate each category on this dummy so you can try all five responses.");
                for (var recordIndex = 0; recordIndex < 5; recordIndex++)
                {
                    var category = (EnergyCategory)recordIndex;
                    var explanation = category switch
                    {
                        EnergyCategory.Aggression => "Aggression cuts your kill cooldown. Watch the timer after Override.",
                        EnergyCategory.Intelligence => "Intelligence tracks the player who supplied the record.",
                        EnergyCategory.Mobility => "Mobility blinks in your movement direction. Face clear floor before Override.",
                        EnergyCategory.Control => "Control jams the recorded player's abilities for a short time.",
                        _ => "Protection gives you a temporary barrier.",
                    };
                    yield return bot.Say(explanation);
                    foreach (var button in CustomButtonManager.Buttons)
                        button.SetTimer(0f);
                    yield return bot.WaitFor($"Mark the dummy to capture {category}.", () => Deadwire.MarkedPlayers.ContainsKey(id), 75f);
                    Deadwire.RpcRequestCapture(bot.Map.Dummies[0], (byte)category);
                    yield return bot.WaitFor($"Capture the {category} record.", () => Deadwire.Records.ContainsKey(id), 15f);
                    if (category == EnergyCategory.Aggression)
                        bot.Player.killTimer = 20f;
                    yield return bot.WaitFor($"Use Override with the {category} record.", () => !Deadwire.Records.ContainsKey(id), 60f);
                }

                break;
            case EnergyThief:
                EnergyThief.ResetState();
                yield return bot.Say("Siphon takes energy from another player. An ability gives typed energy and causes a brownout; waiting gives raw energy. I'll simulate ability use on these targets so you can practise collecting each type.");
                if (bot.Level > 1)
                    bot.Map.StartRoaming();
                var rawBefore = EnergyThief.Energy.GetValueOrDefault(id);
                yield return bot.Say("First, Siphon Practice 6 and let the tether expire. That gives raw energy without a category.");
                yield return bot.WaitFor("Siphon Practice 6 without an ability capture.", () => EnergyThief.TetherTargets.TryGetValue(id, out var rawTarget) && rawTarget == bot.Map.Dummies[5].PlayerId);
                yield return bot.WaitFor("Wait for the raw energy.", () => EnergyThief.Energy.GetValueOrDefault(id) > rawBefore, OptionGroupSingleton<EnergyThiefOptions>.Instance.SiphonDuration + 10f);
                for (var categoryIndex = 0; categoryIndex < 5; categoryIndex++)
                {
                    var category = (EnergyCategory)categoryIndex;
                    var target = bot.Map.Dummies[categoryIndex];
                    target.SetName(category.ToString());
                    bot.ShowGoal(target);
                    yield return bot.WaitFor($"Siphon {category}.", () => EnergyThief.TetherTargets.ContainsKey(id), 90f);
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
                yield return bot.Say("You have enough energy and types. Follow the arrow to the Power Node, start Grid Breach, and stay inside its radius. Moving away or a meeting interrupts it. Other players can ground the node.");
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
                yield return bot.Say("Adrenaline speeds a target up. Sedative slows them down. Choose a serum, inject, then collect when observation finishes. A target can cleanse the experiment, and each sample must come from a different player.");
                for (var sample = 0; sample < OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount; sample++)
                {
                    var required = sample + 1;
                    var serum = sample % 2 == 0 ? InjectorRole.SerumType.Adrenaline : InjectorRole.SerumType.Sedative;
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

                yield return bot.Say("You have enough samples. Use Submit and hold still until it finishes. Moving, dying, or a meeting can stop submission.");
                yield return bot.WaitFor("Submit your samples without moving.", () => InjectorUtilities.Submitted.Contains(id), 120f);
                break;
            case Prankster:
                yield return bot.Say($"Fool {bot.Map.Dummies.Count} different witnesses. Each witness only counts once. Place fake bodies with Prank. A distracted witness reports; an alert one inspects and removes the fake.");
                foreach (var witness in bot.Map.Dummies)
                    bot.Map.StartMovement(bot.Challenges.CoPrankWitness(witness));
                yield return bot.WaitFor("Fool each witness while they are distracted.", () => PranksterUtilities.GetReportCount(id) >= bot.Map.Dummies.Count, bot.Map.Dummies.Count * (OptionGroupSingleton<PranksterOptions>.Instance.PrankCooldown + 60f));
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
                yield return bot.Say("Collect the trace from the body, then choose the living dummy for your Wraith. You need a fresh trace for each hunt. A failed hunt returns your trace.");
                if (bot.Level > 1)
                {
                    bot.Map.Dummies[0].RpcSetRole(CustomRoleManager.CustomMiraRoles.OfType<MirrorBladeRole>().Single().Role);
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
                yield return bot.Say("Earn a kill to unlock Fear Pulse. Use it to pressure the remaining targets. Four kills unlock the Champion offer; keep one dummy alive for that offer.");
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
                yield return bot.Say("Follow your contract target closely until Collection begins. After their head start, catch them and use Cash Out before the collection timer ends.");
                var bountyOptions = OptionGroupSingleton<BountyOptions>.Instance;
                yield return bot.WaitFor("Stay with your target, then Cash Out during Collection.", () => bot.Victory, (bountyOptions.EscortDuration + bountyOptions.HeadStart + bountyOptions.CollectionDuration + 25f) * 2.5f);
                break;
            case Nomad:
                var nomadOptions = OptionGroupSingleton<NomadOptions>.Instance;
                yield return bot.Say($"Place an Anchor, then visit {nomadOptions.RoomsPerRoute:0} different rooms. Stay at each stop long enough for it to count. Wander returns you to the Anchor and scores the route. You need {nomadOptions.ScoreGoal:0} routes, then a meeting.");
                for (var route = 0; route < nomadOptions.ScoreGoal; route++)
                {
                    var score = route + 1;
                    yield return bot.WaitFor($"Complete route {score}, then Wander back.", () => Nomad.Scores.GetValueOrDefault(id) >= score, nomadOptions.RoomsPerRoute * (nomadOptions.VisitDuration + 30f) * 2.5f);
                }

                yield return bot.Say("Your routes are complete. Stay alive through this meeting to finish.");
                yield return Meeting("Vote Skip and survive.", () => true);
                bot.LessonSucceeded &= bot.Victory;
                break;
            case Collector:
                yield return bot.Say("Harvest gathers fragments. Normal kills leave Violence, ability kills leave Ability, and exile or environmental deaths leave Fate. I'll place samples so you can practise each use.");
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
                yield return bot.Say("Claim one of the dummies. I'll arrange that target's death. Then stand still beside the crown, alone, until you collect it.");
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
                yield return bot.Say("Survive the required meetings to reveal your objective. Once it appears, activate Terminate and survive the countdown.");
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
                yield return bot.Say("Ego comes from other players voting against you while you survive. Some dummies will vote for you; the rest will skip. Vote Skip too, and watch your Ego grow.");
                var requiredEgo = (int)OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired;
                while (EgoistRole.States[id].Ego < requiredEgo)
                {
                    var previousEgo = EgoistRole.States[id].Ego;
                    var opposingVotes = bot.Level >= 3 ? 2 : 1;
                    for (var voter = 0; voter < bot.Map.Dummies.Count; voter++)
                        bot.Map.Dummies[voter].GetComponent<DummyBehaviour>().PlayerIdToVoteFor = voter < opposingVotes ? id : 253;
                    yield return Meeting("Vote Skip. Survive the vote to gain Ego.", () => true);
                    if (EgoistRole.States[id].Ego <= previousEgo)
                    {
                        bot.LessonSucceeded = false;
                        yield break;
                    }
                }

                yield return bot.Say("Your Ego is full. Challenge Practice 1 in the next meeting, then vote for them. A challenge commits you to beating that opponent in the vote.");
                for (var voter = 0; voter < bot.Map.Dummies.Count; voter++)
                    bot.Map.Dummies[voter].GetComponent<DummyBehaviour>().PlayerIdToVoteFor = bot.Level >= 3 && voter < 2 ? id : bot.Map.Dummies[0].PlayerId;
                yield return Meeting("Challenge Practice 1, then vote for them. Some votes will be against you.", () => EgoistRole.States[id].OpponentId == bot.Map.Dummies[0].PlayerId);
                bot.LessonSucceeded &= bot.Map.Dummies[0].Data.IsDead;
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
        bot.Objective.text = instructionAfterOpening?.Invoke() ?? instruction;
        var performed = false;
        while (MeetingHud.Instance)
        {
            performed |= completed();
            yield return null;
        }

        while (ExileController.Instance)
            yield return null;
        bot.Objective.text = string.Empty;
        bot.LessonSucceeded &= performed;
    }
}
