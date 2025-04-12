using System.Collections.Generic;
using NewMod.Utilities;

namespace NewMod
{
    /// <summary>
    /// Manages a collection of pending effects that can be applied to players.
    /// </summary>
    public static class PendingEffectManager
    {
        /// <summary>
        /// A set of player targets to which effects are awaiting application.
        /// </summary>
        public static HashSet<PlayerControl> pendingEffects = new HashSet<PlayerControl>();

        /// <summary>
        /// Adds a player to the pending effects set if not already present.
        /// </summary>
        /// <param name="target">The player to receive the pending effect.</param>
        public static void AddPendingEffect(PlayerControl target)
        {
            if (target != null && !pendingEffects.Contains(target))
            {
                pendingEffects.Add(target);
            }
        }

        /// <summary>
        /// Removes a player from the pending effects set if they are currently included.
        /// </summary>
        /// <param name="target">The player whose pending effect should be removed.</param>
        public static void RemovePendingEffect(PlayerControl target)
        {
            if (target != null && pendingEffects.Contains(target))
            {
                pendingEffects.Remove(target);
            }
        }

        /// <summary>
        /// Applies all pending effects to their respective players and then clears the lists of pending effects and waiting players.
        /// </summary>
        public static void ApplyPendingEffects()
        {
            foreach (var target in pendingEffects)
            {
                if (target != null && !target.Data.IsDead && !target.Data.Disconnected)
                {
                    Utils.RpcRandomDrainActions(PlayerControl.LocalPlayer, target);
                }
            }
            pendingEffects.Clear();
            Utils.waitingPlayers.Clear();
        }
    }
}
