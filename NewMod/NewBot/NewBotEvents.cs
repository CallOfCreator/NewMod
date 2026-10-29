using System;
using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Buttons.Roles;
using NewMod.Buttons.Roles.S1;
using NewMod.Components;
using NewMod.GeneralEvents;
using NewMod.GeneralEvents.Season1;
using NewMod.Options;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.UI;
using PathfindingAPI.Navigation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewMod.NewBot;

public sealed class NewBotEvents(NewBot bot)
{
    public bool Running;
    public IGeneralEvent ActiveEvent;
    public (OpenableDoor Door, bool Open)[] Doors = [];

    public IEnumerator CoBrowse()
    {
        IGeneralEvent[] events =
        [
            new AbilityExchangeGE(),
            new CrismonVortexGE(),
            new IdentityCrisisGE(),
            new NegativeRealityGE(),
            new NoMansLandGE(),
            new RoleScrambleGE(),
            new ScrDesyncEffectGE(),
            new SystemOverrideGE(),
        ];
        var choices = events.Select(ge => new NewBotMenu.Choice(ge.Title, ge.AccentColor)).ToList();
        var back = new NewBotMenu.Choice("Categories", Color.white);
        choices.Add(back);
        yield return bot.Say("General events affect the whole match. Pick one to see its warning, try the response, and learn what changes when it ends.");
        while (true)
        {
            bot.Menu = NewBotMenu.Open("General events", choices);
            while (bot.Menu.Component)
                yield return null;
            if (bot.Menu.Selected == null || bot.Menu.Selected == back)
                yield break;
            bot.Level = bot.Extreme ? 4 : 1;
            yield return CoPractice(events[choices.IndexOf(bot.Menu.Selected)]);
        }
    }

    public IEnumerator CoPractice(IGeneralEvent ge)
    {
        var again = new NewBotMenu.Choice("Try again", Color.white);
        var harder = new NewBotMenu.Choice(ge is CrismonVortexGE ? "Stronger vortex" : "Longer exercise", new Color(1f, 0.75f, 0.3f));
        var hasHarderExercise = ge is IdentityCrisisGE or NegativeRealityGE or ScrDesyncEffectGE or NoMansLandGE or CrismonVortexGE;
        var back = new NewBotMenu.Choice("Other events", Color.white);
        while (true)
        {
            var role = ge is AbilityExchangeGE ? RoleId.GetRole<MirrorBladeRole>() : RoleId.GetRole<Aegis>();
            yield return bot.Map.CoPrepare(role, 2, chooseRoom: true);
            if (bot.Map.Positions.Count == 0)
            {
                yield return bot.Say("I couldn't set up the practice area. Choose the event again to retry.");
                yield break;
            }

            Running = true;
            bot.LessonRunning = true;
            bot.LessonSucceeded = true;
            bot.AllowKills = false;
            bot.Failure = null;
            Doors = ge is SystemOverrideGE ? ShipStatus.Instance.AllDoors.Select(door => (door, door.IsOpen)).ToArray() : [];
            var objectivePosition = bot.Objective.GetComponent<AspectPosition>();
            var previousOffset = objectivePosition.DistanceFromEdge;
            objectivePosition.DistanceFromEdge = new Vector3(0f, 1.9f, -40f);
            objectivePosition.AdjustPosition();
            try
            {
                yield return CoLesson(ge);
            }
            finally
            {
                Clear();
                if (objectivePosition)
                {
                    objectivePosition.DistanceFromEdge = previousOffset;
                    objectivePosition.AdjustPosition();
                }
            }

            var succeeded = bot.LessonSucceeded && !bot.Player.Data.IsDead;
            bot.Player.Revive();
            bot.Player.RpcSetRole(RoleTypes.Crewmate);
            bot.Map.ResetPractice();
            bot.Player.NetTransform.RpcSnapTo(bot.Map.Center - bot.Player.Collider.offset);
            yield return bot.Say(succeeded ? "You handled the event. Try it again, or choose another one." : bot.Failure ?? "The event caught you. Try again and watch for the warning.");
            bot.Menu = NewBotMenu.Open(ge.Title, succeeded && hasHarderExercise && bot.Progressive && bot.Level < 4 ? [again, harder, back] : [again, back]);
            while (bot.Menu.Component)
                yield return null;
            if (bot.Menu.Selected == null || bot.Menu.Selected == back)
                yield break;
            if (bot.Menu.Selected == harder)
                bot.Level++;
        }
    }

    public IEnumerator CoLesson(IGeneralEvent ge)
    {
        switch (ge)
        {
            case AbilityExchangeGE:
                yield return CoAbilityExchange(ge);
                break;
            case CrismonVortexGE vortex:
                yield return CoVortex(vortex);
                break;
            case IdentityCrisisGE:
                yield return CoIdentity(ge);
                break;
            case NoMansLandGE:
                yield return CoCeasefire(ge);
                break;
            case RoleScrambleGE:
                yield return CoScramble(ge);
                break;
            case SystemOverrideGE:
                yield return CoDoors(ge);
                break;
            case NegativeRealityGE or ScrDesyncEffectGE:
                yield return CoVisibility(ge);
                break;
        }
    }

    public void Start(IGeneralEvent ge)
    {
        if (!ge.CanOccur())
        {
            bot.Failure = "This event cannot start with the current map and players.";
            bot.LessonSucceeded = false;
            return;
        }

        GeneralEventManager.StopCycle();
        GeneralEventManager.ForceEnd();
        ActiveEvent = ge;
        GeneralEventManager.StartEvent(ge, 0);
    }

    public void Clear()
    {
        if (!Running)
            return;
        Running = false;
        bot.LessonRunning = false;
        if (ReferenceEquals(GeneralEventManager.CurrentEvent, ActiveEvent))
            GeneralEventManager.EndEvent(0);
        ActiveEvent = null;
        bot.Map.StopMovements();
        foreach (var (door, open) in Doors)
        {
            if (door && door.IsOpen != open)
                door.SetDoorway(open);
        }

        Doors = [];
        if (bot.GoalMarker)
            Object.Destroy(bot.GoalMarker.gameObject);
        if (bot.Objective)
            bot.Objective.text = string.Empty;
    }

    public IEnumerator CoWait(string instruction, Func<bool> completed, float seconds, bool duringEvent = false)
    {
        bot.Objective.text = instruction;
        while (bot.LessonSucceeded && bot.Failure == null && !bot.Player.Data.IsDead && !completed())
        {
            if (seconds <= 0f || (duringEvent && !ReferenceEquals(GeneralEventManager.CurrentEvent, ActiveEvent)))
            {
                bot.Failure = duringEvent ? "The event ended before you finished. Try the same exercise again." : "That step took too long. Let's try again.";
                break;
            }
            if (!bot.IsPaused)
                seconds -= Time.deltaTime;
            yield return null;
        }
        bot.LessonSucceeded &= bot.Failure == null && !bot.Player.Data.IsDead;
        bot.Objective.text = string.Empty;
    }

    public IEnumerator CoAbilityExchange(IGeneralEvent ge)
    {
        bot.Map.Dummies[0].RpcSetRole(RoleId.GetType<Aegis>());
        yield return bot.Say("You're MirrorBlade. Ability Exchange will lend you Aegis's Sentinel Ward. Your team and win condition stay the same. Place the borrowed ward before the event ends.");
        Start(ge);
        yield return CoWait("Place the borrowed Sentinel Ward.", () => ShieldArea._active.Any(area => area && area.ownerId == bot.Player.PlayerId), ge.Duration, true);
        if (!bot.LessonSucceeded)
            yield break;
        yield return CoWait("Watch the event banner. Your own ability returns when it ends.", () => !AbilityExchangeGE.Active, ge.Duration + 2f);
        yield return bot.Lessons.CoQuestion("What stayed the same?", "My role and team", "My ability", "Everyone's role");
        if (!bot.LessonSucceeded)
            yield break;
        yield return bot.Say("Reflect is back. Activate it to check that your own ability has returned.");
        CustomButtonSingleton<MirrorReflectButton>.Instance.SetTimer(0f);
        yield return CoWait("Activate Reflect.", () => MirrorBladeRole.ArmedReflections.Contains(bot.Player.PlayerId), 40f);
    }

    public IEnumerator CoCeasefire(IGeneralEvent ge)
    {
        yield return bot.Say("No Man's Land blocks combat, role abilities, vents, sabotage, reporting, and the Use button. You can still walk. Move to the arrow, then wait for your abilities to return. Players already in the Void are exempt.");
        Start(ge);
        yield return CoCourse(2 + bot.Level, true);
        if (!bot.LessonSucceeded)
            yield break;
        yield return CoWait("Stay ready. Abilities return when the event ends.", () => !NoMansLandGE.Active, ge.Duration + 2f);
        yield return bot.Say("The event has ended. Place Sentinel Ward now.");
        yield return CoWait("Place Sentinel Ward.", () => ShieldArea._active.Any(area => area && area.ownerId == bot.Player.PlayerId), 40f);
        if (bot.LessonSucceeded)
            yield return bot.Lessons.CoQuestion("What can you do during No Man's Land?", "Move to safety", "Report a body", "Use a role ability");
    }

    public IEnumerator CoVisibility(IGeneralEvent ge)
    {
        yield return bot.Say(ge is NegativeRealityGE
            ? "Negative Reality changes the screen's colors. Walls and controls stay where they were. Follow the arrows using the room's layout, then check that you still have your role ability."
            : "Screen Desynchronization distorts the image in bursts. It does not teleport players or change the walls. Follow the arrows and keep your bearings between bursts.");
        Start(ge);
        yield return CoCourse(2 + bot.Level, true);
        if (!bot.LessonSucceeded)
            yield break;
        yield return CoWait("Place Sentinel Ward while the screen effect is active.", () => ShieldArea._active.Any(area => area && area.ownerId == bot.Player.PlayerId), ge.Duration, true);
        if (!bot.LessonSucceeded)
            yield break;
        yield return CoWait("Watch the screen return to normal.", () => GeneralEventManager.CurrentEvent == null, ge.Duration + 2f);
        yield return bot.Lessons.CoQuestion("What did the event change?", "The screen effect", "The room's walls", "My controls");
    }

    public IEnumerator CoCourse(int count, bool duringEvent)
    {
        var previous = bot.Player.GetTruePosition();
        for (var step = 0; step < count && bot.LessonSucceeded; step++)
        {
            var destination = bot.Map.RoamingPositions.Where(point => Vector2.Distance(point, previous) > 1f)
                .OrderBy(point => Mathf.Abs(Vector2.Distance(point, previous) - 2.5f)).First();
            bot.ShowGoal(destination);
            yield return CoWait($"Reach the arrow. {step + 1}/{count}", () => Vector2.Distance(bot.Player.GetTruePosition(), destination) < 0.65f, 35f, duringEvent);
            previous = destination;
        }

        if (bot.GoalMarker)
            Object.Destroy(bot.GoalMarker.gameObject);
    }

    public IEnumerator CoIdentity(IGeneralEvent ge)
    {
        var target = bot.Map.Dummies[0];
        bot.ShowGoal(target);
        yield return bot.Say("Watch Practice 1 before you close this dialog. Identity Crisis passes living players' outfits and displayed names to another player. Players in the Void keep their identity. Keep track of this dummy by following its movement.");
        if (bot.GoalMarker)
            Object.Destroy(bot.GoalMarker.gameObject);
        Start(ge);
        if (!bot.LessonSucceeded)
            yield break;
        bot.Map.StartRoaming();
        var following = 0f;
        var required = 3f + bot.Level;
        bot.Objective.text = "Stay close to the original Practice 1, even with a different name.";
        var remaining = ge.Duration;
        while (bot.LessonSucceeded && !bot.Player.Data.IsDead && remaining > 0f && following < required && ReferenceEquals(GeneralEventManager.CurrentEvent, ge))
        {
            if (!bot.IsPaused)
            {
                remaining -= Time.deltaTime;
                var nearby = Vector2.Distance(bot.Player.GetTruePosition(), target.GetTruePosition()) < 1.2f;
                var visible = !PhysicsHelpers.AnythingBetween(bot.Player.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false);
                following = nearby && visible ? following + Time.deltaTime : 0f;
            }
            yield return null;
        }
        bot.Map.StopMovements();
        bot.Objective.text = string.Empty;
        if (following < required)
        {
            bot.LessonSucceeded = false;
            bot.Failure = "You lost the original Practice 1. Watch where they start, then follow their movement rather than their name.";
            yield break;
        }
        yield return CoWait("Watch the original outfits return.", () => GeneralEventManager.CurrentEvent == null, ge.Duration + 2f);
        yield return bot.Lessons.CoQuestion("Did the disguise change anyone's role?", "No, only their appearance", "Yes, their role changed", "Yes, their team changed");
    }

    public IEnumerator CoScramble(IGeneralEvent ge)
    {
        bot.Map.Dummies[0].RpcSetRole(RoleId.GetType<MirrorBladeRole>());
        bot.Map.Dummies[1].RpcSetRole(RoleTypes.Crewmate);
        yield return bot.Say("You start as Aegis. Role Scramble shuffles living players' roles, except players in the Void. Your role may stay the same, but any change lasts after the banner disappears. Check your role name and team before acting.");
        Start(ge);
        yield return CoWait("Read your new role and check your buttons.", () => GeneralEventManager.CurrentEvent == null, ge.Duration + 2f);
        var role = bot.Player.Data.Role;
        yield return bot.Lessons.CoQuestion("Which role do you have now?", role.GetRoleName(), RoleId.GetRole<Aegis>().GetRoleName(), RoleId.GetRole<MirrorBladeRole>().GetRoleName(), RoleManager.Instance.GetRole(RoleTypes.Crewmate).GetRoleName());
        if (!bot.LessonSucceeded)
            yield break;
        yield return bot.Lessons.CoQuestion("Which team are you on now?", role.IsImpostor ? "Impostor" : "Crewmate", "Impostor", "Crewmate");
        if (!bot.LessonSucceeded)
            yield break;
        if (role is Aegis)
        {
            CustomButtonSingleton<AegisButton>.Instance.SetTimer(0f);
            yield return bot.Say("You're Aegis. Place Sentinel Ward to protect your team.");
            yield return CoWait("Place Sentinel Ward.", () => ShieldArea._active.Any(area => area && area.ownerId == bot.Player.PlayerId), 40f);
        }
        else if (role is MirrorBladeRole)
        {
            CustomButtonSingleton<MirrorReflectButton>.Instance.SetTimer(0f);
            yield return bot.Say("You're MirrorBlade, on the impostor team. Try Reflect. In a match, your goal is now to help the impostors win.");
            yield return CoWait("Activate Reflect.", () => MirrorBladeRole.ArmedReflections.Contains(bot.Player.PlayerId), 40f);
        }
        else
        {
            yield return bot.Say("You're a Crewmate. You no longer have Sentinel Ward. Your next job is to finish tasks and identify the impostors.");
            yield return CoCourse(2, false);
        }
        if (bot.LessonSucceeded)
            yield return bot.Lessons.CoQuestion("When does your old role return?", "It does not return automatically", "When the banner disappears", "At the next meeting");
    }

    public IEnumerator CoDoors(IGeneralEvent ge)
    {
        yield return bot.Map.CoFindEscapeRoutes();
        var destinations = bot.Map.EscapePositions.ToArray();
        yield return bot.Say(Doors.Length == 0
            ? "System Override flips and briefly blacks out the screen while trying to close ship doors. This map has no closable ship doors. Keep your bearings as the image changes; your movement controls stay the same."
            : "System Override flips and briefly blacks out the screen, and closes random rooms every few seconds. Your movement controls stay the same. Skeld doors reopen on a timer; other maps may require you to open them.");
        Start(ge);
        if (!bot.LessonSucceeded)
            yield break;
        ShipStatus.Instance.RpcCloseDoorsOfType(bot.Map.Room.RoomId);
        if (destinations.Length > 0)
        {
            var destination = destinations[0];
            bot.ShowGoal(destination);
            yield return CoWait("Reach the marked room. Deal with closed doors on the way.", () => Vector2.Distance(bot.Player.GetTruePosition(), destination) < 0.8f, 90f);
        }
        else
        {
            yield return CoCourse(2 + bot.Level, true);
        }
        if (!bot.LessonSucceeded)
            yield break;
        yield return CoWait("Wait for the door closures to stop.", () => !SystemOverrideGE.Active, ge.Duration + 2f);
        yield return bot.Lessons.CoQuestion("What does System Override do?", "Flips the view and closes doors", "Starts oxygen sabotage", "Disables all abilities");
    }

    public IEnumerator CoVortex(CrismonVortexGE ge)
    {
        var radius = OptionGroupSingleton<GEOptions>.Instance.CrimsonRadius.Value;
        var killRadius = Mathf.Clamp(radius * 0.12f, 0.45f, 0.9f);
        yield return bot.Map.CoFindEscapeRoutes();
        var safePoints = bot.Map.EscapePositions.Concat(bot.Map.RoamingPositions).Where(point => Vector2.Distance(point, bot.Map.Center) > radius + 1f).ToArray();
        var probe = MapPathfinding.CreateRequest(bot.Map.Center, bot.Map.Center, bot.Map.WalkingOptions);
        ge.PracticePullStrength = bot.Level switch
        {
            1 => 0.35f,
            2 => 0.55f,
            3 => 0.8f,
            _ => 1.15f,
        };
        ge.PracticeEscapeDifficulty = bot.Level switch
        {
            1 => GEOptions.EscapeDifficulty.Easy,
            2 or 3 => GEOptions.EscapeDifficulty.Standard,
            _ => GEOptions.EscapeDifficulty.Hard,
        };
        var entryRadius = Mathf.Max(killRadius + 0.65f, radius * (bot.Level == 4 ? 0.68f : 0.72f));
        var caughtPoints = Enumerable.Range(0, 16)
            .Select(index => bot.Map.Center + new Vector2(Mathf.Cos(index * Mathf.PI / 8f), Mathf.Sin(index * Mathf.PI / 8f)) * entryRadius)
            .Where(point => bot.Map.Room.roomArea.OverlapPoint(point) && probe.IsClear(point, bot.Map.WalkingOptions.Radius + 0.2f) && probe.CanMove(bot.Map.Center, point)).ToArray();
        if (safePoints.Length == 0 || caughtPoints.Length == 0)
        {
            bot.LessonSucceeded = false;
            bot.Failure = "This room has no safe escape route for the current vortex size. Retry to set up in another room.";
            yield break;
        }

        var safe = safePoints.OrderBy(point => Vector2.Distance(point, bot.Map.Center)).First();
        var caught = caughtPoints.OrderBy(point => Vector2.SqrMagnitude(point - safe)).First();
        yield return bot.Say(bot.Level == 1
            ? "We'll start near the outer edge of the escape zone, with a gentle pull and an easier escape bar. After the warning, I'll move you there. Keep pressing Space, or tap repeatedly on Android, until you break free. The center is lethal."
            : "This round has a stronger pull. I'll place you near the outer edge of the escape zone after the warning. Start filling the escape bar immediately and move away from the center. Vents will not protect you.");
        bot.Player.MyPhysics.body.velocity = Vector2.zero;
        bot.Player.NetTransform.RpcSnapTo(safe - bot.Player.Collider.offset);
        yield return null;
        Start(ge);
        if (!bot.LessonSucceeded)
            yield break;
        CrismonVortexGE.RpcSetVortexPosition(bot.Player, bot.Map.Center.x, bot.Map.Center.y);
        var vortex = bot.Player.GetComponent<CrismonVortexPhysics>();
        var escaped = vortex.EscapeCount;
        yield return CoWait("Stay outside the vortex until the warning ends.", () => !CrismonVortexGE.IsWarning(), 5f, true);
        if (!bot.LessonSucceeded)
            yield break;
        bot.Player.MyPhysics.body.velocity = Vector2.zero;
        bot.Player.NetTransform.RpcSnapTo(caught - bot.Player.Collider.offset);
        bot.ShowGoal(safe);
        yield return CoWait(Application.platform == RuntimePlatform.Android ? "Tap rapidly to fill the escape bar." : "Press Space repeatedly to fill the escape bar.", () => vortex.EscapeCount > escaped, ge.Duration, true);
        if (!bot.LessonSucceeded)
            yield break;
        GeneralEventManager.EndEvent(0);
        yield return bot.Say("You broke free and returned to safe ground. Keep away from the center. Entering the vortex again puts you at risk again.");
        yield return bot.Lessons.CoQuestion("What protects you from the vortex?", "Staying outside its pull", "Hiding in a vent", "Standing at its center");
    }
}
