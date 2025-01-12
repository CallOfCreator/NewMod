using System;
using System.Diagnostics;
using HarmonyLib;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Patches.Roles.EnergyThief;

[HarmonyPatch(typeof(Application), nameof(Application.Quit), new Type[] { })]
public static class QuitPatch
{
    public static bool Prefix(Application __instance)
    {
        VisionaryUtilities.DeleteAllScreenshots();
        NewMod.Instance.Log.LogMessage("Deleted all Visionary's screenshots Successfully");
        Process.GetCurrentProcess().Kill();
        return false;
    }
}