using System.Collections;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using NewMod.Options;
using NewMod.Options.Modifiers;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers
{
    public class StickyModifier : GameModifier
    {
        public override string ModifierName => "Sticky";
        public override bool HideOnUi => false;
        public override bool ShowInFreeplay => true;
        public static List<PlayerControl> linkedPlayers = [];
        public static bool _IsActive = false;
        public override int GetAmountPerGame()
        {
            return (int)OptionGroupSingleton<ModifiersOptions>.Instance.StickyAmount;
        }
        public override int GetAssignmentChance()
        {
            return OptionGroupSingleton<ModifiersOptions>.Instance.StickyChance;
        }
        public override bool? CanVent()
        {
            return Player.Data.Role.CanVent;
        }
        public override string GetDescription()
        {
            float distance = (int)OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance;
            float duration = (int)OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDuration;

            return $"{ModifierName}: Pulls nearby players within {distance} units for {duration} seconds.";
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (_IsActive) return;

            if (!Player.CanMove || Player.Data.IsDead) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == Player) continue;

                float distance = (int)OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance;

                if (Vector2.Distance(player.GetTruePosition(), Player.GetTruePosition()) < distance)
                {
                    _IsActive = true;
                    linkedPlayers.Add(player);
                    Coroutines.Start(CoFollowStickyPlayer(player));
                    break;
                }
            }
        }
        public IEnumerator CoFollowStickyPlayer(PlayerControl player)
        {
            float duration = (int)OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDuration;
            float timer = 0f;
            float pullStrength = (int)OptionGroupSingleton<StickyModifierOptions>.Instance.PullStrength;
            float stopDistance = 1f;

            while (timer < duration)
            {
                if (player.Data.IsDead || player.Data.Disconnected) break;

                timer += Time.deltaTime;

                var ownerPos = Player.transform.position;
                var targetPos = player.transform.position;

                float distance = Vector3.Distance(ownerPos, targetPos);

                if (distance > stopDistance)
                {
                    Vector3 direction = (ownerPos - targetPos).normalized;
                    Vector3 leashPoint = ownerPos - (direction * stopDistance);

                    player.transform.position = Vector3.Lerp(targetPos, leashPoint, Time.deltaTime * pullStrength);
                }
                yield return null;
            }
            linkedPlayers.Remove(player);

            _IsActive = true;

            if (Player.AmOwner)
            {
                PlayerControl.LocalPlayer.RpcRemoveModifier<StickyModifier>();
            }
        }
        [RegisterEvent]
        public static void OnRoundStart(RoundStartEvent evt)
        {
            linkedPlayers.Clear();
        }
    }
}