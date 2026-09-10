using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using NewMod.Utilities;

namespace NewMod.Debugging.Tabs;

public class EnergyTab : IDebugTab
{
    public string Name => "ENERGY";
    public bool ShouldShow => ShipStatus.Instance != null && PlayerControl.LocalPlayer;
    
    public void BuildGUI()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            GUILayout.Label("Host only");
            return;
        }
        
        GUILayout.Label("STATUS");
        GUILayout.Label($"{PlayerTab.SelectedPlayer.Data.PlayerName} #{PlayerTab.SelectedPlayerId}");
        
        var thief = Utils.PlayerById(PlayerTab.SelectedPlayerId);
        if (!thief || thief.Data.Role is not EnergyThief)
        {
            GUILayout.Label("Select a player with the Energy Thief role");
            return;
        }
        
        EnergyThief.Energy.TryGetValue(thief.PlayerId, out var energy);
        EnergyThief.Categories.TryGetValue(thief.PlayerId, out var categories);
        categories ??= [];

        var options = OptionGroupSingleton<EnergyThiefOptions>.Instance;
        var categoryText = categories.Count == 0 ? "None" : string.Join(" · ", categories);
        var nodeState = EnergyThief.NodeOwnerId == thief.PlayerId ? EnergyThief.BreachActive ? $"Breaching · {Mathf.Max(0f, EnergyThief.BreachEndsAt - Time.time):0.0}s" : "Located" : "Not located";
        
        GUILayout.Label($"Energy  {energy}/{(int)options.EnergyRequired}");
        GUILayout.Label($"Resonance  {categories.Count}/{(int)options.CategoriesRequired}");
        GUILayout.Label(categoryText);
        GUILayout.Label($"Node  {nodeState}");
        
        GUILayout.Label("CAPTURE");

        if (GUILayout.Button("Capture Aggression"))
        {
            CaptureEnergy(thief, EnergyCategory.Aggression, false);
        }
        if (GUILayout.Button("Capture Control"))
        {
            CaptureEnergy(thief, EnergyCategory.Control, false);
        }
        if (GUILayout.Button("Capture Intelligence"))
        {
            CaptureEnergy(thief, EnergyCategory.Intelligence, false);
        }
        if (GUILayout.Button("Capture Mobility"))
        {
            CaptureEnergy(thief, EnergyCategory.Mobility, false);
        }
        if (GUILayout.Button("Capture Protection"))
        {
            CaptureEnergy(thief, EnergyCategory.Protection, false);
        }
        if (GUILayout.Button("Add Raw Energy"))
        {
            CaptureEnergy(thief, EnergyCategory.Aggression, true);
        }
        if (GUILayout.Button("Complete Requirements"))
        {
            CompleteEnergyThiefRequirements(thief);
        }
        if (GUILayout.Button("Cancel Siphon"))
        {
            EnergyThief.RpcConfirmCancelSiphon(PlayerControl.LocalPlayer, thief.PlayerId);
        }
        
        GUILayout.Label("POWER NODE");
        
        if (GUILayout.Button("Go To Node"))
        {
            if (EnergyThief.NodeOwnerId == thief.PlayerId)
                thief.NetTransform.RpcSnapTo(EnergyThief.GetNodePosition() + Vector3.down * 0.5f);
        }
        if (GUILayout.Button("Start Breach"))
        {
            EnergyThief.RpcRequestBreach(thief);
        }
        if (GUILayout.Button("Interrupt Breach"))
        {
            if (EnergyThief.BreachActive && EnergyThief.NodeOwnerId == thief.PlayerId)
                EnergyThief.RpcResolveBreach(PlayerControl.LocalPlayer, thief.PlayerId, false);
        }
        if (GUILayout.Button("Complete Breach"))
        {
            if (EnergyThief.BreachActive && EnergyThief.NodeOwnerId == thief.PlayerId)
                EnergyThief.RpcResolveBreach(PlayerControl.LocalPlayer, thief.PlayerId, true);
        }
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
}