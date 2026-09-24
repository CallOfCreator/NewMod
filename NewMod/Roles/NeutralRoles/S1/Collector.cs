using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.RoleLogic;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

using FragmentKind = CollectorFragmentKind;
using FragmentPower = CollectorManifestResult;

[MiraIgnore]
public class Collector : CrewmateRole, INewModRole
{
    public static readonly Dictionary<uint, FragmentRecord> Fragments = [];
    public static readonly Dictionary<byte, CollectorInventory> Inventories = [];
    public static readonly HashSet<byte> VictoryArmed = [];
    public static readonly HashSet<byte> EnvironmentalDeaths = [];

    public static uint _nextFragmentId;
    public static bool ExilePending;
    public static Vector2 ExilePosition;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Collector");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Collector.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Collector.TabDescription");
    public Color RoleColor => new(0.76f, 0.42f, 0.93f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 35,
            CanModifyChance = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = MiraAssets.Empty
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        text.AppendLine();
        if (VictoryArmed.Contains(PlayerControl.LocalPlayer.PlayerId)) text.AppendLine(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Collector.Tab.ManifestComplete"));
        if (Inventories.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var inventory))
            text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Collector.Tab.Inventory"), inventory.Count(FragmentKind.Violence), inventory.Count(FragmentKind.Ability), inventory.Count(FragmentKind.Fate), OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost));
        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<CollectorGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            foreach (var fragment in Fragments.Values)
                Destroy(fragment.Object);

            Fragments.Clear();
            Inventories.Clear();
            VictoryArmed.Clear();
            EnvironmentalDeaths.Clear();
            _nextFragmentId = 0;
            ExilePending = false;
            ExilePosition = Vector2.zero;

            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.Data.Role is Collector)
                    Inventories[player.PlayerId] = new CollectorInventory();
        }

        if (!evt.TriggeredByIntro && ExilePending && AmongUsClient.Instance.AmHost)
        {
            ExilePending = false;
            RpcSpawnFragment(PlayerControl.LocalPlayer, ++_nextFragmentId, (byte)FragmentKind.Fate, ExilePosition.x, ExilePosition.y);
        }
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is Collector)
        {
            Inventories[evt.Player.PlayerId] = new CollectorInventory();
        }
        else
        {
            Inventories.Remove(evt.Player.PlayerId);
            VictoryArmed.Remove(evt.Player.PlayerId);
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || Inventories.Count == 0)
            return;

        var kind = EnvironmentalDeaths.Remove(evt.Target.PlayerId) || evt.Source == evt.Target ? FragmentKind.Fate : evt.IsIndirectAttack ? FragmentKind.Ability : FragmentKind.Violence;
        var position = evt.DeadBody ? (Vector2)evt.DeadBody.transform.position : evt.Target.GetTruePosition();
        RpcSpawnFragment(PlayerControl.LocalPlayer, ++_nextFragmentId, (byte)kind, position.x, position.y);
    }

    [RegisterEvent]
    public static void OnMeetingResolved(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro || !GameManager.Instance.ShouldCheckForGameEnd || !AmongUsClient.Instance.AmHost)
            return;

        foreach (var playerId in VictoryArmed)
        {
            var player = Utils.PlayerById(playerId);
            if (player && !player.Data.IsDead && !player.Data.Disconnected)
            {
                CustomGameOver.Trigger<CollectorGameOver>([player.Data]);
                return;
            }
        }
    }

    public static void MarkEnvironmental(byte playerId)
    {
        if (AmongUsClient.Instance.AmHost)
            EnvironmentalDeaths.Add(playerId);
    }

    [MethodRpc((uint)CustomRPC.CollectorSpawnFragment, LocalHandling = RpcLocalHandling.After)]
    public static void RpcSpawnFragment(PlayerControl source, uint fragmentId, byte kindId, float x, float y)
    {
        if (!source.IsHost() || Fragments.ContainsKey(fragmentId))
            return;

        var kind = (FragmentKind)kindId;
        var gameObject = new GameObject($"CollectorFragment_{fragmentId}");
        gameObject.transform.position = new Vector3(x, y, -1f);

        var line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 4;
        line.startWidth = 0.065f;
        line.endWidth = 0.065f;
        line.sharedMaterial = Utils.GetCircleMat();
        line.SetPosition(0, new Vector3(0f, 0.28f));
        line.SetPosition(1, new Vector3(0.2f, 0f));
        line.SetPosition(2, new Vector3(0f, -0.28f));
        line.SetPosition(3, new Vector3(-0.2f, 0f));
        line.startColor = line.endColor = kind switch
        {
            FragmentKind.Violence => new Color(0.95f, 0.15f, 0.15f),
            FragmentKind.Ability => new Color(0.72f, 0.2f, 0.95f),
            FragmentKind.Fate => new Color(0.31f, 0.62f, 1f),
            _ => Color.white
        };

        gameObject.SetActive(PlayerControl.LocalPlayer.Data.Role is Collector && !PlayerControl.LocalPlayer.Data.IsDead);
        Fragments[fragmentId] = new FragmentRecord { Kind = kind, Position = new Vector2(x, y), Object = gameObject };
    }

    [MethodRpc((uint)CustomRPC.CollectorRequestHarvest)]
    public static void RpcRequestHarvest(PlayerControl source, uint fragmentId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Collector || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || !Fragments.TryGetValue(fragmentId, out var fragment) || Vector2.Distance(source.GetTruePosition(), fragment.Position) > OptionGroupSingleton<CollectorOptions>.Instance.HarvestRange)
            return;

        if (PhysicsHelpers.AnythingBetween(source.GetTruePosition(), fragment.Position, Constants.ShipAndObjectsMask, false)) return;
        RpcConfirmHarvest(PlayerControl.LocalPlayer, source.PlayerId, fragmentId, (byte)fragment.Kind);
    }

    [MethodRpc((uint)CustomRPC.CollectorConfirmHarvest, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmHarvest(PlayerControl source, byte collectorId, uint fragmentId, byte kindId)
    {
        if (!source.IsHost() || !Fragments.Remove(fragmentId, out var fragment) || !Inventories.TryGetValue(collectorId, out var inventory))
            return;

        Destroy(fragment.Object);
        inventory.Add((FragmentKind)kindId);

        var collector = Utils.PlayerById(collectorId);
        var local = PlayerControl.LocalPlayer;
        var options = OptionGroupSingleton<CollectorOptions>.Instance;
        if (local.PlayerId != collectorId && !local.Data.IsDead && Vector2.Distance(local.GetTruePosition(), collector.GetTruePosition()) <= options.HarvestRevealRange)
            Coroutines.Start(CoRevealCollector(collector, options.HarvestRevealDuration));
    }

    [MethodRpc((uint)CustomRPC.CollectorRequestManifest)]
    public static void RpcRequestManifest(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Collector || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || VictoryArmed.Contains(source.PlayerId) || !Inventories.TryGetValue(source.PlayerId, out var inventory))
            return;

        if (!inventory.CanManifest)
            return;

        RpcConfirmManifest(PlayerControl.LocalPlayer, source.PlayerId, (byte)inventory.Manifest());
    }

    [MethodRpc((uint)CustomRPC.CollectorConfirmManifest, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmManifest(PlayerControl source, byte collectorId, byte result)
    {
        if (!source.IsHost() || !Inventories.TryGetValue(collectorId, out var inventory))
            return;

        var power = (FragmentPower)result;
        if (!AmongUsClient.Instance.AmHost)
            inventory.Manifest();

        if (power == FragmentPower.Victory)
        {
            VictoryArmed.Add(collectorId);
            Coroutines.Start(CoroutinesHelper.CoNotify("A Collector has completed their collection.\nFind them before the next meeting ends."));
            return;
        }

        var player = Utils.PlayerById(collectorId);
        if (player.AmOwner)
            Coroutines.Start(CoFragmentPower(player, power));
    }

    [MethodRpc((uint)CustomRPC.CollectorRequestConvert)]
    public static void RpcRequestConvert(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Collector || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || VictoryArmed.Contains(source.PlayerId) || !Inventories.TryGetValue(source.PlayerId, out var inventory) || !inventory.CanConvert((int)OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost)) return;
        RpcConfirmConvert(PlayerControl.LocalPlayer, source.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.CollectorConfirmConvert)]
    public static void RpcConfirmConvert(PlayerControl source, byte ownerId)
    {
        if (!source.IsHost()) return;
        Inventories[ownerId].Convert((int)OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost);
        if (PlayerControl.LocalPlayer.PlayerId == ownerId)
            Coroutines.Start(CoroutinesHelper.CoNotify("Duplicates converted into a missing fragment."));
    }

    public static IEnumerator CoFragmentPower(PlayerControl player, FragmentPower power)
    {
        var options = OptionGroupSingleton<CollectorOptions>.Instance;
        if (power == FragmentPower.Drift)
        {
            var originalSpeed = player.MyPhysics.Speed;
            player.MyPhysics.Speed *= 1f + options.DriftSpeedBonus / 100f;
            yield return new WaitForSeconds(options.DriftDuration);
            player.MyPhysics.Speed = originalSpeed;
            yield break;
        }

        GameObject nearest = null;
        var distance = float.MaxValue;
        foreach (var fragment in Fragments.Values)
        {
            var candidate = Vector2.Distance(player.GetTruePosition(), fragment.Position);
            if (candidate >= distance)
                continue;

            distance = candidate;
            nearest = fragment.Object;
        }

        if (!nearest)
            yield break;

        var line = nearest.GetComponent<LineRenderer>();
        var start = line.startColor;
        line.startColor = line.endColor = Color.yellow;
        yield return new WaitForSeconds(options.TraceDuration);
        if (line)
            line.startColor = line.endColor = start;
    }

    public static IEnumerator CoRevealCollector(PlayerControl collector, float duration)
    {
        var gameObject = new GameObject("CollectorHarvestReveal") { layer = 5 };
        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.Arrow.LoadAsset();
        renderer.color = new Color(0.76f, 0.42f, 0.93f);

        var arrow = gameObject.AddComponent<ArrowBehaviour>();
        arrow.alwaysMaxSize = true;
        arrow.MaxScale = 0.65f;

        while (duration > 0f && collector && !collector.Data.Disconnected)
        {
            arrow.target = collector.transform.position;
            duration -= Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public class FragmentRecord
    {
        public FragmentKind Kind;
        public GameObject Object;
        public Vector2 Position;
    }
}