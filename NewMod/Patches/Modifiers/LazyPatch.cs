using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Modifiers;
using NewMod.Modifiers;

namespace NewMod.Patches.Modifiers
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
    public static class LazyModifierSetTasksPatch
    {
        public static void Prefix(PlayerControl __instance, List<NetworkedPlayerInfo.TaskInfo> tasks)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!__instance.HasModifier<LazyModifier>()) return;
            if ( tasks.Count <= 1) return;

            var keep = tasks[0];
            tasks.Clear();
            tasks.Add(keep);
        }
    }
}
