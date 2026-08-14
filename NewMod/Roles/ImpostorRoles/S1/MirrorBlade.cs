using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
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
    public static bool _reflecting;
    public string RoleName => "MirrorBlade";
    public string RoleDescription => "Reflect the strike meant for you.";

    public string RoleLongDescription =>
        "Arm your mirror stance. The next murder targeting you is turned back on the attacker. A Wraith that reaches you is reflected and hunts its caller instead.";

    public Color RoleColor => new Color32(192, 220, 255, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public NewModFaction Faction => NewModFaction.Apex;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AffectedByLightOnAirship = false,
        CanUseSabotage = true,
        CanUseVent = true,
        UseVanillaKillButton = true,
        TasksCountForProgress = false,
        Icon = MiraAssets.Empty,
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
        var state = ArmedReflections.Contains(PlayerControl.LocalPlayer.PlayerId) ? "armed" : "idle";

        tabText.AppendLine();
        tabText.AppendLine($"<size=65%>Reflect state: <color=#C0DCFF>{state}</color></size>");
        tabText.AppendLine("<size=65%><color=#B7D8FF>Reflect a murder back at its attacker. Reflected Wraiths turn around and hunt their caller.</color></size>");

        return tabText;
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (_reflecting || !PlayerControl.LocalPlayer.IsHost())
            return;

        if (evt.Target.Data.Role is not MirrorBladeRole || !ArmedReflections.Remove(evt.Target.PlayerId))
            return;

        evt.Cancel();

        if (!evt.Source || evt.Source.Data.IsDead || evt.Source.Data.Disconnected)
            return;

        RpcReflectionTriggered(evt.Target, evt.Source.PlayerId, -1);
        _reflecting = true;

        try
        {
            evt.Target.RpcCustomMurder(evt.Source, true, false, true, false, false, true);
        }
        finally
        {
            _reflecting = false;
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        ArmedReflections.Clear();
        _reflecting = false;
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ArmedReflections.Clear();
        _reflecting = false;
    }

    [MethodRpc((uint)CustomRPC.MirrorBladeArm)]
    public static void RpcArmReflection(PlayerControl source)
    {
        ArmedReflections.Add(source.PlayerId);
        Coroutines.Start(CoDisarmReflection(source.PlayerId, OptionGroupSingleton<MirrorBladeOptions>.Instance.ReflectWindow));

        if (source.AmOwner)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#C0DCFF>Mirror stance armed.</color>"));
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

    private static IEnumerator CoDisarmReflection(byte playerId, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ArmedReflections.Remove(playerId) && PlayerControl.LocalPlayer.PlayerId == playerId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#AFC6D9>Mirror stance faded.</color>"));
    }
}