using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using HarmonyLib;
using MiraAPI;

namespace NewMod.Cosmetics;

internal static class CorsacCosmeticsIntegration
{
    public static void Initialize(Harmony harmony)
    {
        harmony.Unpatch(AccessTools.Method(typeof(HatsTab), nameof(HatsTab.OnEnable)), HarmonyPatchType.All, MiraApiPlugin.Id);
        harmony.Unpatch(AccessTools.Method(typeof(VisorsTab), nameof(VisorsTab.OnEnable)), HarmonyPatchType.All, MiraApiPlugin.Id);
        harmony.Unpatch(AccessTools.Method(typeof(HatsTab), nameof(HatsTab.Update)), HarmonyPatchType.Prefix, MiraApiPlugin.Id);
        harmony.Unpatch(AccessTools.Method(typeof(VisorsTab), nameof(VisorsTab.Update)), HarmonyPatchType.Prefix, MiraApiPlugin.Id);
        harmony.Unpatch(AccessTools.Method(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable)), HarmonyPatchType.Prefix, MiraApiPlugin.Id);
        harmony.Unpatch(AccessTools.Method(typeof(NameplatesTab), nameof(NameplatesTab.Update)), HarmonyPatchType.Prefix, MiraApiPlugin.Id);
        harmony.Unpatch(AccessTools.Method(typeof(HatsTab), nameof(HatsTab.OnEnable)), HarmonyPatchType.Prefix, NewMod.CorsacPluginId);
        harmony.Unpatch(AccessTools.Method(typeof(VisorsTab), nameof(VisorsTab.OnEnable)), HarmonyPatchType.Prefix, NewMod.CorsacPluginId);

        NewModCosmeticsRegistry.RegisterHat("og_newmod", NewModAsset.OG_NewModHat.LoadAsset(), new HatMetadata { Name = "OG NewMod", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("glitch_reality", NewModAsset.GlitchedRealityHat.LoadAsset(), new HatMetadata { Name = "Glitch Reality", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("mint_icecream", NewModAsset.MintIceCreamHat.LoadAsset(), new HatMetadata { Name = "Mint Ice Cream", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("strawberry_icecream", NewModAsset.StrawberryIceCreamHat.LoadAsset(), new HatMetadata { Name = "Strawberry Ice Cream", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("pizza", NewModAsset.PizzaHat.LoadAsset(), new HatMetadata { Name = "Pizza", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("squeeze_cap", NewModAsset.SqueezeCapHat.LoadAsset(), new HatMetadata { Name = "Squeeze Cap", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("zros", NewModAsset.ZrosHat.LoadAsset(), new HatMetadata { Name = "Zro's Hat", InFront = true, NoBounce = false });
        NewModCosmeticsRegistry.RegisterHat("igotanidea", NewModAsset.IGotanIdeaHat.LoadAsset(), new HatMetadata { Name = "i got an idea", InFront = true, NoBounce = false });

        NewModCosmeticsRegistry.RegisterVisor("malicious_look", NewModAsset.MaliciousLook.LoadAsset(), new VisorMetadata { Name = "Malicious Look" });
        NewModCosmeticsRegistry.RegisterVisor("cotton_memories", NewModAsset.CottonMemoriesVisor.LoadAsset(), new VisorMetadata { Name = "Cotton Memories Visor" });

        NewModCosmeticsRegistry.RegisterNamePlate("nm_rave", NewModAsset.NMraveNameplate.LoadAsset(), new NameplateMetadata { Name = "NM Rave" });
        NewModCosmeticsRegistry.RegisterNamePlate("sunny_sky", NewModAsset.SunnyNameplate.LoadAsset(), new NameplateMetadata { Name = "Sunny Sky" });

        var original = AccessTools.Method(typeof(CosmeticsLoader), nameof(CosmeticsLoader.InstallCosmetics));
        var prefix = AccessTools.Method(typeof(CorsacCosmeticsIntegration), nameof(InjectCosmetics));
        harmony.Patch(original, new HarmonyMethod(prefix));

        NewMod.Instance.Log.LogMessage("Registered NewMod cosmetics through Corsac Cosmetics");
    }

    private static void InjectCosmetics()
    {
        NewModCosmeticsRegistry.InjectToCorsac();
    }
}