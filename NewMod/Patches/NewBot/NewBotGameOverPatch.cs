using System.Globalization;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using NewMod.Buttons.Roles.S1;
using NewMod.Components.Minigames;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;
using UnityEngine;
using NewBotController = NewMod.NewBot.NewBot;

namespace NewMod.Patches.NewBot;

[HarmonyPatch(typeof(CustomGameOverRpc), nameof(CustomGameOverRpc.Handle))]
public static class NewBotGameOverPatch
{
    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent evt)
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.LessonRunning || evt.Source != bot.Player || evt.Target == bot.Player)
            return;
        if (!bot.AllowKills || (bot.ExpectedVictim != byte.MaxValue && evt.Target.PlayerId != bot.ExpectedVictim))
            bot.Failure = "That wasn't the target for this step. I'll reset the trial. Follow the instruction before attacking.";
    }

    [RegisterEvent]
    public static void OnAbility(MiraButtonClickEvent evt)
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.LessonRunning || evt.IsCancelled || !evt.Button.CanClick())
            return;
        if (evt.Button is ArbitratorLeverageButton leverage && leverage.Target)
            bot.Observation = ArbitratorRole.LastVotes[bot.Player.PlayerId] == ArbitratorRole.LastVotes[leverage.Target.PlayerId] ? "Same vote" : "Different vote";
    }

    public static bool Prefix(PlayerControl innerNetObject, GameOverData data)
    {
        var newBot = NewBotController.Instance;
        if (!newBot || !newBot.Practicing || AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
            return true;
        if (GameOverManager.TryGetGameOver(data.Reason, out var gameOver) && gameOver.VerifyCondition(innerNetObject, data.Winners.ToArray()))
        {
            newBot.Victory = data.Winners.Any(winner => winner.PlayerId == newBot.Player.PlayerId);
            newBot.LessonSucceeded = newBot.Victory;
        }

        return false;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class NewBotFreeplayEndPatch
{
    public static bool Prefix()
    {
        return !NewBotController.Instance || AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay;
    }
}

[HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
public static class NewBotKillButtonPatch
{
    [HarmonyPriority(Priority.First)]
    public static bool Prefix()
    {
        var bot = NewBotController.Instance;
        return !bot || !bot.Practicing || bot.CanUseNormalKill;
    }
}

[HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
public static class NewBotKillTargetPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Prefix(ref PlayerControl target)
    {
        var bot = NewBotController.Instance;
        if (bot && bot.Practicing && !bot.CanUseNormalKill)
            target = null;
    }
}

[HarmonyPatch(typeof(VerifyMinigame), nameof(VerifyMinigame.Verify))]
public static class NewBotVerifyPatch
{
    public static void Prefix(VerifyMinigame __instance, VerifierClaimType claim, bool expected)
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.LessonRunning || VerifierUtilities.UsedThisMeeting)
            return;
        if (__instance.Target.PlayerId != bot.VerificationTarget || claim != bot.VerificationClaim || expected != bot.VerificationExpected)
            bot.Failure = "That checked a different claim. Match the player, action and yes or no choice to the evidence.";
    }
}

[HarmonyPatch(typeof(Specialist), nameof(Specialist.Scan))]
public static class NewBotScanPatch
{
    public static void Prefix()
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.LessonRunning || Specialist.ScanStates[bot.Player.PlayerId].Charges == 0)
            return;
        var position = bot.Player.GetTruePosition();
        switch (Specialist.ScanStates[bot.Player.PlayerId].Mode)
        {
            case Specialist.ScanMode.Presence:
                bot.Observation = Helpers.GetClosestPlayers(bot.Player, 5f).Count(player => !player.Data.IsDead && !player.Data.Disconnected && !player.inVent).ToString(CultureInfo.InvariantCulture);
                break;
            case Specialist.ScanMode.Forensics:
                var bodies = Helpers.GetNearestDeadBodies(position, 5f, Helpers.CreateFilter(Constants.NotShipMask));
                bot.Observation = bodies.Count == 0 ? "No bodies" : "A body 2 to 5 units away";
                if (bodies.Any(body => Vector2.Distance(position, body.TruePosition) <= 2f))
                    bot.Observation = "A body within 2 units";
                break;
            default:
                bot.Observation = Specialist.Disturbances.Any(entry => Time.time - entry.Time <= 15f && Vector2.Distance(position, entry.Position) <= 5f) ? "Recent violence" : "No recent violence";
                break;
        }
    }
}

[HarmonyPatch(typeof(WardenRole), nameof(WardenRole.InspectResidual))]
public static class NewBotResidualPatch
{
    public static void Prefix(GameObject mark)
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.LessonRunning || !WardenRole.ResidualMarks.TryGetValue(mark, out var clue))
            return;
        bot.Observation = WardenRole.ClueType switch
        {
            0 => clue.EnteredAfterSeal ? "Entered after the seal" : "Already inside",
            1 => clue.UsedAbilityRecently ? "Used an ability" : "No recent ability",
            _ => $"{clue.PresenceTime:0} seconds",
        };
    }
}

[HarmonyPatch(typeof(VisionaryUtilities), nameof(VisionaryUtilities.RpcCameraReady))]
public static class NewBotPhotoPatch
{
    public static void Postfix(int id, bool failed)
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.LessonRunning || bot.Player.Data.Role is not TheVisionary || failed || !bot.Map.Dummies[1].Data.IsDead)
            return;
        var camera = VisionaryUtilities.Cameras[id];
        if (!camera.Object || camera.Owner != bot.Player.PlayerId)
            return;
        var offset = bot.Map.Dummies[1].GetTruePosition() - (Vector2)camera.Object.transform.position;
        var angle = Mathf.Repeat(Mathf.Atan2(camera.Direction.y, camera.Direction.x) * Mathf.Rad2Deg + 90f, 180f) - 90f;
        var local = Quaternion.Euler(0f, 0f, -angle) * offset;
        var options = OptionGroupSingleton<VisionaryOptions>.Instance;
        if (Mathf.Abs(local.x) <= options.PhotoViewHeight * options.PhotoAspect / 2f && Mathf.Abs(local.y) <= options.PhotoViewHeight / 2f)
            bot.EvidenceCamera = id;
    }
}

[HarmonyPatch(typeof(VisionaryUtilities), nameof(VisionaryUtilities.RpcBroadcast))]
public static class NewBotBroadcastPatch
{
    public static void Prefix(PlayerControl source, int id)
    {
        var bot = NewBotController.Instance;
        if (bot && bot.LessonRunning && source == bot.Player && id != bot.EvidenceCamera)
            bot.Failure = "That photo did not capture the staged body. Collect and show the camera covering the marked scene.";
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CanReportBodies))]
public static class NewBotReportPatch
{
    public static void Postfix(ref bool __result)
    {
        if (NewBotController.Instance && NewBotController.Instance.LessonRunning)
            __result = false;
    }
}
