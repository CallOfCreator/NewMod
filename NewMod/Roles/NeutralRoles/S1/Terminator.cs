using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public class TerminatorRole : CrewmateRole, INewModRole
{
    public static int MeetingsSurvived;
    public static bool ObjectiveSpawned;
    public static ArrowBehaviour ObjectiveArrow;
    public static ArrowBehaviour ThreatArrow;
    public static TextMeshPro ThreatText;
    public static bool FinalCountdownActive;
    public static Vector2 ObjectivePosition;
    public static GameObject ObjectiveMarker;
    public static float _baseSpeed;
    public string RoleName => "Terminator";
    public string RoleDescription => "Survive. Escalate. Terminate.";

    public string RoleLongDescription =>
        "Your existence is publicly announced. Survive meetings to escalate your speed. After enough meetings, a final objective appears. Reach it to win alone.";

    public Color RoleColor => new Color32(217, 110, 32, 255);
    public NewModFaction Faction => NewModFaction.Entropy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup => RoleOptionsGroup.Neutral;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AffectedByLightOnAirship = false,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = true,
        CanUseSabotage = false,
        TasksCountForProgress = false,
        ShowInFreeplay = true,
        HideSettings = false,
        MaxRoleCount = 2,
        OptionsScreenshot = MiraAssets.Empty,
        Icon = MiraAssets.Empty,
        CanModifyChance = true,
        RoleHintType = RoleHintType.RoleTab
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var required = (int)OptionGroupSingleton<TerminatorOptions>.Instance.MeetingsBeforeObjective;
        var left = Mathf.Max(0, required - MeetingsSurvived);

        tabText.AppendLine();
        tabText.AppendLine($"<size=65%>Meetings survived: <color=#FFD166>{MeetingsSurvived}</color>/<color=#B7B7B7>{required}</color></size>");

        if (FinalCountdownActive)
            tabText.AppendLine("<size=65%><color=#FF453A>Final sequence active. Survive until termination.</color></size>");
        else if (ObjectiveSpawned)
            tabText.AppendLine("<size=65%><color=#FF8C32>Final Objective active. Reach the marked zone and begin termination.</color></size>");
        else
            tabText.AppendLine($"<size=65%><color=#FFD166>{left} meeting{(left == 1 ? "" : "s")} left before your Final Objective.</color></size>");

        return tabText;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<TerminatorGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        var terminator = GetTerminator();

        if (evt.TriggeredByIntro)
        {
            ResetState();

            if (terminator)
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#D96E20><b>WARNING:</b> The Terminator is aboard.</color>"));

            if (PlayerControl.LocalPlayer.Data.Role is TerminatorRole)
                _baseSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed > 0f ? PlayerControl.LocalPlayer.MyPhysics.Speed : 1f;

            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is TerminatorRole)
            ApplySpeedBonus();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        var terminator = GetTerminator();

        if (!terminator || terminator.Data.IsDead || terminator.Data.Disconnected)
            return;

        Coroutines.Start(CoShowMeetingWarning(evt.MeetingHud));
    }

    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent evt)
    {
        var terminator = GetTerminator();

        if (!terminator || terminator.Data.IsDead || terminator.Data.Disconnected)
            return;

        MeetingsSurvived++;

        if (PlayerControl.LocalPlayer.Data.Role is TerminatorRole)
            ApplySpeedBonus();

        if (!PlayerControl.LocalPlayer.IsHost() || ObjectiveSpawned)
            return;

        var required = (int)OptionGroupSingleton<TerminatorOptions>.Instance.MeetingsBeforeObjective;

        if (MeetingsSurvived >= required)
        {
            var pos = PickObjectivePosition();
            RpcSpawnObjective(terminator, pos.x, pos.y);
        }
    }

    private static IEnumerator CoShowMeetingWarning(MeetingHud hud)
    {
        while (hud && hud.CurrentState == MeetingHud.VoteStates.Animating)
            yield return null;

        yield return new WaitForSeconds(0.35f);

        var required = (int)OptionGroupSingleton<TerminatorOptions>.Instance.MeetingsBeforeObjective;
        var state = MeetingsSurvived >= required ? "Final Objective is active." : $"Escalation {MeetingsSurvived}/{required}.";

        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#D96E20>Terminator detected. {state}</color>"));
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ResetState();
    }

    [MethodRpc((uint)CustomRPC.TerminatorObjective)]
    public static void RpcSpawnObjective(PlayerControl source, float x, float y)
    {
        ObjectiveSpawned = true;
        ObjectivePosition = new Vector2(x, y);

        if (ObjectiveMarker)
            Destroy(ObjectiveMarker);

        var radius = OptionGroupSingleton<TerminatorOptions>.Instance.FinalObjectiveRadius;
        ObjectiveMarker = Utils.CreateCircle("TerminatorFinalObjective", new Vector3(x, y, 0f), radius, new Color32(217, 110, 32, 130), 600f);
        CreateObjectiveArrow(ObjectivePosition);
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#D96E20><b>Terminator Final Objective revealed.</b></color>"));
    }

    [MethodRpc((uint)CustomRPC.TerminatorFinalCountdown)]
    public static void RpcStartFinalCountdown(PlayerControl source)
    {
        if (FinalCountdownActive || source.Data.Role is not TerminatorRole)
            return;

        var radius = OptionGroupSingleton<TerminatorOptions>.Instance.FinalObjectiveRadius;
        if (Vector2.Distance(source.GetTruePosition(), ObjectivePosition) > radius)
            return;

        FinalCountdownActive = true;

        if (ObjectiveMarker)
            Destroy(ObjectiveMarker);

        ObjectiveMarker = null;
        DestroyObjectiveArrow();
        Coroutines.Start(CoFinalCountdown(source));
    }

    private static IEnumerator CoFinalCountdown(PlayerControl terminator)
    {
        const float duration = 10f;
        var hud = HudManager.Instance;

        ThreatText = Helpers.CreateTextLabel("TerminatorThreatText", hud.transform, AspectPosition.EdgeAlignments.Top, new Vector3(0f, 0.35f, -20f), 2.2f);
        ThreatText.color = new Color32(255, 70, 45, 255);
        ThreatText.fontStyle = FontStyles.Bold;

        if (PlayerControl.LocalPlayer.PlayerId != terminator.PlayerId)
        {
            var arrowObject = new GameObject("TerminatorThreatArrow") { layer = 5 };
            var renderer = arrowObject.AddComponent<SpriteRenderer>();
            renderer.sprite = NewModAsset.Arrow.LoadAsset();
            renderer.color = new Color32(255, 45, 45, 255);

            ThreatArrow = arrowObject.AddComponent<ArrowBehaviour>();
            ThreatArrow.target = terminator.transform.position;
            ThreatArrow.alwaysMaxSize = true;
            ThreatArrow.MaxScale = 0.8f;
        }

        var timeLeft = duration;
        var alertTimer = 0f;

        while (timeLeft > 0f && FinalCountdownActive && !terminator.Data.IsDead && !terminator.Data.Disconnected)
        {
            var inMeeting = MeetingHud.Instance || ExileController.Instance;

            ThreatText.gameObject.SetActive(!inMeeting);
            if (ThreatArrow)
            {
                ThreatArrow.gameObject.SetActive(!inMeeting);
                ThreatArrow.target = terminator.transform.position;
            }

            if (inMeeting)
            {
                yield return null;
                continue;
            }

            ThreatText.text = $"STOP THE TERMINATOR BEFORE IT'S TOO LATE!!!\n<size=75%>{Mathf.CeilToInt(timeLeft)}</size>";

            alertTimer -= Time.deltaTime;
            if (alertTimer <= 0f)
            {
                hud.AlertFlash.Flash();
                if (Constants.ShouldPlaySfx())
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 0.9f);
                alertTimer = 1f;
            }

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        if (ThreatArrow)
            Destroy(ThreatArrow.gameObject);
        if (ThreatText)
            Destroy(ThreatText.gameObject);

        ThreatArrow = null;
        ThreatText = null;
        FinalCountdownActive = false;

        if (timeLeft <= 0f && !terminator.Data.IsDead && !terminator.Data.Disconnected && AmongUsClient.Instance.AmHost)
            CustomGameOver.Trigger<TerminatorGameOver>([terminator.Data]);
    }

    public static void ApplySpeedBonus()
    {
        if (_baseSpeed <= 0f)
            _baseSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed > 0f ? PlayerControl.LocalPlayer.MyPhysics.Speed : 1f;

        var bonus = OptionGroupSingleton<TerminatorOptions>.Instance.SpeedBonusPerMeeting / 100f;
        PlayerControl.LocalPlayer.MyPhysics.Speed = _baseSpeed * (1f + MeetingsSurvived * bonus);
    }

    public static void CreateObjectiveArrow(Vector2 target)
    {
        DestroyObjectiveArrow();

        if (PlayerControl.LocalPlayer.Data.Role is not TerminatorRole)
            return;

        var go = new GameObject("TerminatorObjectiveArrow");
        go.layer = 5;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.Arrow.LoadAsset();
        renderer.color = new Color32(217, 110, 32, 255);

        ObjectiveArrow = go.AddComponent<ArrowBehaviour>();
        ObjectiveArrow.target = target;
        ObjectiveArrow.alwaysMaxSize = true;
        ObjectiveArrow.MaxScale = 0.75f;
    }

    public static void DestroyObjectiveArrow()
    {
        if (ObjectiveArrow)
            Destroy(ObjectiveArrow.gameObject);

        ObjectiveArrow = null;
    }

    public static PlayerControl GetTerminator()
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p && p.Data != null && p.Data.Role is TerminatorRole);
    }

    public static Vector2 PickObjectivePosition()
    {
        var ship = ShipStatus.Instance;

        if (!ship)
            return PlayerControl.LocalPlayer.GetTruePosition();

        SystemTypes[] rooms;

        if (ship is AirshipStatus)
            rooms =
            [
                SystemTypes.VaultRoom,
                SystemTypes.Cockpit,
                SystemTypes.Armory,
                SystemTypes.Kitchen,
                SystemTypes.ViewingDeck,
                SystemTypes.HallOfPortraits,
                SystemTypes.CargoBay,
                SystemTypes.Showers,
                SystemTypes.Engine,
                SystemTypes.Brig,
                SystemTypes.MeetingRoom,
                SystemTypes.Records,
                SystemTypes.Lounge,
                SystemTypes.MainHall,
                SystemTypes.Medical
            ];
        else
            rooms = ship.Type switch
            {
                ShipStatus.MapType.Ship =>
                [
                    SystemTypes.Cafeteria,
                    SystemTypes.Admin,
                    SystemTypes.Weapons,
                    SystemTypes.Shields,
                    SystemTypes.Storage,
                    SystemTypes.Electrical,
                    SystemTypes.Security,
                    SystemTypes.MedBay,
                    SystemTypes.Reactor,
                    SystemTypes.Nav
                ],

                ShipStatus.MapType.Hq =>
                [
                    SystemTypes.Launchpad,
                    SystemTypes.MedBay,
                    SystemTypes.LockerRoom,
                    SystemTypes.Laboratory,
                    SystemTypes.Office,
                    SystemTypes.Admin,
                    SystemTypes.Greenhouse,
                    SystemTypes.Balcony
                ],

                ShipStatus.MapType.Pb =>
                [
                    SystemTypes.Dropship,
                    SystemTypes.Office,
                    SystemTypes.Laboratory,
                    SystemTypes.Specimens,
                    SystemTypes.Storage,
                    SystemTypes.Electrical,
                    SystemTypes.Weapons,
                    SystemTypes.Comms,
                    SystemTypes.BoilerRoom
                ],

                ShipStatus.MapType.Fungle =>
                [
                    SystemTypes.RecRoom,
                    SystemTypes.Lookout,
                    SystemTypes.Beach,
                    SystemTypes.Highlands,
                    SystemTypes.Jungle,
                    SystemTypes.SleepingQuarters,
                    SystemTypes.Kitchen,
                    SystemTypes.MiningPit,
                    SystemTypes.FishingDock
                ],

                _ =>
                [
                    SystemTypes.Cafeteria,
                    SystemTypes.Admin,
                    SystemTypes.Storage,
                    SystemTypes.Electrical
                ]
            };

        var positions = new List<Vector2>();

        if (ship.FastRooms != null)
            foreach (var roomType in rooms)
            {
                if (!ship.FastRooms.TryGetValue(roomType, out var room) || !room)
                    continue;

                positions.Add(room.roomArea ? room.roomArea.bounds.center : room.transform.position);
            }

        if (positions.Count == 0 && ship.AllRooms != null)
            foreach (var room in ship.AllRooms)
            {
                if (!room)
                    continue;

                if (room.RoomId is SystemTypes.Hallway or SystemTypes.Outside or SystemTypes.Ventilation or SystemTypes.Sabotage or SystemTypes.Doors)
                    continue;

                positions.Add(room.roomArea ? room.roomArea.bounds.center : room.transform.position);
            }

        if (positions.Count > 0)
            return positions[Random.Range(0, positions.Count)];

        return ship.InitialSpawnCenter != Vector2.zero ? ship.InitialSpawnCenter : PlayerControl.LocalPlayer.GetTruePosition();
    }

    public static void ResetState()
    {
        DestroyObjectiveArrow();

        if (ThreatArrow)
            Destroy(ThreatArrow.gameObject);

        if (ThreatText)
            Destroy(ThreatText.gameObject);

        ThreatArrow = null;
        ThreatText = null;

        MeetingsSurvived = 0;
        ObjectiveSpawned = false;
        FinalCountdownActive = false;
        ObjectivePosition = Vector2.zero;
        _baseSpeed = 0f;

        if (ObjectiveMarker)
            Destroy(ObjectiveMarker);

        ObjectiveMarker = null;
    }
}