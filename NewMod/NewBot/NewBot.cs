using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using DialogAPI;
using DialogAPI.Components;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.ImpostorRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.UI;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace NewMod.NewBot;

[RegisterInIl2Cpp]
public sealed class NewBot : MonoBehaviour
{
    public static NewBot Instance;
    public bool Progressive;
    public bool Extreme;
    public int Level = 1;
    public PlayerControl Player;
    public RoleTypes OriginalRole;
    public Vector2 OriginalPosition;
    public DialogUI Dialog;

    [HideFromIl2Cpp]
    public bool IsPaused => Dialog || MeetingHud.Instance || ExileController.Instance;

    [HideFromIl2Cpp]
    public bool CanUseNormalKill => LessonRunning && AllowKills && Player.Data.Role is not (Edgeveil or PulseBlade or Revenant or WraithCaller or MirrorBladeRole or Bounty);

    [HideFromIl2Cpp]
    public bool CanLeavePracticeRoom => Level == 4 && Player.Data.Role is Revenant or Bounty;

    [HideFromIl2Cpp]
    public IEnumerator Coro { get; set; }

    [HideFromIl2Cpp]
    public NewBotMenu Menu { get; set; }

    public TextMeshPro Objective;
    public LoadingBarManager Loading;
    public bool Practicing;
    public bool LessonRunning;
    public bool AllowKills;
    public byte ExpectedVictim = byte.MaxValue;
    public string Failure;
    public string Instruction;
    public string Observation;
    public int EvidenceCamera = -1;
    public VerifierClaimType VerificationClaim;
    public bool VerificationExpected;
    public byte VerificationTarget;
    public float StepRemaining;
    public ArrowBehaviour GoalMarker;
    public PlayerControl GoalTarget;

    [HideFromIl2Cpp]
    public IEnumerator LessonCoro { get; set; }

    public bool LessonSucceeded;
    public bool Victory;

    [HideFromIl2Cpp]
    public NewBotMap Map { get; }

    [HideFromIl2Cpp]
    public NewBotChallenges Challenges { get; }

    [HideFromIl2Cpp]
    public NewBotLessons Lessons { get; }

    public NewBot(nint ptr) : base(ptr)
    {
        Map = new NewBotMap(this);
        Challenges = new NewBotChallenges(this);
        Lessons = new NewBotLessons(this);
    }

    public static void Open()
    {
        if (!ShipStatus.Instance)
            return;
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay || MeetingHud.Instance)
            return;
        if (Instance)
            return;
        Instance = HudManager.Instance.gameObject.AddComponent<NewBot>();
        Instance.Player = PlayerControl.LocalPlayer;
        Instance.Map.Ship = ShipStatus.Instance;
        Instance.OriginalRole = Instance.Player.Data.Role.Role;
        Instance.OriginalPosition = Instance.Player.GetTruePosition();
        Instance.Map.WalkingOptions.Radius = Mathf.Max(0.18f, Mathf.Max(Instance.Player.Collider.bounds.extents.x, Instance.Player.Collider.bounds.extents.y)) + 0.05f;
        Instance.Coro = Instance.CoWelcome();
        Coroutines.Start(Instance.Coro);
    }

    [HideFromIl2Cpp]
    public IEnumerator CoWelcome()
    {
        if (Minigame.Instance)
        {
            Minigame.Instance.ForceClose();
            yield return null;
        }

        yield return Say("Welcome to NewMod! I'm NewBot. Pick a role and we'll learn it together, one action at a time. This is Freeplay: you can take your time and try again.");
        yield return Say("Learn the role at your own pace, or work through harder rounds. Extreme starts with the toughest opponents. Mistakes reset the trial so you can try again.");
        var relaxed = new NewBotMenu.Choice("Learn the role", Color.white);
        var progressive = new NewBotMenu.Choice("Harder rounds", new Color(1f, 0.75f, 0.3f));
        var extreme = new NewBotMenu.Choice("Extreme", new Color(1f, 0.35f, 0.35f));
        Menu = NewBotMenu.Open("Choose your pace", [relaxed, progressive, extreme]);
        while (Menu.Component)
            yield return null;
        if (Menu.Selected == null)
        {
            Coro = null;
            Destroy(this);
            yield break;
        }

        Extreme = Menu.Selected == extreme;
        Progressive = Menu.Selected != relaxed;
        yield return CoBrowseRoles();
        Coro = null;
        Destroy(this);
    }

    [HideFromIl2Cpp]
    public IEnumerator Say(string text)
    {
        while (Minigame.Instance)
            yield return null;
        Dialog = DialogUI.CreateDialog();
        Dialog.Open([new DialogLine(Dialog, "NEWBOT", text)]);
        while (Dialog)
            yield return null;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoBrowseRoles()
    {
        var crew = new NewBotMenu.Choice("Crewmate", new Color(0.4f, 0.85f, 1f));
        var impostor = new NewBotMenu.Choice("Impostor", Palette.ImpostorRed);
        var neutral = new NewBotMenu.Choice("Neutral", new Color(0.85f, 0.6f, 1f));
        var previous = new NewBotMenu.Choice("Previous page", Color.white);
        var next = new NewBotMenu.Choice("Next page", Color.white);
        var back = new NewBotMenu.Choice("Categories", Color.white);
        while (true)
        {
            Menu = NewBotMenu.Open("Pick a team", [crew, impostor, neutral]);
            while (Menu.Component)
                yield return null;
            var category = Menu.Selected;
            if (category == null)
                yield break;
            var team = ModdedRoleTeams.Custom;
            if (category == crew)
                team = ModdedRoleTeams.Crewmate;
            else if (category == impostor)
                team = ModdedRoleTeams.Impostor;
            var roles = CustomRoleManager.CustomMiraRoles.Where(role => role.GetType().Assembly == typeof(NewMod).Assembly && role.Team == team).OrderBy(role => role.RoleName).Select(role => new NewBotMenu.Choice(role.RoleName, role.RoleColor, (RoleBehaviour)role)).ToArray();
            var page = 0;
            while (true)
            {
                var choices = roles.Skip(page * 9).Take(9).ToList();
                if (page > 0)
                    choices.Add(previous);
                choices.Add(back);
                if ((page + 1) * 9 < roles.Length)
                    choices.Add(next);
                Menu = NewBotMenu.Open($"{category.Text}   {page + 1}/{System.Math.Max(1, (roles.Length + 8) / 9)}", choices);
                while (Menu.Component)
                    yield return null;
                var selected = Menu.Selected;
                if (selected == null || selected == back)
                    break;
                if (selected == previous)
                {
                    page--;
                    continue;
                }

                if (selected == next)
                {
                    page++;
                    continue;
                }

                Level = Extreme ? 4 : 1;
                yield return Lessons.CoPractice(selected.Role);
            }
        }
    }

    public void OnDestroy()
    {
        Coroutines.Stop(Coro);
        if (LessonRunning)
            Coroutines.Stop(LessonCoro);
        LessonRunning = false;
        if (GoalMarker)
            Destroy(GoalMarker.gameObject);
        Map.StopMovements();
        if (Dialog)
            Dialog.ForceClose();
        if (Menu?.Component)
            Menu.ForceClose();
        if (Loading)
            Loading.ToggleLoadingBar(false);
        if (Map.Transition)
            Destroy(Map.Transition.gameObject);
        if (Objective)
            Destroy(Objective.gameObject);
        if (Practicing && Map.Ship && Map.Ship == ShipStatus.Instance && Player && Player == PlayerControl.LocalPlayer && HudManager.Instance && AmongUsClient.Instance)
        {
            Player.RpcSetRole(RoleTypes.Crewmate);
            Map.ClearDummies();
            Map.ResetPractice();
            Player.Revive();
            Player.RpcSetRole(OriginalRole);
            Player.NetTransform.RpcSnapTo(OriginalPosition - Player.Collider.offset);
            HudManager.Instance.SetHudActive(Player, Player.Data.Role, true);
        }

        if (Instance == this)
            Instance = null;
    }

    [HideFromIl2Cpp]
    public IEnumerator WaitFor(string instruction, System.Func<bool> completed, float seconds = 120f)
    {
        StepRemaining = Progressive ? seconds : seconds * 3f;
        Instruction = instruction;
        Objective.text = instruction;
        while (LessonSucceeded && (!Player.Data.IsDead || Player.Data.Role is Revenant) && !completed())
            yield return null;
        Objective.text = string.Empty;
        Instruction = string.Empty;
        StepRemaining = 180f;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoExecuteLesson(RoleBehaviour role)
    {
        yield return null;
        yield return Lessons.CoRunLesson(role);
        LessonRunning = false;
        LessonCoro = null;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoSupervise(RoleBehaviour role)
    {
        Failure = null;
        Instruction = string.Empty;
        Observation = null;
        EvidenceCamera = -1;
        VerificationTarget = byte.MaxValue;
        StepRemaining = 180f;
        AllowKills = role is MirrorBladeRole or Edgeveil or PulseBlade or Revenant or Shade or WraithCaller or Tyrant or Voidwalker or Bounty;
        ExpectedVictim = AllowKills && role is not Tyrant ? Map.Dummies[0].PlayerId : byte.MaxValue;
        LessonRunning = true;
        HudManager.Instance.SetHudActive(Player, role, true);
        var displayedSeconds = -1;
        string displayedInstruction = null;
        LessonCoro = CoExecuteLesson(role);
        Coroutines.Start(LessonCoro);
        while (LessonRunning && Failure == null)
        {
            if (role is Revenant && Revenant.Phases.TryGetValue(Player.PlayerId, out var revenantPhase) && revenantPhase == Revenant.Phase.PermanentlyDead)
                Failure = "Your fake body was exposed. Choose a safer place before using Feign Death.";
            if (Player.Data.IsDead && role is not Revenant)
                Failure = "You were caught. Let's reset the trial and try again.";
            if (!IsPaused)
            {
                if (!CanLeavePracticeRoom && role is not Nomad and not EnergyThief and not Beacon and not TerminatorRole && !Map.Room.roomArea.OverlapPoint(Player.GetTruePosition()) && Vector2.Distance(Player.GetTruePosition(), Map.Center) > 8f)
                    Failure = "You've left the practice area. I'll bring you back and reset this trial.";
                if (StepRemaining > 0f)
                {
                    StepRemaining -= Time.deltaTime;
                    var seconds = Mathf.CeilToInt(StepRemaining);
                    if (Progressive && !string.IsNullOrEmpty(Instruction) && (seconds != displayedSeconds || Instruction != displayedInstruction))
                    {
                        Objective.text = $"{Instruction}\n<size=75%>{seconds} seconds left</size>";
                        displayedSeconds = seconds;
                        displayedInstruction = Instruction;
                    }
                    if (StepRemaining <= 0f)
                        Failure = "Time ran out. I'll reset the trial. Read the target and plan your next move before starting.";
                }
            }

            yield return null;
        }

        if (LessonRunning)
            Coroutines.Stop(LessonCoro);
        LessonRunning = false;
        LessonCoro = null;
        LessonSucceeded &= Failure == null;
        Instruction = string.Empty;
        Objective.text = string.Empty;
        if (GoalMarker)
            Destroy(GoalMarker.gameObject);
    }

    public void ShowGoal(Vector2 position)
    {
        GoalTarget = null;
        if (!GoalMarker)
        {
            var marker = new GameObject("NewBotGoal");
            var image = marker.AddComponent<SpriteRenderer>();
            image.sprite = NewModAsset.Arrow.LoadAsset();
            image.color = new Color(0.8f, 0.55f, 1f);
            GoalMarker = marker.AddComponent<ArrowBehaviour>();
            GoalMarker.image = image;
            GoalMarker.MaxScale = 0.5f / Mathf.Max(image.sprite.bounds.size.x, image.sprite.bounds.size.y);
            GoalMarker.alwaysMaxSize = true;
        }

        GoalMarker.target = new Vector3(position.x, position.y, -2f);
        GoalMarker.UpdatePosition();
    }

    public void ShowGoal(PlayerControl target)
    {
        ShowGoal(target.GetTruePosition());
        GoalTarget = target;
    }

    public void LateUpdate()
    {
        if (Practicing && !CanUseNormalKill)
            HudManager.Instance.KillButton.gameObject.SetActive(false);
        if (!GoalMarker || !Camera.main)
            return;
        if (GoalTarget)
        {
            var position = GoalTarget.GetTruePosition();
            GoalMarker.target = new Vector3(position.x, position.y, -2f);
        }

        GoalMarker.UpdatePosition();
        GoalMarker.image.enabled = !IsPaused;
    }
}
