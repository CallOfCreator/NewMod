using NewMod.Modifiers.S1;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public class Collector : CrewmateRole, INewModRole
{
    public static readonly Dictionary<uint, FragmentRecord> Fragments = [];
    public static readonly Dictionary<byte, int[]> Inventories = [];
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
            text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Collector.Tab.Inventory"), inventory[(int)FragmentKind.Violence], inventory[(int)FragmentKind.Ability], inventory[(int)FragmentKind.Fate], OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost));
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
                    Inventories[player.PlayerId] = new int[3];
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
            Inventories[evt.Player.PlayerId] = new int[3];
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

        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = (kind switch
        {
            FragmentKind.Violence => NewModAsset.ViolenceFragment,
            FragmentKind.Ability => NewModAsset.AbilityFragment,
            _ => NewModAsset.FateFragment
        }).LoadAsset();
        var size = renderer.sprite.bounds.size;
        gameObject.transform.localScale = Vector3.one * (0.6f / Mathf.Max(size.x, size.y));

        gameObject.SetActive(PlayerControl.LocalPlayer.Data.Role is Collector && !PlayerControl.LocalPlayer.Data.IsDead);
        Fragments[fragmentId] = new FragmentRecord { Kind = kind, Position = new Vector2(x, y), Object = gameObject };
    }

    [MethodRpc((uint)CustomRPC.CollectorRequestHarvest)]
    public static void RpcRequestHarvest(PlayerControl source, uint fragmentId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Collector || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || !Fragments.TryGetValue(fragmentId, out var fragment) || Vector2.Distance(source.GetTruePosition(), fragment.Position) > OptionGroupSingleton<CollectorOptions>.Instance.HarvestRange)
            return;

        if (PhysicsHelpers.AnythingBetween(source.GetTruePosition(), fragment.Position, Constants.ShipAndObjectsMask, false)) return;
        OverclockedModifier.Pulse(source);
        RpcConfirmHarvest(PlayerControl.LocalPlayer, source.PlayerId, fragmentId, (byte)fragment.Kind);
    }

    [MethodRpc((uint)CustomRPC.CollectorConfirmHarvest, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmHarvest(PlayerControl source, byte collectorId, uint fragmentId, byte kindId)
    {
        if (!source.IsHost() || !Fragments.Remove(fragmentId, out var fragment) || !Inventories.TryGetValue(collectorId, out var inventory))
            return;

        Destroy(fragment.Object);
        inventory[kindId]++;

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

        if (!CanManifest(inventory))
            return;

        RpcConfirmManifest(PlayerControl.LocalPlayer, source.PlayerId, (byte)Manifest(inventory));
    }

    [MethodRpc((uint)CustomRPC.CollectorConfirmManifest, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmManifest(PlayerControl source, byte collectorId, byte result)
    {
        if (!source.IsHost() || !Inventories.TryGetValue(collectorId, out var inventory))
            return;

        var power = (FragmentPower)result;
        if (!AmongUsClient.Instance.AmHost)
            Manifest(inventory);

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
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Collector || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || VictoryArmed.Contains(source.PlayerId) || !Inventories.TryGetValue(source.PlayerId, out var inventory) || !CanConvert(inventory, (int)OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost)) return;
        RpcConfirmConvert(PlayerControl.LocalPlayer, source.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.CollectorConfirmConvert)]
    public static void RpcConfirmConvert(PlayerControl source, byte ownerId)
    {
        if (!source.IsHost()) return;
        ConvertFragments(Inventories[ownerId], (int)OptionGroupSingleton<CollectorOptions>.Instance.ConversionCost);
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

        var renderer = nearest.GetComponent<SpriteRenderer>();
        var start = renderer.color;
        renderer.color = Color.yellow;
        yield return new WaitForSeconds(options.TraceDuration);
        if (renderer)
            renderer.color = start;
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

    public enum FragmentKind : byte
    {
        Violence,
        Ability,
        Fate
    }

    public enum FragmentPower : byte
    {
        None,
        Victory,
        Trace,
        Drift
    }

    public static bool CanConvert(int[] fragments, int cost)
    {
        return System.Array.Exists(fragments, count => count == 0) && System.Array.Exists(fragments, count => count > cost);
    }

    public static bool ConvertFragments(int[] fragments, int cost)
    {
        var missing = System.Array.FindIndex(fragments, count => count == 0);
        var donor = System.Array.FindIndex(fragments, count => count > cost);
        if (missing < 0 || donor < 0)
            return false;

        fragments[donor] -= cost;
        fragments[missing]++;
        return true;
    }

    public static bool CanManifest(int[] fragments)
    {
        return fragments.All(count => count > 0) || fragments.Any(count => count >= 2);
    }

    public static FragmentPower Manifest(int[] fragments)
    {
        if (fragments.All(count => count > 0))
        {
            for (var index = 0; index < fragments.Length; index++)
                fragments[index]--;
            return FragmentPower.Victory;
        }

        var pair = System.Array.FindIndex(fragments, count => count >= 2);
        if (pair < 0)
            return FragmentPower.None;

        fragments[pair] -= 2;
        return pair == (int)FragmentKind.Fate ? FragmentPower.Drift : FragmentPower.Trace;
    }
}