using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Reactor.Utilities;
using MiraAPI.Utilities;
using System.Linq;
using TMPro;
using MiraAPI.Networking;

namespace NewMod.Utilities
{
    public static class CoroutinesHelper
    {
        public static Dictionary<byte, int> bodiesCreated = new Dictionary<byte, int>();
        public static Dictionary<byte, int> drainCount = new Dictionary<byte, int>();
        private static TextMeshPro timerLabel;

        public static IEnumerator CoNotify(string message)
        {
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(HudManager.Instance.TaskCompleteSound, false, 1f, null);
            }

            HudManager.Instance.TaskCompleteOverlay.gameObject.SetActive(true);

            var textComponent = HudManager.Instance.TaskCompleteOverlay.GetComponentInChildren<TextMeshPro>();

            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.fontSize = Mathf.Clamp(3.5f - (message.Length / 20f), 2f, 3.5f);
            }

            yield return Effects.Slide2D(HudManager.Instance.TaskCompleteOverlay, new Vector2(0f, -8f), Vector2.zero, 0.25f);

            for (float time = 0f; time < 0.95f; time += Time.deltaTime)
            {
                yield return null;
            }

            yield return Effects.Slide2D(HudManager.Instance.TaskCompleteOverlay, Vector2.zero, new Vector2(0f, 8f), 0.25f);

            HudManager.Instance.TaskCompleteOverlay.gameObject.SetActive(false);
        }

        public static IEnumerator CoMissionTimer(PlayerControl target, float duration)
        {
            duration = Mathf.Min(duration, 30f);

            timerLabel = Helpers.CreateTextLabel("MissionTimerText", HudManager.Instance.transform, AspectPosition.EdgeAlignments.RightTop, new(5f, 1.5f, 0f), fontSize: 3f, textAlignment: TextAlignmentOptions.Right);

            timerLabel!.text = $"Time Remaining: {duration}s";
            timerLabel.color = Color.yellow;

            float timeRemaining = duration;

            while (timeRemaining > 0)
            {
                yield return new WaitForSeconds(1f);
                timeRemaining -= 1f;

                timerLabel.text = $"Time Remaining: {Mathf.CeilToInt(timeRemaining)}s";

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
                }
                else
                {
                    timerLabel.color = Color.green;
                }
            }
            Object.Destroy(timerLabel.gameObject);
            Utils.MissionFails(target, PlayerControl.LocalPlayer);
        }

        public static IEnumerator UsePranksterAbilities(PlayerControl target)
        {
            if (!bodiesCreated.ContainsKey(target.PlayerId))
            {
                bodiesCreated[target.PlayerId] = 0;
            }
            while (true)
            {
                if (target.Data.IsDead)
                {
                    Utils.MissionFails(target, PlayerControl.LocalPlayer);
                    yield break;
                }

                if (Input.GetKeyDown(KeyCode.F5))
                {
                    PranksterUtilities.CreatePranksterDeadBody(target, target.PlayerId);
                    bodiesCreated[target.PlayerId]++;

                    Coroutines.Start(CoNotify($"<color=yellow>Bodies created: {bodiesCreated[target.PlayerId]}/2</color>"));

                    if (bodiesCreated[target.PlayerId] >= 2)
                    {
                        Utils.MissionSuccess(target, PlayerControl.LocalPlayer);
                        yield break;
                    }
                }
            }
        }

        public static IEnumerator UseEnergyThiefAbilities(PlayerControl target)
        {
            float drainRange = 3.5f;

            if (!drainCount.ContainsKey(target.PlayerId))
            {
                drainCount[target.PlayerId] = 0;
            }
            while (true)
            {
                if (target.Data.IsDead)
                {
                    Utils.MissionFails(target, PlayerControl.LocalPlayer);
                    yield break;
                }
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    var playersInRange = Helpers.GetClosestPlayers(
                        target,
                        drainRange,
                        ignoreColliders: true,
                        ignoreSource: true
                    ).Where(p => !p.Data.IsDead && !p.Data.Disconnected && p != PlayerControl.LocalPlayer).ToList();

                    if (playersInRange.Count > 0)
                    {
                        var victim = playersInRange[0];

                        Utils.RpcRandomDrainActions(target, victim);
                        drainCount[target.PlayerId]++;
                        if (target.AmOwner)
                        {
                            Coroutines.Start(CoNotify($"<color=#00FA9A><b><i>You have drained energy from {victim.Data.PlayerName}!</i></b></color>"));
                        }
                        if (victim.AmOwner)
                        {
                            Coroutines.Start(CoNotify("<color=#FF0000><b><i>Your energy has been drained!</i></b></color>"));
                        }

                        if (drainCount[target.PlayerId] >= 2)
                        {
                            Utils.MissionSuccess(target, PlayerControl.LocalPlayer);
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
        public static IEnumerator CoReviveAndKill(PlayerControl target)
        {
            bool revived = false;
            byte revivedParentId = 255;

            Coroutines.Start(CoNotify("<color=#8A2BE2><i><b>Press F5 to revive a dead player!</b></i></color>"));

            while (true)
            {
                if (target.Data.IsDead)
                {
                    Utils.MissionFails(target, PlayerControl.LocalPlayer);
                    yield break;
                }
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    if (!revived)
                    {
                        var deadBody = Utils.GetClosestBody();
                        if (deadBody == null)
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
                    else
                    {
                        var revivedData = GameData.Instance.GetPlayerById(revivedParentId);
                        if (revivedData != null && revivedData.Object != null && !revivedData.Object.Data.IsDead)
                        {
                            PlayerControl.LocalPlayer.RpcCustomMurder(revivedData.Object, createDeadBody: true, didSucceed: true, showKillAnim: false, playKillSound: true, teleportMurderer: false);
                            Utils.MissionSuccess(target, PlayerControl.LocalPlayer);
                            yield break;
                        }
                    }
                }
                yield return null;
            }
        }
        public static IEnumerator CoHandleWantedTarget(ArrowBehaviour arrow, PlayerControl mostwantedTarget, PlayerControl target)
        {
            while (!mostwantedTarget.Data.IsDead && !mostwantedTarget.Data.Disconnected)
            {
                arrow.target = mostwantedTarget.transform.position;
                yield return null;
            }
            Object.Destroy(arrow.gameObject);

            yield return new WaitForSeconds(0.5f);

            var killer = Utils.GetKiller(mostwantedTarget);
            if (killer != null && killer == target)
            {
                Utils.MissionSuccess(target, PlayerControl.LocalPlayer);
            }
            else
            {
                Utils.MissionFails(target, PlayerControl.LocalPlayer);
            }
            yield break;
        }
    }
}
