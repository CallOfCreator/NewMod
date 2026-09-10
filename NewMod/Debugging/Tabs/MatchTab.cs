using MiraAPI.Hud;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Debugging.Tabs;

public class MatchTab : IDebugTab
{
    public string Name => "MATCH";
    public bool ShouldShow => ShipStatus.Instance != null && PlayerControl.LocalPlayer;

    private float _zoom;
    private bool _forceSeasons;

    public void BuildGUI()
    {
        if (!PlayerControl.LocalPlayer) return;
        
        _zoom = DebugWindow.Instance.Zoom;
        var meeting = MeetingHud.Instance;
        
        GUILayout.Label("CAMERA MOUSE WHEEL");

        float prevZoom = _zoom;
        
        GUILayout.BeginHorizontal();

        _zoom = GUILayout.HorizontalSlider(DebugWindow.Instance.Zoom, DebugWindow.ZoomMin, DebugWindow.ZoomMax);
        GUILayout.Label(_zoom.ToString());
        
        GUILayout.EndHorizontal();
        
        if (_zoom != prevZoom)
        {
            DebugWindow.Instance.ApplyZoom(_zoom);
        }

        if (GUILayout.Button("Reset _zoom"))
        {
            DebugWindow.Instance.ApplyZoom(DebugWindow.ZoomDefault);
        }
        
        GUILayout.Label("LOCAL ABILITIES");
        
        //GUILayout.BeginHorizontal();

        if (GUILayout.Button("Reset Kill Cooldown"))
        {
            PlayerControl.LocalPlayer.SetKillTimer(0f);
        }
        if (GUILayout.Button("Reset Button Cooldowns"))
        {
            foreach (var button in CustomButtonManager.Buttons)
                button.ResetCooldownAndOrEffect();
        }
        if (GUILayout.Button("Reset Button Uses to 3"))
        {
            foreach (var button in CustomButtonManager.Buttons)
                button.SetUses(3);
        }
        
        //GUILayout.EndHorizontal();

        if (meeting)
        {
            if (GUILayout.Button("Cast Random Vote"))
            {
                var target = Utils.GetRandomPlayer(player => !player.Data.IsDead && !player.Data.Disconnected);

                if (target)
                    meeting.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
            }
            
            if (GUILayout.Button("Close Meeting")) meeting.Close();
        }
        else
        {
            GUILayout.Label("No active meeting");
        }

        bool prevSeasons = _forceSeasons;

        _forceSeasons = GUILayout.Toggle(_forceSeasons, "Force Seasons");

        if (_forceSeasons != prevSeasons)
        {
            NewMod.ForceEnableAllSeasons.Value = _forceSeasons;
        }
        
        GUILayout.Label($"Force all seasons: {(NewMod.ForceEnableAllSeasons.Value ? "ON" : "OFF")}");
    }
}
