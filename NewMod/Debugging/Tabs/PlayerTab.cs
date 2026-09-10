using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Debugging.Tabs;

public class PlayerTab : IDebugTab
{
    public string Name => "PLAYER";
    public bool ShouldShow => ShipStatus.Instance != null && PlayerControl.LocalPlayer;
    
    public static byte SelectedPlayerId = byte.MaxValue;
    public static PlayerControl SelectedPlayer;
    public static string SelectedRole = "Terminator";

    public void BuildGUI()
    {
        if (!PlayerControl.LocalPlayer) return;
        
        var players = new List<PlayerControl>();

        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data?.Role != null && !player.Data.Disconnected)
                players.Add(player);
        
        if (players.All(player => player.PlayerId != SelectedPlayerId))
            SelectedPlayerId = players[0].PlayerId;

        var selectedPlayerIndex = players.FindIndex(player => player.PlayerId == SelectedPlayerId);
        SelectedPlayer = players[selectedPlayerIndex];
        var roles = CustomRoleManager.CustomMiraRoles.Where(role => role.GetType().Assembly == typeof(NewMod).Assembly).OrderBy(role => role.RoleName).ToArray();

        GUILayout.Label("TARGET");
        
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<"))
        {
            selectedPlayerIndex = selectedPlayerIndex == 0 ? players.Count - 1 : selectedPlayerIndex - 1;
            SelectedPlayerId = players[selectedPlayerIndex].PlayerId;
        }

        GUILayout.Box($"{SelectedPlayer.Data.PlayerName}  #{SelectedPlayer.PlayerId}");
        
        if (GUILayout.Button(">"))
        {
            selectedPlayerIndex = selectedPlayerIndex == players.Count - 1 ? 0 : selectedPlayerIndex + 1;
            SelectedPlayerId = players[selectedPlayerIndex].PlayerId;
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.Label("POSITION");
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Go To Target"))
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(SelectedPlayer.GetTruePosition());
        if (GUILayout.Button("Bring Target Here")) SelectedPlayer.NetTransform.RpcSnapTo(PlayerControl.LocalPlayer.GetTruePosition());

        GUILayout.EndHorizontal();
        
        GUILayout.Label("STATE");
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Kill")) SelectedPlayer.RpcAdvancedCustomMurder(SelectedPlayer, MeetingCheck.OutsideMeeting, true, true, resetKillTimer: false, teleportMurderer: false, showKillAnim: false, playKillSound: false);
        if (GUILayout.Button("Revive"))
            Utils.HandleRevive(PlayerControl.LocalPlayer, SelectedPlayer.PlayerId, RoleTypes.Crewmate,
                SelectedPlayer.GetTruePosition().x, SelectedPlayer.GetTruePosition().y);
        
        GUILayout.EndHorizontal();
        
        GUILayout.Label("ASSIGN ROLE");
        
        GUILayout.BeginHorizontal();

        if (roles.Length == 0)
        {
            GUILayout.Label("No registered NewMod roles");
        }
        else
        {
            if (roles.All(role => role.RoleName != SelectedRole))
                            SelectedRole = roles[0].RoleName;
            
            var selectedRoleIndex = Array.FindIndex(roles, role => role.RoleName == SelectedRole);
                        
            if (GUILayout.Button("<"))
            {
                selectedRoleIndex = selectedRoleIndex == 0 ? roles.Length - 1 : selectedRoleIndex - 1;

                SelectedRole = roles[selectedRoleIndex].RoleName;
            }

            GUILayout.Box($"{SelectedRole}");
        
            if (GUILayout.Button(">"))
            {
                selectedRoleIndex = selectedRoleIndex == roles.Length - 1 ? 0 : selectedRoleIndex + 1;

                SelectedRole = roles[selectedRoleIndex].RoleName;
            }
        }
        
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Assign Role"))
        {
            var role = (RoleBehaviour)roles.First(candidate => candidate.RoleName == SelectedRole);
            SelectedPlayer.RpcSetRole(role.Role);
        }
    }
}
