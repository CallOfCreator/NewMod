using System.Collections.Generic;
using System.Reflection;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Tools;
using CorsacCosmetics.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NewMod.Cosmetics;

public static class NewModCosmeticsRegistry
{
    public static readonly List<CustomHat> PendingHats = [];
    public static readonly List<CustomVisor> PendingVisors = [];
    public static readonly List<CustomNamePlate> PendingNamePlates = [];

    public static void RegisterHat(string name, Sprite sprite, HatMetadata? meta = null)
    {
        var m = meta ?? new HatMetadata { Name = name };
        var id = Names.Normalize(name, "hat", "newmod");

        if (PendingHats.Exists(hat => hat.Id == id))
            return;

        sprite.hideFlags = HideFlags.HideAndDontSave;
        sprite.texture.hideFlags = HideFlags.HideAndDontSave;

        var viewData = ScriptableObject.CreateInstance<HatViewData>();
        viewData.name = m.Name;
        viewData.hideFlags = HideFlags.HideAndDontSave;
        viewData.MainImage = sprite;
        viewData.MatchPlayerColor = m.MatchPlayerColor;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = m.Name;
        previewData.hideFlags = HideFlags.HideAndDontSave;
        previewData.PreviewSprite = sprite;

        var hatData = ScriptableObject.CreateInstance<HatData>();
        hatData.name = m.Name;
        hatData.hideFlags = HideFlags.HideAndDontSave;
        hatData.StoreName = m.Name;
        hatData.ProductId = id;
        hatData.BundleId = "";
        hatData.Free = true;
        hatData.BlocksVisors = m.BlocksVisors;
        hatData.NoBounce = m.NoBounce;
        hatData.InFront = m.InFront;
        hatData.PreviewCrewmateColor = m.MatchPlayerColor;
        hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.HatViewData));
        hatData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        PendingHats.Add(new CustomHat(id, hatData, viewData, previewData));
    }

    public static void RegisterVisor(string name, Sprite sprite, VisorMetadata? meta = null)
    {
        var m = meta ?? new VisorMetadata { Name = name };
        var id = Names.Normalize(name, "visor", "newmod");

        if (PendingVisors.Exists(visor => visor.Id == id))
            return;

        sprite.hideFlags = HideFlags.HideAndDontSave;
        sprite.texture.hideFlags = HideFlags.HideAndDontSave;

        var viewData = ScriptableObject.CreateInstance<VisorViewData>();
        viewData.name = m.Name;
        viewData.hideFlags = HideFlags.HideAndDontSave;
        viewData.MatchPlayerColor = m.MatchPlayerColor;
        viewData.ClimbFrame = SpriteTools.EmptySprite;
        viewData.IdleFrame = sprite;
        viewData.LeftIdleFrame = sprite;
        viewData.FloorFrame = sprite;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = m.Name;
        previewData.hideFlags = HideFlags.HideAndDontSave;
        previewData.PreviewSprite = sprite;

        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = m.Name;
        visorData.hideFlags = HideFlags.HideAndDontSave;
        visorData.ProductId = id;
        visorData.BundleId = "";
        visorData.Free = true;
        visorData.behindHats = m.BehindHats;
        visorData.PreviewCrewmateColor = m.MatchPlayerColor;
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        PendingVisors.Add(new CustomVisor(id, visorData, viewData, previewData));
    }

    public static void RegisterNamePlate(string name, Sprite sprite, NameplateMetadata? meta = null)
    {
        var m = meta ?? new NameplateMetadata { Name = name };
        var id = Names.Normalize(name, "nameplate", "newmod");

        if (PendingNamePlates.Exists(nameplate => nameplate.Id == id))
            return;

        sprite.hideFlags = HideFlags.HideAndDontSave;
        sprite.texture.hideFlags = HideFlags.HideAndDontSave;

        var viewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        viewData.name = m.Name;
        viewData.hideFlags = HideFlags.HideAndDontSave;
        viewData.Image = sprite;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = m.Name;
        previewData.hideFlags = HideFlags.HideAndDontSave;
        previewData.PreviewSprite = sprite;

        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = m.Name;
        namePlateData.hideFlags = HideFlags.HideAndDontSave;
        namePlateData.ProductId = id;
        namePlateData.BundleId = "";
        namePlateData.Free = true;
        namePlateData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.NamePlateViewData));
        namePlateData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        PendingNamePlates.Add(new CustomNamePlate(id, namePlateData, viewData, previewData));
    }

    public static void InjectToCorsac()
    {
        var loader = CosmeticsLoader.Instance;
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var loaderType = typeof(CosmeticsLoader);

        var hatLoader = (HatLoader)loaderType.GetField("_hatLoader", flags)!.GetValue(loader)!;
        var visorLoader = (VisorLoader)loaderType.GetField("_visorLoader", flags)!.GetValue(loader)!;
        var nameplateLoader = (NameplateLoader)loaderType.GetField("_nameplateLoader", flags)!.GetValue(loader)!;
        var cosmeticGroup = (CosmeticReleaseGroup)loaderType.GetProperty("CosmeticGroup", flags)!.GetValue(loader)!;
        var customGroups = (Dictionary<string, string>)loaderType.GetProperty("CustomGroups", flags)!.GetValue(loader)!;

        customGroups["newmod"] = "NewMod";

        foreach (var hat in PendingHats)
        {
            if (hatLoader.CustomHats.ContainsKey(hat.Id))
                continue;

            hatLoader.CustomHats[hat.Id] = hat;
            loader.HatGroups.AddGroup("newmod");

            if (!cosmeticGroup.ids.Contains(hat.Id))
                cosmeticGroup.ids.Add(hat.Id);

            hat.HatData.ViewDataRef.LoadAsset<HatViewData>();
            hat.HatData.PreviewData.LoadAsset<PreviewViewData>();
        }

        foreach (var visor in PendingVisors)
        {
            if (visorLoader.CustomVisors.ContainsKey(visor.Id))
                continue;

            visorLoader.CustomVisors[visor.Id] = visor;
            loader.VisorGroups.AddGroup("newmod");

            if (!cosmeticGroup.ids.Contains(visor.Id))
                cosmeticGroup.ids.Add(visor.Id);

            visorLoader.CustomVisors[visor.Id] = visor;
            visor.VisorData.ViewDataRef.LoadAsset<VisorViewData>();
            visor.VisorData.PreviewData.LoadAsset<PreviewViewData>();
        }

        foreach (var plate in PendingNamePlates)
        {
            if (nameplateLoader.CustomNamePlates.ContainsKey(plate.Id))
                continue;

            nameplateLoader.CustomNamePlates[plate.Id] = plate;
            loader.NameplateGroups.AddGroup("newmod");

            if (!cosmeticGroup.ids.Contains(plate.Id))
                cosmeticGroup.ids.Add(plate.Id);

            nameplateLoader.CustomNamePlates[plate.Id] = plate;
            plate.NamePlateData.ViewDataRef.LoadAsset<NamePlateViewData>();
            plate.NamePlateData.PreviewData.LoadAsset<PreviewViewData>();
        }
    }
}
