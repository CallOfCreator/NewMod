using HarmonyLib;

namespace NewMod.Patches.Roles.EnergyThief;


[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
public class MeetingHud_OnDestroy_Patch
{
    public static void Postfix(MeetingHud __instance)
    {
        PendingEffectManager.ApplyPendingEffects();
    }
}