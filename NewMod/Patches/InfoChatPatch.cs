using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.Roles;
using NewMod.GameModes.WraithSiegeGamemode;
using NewMod.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch]
public static class InfoChatPatch
{
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void OnLobbyStart(LobbyBehaviour __instance)
    {
        Coroutines.Start(CoWelcome(__instance));
    }

    public static IEnumerator CoWelcome(LobbyBehaviour lobby)
    {
        while (lobby && (!HudManager.Instance || !HudManager.Instance.Chat || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data))
            yield return null;

        if (!lobby)
            yield break;

        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Welcome to NewMod! Type /help for role, faction and game mode commands. This message is only visible to you.", false);
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    public static bool Prefix(ChatController __instance)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || __instance.quickChatMenu.IsOpen || __instance.quickChatMenu.CanSend)
            return true;

        var text = __instance.freeChatField.Text.Trim();
        var parts = text.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return true;

        var command = parts[0].ToLowerInvariant();
        if (command is not ("/help" or "/roles" or "/role" or "/r" or "/gamemodes" or "/gamemode" or "/factions" or "/faction"))
            return true;

        var query = parts.Length > 1 ? string.Concat(parts[1].Where(character => !char.IsWhiteSpace(character))) : "";
        string reply;

        if (command == "/help")
        {
            reply = "/roles lists NewMod roles. /r <name> explains one role. /factions explains role styles. /gamemodes lists game modes.";
        }
        else if (command is "/role" or "/r" or "/roles")
        {
            var roles = CustomRoleManager.CustomMiraRoles.Where(role => role.GetType().Assembly == typeof(InfoChatPatch).Assembly).OrderBy(role => role.RoleName).ToArray();
            if (command == "/roles" && query.Length == 0)
            {
                reply = string.Join(", ", roles.Select(role => role.RoleName));
            }
            else if (command is "/role" or "/r" && query.Length > 0)
            {
                var role = roles.FirstOrDefault(role => string.Concat(role.RoleName.Where(character => !char.IsWhiteSpace(character))).Equals(query, StringComparison.OrdinalIgnoreCase));
                if (role == null)
                {
                    reply = "Invalid role. Use /roles to see the names, then /r <name> for details.";
                }
                else
                {
                    var heading = $"<color=#{ColorUtility.ToHtmlStringRGB(role.RoleColor)}>{role.RoleName}</color> ({(role.Team == ModdedRoleTeams.Custom ? "Neutral" : role.Team.ToString())})" + (role is INewModRole newModRole ? $" | {Utils.GetFactionDisplay(newModRole)}" : "");
                    reply = $"{heading}\n{role.RoleLongDescription}";
                }
            }
            else
            {
                reply = "Use /r <name> for role details, for example /r Verifier. Use /roles to see the names.";
            }
        }
        else if (command is "/faction" or "/factions")
        {
            var factions = Enum.GetValues<NewModFaction>();
            if (command == "/faction")
                factions = factions.Where(faction => faction.ToString().Equals(query, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (factions.Length == 0 || (command == "/factions" && query.Length > 0))
                reply = "Use /factions to learn about all four groups, or /faction <name> for details.";
            else
                reply = "Factions group role styles, not teams. Sharing a faction does not guarantee a shared win.\n" + string.Join("\n", factions.Select(faction =>
                {
                    var description = faction switch
                    {
                        NewModFaction.Apex => "Offensive roles built around hunting, kills and combat power.",
                        NewModFaction.Entropy => "Independent roles with their own objectives and victory rules.",
                        NewModFaction.Sentinel => "Crew-aligned roles focused on information, protection and control.",
                        NewModFaction.Rift => "Roles built around unusual movement, energy and interactions.",
                        _ => ""
                    };
                    var details = $"{faction}: {description}";
                    if (command == "/faction")
                    {
                        var names = CustomRoleManager.CustomMiraRoles.OfType<INewModRole>().Where(role => role.Faction == faction).Select(role => role.RoleName).OrderBy(name => name);
                        details += $"\nRoles: {string.Join(", ", names)}\nUse /role <name> for that role's objective.";
                    }

                    return details;
                }));
        }
        else
        {
            AbstractGameMode[] modes = [new ClassicMode(), new HideAndSeekMode(), new WraithSiege()];
            if (command == "/gamemodes" && query.Length == 0)
            {
                reply = $"Game modes: {string.Join(", ", modes.Select(mode => mode.Name))}\nUse /gamemode <name> for details.";
            }
            else if (command == "/gamemode")
            {
                var mode = query.Length == 0 ? CustomGameModeManager.ActiveMode : modes.FirstOrDefault(mode => string.Concat(mode.Name.Where(character => !char.IsWhiteSpace(character))).Equals(query, StringComparison.OrdinalIgnoreCase));
                reply = mode == null ? "GameMode not found. Use /gamemodes to see the names." : $"{mode.Name}\n{mode.Description}";
            }
            else
            {
                reply = "Use /gamemodes to list modes, or /gamemode <name> for details.";
            }
        }

        __instance.freeChatField.Clear();
        __instance.quickChatField.Clear();
        __instance.quickChatMenu.Clear();
        __instance.UpdateChatMode();

        __instance.AddChat(PlayerControl.LocalPlayer, reply.Replace("—", ",").Replace("–", "-"), false);

        return false;
    }
}
