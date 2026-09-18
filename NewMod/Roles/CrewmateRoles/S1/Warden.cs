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
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.CrewmateRoles.S1;

public class WardenRole : CrewmateRole, INewModRole
{
    public const float AbilityRecentWindow = 4f;

    public static float SealStartedAt;
    public static Vector2 SealOrigin;
    public static int _sealVersion;

    public static readonly Dictionary<byte, bool> InsideStates = [];
    public static readonly Dictionary<byte, float> EnteredAt = [];
    public static readonly Dictionary<byte, float> AbilityUsedAt = [];

    public static readonly Dictionary<byte, ( byte KillerId, bool EnteredAfterSeal, bool UsedAbilityRecently, float PresenceTime )> KillSnapshots = [];

    public static readonly Dictionary<GameObject, (bool EnteredAfterSeal, bool UsedAbilityRecently, float PresenceTime)> ResidualMarks = [];
    public static int ClueType;
    public static bool SealActive;
    public static SystemTypes SealedRoom;
    public static byte SealOwnerId = byte.MaxValue;
    public static float SealEndsAt;

    public string RoleName => "Warden";
    public string RoleDescription => "Watch a room and investigate kills inside it.";
    public string RoleLongDescription => "Seal your room to block venting and door sabotage. Players can still walk in, leave, and kill.\nYou see a pulse when someone crosses the room boundary.\nIf someone is killed inside, inspect the trace for a clue about the attacker.";

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

        tabText.Append($"\n<size=65%>{RoleColor.ToTextColor()}Seal: {options.SealDuration:0.#}s | Cooldown: {options.SealCooldown:0.#}s</color>\nBlocks vents and door sabotage; walking and attacks remain possible.\n<color=#FFCF70>Clue:</color> {(ClueType == 0 ? "Entry timing" : ClueType == 1 ? "Recent ability" : "Time inside")}\nChoose a clue, then inspect a trace left by a kill.</size>");

        return tabText;
    }

    public static PlainShipRoom GetRoom(Vector2 position)
    {
        foreach (var room in ShipStatus.Instance.AllRooms)
            if (room.roomArea && room.roomArea.OverlapPoint(position))
                return room;

        return null;
    }

    public static bool IsInSealedRoom(Vector2 position)
    {
        var room = GetRoom(position);
        return room && room.RoomId == SealedRoom;
    }

    [MethodRpc((uint)CustomRPC.WardenSeal)]
    public static void RpcStartSeal(PlayerControl source, byte roomId)
    {
        if (source.Data.Role is not WardenRole || source.Data.IsDead || MeetingHud.Instance || SealActive)
            return;
        var currentRoom = GetRoom(source.GetTruePosition());
        if (!currentRoom || currentRoom.RoomId != (SystemTypes)roomId)
            return;
        var duration = OptionGroupSingleton<WardenOptions>.Instance.SealDuration;
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

        var seal = new GameObject("WardenSeal");
        seal.transform.SetParent(ShipStatus.Instance.transform, false);
        seal.transform.position = new Vector3(SealOrigin.x, SealOrigin.y, -0.1f);
        seal.transform.localScale = Vector3.one * 0.65f;
        seal.AddComponent<SpriteRenderer>().sprite = NewModAsset.WardenSeal.LoadAsset();
        Destroy(seal, duration);
        Coroutines.Start(CoRunSeal(_sealVersion));

        var room = DestroyableSingleton<TranslationController>.Instance.GetString(SealedRoom);
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#3AA6FF>Seal active:</color> {room}"));
    }

    public static IEnumerator CoRunSeal(int version)
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

        if (PlayerControl.LocalPlayer.PlayerId == SealOwnerId && !PlayerControl.LocalPlayer.Data.IsDead) Coroutines.Start(CoroutinesHelper.CoNotify("<color=#B7B7B7>Seal expired.</color>"));
    }

    public static void TrackRoomTransitions(bool showPulse)
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

    public static IEnumerator CoShowPulse(Vector2 position, bool entering)
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

        Destroy(go);
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
        if (SealActive && evt.Vent && IsInSealedRoom(evt.Vent.transform.position)) evt.Cancel();
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
        if (!SealActive || evt.IsCancelled || MeetingHud.Instance || ExileController.Instance || !evt.Button.CanClick()) return;

        RpcTrackAbilityUse(PlayerControl.LocalPlayer);
    }

    [RegisterEvent]
    public static void OnVanillaButtonClick(VanillaButtonClickEvent evt)
    {
        if (!SealActive || evt.IsCancelled || MeetingHud.Instance || ExileController.Instance)
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

        if (!SealActive || !IsInSealedRoom(evt.Source.GetTruePosition()) || !IsInSealedRoom(evt.Target.GetTruePosition())) return;

        var enteredAfterSeal = EnteredAt.TryGetValue(evt.Source.PlayerId, out var enteredAt);

        var usedAbilityRecently = AbilityUsedAt.TryGetValue(evt.Source.PlayerId, out var usedAt) && Time.time - usedAt <= AbilityRecentWindow;

        var presenceStartedAt = enteredAfterSeal ? enteredAt : SealStartedAt;

        var presenceTime = Mathf.Max(0f, Time.time - presenceStartedAt);

        KillSnapshots[evt.Target.PlayerId] = (evt.Source.PlayerId, enteredAfterSeal, usedAbilityRecently, presenceTime);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || !KillSnapshots.Remove(evt.Target.PlayerId, out var snapshot)) return;

        if (snapshot.KillerId != evt.Source.PlayerId)
            return;

        var warden = Utils.PlayerById(SealOwnerId);

        if (!warden)
            return;

        RpcCreateResidualMark(PlayerControl.LocalPlayer, SealOwnerId, SealOrigin.x, SealOrigin.y, snapshot.EnteredAfterSeal, snapshot.UsedAbilityRecently, Mathf.Round(snapshot.PresenceTime));
    }

    [MethodRpc((uint)CustomRPC.WardenResidualMark)]
    public static void RpcCreateResidualMark(PlayerControl source, byte ownerId, float x, float y, bool enteredAfterSeal, bool usedAbilityRecently, float presenceTime)
    {
        if (source.OwnerId != AmongUsClient.Instance.HostId || PlayerControl.LocalPlayer.PlayerId != ownerId || PlayerControl.LocalPlayer.Data.IsDead)
            return;

        var mark = new GameObject("WardenResidualMark") { layer = PlayerControl.LocalPlayer.gameObject.layer };

        mark.transform.position = new Vector3(x, y, 0f);
        mark.transform.localScale = Vector3.one * 0.45f;

        var renderer = mark.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.ResidualTrace.LoadAsset();
        renderer.sortingOrder = 100;

        ResidualMarks[mark] = (enteredAfterSeal, usedAbilityRecently, presenceTime);

        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#3AA6FF>Violence detected.</color> A Residual Trace was recorded."));

        Coroutines.Start(CoExpireResidualMark(mark, OptionGroupSingleton<WardenOptions>.Instance.ResidualMarkDuration));
    }

    public static IEnumerator CoExpireResidualMark(GameObject mark, float duration)
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

        Destroy(mark);
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
        if (!ResidualMarks.Remove(mark, out var clue))
            return;

        Destroy(mark);

        var result = ClueType switch
        {
            0 => clue.EnteredAfterSeal ? "The attacker entered after the seal was placed." : "The attacker was inside when the seal was placed.",
            1 => clue.UsedAbilityRecently ? "The attacker used an ability within 4s before the attack." : "No ability use was recorded from the attacker in the preceding 4s.",
            _ => $"The attacker had been inside for about {clue.PresenceTime:0}s."
        };
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#3AA6FF>Residual Trace</color>\n{result}"));
        //NewModAchievementsTab.TraceEvidence.Unlock();
    }

    public static void ResetState(bool preserveMarks = false)
    {
        _sealVersion++;
        ClueType = 0;

        SealActive = false;
        SealOwnerId = byte.MaxValue;
        SealStartedAt = 0f;
        SealEndsAt = 0f;
        SealOrigin = Vector2.zero;

        InsideStates.Clear();
        EnteredAt.Clear();
        AbilityUsedAt.Clear();
        KillSnapshots.Clear();

        if (preserveMarks)
            return;

        foreach (var mark in ResidualMarks.Keys)
            Destroy(mark);

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
        ResetState(true);
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ResetState();
    }
}