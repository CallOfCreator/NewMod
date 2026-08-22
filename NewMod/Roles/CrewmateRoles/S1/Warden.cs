using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using NewMod.Achievements;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles.S1;

[MiraIgnore]
public class WardenRole : CrewmateRole, INewModRole
{
    private const float AbilityRecentWindow = 4f;
    public static bool SealActive { get; private set; }
    public static SystemTypes SealedRoom { get; private set; }
    public static byte SealOwnerId { get; private set; } = byte.MaxValue;
    public static float SealEndsAt { get; private set; }

    private static float SealStartedAt;
    private static Vector2 SealOrigin;
    private static int _sealVersion;

    private static readonly Dictionary<byte, bool> InsideStates = [];
    private static readonly Dictionary<byte, float> EnteredAt = [];
    private static readonly Dictionary<byte, float> AbilityUsedAt = [];

    private static readonly Dictionary<byte, ( byte KillerId, bool EnteredAfterSeal, bool UsedAbilityRecently, bool ImpostorAligned, float PresenceTime )> KillSnapshots = [];

    private static readonly Dictionary<GameObject, string> ResidualMarks = [];

    public string RoleName => "Warden";
    public string RoleDescription => "Seal. Observe. Investigate.";
    public string RoleLongDescription => "Seal your current room to block venting and door sabotage.\n" + "Track movement across its boundary and inspect Residual Traces left behind by violence.";

    public Color RoleColor => new Color32(58, 166, 255, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public NewModFaction Faction => NewModFaction.Sentinel;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            OptionsScreenshot = MiraAssets.Empty,
            Icon = NewModAsset.WardenIcon,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            CanUseSabotage = false,
            TasksCountForProgress = true,
            MaxRoleCount = 1,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        var options = OptionGroupSingleton<WardenOptions>.Instance;

        tabText.AppendLine();

        if (SealActive && PlayerControl.LocalPlayer.PlayerId == SealOwnerId)
        {
            var room = DestroyableSingleton<TranslationController>.Instance.GetString(SealedRoom);
            var remaining = Mathf.Max(0f, SealEndsAt - Time.time);

            tabText.AppendLine($"<size=65%>Sealed: <color=#3AA6FF>{room}</color>  •  <color=#FFD166>{remaining:0.0}s</color> remaining</size>");
        }
        else
        {
            tabText.AppendLine($"<size=65%>Seal Duration: <color=#3AA6FF>{options.SealDuration:0.#}s</color>  •  Cooldown: <color=#B7B7B7>{options.SealCooldown:0.#}s</color></size>");
        }

        tabText.AppendLine("<size=65%><color=#3AA6FF>Seal:</color> blocks venting and door sabotage inside your current room.</size>");

        tabText.AppendLine("<size=65%><color=#8BD6FF>Residual Traces:</color> violence is recorded at your Seal point and can be inspected for one clue.</size>");

        return tabText;
    }

    public static PlainShipRoom GetRoom(Vector2 position)
    {
        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room.roomArea && room.roomArea.OverlapPoint(position))
                return room;
        }

        return null;
    }

    public static bool IsInSealedRoom(Vector2 position)
    {
        var room = GetRoom(position);
        return room && room.RoomId == SealedRoom;
    }

    [MethodRpc((uint)CustomRPC.WardenSeal)]
    public static void RpcStartSeal(PlayerControl source, byte roomId, float duration)
    {
        SealOwnerId = source.PlayerId;
        SealedRoom = (SystemTypes)roomId;
        SealActive = true;
        SealStartedAt = Time.time;
        SealEndsAt = Time.time + duration;
        SealOrigin = source.GetTruePosition();

        _sealVersion++;

        InsideStates.Clear();
        EnteredAt.Clear();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.IsDead || player.Data.Disconnected)
                continue;

            InsideStates[player.PlayerId] = IsInSealedRoom(player.GetTruePosition());
        }

        Coroutines.Start(CoRunSeal(_sealVersion));

        if (source.AmOwner)
        {
            var room = DestroyableSingleton<TranslationController>.Instance.GetString(SealedRoom);
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#3AA6FF>Seal active:</color> {room}"));
        }
    }

    private static IEnumerator CoRunSeal(int version)
    {
        while (SealActive && version == _sealVersion && Time.time < SealEndsAt)
        {
            var localPlayer = PlayerControl.LocalPlayer;

            if (localPlayer.inVent && Vent.currentVent && IsInSealedRoom(Vent.currentVent.transform.position))
            {
                var vent = Vent.currentVent;
                vent.SetButtons(false);
                localPlayer.MyPhysics.RpcExitVent(vent.Id);
            }

            var isWarden = localPlayer.PlayerId == SealOwnerId;

            if (AmongUsClient.Instance.AmHost || isWarden)
                TrackRoomTransitions(isWarden && !localPlayer.Data.IsDead);

            yield return new WaitForFixedUpdate();
        }

        if (version != _sealVersion)
            yield break;

        SealActive = false;
        InsideStates.Clear();
        EnteredAt.Clear();

        if (PlayerControl.LocalPlayer.PlayerId == SealOwnerId && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#B7B7B7>Seal expired.</color>"));
        }
    }

    private static void TrackRoomTransitions(bool showPulse)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.IsDead || player.Data.Disconnected)
                continue;

            var inside = IsInSealedRoom(player.GetTruePosition());

            if (!InsideStates.TryGetValue(player.PlayerId, out var wasInside))
            {
                InsideStates[player.PlayerId] = inside;
                continue;
            }

            if (inside == wasInside)
                continue;

            InsideStates[player.PlayerId] = inside;

            if (inside)
                EnteredAt[player.PlayerId] = Time.time;

            if (showPulse)
                Coroutines.Start(CoShowPulse(player.GetTruePosition(), inside));
        }
    }

    private static IEnumerator CoShowPulse(Vector2 position, bool entering)
    {
        var go = new GameObject("WardenRoomPulse") { layer = PlayerControl.LocalPlayer.gameObject.layer };

        go.transform.position = position;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.RadarIcon.LoadAsset();
        renderer.sortingOrder = 100;

        var pulseColor = entering ? new Color(0.23f, 0.65f, 1f, 0.85f) : new Color(1f, 0.72f, 0.25f, 0.85f);

        var timer = 0f;
        const float duration = 0.55f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            var progress = Mathf.Clamp01(timer / duration);

            go.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 0.85f, progress);

            renderer.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, pulseColor.a * (1f - progress));

            yield return null;
        }

        Object.Destroy(go);
    }

    [RegisterEvent]
    public static void OnPlayerCanUse(PlayerCanUseEvent evt)
    {
        if (!SealActive || !evt.IsVent)
            return;

        var vent = evt.Usable.TryCast<Vent>();

        if (vent && IsInSealedRoom(vent.transform.position))
            evt.Cancel();
    }

    [RegisterEvent]
    public static void OnEnterVent(EnterVentEvent evt)
    {
        if (SealActive && evt.Vent && IsInSealedRoom(evt.Vent.transform.position))
        {
            evt.Cancel();
        }
    }

    [RegisterEvent]
    public static void OnCloseDoors(CloseDoorsEvent evt)
    {
        if (SealActive && evt.Room == SealedRoom)
            evt.Cancel();
    }

    [RegisterEvent]
    public static void OnMiraButtonClick(MiraButtonClickEvent evt)
    {
        if (MeetingHud.Instance || ExileController.Instance || !evt.Button.CanClick())
        {
            return;
        }

        RpcTrackAbilityUse(PlayerControl.LocalPlayer);
    }

    [RegisterEvent]
    public static void OnVanillaButtonClick(VanillaButtonClickEvent evt)
    {
        if (MeetingHud.Instance || ExileController.Instance)
            return;

        RpcTrackAbilityUse(PlayerControl.LocalPlayer);
    }

    [MethodRpc((uint)CustomRPC.WardenAbilityUsed)]
    public static void RpcTrackAbilityUse(PlayerControl source)
    {
        AbilityUsedAt[source.PlayerId] = Time.time;
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        KillSnapshots.Remove(evt.Target.PlayerId);

        if (!SealActive || !IsInSealedRoom(evt.Source.GetTruePosition()) || !IsInSealedRoom(evt.Target.GetTruePosition()))
        {
            return;
        }

        var enteredAfterSeal = EnteredAt.TryGetValue(evt.Source.PlayerId, out var enteredAt);

        var usedAbilityRecently = AbilityUsedAt.TryGetValue(evt.Source.PlayerId, out var usedAt) && Time.time - usedAt <= AbilityRecentWindow;

        var presenceStartedAt = enteredAfterSeal ? enteredAt : SealStartedAt;

        var presenceTime = Mathf.Max(0f, Time.time - presenceStartedAt);

        KillSnapshots[evt.Target.PlayerId] = (evt.Source.PlayerId, enteredAfterSeal, usedAbilityRecently, evt.Source.Data.Role.IsImpostor, presenceTime);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || !KillSnapshots.TryGetValue(evt.Target.PlayerId, out var snapshot))
        {
            return;
        }

        KillSnapshots.Remove(evt.Target.PlayerId);

        if (snapshot.KillerId != evt.Source.PlayerId)
            return;

        var warden = Utils.PlayerById(SealOwnerId);

        if (!warden)
            return;

        var clueType = Random.Range(0, 4);

        var clueValue = clueType switch
        {
            0 => snapshot.ImpostorAligned ? 1f : 0f,
            1 => snapshot.EnteredAfterSeal ? 1f : 0f,
            2 => snapshot.UsedAbilityRecently ? 1f : 0f,
            3 => Mathf.Max(1f, Mathf.Round(snapshot.PresenceTime)),
            _ => 0f
        };

        RpcCreateResidualMark(warden, SealOrigin.x, SealOrigin.y, (byte)clueType, clueValue);
    }

    [MethodRpc((uint)CustomRPC.WardenResidualMark)]
    public static void RpcCreateResidualMark(PlayerControl source, float x, float y, byte clueType, float clueValue)
    {
        if (!source.AmOwner || source.Data.IsDead)
            return;

        var clue = clueType switch
        {
            0 => clueValue > 0.5f ? "The killer was <color=#FF5A5A>Impostor-aligned</color>." : "The killer was <color=#8BD6FF>non-Impostor-aligned</color>.",

            1 => clueValue > 0.5f ? "The killer entered the room after your Seal was placed." : "The killer was already inside when you sealed the room.",

            2 => clueValue > 0.5f ? "The killer used an ability shortly before the kill." : "No recent ability use was detected from the killer.",

            3 => $"The killer had been inside the sealed room for about <color=#FFD166>{clueValue:0}s</color> before the kill.",

            _ => "The residual signature is unreadable."
        };

        var mark = new GameObject("WardenResidualMark") { layer = PlayerControl.LocalPlayer.gameObject.layer };

        mark.transform.position = new Vector3(x, y, 0f);
        mark.transform.localScale = Vector3.one * 0.45f;

        var renderer = mark.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.ResidualTrace.LoadAsset();
        renderer.sortingOrder = 100;

        ResidualMarks[mark] = clue;

        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#3AA6FF>Violence detected.</color> A Residual Trace was recorded."));

        Coroutines.Start(CoExpireResidualMark(mark, OptionGroupSingleton<WardenOptions>.Instance.ResidualMarkDuration));
    }

    private static IEnumerator CoExpireResidualMark(GameObject mark, float duration)
    {
        var timer = 0f;

        while (timer < duration && mark)
        {
            if (!MeetingHud.Instance)
                timer += Time.deltaTime;

            var scale = 0.45f + Mathf.Sin(Time.time * 3.5f) * 0.025f;

            mark.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        if (!mark || !ResidualMarks.Remove(mark))
            yield break;

        Object.Destroy(mark);
    }

    public static GameObject GetNearestResidualMark(Vector2 position, float range)
    {
        GameObject nearest = null;
        var nearestDistance = range;

        foreach (var pair in ResidualMarks)
        {
            if (!pair.Key)
                continue;

            var distance = Vector2.Distance(position, pair.Key.transform.position);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearest = pair.Key;
        }

        return nearest;
    }

    public static void InspectResidual(GameObject mark)
    {
        if (!ResidualMarks.TryGetValue(mark, out var clue))
            return;

        ResidualMarks.Remove(mark);
        Object.Destroy(mark);

        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#3AA6FF>Residual Trace</color>\n{clue}"));
        //NewModAchievementsTab.TraceEvidence.Unlock();
    }

    private static void ResetState()
    {
        _sealVersion++;

        SealActive = false;
        SealOwnerId = byte.MaxValue;
        SealStartedAt = 0f;
        SealEndsAt = 0f;
        SealOrigin = Vector2.zero;

        InsideStates.Clear();
        EnteredAt.Clear();
        AbilityUsedAt.Clear();
        KillSnapshots.Clear();

        foreach (var pair in ResidualMarks)
        {
            if (pair.Key)
                Object.Destroy(pair.Key);
        }

        ResidualMarks.Clear();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
            ResetState();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        _sealVersion++;

        SealActive = false;
        SealOwnerId = byte.MaxValue;
        SealStartedAt = 0f;
        SealEndsAt = 0f;
        SealOrigin = Vector2.zero;

        InsideStates.Clear();
        EnteredAt.Clear();
        AbilityUsedAt.Clear();
        KillSnapshots.Clear();
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ResetState();
    }
}