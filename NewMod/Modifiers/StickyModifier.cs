using System.Collections;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using NewMod.Options.Modifiers;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers
{
    public class StickyModifier : TimedModifier
    {
        public override string ModifierName => "Sticky";
        public override bool AutoStart => OptionGroupSingleton<StickyModifierOptions>.Instance.EnableModifier;
        public override float Duration => (int)OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDuration;
        public override bool HideOnUi => false;
        public override bool ShowInFreeplay => true;
        public override bool RemoveOnComplete => true;
        public static List<PlayerControl> linkedPlayers = new();
        public override bool? CanVent()
        {
            return Player.Data.Role.CanVent;
        }
        public override string GetDescription()
        {
            float distance = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance.Value;
            float duration = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDuration.Value;

            return $"{ModifierName}: Pulls nearby players within {distance} units for {duration} seconds.";
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!Player.CanMove) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == Player || linkedPlayers.Contains(player)) continue;

                float distance = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance.Value;

                if (Vector2.Distance(player.GetTruePosition(), Player.GetTruePosition()) < distance)
                {
                    linkedPlayers.Add(player);
                    Coroutines.Start(CoFollowStickyPlayer(player));
                }
            }
        }
        public IEnumerator CoFollowStickyPlayer(PlayerControl player)
        {
            float duration = Duration;

            var info = new StickyState
            {
                StickyOwner = Player,
                LinkedPlayer = player,
                velocity = Vector3.zero,
            };

            yield return HudManager.Instance.StartCoroutine(
                Effects.Overlerp(duration, new System.Action<float>((t) =>
                {
                    Vector3 targetPos = info.LinkedPlayer.transform.position;
                    Vector3 currentPos = info.StickyOwner.transform.position;

                    info.LinkedPlayer.transform.position = Vector3.SmoothDamp(
                        targetPos,
                        currentPos,
                        ref info.velocity,
                        t
                    );
                })
            ));

            linkedPlayers.Remove(player);
        }
    }

    class StickyState
    {
        public PlayerControl StickyOwner;
        public PlayerControl LinkedPlayer;
        public Vector3 velocity;
    }
}
