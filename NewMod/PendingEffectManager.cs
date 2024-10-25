using System.Collections.Generic;
using NewMod.Utilities;

namespace NewMod
{
    public static class PendingEffectManager
    {
        public static HashSet<PlayerControl> pendingEffects = new HashSet<PlayerControl>();
        public static void AddPendingEffect(PlayerControl target)
        {
            if (target != null && !pendingEffects.Contains(target))
            {
                pendingEffects.Add(target);
            }
        }
        public static void RemovePendingEffect(PlayerControl target)
        {
            if (target != null &&  pendingEffects.Contains(target))
            {
                pendingEffects.Remove(target);
            }
        }
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
