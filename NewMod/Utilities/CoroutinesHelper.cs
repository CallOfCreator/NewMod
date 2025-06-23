using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Reactor.Utilities;
using MiraAPI.Utilities;
using System.Linq;
using TMPro;
using MiraAPI.Networking;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Utilities
{
    /// <summary>
    /// Provides helper coroutines and utility methods.
    /// </summary>
    public static class CoroutinesHelper
    {
        /// <summary>
        /// Keeps track of the number of fake bodies created by each player, keyed by their PlayerId.
        /// </summary>
        public static Dictionary<byte, int> bodiesCreated = new Dictionary<byte, int>();

        /// <summary>
        /// Tracks the number of energy drains performed by each player, keyed by their PlayerId.
        /// </summary>
        public static Dictionary<byte, int> drainCount = new Dictionary<byte, int>();

        /// <summary>
        /// Reference to a <see cref="TextMeshPro"/> element used for displaying mission-related timers.
        /// </summary>
        private static TextMeshPro timerLabel;

        /// <summary>
        /// Displays a temporary notification on the screen using an overlay animation.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator CoNotify(string message)
        {
            // Play sound if allowed.
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(HudManager.Instance.TaskCompleteSound, false, 1f, null);
            }

            var overlay = HudManager.Instance.TaskCompleteOverlay;
            var obj = Object.Instantiate(overlay.gameObject, overlay.transform.parent);

            var textComponent = obj.GetComponentInChildren<TextMeshPro>();

            // Adjust font size based on message length
            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.fontSize = Mathf.Clamp(3.5f - (message.Length / 20f), 2f, 3.5f);
            }
            obj.gameObject.SetActive(true);

            yield return new WaitForEndOfFrame();

            if (textComponent != null)
            {
                textComponent.text = message;
            }

            // Animate the overlay into view
            yield return Effects.Slide2D(obj.transform, new Vector2(0f, -8f), Vector2.zero, 0.25f);

            // Display for a short duration
            for (float time = 0f; time < 0.95f; time += Time.deltaTime)
            {
                yield return null;
            }

            // Animate the overlay out of view
            yield return Effects.Slide2D(obj.transform, Vector2.zero, new Vector2(0f, 8f), 0.25f);

            GameObject.Destroy(obj);
        }
        /// <summary>
        /// Starts and displays a countdown timer for a mission, then fails the mission if time expires.
        /// </summary>
        /// <param name="target">The player assigned to the mission.</param>
        /// <param name="duration">The desired duration for the mission timer (clamped to 30 seconds max).</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator CoMissionTimer(PlayerControl target, float duration)
        {
            // Clamp duration to a maximum of 30 seconds
            duration = Mathf.Min(duration, 30f);

            // Create a text label for the mission timer
            timerLabel = Helpers.CreateTextLabel(
                "MissionTimerText",
                HudManager.Instance.transform,
                AspectPosition.EdgeAlignments.LeftBottom,
                new(9.9f, 3.5f, 0f),
                fontSize: 3f,
                textAlignment: TextAlignmentOptions.BottomLeft
            );

            timerLabel!.text = $"Time Remaining: {duration}s";
            timerLabel.color = Color.yellow;

            float timeRemaining = duration;

            while (timeRemaining > 0)
            {
                // If the assigned player is unassigned, cancel the timer
                if (SpecialAgent.AssignedPlayer == null)
                {
                    if (HudManager.Instance.FullScreen.gameObject.activeSelf)
                        HudManager.Instance.FullScreen.gameObject.SetActive(false);
                    Object.Destroy(timerLabel.gameObject);
                    yield break;
                }
                yield return new WaitForSeconds(1f);
                timeRemaining -= 1f;

                timerLabel.text = $"Time Remaining: {Mathf.CeilToInt(timeRemaining)}s";

                // Manage colors and background overlay based on remaining time
                if (timeRemaining <= 10f)
                {
                    timerLabel.color = Color.red;
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 0.8f);
                    }
                    HudManager.Instance.FullScreen.color = new Color(1f, 0f, 0f, 0.1f);
                    HudManager.Instance.FullScreen.gameObject.SetActive(true);
                }
                else if (timeRemaining <= 20f)
                {
                    timerLabel.color = Color.yellow;
                    if (HudManager.Instance.FullScreen.gameObject.activeSelf)
                        HudManager.Instance.FullScreen.gameObject.SetActive(false);
                }
                else
                {
                    timerLabel.color = Color.green;
                    if (HudManager.Instance.FullScreen.gameObject.activeSelf)
                        HudManager.Instance.FullScreen.gameObject.SetActive(false);
                }
            }
            // Time has expired, destroy the timer and fail the mission
            Object.Destroy(timerLabel.gameObject);
            SoundManager.Instance.StopSound(ShipStatus.Instance.SabotageSound);
            HudManager.Instance.FullScreen.gameObject.SetActive(false);
            Utils.RpcMissionFails(PlayerControl.LocalPlayer, target);
        }
        /// <summary>
        /// Allows a Prankster to create fake dead bodies by pressing F5, fulfilling a mission if enough bodies are created.
        /// </summary>
        /// <param name="target">The player executing the prankster abilities.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator UsePranksterAbilities(PlayerControl target)
        {
            // Initialize dictionary entry for this player if missing
            if (!bodiesCreated.ContainsKey(target.PlayerId))
            {
                bodiesCreated[target.PlayerId] = 0;
            }
            while (true)
            {
                // If the player dies mid-mission, fail the mission
                if (target.Data.IsDead)
                {
                    Utils.RpcMissionFails(PlayerControl.LocalPlayer, target);
                    yield break;
                }

                // Press F5 to create a fake dead body
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    PranksterUtilities.CreatePranksterDeadBody(target, target.PlayerId);
                    bodiesCreated[target.PlayerId]++;
                    if (target.AmOwner)
                    {
                        Coroutines.Start(CoNotify($"<color=yellow>Bodies created: {bodiesCreated[target.PlayerId]}/2</color>"));
                    }
                    // Once enough bodies are created, succeed the mission
                    if (bodiesCreated[target.PlayerId] >= 2)
                    {
                        Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, target);
                        yield break;
                    }
                }
            }
        }
        /// <summary>
        /// Allows an Energy Thief to drain nearby players' energy by pressing F5, fulfilling a mission after enough drains.
        /// </summary>
        /// <param name="target">The player executing the energy draining abilities.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator UseEnergyThiefAbilities(PlayerControl target)
        {
            float drainRange = 3.5f;

            // Initialize dictionary entry for this player if missing
            if (!drainCount.ContainsKey(target.PlayerId))
            {
                drainCount[target.PlayerId] = 0;
            }
            while (true)
            {
                // If the player dies mid-mission, fail the mission
                if (target.Data.IsDead)
                {
                    Utils.RpcMissionFails(PlayerControl.LocalPlayer, target);
                    yield break;
                }
                // Press F5 to drain energy from a nearby player
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    var playersInRange = Helpers.GetClosestPlayers(
                        target,
                        drainRange,
                        ignoreColliders: true,
                        ignoreSource: true
                    )
                    .Where(p => !p.Data.IsDead && !p.Data.Disconnected && p != PlayerControl.LocalPlayer)
                    .ToList();

                    if (playersInRange.Count > 0)
                    {
                        var victim = playersInRange[0];

                        Utils.RpcRandomDrainActions(target, victim);
                        drainCount[target.PlayerId]++;

                        // Notify both the drainer and the drained player
                        if (target.AmOwner)
                        {
                            Coroutines.Start(CoNotify($"<color=#00FA9A><b><i>You have drained energy from {victim.Data.PlayerName}!</i></b></color>"));
                        }
                        if (victim.AmOwner)
                        {
                            Coroutines.Start(CoNotify("<color=#FF0000><b><i>Your energy has been drained!</i></b></color>"));
                        }

                        // After enough drains, succeed the mission
                        if (drainCount[target.PlayerId] >= 2)
                        {
                            Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, target);
                            yield break;
                        }
                    }
                    else
                    {
                        if (target.AmOwner)
                        {
                            Coroutines.Start(CoNotify("<color=#FFA500><b><i>No players nearby to drain energy from.</i></b></color>"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows a player to revive a dead player and then kill them again. F5 is used to initiate each action.
        /// </summary>
        /// <param name="target">The player controlling the revive and kill actions.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator CoReviveAndKill(PlayerControl target)
        {
            bool revived = false;
            byte revivedParentId = 255;

            // Prompt the player to press F5 for the initial revive
            if (target.AmOwner)
            {
                Coroutines.Start(CoNotify("<color=#8A2BE2><i><b>Press F5 to revive a dead player!</b></i></color>"));
            }
            while (true)
            {
                if (target.Data.IsDead)
                {
                    Utils.RpcMissionFails(PlayerControl.LocalPlayer, target);
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

                            Utils.RpcRevive(deadBody);

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
                            PlayerControl.LocalPlayer.RpcCustomMurder(
                                revivedData.Object,
                                createDeadBody: true,
                                didSucceed: true,
                                showKillAnim: false,
                                playKillSound: true,
                                teleportMurderer: false
                            );
                            Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, target);
                            yield break;
                        }
                    }
                }
                yield return null;
            }
        }

        /// <summary>
        /// Handles logic for tracking and validating a "most wanted" target using an arrow indicator.
        /// </summary>
        /// <param name="arrow">An <see cref="ArrowBehaviour"/> used to point toward the target.</param>
        /// <param name="mostwantedTarget">The most wanted target player.</param>
        /// <param name="target">The player assigned to eliminate the most wanted target.</param>
        /// <returns>An <see cref="IEnumerator"/> for coroutine control.</returns>
        public static IEnumerator CoHandleWantedTarget(ArrowBehaviour arrow, PlayerControl mostwantedTarget, PlayerControl target)
        {
            // Keep updating the arrow's position as long as the target is alive
            while (!mostwantedTarget.Data.IsDead && !mostwantedTarget.Data.Disconnected)
            {
                arrow.target = mostwantedTarget.transform.position;
                yield return null;
            }
            Object.Destroy(arrow.gameObject);

            yield return new WaitForSeconds(0.5f);

            // If the assigned player was the killer, mission succeeds; otherwise, it fails
            var killer = Utils.GetKiller(mostwantedTarget);
            if (killer != null && killer == target)
            {
                Utils.RpcMissionSuccess(PlayerControl.LocalPlayer, target);
            }
            else
            {
                Utils.RpcMissionFails(PlayerControl.LocalPlayer, target);
            }
            yield break;
        }
    }
}
