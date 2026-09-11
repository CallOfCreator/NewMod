using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using NewMod.Components.ScreenEffects;
using NewMod.Roles.NeutralRoles;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace NewMod.Utilities;

/// <summary>
///     Provides helper coroutines and utility methods.
/// </summary>
public static class CoroutinesHelper
{
    private static readonly Queue<string> Notifications = new();
    private static GameObject _notification;
    private static HudManager _notificationHud;

    /// <summary>
    ///     Keeps track of the number of fake bodies created by each player, keyed by their PlayerId.
    /// </summary>
    public static Dictionary<byte, int> bodiesCreated = new();

    /// <summary>
    ///     Tracks the number of energy drains performed by each player, keyed by their PlayerId.
    /// </summary>
    public static Dictionary<byte, int> drainCount = new();

    /// <summary>
    ///     Displays a temporary notification on the screen using an overlay animation.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator CoNotify(string message)
    {
        var hud = HudManager.Instance;
        if (!hud || !hud.TaskCompleteOverlay)
            yield break;

        if (_notificationHud != hud)
        {
            Notifications.Clear();
            _notification = null;
            _notificationHud = hud;
        }

        Notifications.Enqueue(message);

        if (_notification)
            yield break;

        while (hud && hud == _notificationHud && Notifications.Count > 0)
        {
            message = Notifications.Dequeue();

            if (Constants.ShouldPlaySfx())
                SoundManager.Instance.PlaySound(hud.TaskCompleteSound, false);

            var obj = Object.Instantiate(hud.TaskCompleteOverlay.gameObject, hud.transform);
            _notification = obj;
            obj.transform.localPosition = new Vector3(0f, -8f, Minigame.Depth - 20f);
            obj.transform.SetAsLastSibling();
            obj.SetActive(true);
            var textComponent = obj.GetComponentInChildren<TextMeshPro>(true);
            var translator = textComponent.GetComponent<TextTranslatorTMP>();

            if (translator)
            {
                translator.enabled = false;
                Object.Destroy(translator);
            }

            textComponent.text = message;
            textComponent.fontSize = Mathf.Clamp(3.5f - message.Length / 20f, 2f, 3.5f);
            for (var elapsed = 0f; elapsed < 2.75f; elapsed += Time.unscaledDeltaTime)
            {
                if (!hud || !obj || hud != _notificationHud)
                    yield break;

                var y = elapsed < 0.25f ? Mathf.SmoothStep(-8f, 0f, elapsed / 0.25f) : elapsed < 2.5f ? 0f : Mathf.SmoothStep(0f, 8f, (elapsed - 2.5f) / 0.25f);
                obj.transform.localPosition = new Vector3(0f, y, Minigame.Depth - 20f);
                yield return null;
            }

            Object.Destroy(obj);
            _notification = null;
        }
    }

    /// <summary>
    ///     Starts and displays a countdown timer for a mission, then fails the mission if time expires.
    /// </summary>
    /// <param name="target">The player assigned to the mission.</param>
    /// <param name="duration">The desired duration for the mission timer (clamped to 30 seconds max).</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator CoMissionTimer(PlayerControl specialAgent, PlayerControl target, float duration)
    {
        // Clamp duration to a maximum of 30 seconds
        duration = Mathf.Min(duration, 30f);

        // Create a text label for the mission timer
        var timerLabel = Helpers.CreateTextLabel("MissionTimerText", HudManager.Instance.transform, AspectPosition.EdgeAlignments.LeftBottom, new Vector3(9.9f, 3.5f, 0f), 3f, TextAlignmentOptions.BottomLeft);

        timerLabel!.text = $"Time Remaining: {duration}s";
        timerLabel.color = Color.yellow;

        var timeRemaining = duration;

        while (timeRemaining > 0)
        {
            // If the assigned player is unassigned, cancel the timer
            if (!target || target.Data == null || SpecialAgent.AssignedPlayer != target)
            {
                if (timerLabel)
                    Object.Destroy(timerLabel.gameObject);
                yield break;
            }

            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;

            if (timerLabel)
            {
                timerLabel.text = $"Time Remaining: {Mathf.CeilToInt(timeRemaining)}s";
                timerLabel.color = timeRemaining <= 10f ? Color.red : timeRemaining <= 20f ? Color.yellow : Color.green;
            }
        }

        // Time has expired, destroy the timer and fail the mission
        if (timerLabel)
            Object.Destroy(timerLabel.gameObject);

        if (target && target.Data != null && SpecialAgent.AssignedPlayer == target)
            Utils.RpcMissionFails(PlayerControl.LocalPlayer, specialAgent, target);
    }

    /// <summary>
    ///     Allows a Prankster to create fake dead bodies by pressing F5, fulfilling a mission if enough bodies are created.
    /// </summary>
    /// <param name="target">The player executing the prankster abilities.</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator UsePranksterAbilities(PlayerControl specialAgent, PlayerControl target)
    {
        // Initialize dictionary entry for this player if missing
        if (!bodiesCreated.ContainsKey(target.PlayerId)) bodiesCreated[target.PlayerId] = 0;

        while (true)
        {
            // If the player dies mid-mission, fail the mission
            if (!target || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
            {
                if (target)
                    Utils.RpcMissionFails(PlayerControl.LocalPlayer, specialAgent, target);
                yield break;
            }

            // Press F5 to create a fake dead body
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PranksterUtilities.CreatePranksterDeadBody(target, target.PlayerId);
                bodiesCreated[target.PlayerId]++;
                if (target.AmOwner) Coroutines.Start(CoNotify($"<color=yellow>Bodies created: {bodiesCreated[target.PlayerId]}/2</color>"));

                // Once enough bodies are created, succeed the mission
                if (bodiesCreated[target.PlayerId] >= 2)
                {
                    Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, specialAgent, target);
                    yield break;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    ///     Allows an Energy Thief to drain nearby players' energy by pressing F5, fulfilling a mission after enough drains.
    /// </summary>
    /// <param name="target">The player executing the energy draining abilities.</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator UseEnergyThiefAbilities(PlayerControl specialAgent, PlayerControl target)
    {
        var drainRange = 3.5f;

        // Initialize dictionary entry for this player if missing
        if (!drainCount.ContainsKey(target.PlayerId)) drainCount[target.PlayerId] = 0;

        while (true)
        {
            // If the player dies mid-mission, fail the mission
            if (!target || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
            {
                if (target)
                    Utils.RpcMissionFails(PlayerControl.LocalPlayer, specialAgent, target);
                yield break;
            }

            // Press F5 to drain energy from a nearby player
            if (Input.GetKeyDown(KeyCode.F5))
            {
                var playersInRange = Helpers.GetClosestPlayers(target, drainRange).Where(p => !p.Data.IsDead && !p.Data.Disconnected).ToList();

                if (playersInRange.Count > 0)
                {
                    var victim = playersInRange[0];

                    Utils.RpcRandomDrainActions(target, victim);
                    drainCount[target.PlayerId]++;

                    // Notify both the drainer and the drained player
                    if (target.AmOwner) Coroutines.Start(CoNotify($"<color=#00FA9A><b><i>You have drained energy from {victim.Data.PlayerName}!</i></b></color>"));

                    if (victim.AmOwner) Coroutines.Start(CoNotify("<color=#FF0000><b><i>Your energy has been drained!</i></b></color>"));

                    // After enough drains, succeed the mission
                    if (drainCount[target.PlayerId] >= 2)
                    {
                        Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, specialAgent, target);
                        yield break;
                    }
                }
                else
                {
                    if (target.AmOwner) Coroutines.Start(CoNotify("<color=#FFA500><b><i>No players nearby to drain energy from.</i></b></color>"));
                }
            }

            yield return null;
        }
    }

    /// <summary>
    ///     Allows a player to revive a dead player and then kill them again. F5 is used to initiate each action.
    /// </summary>
    /// <param name="target">The player controlling the revive and kill actions.</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator CoReviveAndKill(PlayerControl specialAgent, PlayerControl target)
    {
        var revived = false;
        byte revivedParentId = 255;

        // Prompt the player to press F5 for the initial revive
        if (target.AmOwner) Coroutines.Start(CoNotify("<color=#8A2BE2><i><b>Press F5 to revive a dead player!</b></i></color>"));

        while (true)
        {
            if (!target || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
            {
                if (target)
                    Utils.RpcMissionFails(PlayerControl.LocalPlayer, specialAgent, target);
                yield break;
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                // Perform the revive if not yet done
                if (!revived)
                {
                    var deadBody = Utils.GetClosestBody();
                    if (deadBody == null && target.AmOwner)
                    {
                        Coroutines.Start(CoNotify("<color=#FFA500><b>No dead body found! Move closer and press F5 again.</b></color>"));
                    }
                    else
                    {
                        revivedParentId = deadBody.ParentId;

                        Utils.HandleRevive(target, deadBody.ParentId, RoleTypes.Crewmate, deadBody.transform.position.x, deadBody.transform.position.y);

                        yield return new WaitForSeconds(0.5f);

                        Coroutines.Start(CoNotify("<color=#8A2BE2><i><b>Player revived! Press F5 to kill them again!</b></i></color>"));

                        revived = true;
                    }
                }
                // If revived, press F5 again to kill the revived player
                else
                {
                    var revivedData = GameData.Instance.GetPlayerById(revivedParentId);
                    if (revivedData != null && revivedData.Object != null && !revivedData.Object.Data.IsDead)
                    {
                        PlayerControl.LocalPlayer.RpcCustomMurder(revivedData.Object, createDeadBody: true, didSucceed: true, showKillAnim: false, playKillSound: true, teleportMurderer: false);
                        Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, specialAgent, target);
                        yield break;
                    }
                }
            }

            yield return null;
        }
    }

    /// <summary>
    ///     Handles logic for tracking and validating a "most wanted" target using an arrow indicator.
    /// </summary>
    /// <param name="arrow">An <see cref="ArrowBehaviour" /> used to point toward the target.</param>
    /// <param name="mostwantedTarget">The most wanted target player.</param>
    /// <param name="target">The player assigned to eliminate the most wanted target.</param>
    /// <returns>An <see cref="IEnumerator" /> for coroutine control.</returns>
    public static IEnumerator CoHandleWantedTarget(PlayerControl specialAgent, ArrowBehaviour arrow, PlayerControl mostwantedTarget, PlayerControl target)
    {
        while (mostwantedTarget && mostwantedTarget.Data != null && !mostwantedTarget.Data.IsDead && !mostwantedTarget.Data.Disconnected)
        {
            if (arrow)
                arrow.target = mostwantedTarget.transform.position;
            yield return null;
        }

        if (arrow)
            Object.Destroy(arrow.gameObject);

        var killer = mostwantedTarget && mostwantedTarget.Data != null && !mostwantedTarget.Data.Disconnected ? Utils.GetKiller(mostwantedTarget) : null;

        yield return new WaitForSeconds(0.5f);

        if (!target || target.Data == null || SpecialAgent.AssignedPlayer != target)
            yield break;

        if (killer == target)
            Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, specialAgent, target);
        else
            Utils.RpcMissionFails(PlayerControl.LocalPlayer, specialAgent, target);
    }

    /// <summary>
    ///     Resets the player's movement speed after the given delay.
    ///     Used to revert Adrenaline serum effect.
    /// </summary>
    /// <param name="target">The player whose speed will be reset.</param>
    /// <param name="originalSpeed">The original speed value to restore.</param>
    /// <param name="delay">The delay in seconds before restoring speed.</param>
    public static IEnumerator ResetSpeedAfterDelay(PlayerControl target, float originalSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null && !target.Data.IsDead) target.MyPhysics.Speed = originalSpeed;
    }

    /// <summary>
    ///     Enables movement for a player after a given delay.
    ///     Used to revert Paralysis serum effect.
    /// </summary>
    /// <param name="target">The player to re-enable movement for.</param>
    /// <param name="delay">The delay in seconds before allowing movement.</param>
    public static IEnumerator EnableMovementAfterDelay(PlayerControl target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null && !target.Data.IsDead)
        {
            target.moveable = true;
            target.MyPhysics.inputHandler.enabled = true;
        }
    }

    /// <summary>
    ///     Resets the player's rotation after a specified delay.
    ///     Useful for restoring normal orientation after bounce/spin effects (e.g. Bounce Serum).
    /// </summary>
    /// <param name="target">The player whose rotation will be reset.</param>
    /// <param name="delay">The delay in seconds before resetting rotation.</param>
    public static IEnumerator ResetRotationAfterDelay(PlayerControl target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null && !target.Data.IsDead) target.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    ///     Resets any lingering repel-related effects on the target player after the Repel Serum expires.
    /// </summary>
    /// <param name="target">The player to reset after the repel effect.</param>
    /// <param name="delay">The delay in seconds before performing the reset.</param>
    public static IEnumerator ResetRepelEffect(PlayerControl target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!target.Data.IsDead && !target.Data.Disconnected) target.MyPhysics.body.velocity = Vector2.zero;
    }

    /// <summary>
    ///     Coroutine that waits for a given duration before destroying a specified GameObject.
    /// </summary>
    /// <param name="go">The GameObject to destroy after the delay.</param>
    /// <param name="duration">The time in seconds to wait before destroying the object.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public static IEnumerator DespawnCircle(GameObject go, float duration)
    {
        yield return new WaitForSeconds(duration);
        go.Destroy();
    }

    /// <summary>
    ///     Coroutine that waits for a given duration and then removes
    ///     specific visual effects from a Camera.
    /// </summary>
    /// <param name="cam">The Camera to check for and remove effects from.</param>
    /// <param name="duration">The time in seconds to wait before removing the effects.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public static IEnumerator RemoveCameraEffect(Camera cam, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (cam && cam.TryGetComponent<ScreenEffectSystem>(out var effects))
            effects.Clear();
    }
}