using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Roles;
using NewMod.Components.ScreenEffects;
using NewMod.GeneralEvents;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Utilities;
using ReactUI.Core;
using ReactUI.Hooks;
using UnityEngine;
using static ReactUI.UI;

namespace NewMod.UI;

public static class NewModDebugPanel
{
    private static readonly Func<VNode> Root = Component(Render);

    private static int _componentId;
    private static int _screenWidth;
    private static int _screenHeight;
    private static float _nextRefresh;
    private static Page _page;
    private static byte _selectedPlayerId = byte.MaxValue;
    private static string _selectedRole = "Terminator";

    public static bool IsOpen { get; private set; }
    internal static bool CameraControlsOpen => IsOpen && _page == Page.Match;

    public static void Mount()
    {
        ReactUI.UI.Render(Root);
    }

    public static void SetOpen(bool open)
    {
        IsOpen = open;

        if (_componentId != 0)
            Scheduler.ScheduleRender(_componentId);
    }

    public static void Tick()
    {
        if (_screenWidth != Screen.width || _screenHeight != Screen.height)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            NewModDebugStyles.Register();

            if (_componentId != 0)
                Scheduler.ScheduleRender(_componentId);
        }

        if (!IsOpen || _componentId == 0 || Time.unscaledTime < _nextRefresh)
            return;

        _nextRefresh = Time.unscaledTime + 0.25f;
        Scheduler.ScheduleRender(_componentId);
    }

    private static VNode Render()
    {
        _componentId = HooksRuntime.Current.ComponentId;

        if (!IsOpen)
            return Div();

        return Div(ClassName("nm-debug-panel"), Header(), Navigation(), ScrollView(ClassName("nm-debug-scroll"), PageContent()));
    }

    private static VNode Header()
    {
        return Div(ClassName("nm-debug-header"), Div(ClassName("nm-debug-brand"), Text("NEWMOD", ClassName("nm-debug-title")), Text("/ DEBUG", ClassName("nm-debug-title-muted"))), Button("×", () => SetOpen(false), ClassName("nm-debug-close")));
    }

    private static VNode Navigation()
    {
        return Div(ClassName("nm-debug-tabs"), Tab("PLAYER", Page.Player), Tab("MATCH", Page.Match), Tab("ENERGY", Page.EnergyThief), Tab("EVENTS", Page.Events), Tab("EFFECTS", Page.Effects));
    }

    private static VNode Tab(string label, Page page)
    {
        var style = _page == page ? "nm-debug-tab nm-debug-tab-active" : "nm-debug-tab";

        return Button(label, () =>
        {
            _page = page;
            Scheduler.ScheduleRender(_componentId);
        }, ClassName(style));
    }

    private static VNode PageContent()
    {
        return _page switch
        {
            Page.Player => PlayerPage(),
            Page.Match => MatchPage(),
            Page.EnergyThief => EnergyThiefPage(),
            Page.Events => EventsPage(),
            Page.Effects => EffectsPage(),
            _ => Div()
        };
    }

    private static VNode PlayerPage()
    {
        var players = new List<PlayerControl>();

        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data?.Role != null && !player.Data.Disconnected)
                players.Add(player);

        if (players.Count == 0)
            return Section("TARGET", Text("No initialized players.", ClassName("nm-debug-muted")));

        if (players.All(player => player.PlayerId != _selectedPlayerId))
            _selectedPlayerId = players[0].PlayerId;

        var selectedPlayerIndex = players.FindIndex(player => player.PlayerId == _selectedPlayerId);
        var selectedPlayer = players[selectedPlayerIndex];
        var roles = CustomRoleManager.CustomMiraRoles.Where(role => role.GetType().Assembly == typeof(NewMod).Assembly).OrderBy(role => role.RoleName).ToArray();

        VNode roleSection;

        if (roles.Length == 0)
        {
            roleSection = Section("ASSIGN ROLE", Text("No registered NewMod roles.", ClassName("nm-debug-muted")));
        }
        else
        {
            if (roles.All(role => role.RoleName != _selectedRole))
                _selectedRole = roles[0].RoleName;

            var selectedRoleIndex = Array.FindIndex(roles, role => role.RoleName == _selectedRole);

            roleSection = Section("ASSIGN ROLE", Selector(_selectedRole, () =>
            {
                selectedRoleIndex = (selectedRoleIndex - 1 + roles.Length) % roles.Length;
                _selectedRole = roles[selectedRoleIndex].RoleName;
                Scheduler.ScheduleRender(_componentId);
            }, () =>
            {
                selectedRoleIndex = (selectedRoleIndex + 1) % roles.Length;
                _selectedRole = roles[selectedRoleIndex].RoleName;
                Scheduler.ScheduleRender(_componentId);
            }), MutatingActions(("Assign role", () =>
            {
                var role = (RoleBehaviour)roles.First(candidate => candidate.RoleName == _selectedRole);
                selectedPlayer.RpcSetRole(role.Role);
            }, false)));
        }

        return Div(ClassName("nm-debug-stack"), Section("TARGET", Selector($"{selectedPlayer.Data.PlayerName}  #{selectedPlayer.PlayerId}", () =>
        {
            selectedPlayerIndex = (selectedPlayerIndex - 1 + players.Count) % players.Count;
            _selectedPlayerId = players[selectedPlayerIndex].PlayerId;
            Scheduler.ScheduleRender(_componentId);
        }, () =>
        {
            selectedPlayerIndex = (selectedPlayerIndex + 1) % players.Count;
            _selectedPlayerId = players[selectedPlayerIndex].PlayerId;
            Scheduler.ScheduleRender(_componentId);
        })), Section("POSITION", MutatingActions(("Go to target", () => PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(selectedPlayer.GetTruePosition()), false), ("Bring target here", () => selectedPlayer.NetTransform.RpcSnapTo(PlayerControl.LocalPlayer.GetTruePosition()), false))), Section("STATE", MutatingActions(("Kill", () => selectedPlayer.RpcAdvancedCustomMurder(selectedPlayer, MeetingCheck.OutsideMeeting, true, true, resetKillTimer: false, teleportMurderer: false, showKillAnim: false, playKillSound: false), true), ("Revive", () => Utils.HandleRevive(PlayerControl.LocalPlayer, selectedPlayer.PlayerId, RoleTypes.Crewmate, selectedPlayer.GetTruePosition().x, selectedPlayer.GetTruePosition().y), false))), roleSection);
    }

    private static VNode MatchPage()
    {
        var zoom = DebugWindow.Instance.Zoom;
        var meeting = MeetingHud.Instance;
        var meetingSection = meeting ? Section("MEETING", MutatingActions(("Cast random vote", () => CastRandomVote(meeting), false), ("Close meeting", meeting.Close, true))) : Section("MEETING", Text("No active meeting.", ClassName("nm-debug-muted")));

        return Div(ClassName("nm-debug-stack"), Section("CAMERA · MOUSE WHEEL", Div(ClassName("nm-debug-row"), Slider(zoom, DebugWindow.Instance.ApplyZoom, DebugWindow.ZoomMin, DebugWindow.ZoomMax, ClassName("nm-debug-slider"), 0.1f, 14f, 24f, 5f, 5f), Text(zoom.ToString("0.0"), ClassName("nm-debug-value"))), MutatingActions(("Reset zoom", () => DebugWindow.Instance.ApplyZoom(3f), false))), Section("LOCAL ABILITIES", MutatingActions(("Reset kill cooldown", () => PlayerControl.LocalPlayer.SetKillTimer(0f), false), ("Reset button cooldowns", ResetButtonCooldowns, false), ("Set button uses to 3", () =>
        {
            foreach (var button in CustomButtonManager.Buttons)
                button.SetUses(3);
        }, false))), meetingSection, Section("SEASONS", Toggle(NewMod.ForceEnableAllSeasons.Value, value => NewMod.ForceEnableAllSeasons.Value = value, ClassName("nm-debug-toggle")), Text($"Force all seasons: {(NewMod.ForceEnableAllSeasons.Value ? "ON" : "OFF")}", ClassName("nm-debug-value"))));
    }

    private static VNode EventsPage()
    {
        return Div(ClassName("nm-debug-stack"), Section("CYCLE", MutatingActions(("Start", GeneralEventManager.StartCycle, false), ("Stop", GeneralEventManager.StopCycle, false), ("End current event", GeneralEventManager.ForceEnd, true))), Section("FORCE EVENT", MutatingActions(GeneralEventManager.RegisteredEvents.Select(generalEvent => (generalEvent.Title, (Action)(() => GeneralEventManager.ForceEvent(generalEvent.GetType())), false)).ToArray())));
    }

    private static VNode EnergyThiefPage()
    {
        if (!AmongUsClient.Instance.AmHost)
            return Section("ENERGY THIEF", Text("Host only.", ClassName("nm-debug-muted")));

        var thief = Utils.PlayerById(_selectedPlayerId);

        if (!thief || thief.Data.Role is not EnergyThief)
            return Section("ENERGY THIEF", Text("Select a player with the Energy Thief role.", ClassName("nm-debug-muted")));

        EnergyThief.Energy.TryGetValue(thief.PlayerId, out var energy);
        EnergyThief.Categories.TryGetValue(thief.PlayerId, out var categories);
        categories ??= [];

        var options = OptionGroupSingleton<EnergyThiefOptions>.Instance;
        var categoryText = categories.Count == 0 ? "None" : string.Join(" · ", categories);
        var nodeState = EnergyThief.NodeOwnerId == thief.PlayerId ? EnergyThief.BreachActive ? $"Breaching · {Mathf.Max(0f, EnergyThief.BreachEndsAt - Time.time):0.0}s" : "Located" : "Not located";

        return Div(ClassName("nm-debug-stack"), Section("STATUS", Text($"{thief.Data.PlayerName}  #{thief.PlayerId}", ClassName("nm-debug-value")), Text($"Energy  {energy}/{(int)options.EnergyRequired}", ClassName("nm-debug-label")), Text($"Resonance  {categories.Count}/{(int)options.CategoriesRequired}", ClassName("nm-debug-label")), Text(categoryText, ClassName("nm-debug-muted")), Text($"Node  {nodeState}", ClassName("nm-debug-label"))), Section("CAPTURE", MutatingActions(("Capture aggression", () => CaptureEnergy(thief, EnergyCategory.Aggression, false), false), ("Capture control", () => CaptureEnergy(thief, EnergyCategory.Control, false), false), ("Capture intelligence", () => CaptureEnergy(thief, EnergyCategory.Intelligence, false), false), ("Capture mobility", () => CaptureEnergy(thief, EnergyCategory.Mobility, false), false), ("Capture protection", () => CaptureEnergy(thief, EnergyCategory.Protection, false), false), ("Add raw energy", () => CaptureEnergy(thief, EnergyCategory.Aggression, true), false), ("Complete requirements", () => CompleteEnergyThiefRequirements(thief), false), ("Cancel siphon", () => EnergyThief.RpcConfirmCancelSiphon(PlayerControl.LocalPlayer, thief.PlayerId), false))), Section("POWER NODE", MutatingActions(("Go to node", () =>
        {
            if (EnergyThief.NodeOwnerId == thief.PlayerId)
                thief.NetTransform.RpcSnapTo(EnergyThief.GetNodePosition() + Vector3.down * 0.5f);
        }, false), ("Start breach", () => EnergyThief.RpcRequestBreach(thief), false), ("Interrupt breach", () =>
        {
            if (EnergyThief.BreachActive && EnergyThief.NodeOwnerId == thief.PlayerId)
                EnergyThief.RpcResolveBreach(PlayerControl.LocalPlayer, thief.PlayerId, false);
        }, false), ("Complete breach", () =>
        {
            if (EnergyThief.BreachActive && EnergyThief.NodeOwnerId == thief.PlayerId)
                EnergyThief.RpcResolveBreach(PlayerControl.LocalPlayer, thief.PlayerId, true);
        }, true))));
    }

    private static VNode EffectsPage()
    {
        var camera = Camera.main;
        var glitch = camera.GetScreenEffect<GlitchEffect>();
        var distortion = camera.GetScreenEffect<DistorationWaveEffect>();
        var flux = camera.GetScreenEffect<ShadowFluxEffect>();
        var sections = new List<VNode> { Section("EFFECTS", MutatingActions(("Energy breach", PlayEnergyThiefBreak, false), ("Glitch", AddEffect<GlitchEffect>, false), ("Earthquake", AddEffect<EarthquakeEffect>, false), ("Pulse hue", AddEffect<SlowPulseHueEffect>, false), ("Distortion wave", AddEffect<DistorationWaveEffect>, false), ("Shadow flux", AddEffect<ShadowFluxEffect>, false), ("Negative reality", AddEffect<NegativeRealityEffect>, false), ("Shattered glass", AddEffect<ShatteredGlassEffect>, false), ("Glitch V2", AddEffect<ScrDesyncEffect>, false), ("Remove all", RemoveEffects, true))) };

        if (glitch != null && glitch.Active) sections.Add(Section("GLITCH", EffectSlider("Intensity", glitch.intensity, value => glitch.intensity = value, 0f, 1f), EffectSlider("Block size", glitch.blockSize, value => glitch.blockSize = value, 8f, 128f), EffectSlider("Colour split", glitch.colorSplit, value => glitch.colorSplit = value, 0f, 3f), EffectSlider("Speed", glitch.speed, value => glitch.speed = value, 0f, 10f)));

        if (distortion != null && distortion.Active) sections.Add(Section("DISTORTION WAVE", EffectSlider("Amplitude", distortion.amplitude, value => distortion.amplitude = value, 0f, 0.25f), EffectSlider("Frequency", distortion.frequency, value => distortion.frequency = value, 0f, 12f), EffectSlider("Speed", distortion.speed, value => distortion.speed = value, 0f, 5f), EffectSlider("Radius", distortion.radius, value => distortion.radius = value, 0f, 1f), EffectSlider("Falloff", distortion.falloff, value => distortion.falloff = value, 0f, 5f)));

        if (flux != null && flux.Active) sections.Add(Section("SHADOW FLUX", EffectSlider("Noise scale", flux.noiseScale, value => flux.noiseScale = value, 0f, 5f), EffectSlider("Speed", flux.speed, value => flux.speed = value, 0f, 3f), EffectSlider("Edge width", flux.edgeWidth, value => flux.edgeWidth = value, 0f, 1f), EffectSlider("Threshold", flux.threshold, value => flux.threshold = value, 0f, 1f), EffectSlider("Opacity", flux.opacity, value => flux.opacity = value, 0f, 1f), EffectSlider("Darkness", flux.darkness, value => flux.darkness = value, 0f, 1f)));

        return Div(ClassName("nm-debug-stack"), sections);
    }

    private static VNode Section(string title, params VNode[] content)
    {
        var children = new List<VNode> { Text(title, ClassName("nm-debug-section-title")) };
        children.AddRange(content);
        return Div(ClassName("nm-debug-section"), children);
    }

    private static VNode Selector(string value, Action previous, Action next)
    {
        return Div(ClassName("nm-debug-selector"), Button("‹", previous, ClassName("nm-debug-selector-button")), Text(value, ClassName("nm-debug-selector-value")), Button("›", next, ClassName("nm-debug-selector-button")));
    }

    private static VNode MutatingActions(params (string Label, Action Action, bool Danger)[] actions)
    {
        return Div(ClassName("nm-debug-row-wrap"), actions.Select(action => Button(action.Label, action.Action, ClassName(action.Danger ? "nm-debug-button nm-debug-button-danger" : "nm-debug-button"))));
    }

    private static VNode EffectSlider(string label, float value, Action<float> onChange, float minimum, float maximum)
    {
        return Div(ClassName("nm-debug-row"), Text(label, ClassName("nm-debug-label")), Slider(value, onChange, minimum, maximum, ClassName("nm-debug-slider"), 0f, 14f, 24f, 5f, 5f), Text(value.ToString("0.00"), ClassName("nm-debug-value")));
    }

    private static void ResetButtonCooldowns()
    {
        foreach (var button in CustomButtonManager.Buttons)
            button.ResetCooldownAndOrEffect();
    }

    private static void CaptureEnergy(PlayerControl thief, EnergyCategory category, bool raw)
    {
        if (EnergyThief.TetherTargets.ContainsKey(thief.PlayerId))
            EnergyThief.RpcConfirmCancelSiphon(PlayerControl.LocalPlayer, thief.PlayerId);

        var target = Utils.GetRandomPlayer(player => player != thief && !player.Data.IsDead && !player.Data.Disconnected);

        if (!target)
            return;

        EnergyThief.RpcConfirmSiphon(PlayerControl.LocalPlayer, thief.PlayerId, target.PlayerId, OptionGroupSingleton<EnergyThiefOptions>.Instance.SiphonDuration);
        EnergyThief.RpcConfirmCapture(PlayerControl.LocalPlayer, thief.PlayerId, target.PlayerId, (byte)category, raw);
    }

    private static void CompleteEnergyThiefRequirements(PlayerControl thief)
    {
        var categories = new[] { EnergyCategory.Aggression, EnergyCategory.Control, EnergyCategory.Intelligence, EnergyCategory.Mobility, EnergyCategory.Protection };

        foreach (var category in categories)
            if (!EnergyThief.Categories.TryGetValue(thief.PlayerId, out var captured) || !captured.Contains(category))
                CaptureEnergy(thief, category, false);

        EnergyThief.Energy.TryGetValue(thief.PlayerId, out var energy);
        var missingEnergy = Mathf.Max(0, (int)OptionGroupSingleton<EnergyThiefOptions>.Instance.EnergyRequired - energy);
        var rawCaptures = Mathf.CeilToInt(missingEnergy / (float)EnergyThief.RawEnergyGain);

        for (var i = 0; i < rawCaptures; i++)
            CaptureEnergy(thief, EnergyCategory.Aggression, true);
    }

    private static void CastRandomVote(MeetingHud meeting)
    {
        var target = Utils.GetRandomPlayer(player => !player.Data.IsDead && !player.Data.Disconnected);

        if (target)
            meeting.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
    }

    private static void AddEffect<T>() where T : ScreenEffect, new()
    {
        Camera.main.AddScreenEffect<T>();
    }

    private static void PlayEnergyThiefBreak()
    {
        var effect = Camera.main.GetScreenEffect<EnergyThiefBreakEffect>();

        if (effect != null && effect.Active)
            effect.Restart();
        else
            Camera.main.AddScreenEffect<EnergyThiefBreakEffect>();
    }

    private static void RemoveEffects()
    {
        Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 0f));
    }

    private enum Page
    {
        Player,
        Match,
        EnergyThief,
        Events,
        Effects
    }
}