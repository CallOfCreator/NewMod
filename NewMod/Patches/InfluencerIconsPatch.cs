using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Roles;

namespace NewMod.Patches;

[HarmonyPatch]
public static class InfluencerIconsPatch
{
    public static readonly List<int> RoleIconIndices = [];
    public static bool ForceNewModIcons;
    public static int PreviewOffset;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static void AddRoleIcons(ShipStatus __instance)
    {
        RoleIconIndices.Clear();
        ForceNewModIcons = false;
        PreviewOffset = 0;
        var sprites = __instance.GetSocialMediumSpriteList();
        var roles = CustomRoleManager.CustomMiraRoles.Where(role => role.GetType().Assembly == typeof(NewMod).Assembly).OrderBy(role => role.GetType().FullName, StringComparer.Ordinal);

        foreach (var role in roles)
        {
            var sprite = role.Configuration.Icon?.LoadAsset();
            if (!sprite)
                continue;

            var index = sprites.IndexOf(sprite);
            if (index < 0)
            {
                if (sprites.Count > byte.MaxValue)
                    break;

                index = sprites.Count;
                sprites.Add(sprite);
            }

            if (index <= byte.MaxValue && !RoleIconIndices.Contains(index))
                RoleIconIndices.Add(index);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpiritGuideRole), nameof(SpiritGuideRole.GenerateRandomImages))]
    public static bool GeneratePreview(SpiritGuideRole __instance)
    {
        if (!ForceNewModIcons || !__instance.Player.AmOwner || RoleIconIndices.Count == 0)
            return true;

        if (__instance.imagesGenerated)
            return false;

        var indices = new int[__instance.imageButtons.Count];
        for (var i = 0; i < indices.Length; i++)
        {
            var index = RoleIconIndices[(PreviewOffset + i) % RoleIconIndices.Count];
            indices[i] = index;
            __instance.imageButtons[i].SetSprite(__instance.possibleSprites[index]);
        }

        PreviewOffset = (PreviewOffset + indices.Length) % RoleIconIndices.Count;
        __instance.previouslyGeneratedImageIndices = indices;
        __instance.imagesGenerated = true;
        return false;
    }

    public static void SetForcedPreview(bool enabled)
    {
        ForceNewModIcons = enabled;
        PreviewOffset = 0;
        RefreshIcons();
    }

    public static void RefreshIcons()
    {
        var player = PlayerControl.LocalPlayer;
        if (!player || player.Data?.Role is not SpiritGuideRole role || role.SendingImage)
            return;

        role.imagesGenerated = false;
        if (!role.spiritGuidePanel.activeSelf)
            return;

        role.ResetPanelImages();
        role.GenerateRandomImages();
    }
}
