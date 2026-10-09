using NewMod.Modifiers.S1;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles.S1;

[MiraIgnore]
public class MirrorBladeRole : ImpostorRole, INewModRole
{
    public static readonly HashSet<byte> ArmedReflections = new();
    public static bool Reflecting;
    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.IntroBlurb");

    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.TabDescription");

    public Color RoleColor => new Color32(192, 220, 255, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Apex;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = false,
            CanUseSabotage = true,
            CanUseVent = true,
            UseVanillaKillButton = true,
            TasksCountForProgress = false,
            Icon = NewModAsset.ReflectIcon,
            OptionsScreenshot = MiraAssets.Empty,
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
        var state = ArmedReflections.Contains(PlayerControl.LocalPlayer.PlayerId) ? MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.Tab.Armed") : MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.Tab.Idle");

        tabText.AppendLine();
        tabText.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.Tab.ReflectState"), state));
        var options = OptionGroupSingleton<MirrorBladeOptions>.Instance;
        tabText.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.Tab.Timing"), options.ReflectWindow, options.ReflectCooldown));
        tabText.AppendLine(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.S1.MirrorBladeRole.Tab.ParryRules"));

        return tabText;
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (!Reflecting && ArmedReflections.Contains(evt.Source.PlayerId))
        {
            evt.Cancel();
            return;
        }

        if (Reflecting || !PlayerControl.LocalPlayer.IsHost())
            return;

        if (evt.Target.Data.Role is not MirrorBladeRole || !ArmedReflections.Remove(evt.Target.PlayerId))
            return;

        evt.Cancel();

        if (evt.Source.Data.IsDead || evt.Source.Data.Disconnected)
            return;

        RpcReflectionTriggered(evt.Target, evt.Source.PlayerId, -1);
        Reflecting = true;
        evt.Target.RpcCustomMurder(evt.Source, true, false, true, false, false);
        Reflecting = false;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        ArmedReflections.Clear();
        Reflecting = false;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ArmedReflections.Clear();
        Reflecting = false;
    }

    [MethodRpc((uint)CustomRPC.MirrorBladeArm)]
    public static void RpcArmReflection(PlayerControl source)
    {
        if (source.Data.Role is not MirrorBladeRole || source.Data.IsDead || source.inVent || MeetingHud.Instance || !ArmedReflections.Add(source.PlayerId))
            return;
        OverclockedModifier.Pulse(source);
        Coroutines.Start(CoDisarmReflection(source, OptionGroupSingleton<MirrorBladeOptions>.Instance.ReflectWindow));

        if (source.AmOwner)
            HudManager.Instance.KillButton.SetTarget(null);
    }

    [MethodRpc((uint)CustomRPC.MirrorBladeReflect)]
    public static void RpcReflectionTriggered(PlayerControl source, byte attackerId, int npcId)
    {
        ArmedReflections.Remove(source.PlayerId);

        var attacker = Utils.PlayerById(attackerId);
        var reflectedWraith = npcId >= 0;

        if (reflectedWraith && attacker)
        {
            var key = ((uint)attacker.PlayerId << 16) | (uint)npcId;
            if (WraithCallerUtilities.ActiveNpcs.TryGetValue(key, out var npc))
                npc.RedirectToOwner(source);
        }

        if (source.AmOwner)
        {
            var text = reflectedWraith ? "<color=#C0DCFF><b>REFLECTED.</b></color> The Wraith has turned on its caller." : "<color=#C0DCFF><b>REFLECTED.</b></color> Their strike was turned back on them.";
            Coroutines.Start(CoroutinesHelper.CoNotify(text));
        }

        if (attacker && attacker.AmOwner)
        {
            var text = reflectedWraith ? "<color=#FF8A80>Your Wraith was reflected. It is hunting you now.</color>" : "<color=#FF8A80>Your attack was reflected back at you.</color>";
            Coroutines.Start(CoroutinesHelper.CoNotify(text));
        }
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        ArmedReflections.Clear();
    }

    public static IEnumerator CoDisarmReflection(PlayerControl player, float delay)
    {
        var end = Time.time + delay;
        while (player && !player.Data.IsDead && !MeetingHud.Instance && ArmedReflections.Contains(player.PlayerId) && Time.time < end)
        {
            player.cosmetics.currentBodySprite.BodySprite.UpdateOutline(new Color32(192, 220, 255, 255));
            yield return null;
        }

        if (player)
        {
            ArmedReflections.Remove(player.PlayerId);
            player.cosmetics.currentBodySprite.BodySprite.UpdateOutline(null);
        }
    }
}