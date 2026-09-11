using System.Collections.Generic;
using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.AssetReaders;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Sources;
using CorsacCosmetics.Cosmetics.Visors;
using UnityEngine;
using CorsacCosmeticType = CorsacCosmetics.Cosmetics.CosmeticType;

namespace NewMod.Cosmetics;

public static class NewModCosmeticsRegistry
{
    public static string SourceId = "newmod";
    public static string GroupName = "NewMod";
    public static readonly List<CosmeticDescriptor> Cosmetics = new();
    public static readonly Dictionary<string, string> HatIds = new();

    public static void RegisterHat(string name, Sprite sprite, HatMetadata? meta = null)
    {
        var descriptor = Register(name, sprite, CorsacCosmeticType.Hat, meta ?? new HatMetadata { Name = name });
        HatIds[name] = descriptor.Id;
    }

    public static void RegisterVisor(string name, Sprite sprite, VisorMetadata? meta = null)
    {
        Register(name, sprite, CorsacCosmeticType.Visor, meta ?? new VisorMetadata { Name = name });
    }

    public static void RegisterNamePlate(string name, Sprite sprite, NamePlateMetadata? meta = null)
    {
        Register(name, sprite, CorsacCosmeticType.NamePlate, meta ?? new NamePlateMetadata { Name = name });
    }

    public static CosmeticDescriptor Register(string name, Sprite sprite, CorsacCosmeticType type, ICosmeticMetadata metadata)
    {
        var reader = new NewModCosmeticAssetReader();
        reader.Sprites[""] = sprite;
        reader.Sprites["preview"] = sprite;
        if (type == CorsacCosmeticType.Visor)
        {
            reader.Sprites["left"] = sprite;
            reader.Sprites["floor"] = sprite;
        }

        var descriptor = new CosmeticDescriptor(SourceId, GroupName, name, type, metadata, reader);
        var existing = Cosmetics.Find(cosmetic => cosmetic.Id == descriptor.Id);
        if (existing != null)
            return existing;

        sprite.hideFlags = HideFlags.HideAndDontSave;
        sprite.texture.hideFlags = HideFlags.HideAndDontSave;
        Cosmetics.Add(descriptor);
        return descriptor;
    }
}

public class NewModCosmeticSource : ICosmeticSource
{
    public string SourceId => NewModCosmeticsRegistry.SourceId;

    public Task<IEnumerable<CosmeticDescriptor>> DiscoverAsync()
    {
        return Task.FromResult<IEnumerable<CosmeticDescriptor>>(NewModCosmeticsRegistry.Cosmetics.ToArray());
    }
}

public class NewModCosmeticAssetReader : ICosmeticAssetReader
{
    public readonly Dictionary<string, Sprite> Sprites = new();

    public Task<Sprite> LoadSpriteAsync(string spriteKey)
    {
        if (!Sprites.TryGetValue(spriteKey, out var template))
            return Task.FromResult<Sprite>(null);

        var texture = Object.Instantiate(template.texture);
        var rect = template.rect;
        var pivot = new Vector2(template.pivot.x / rect.width, template.pivot.y / rect.height);
        var sprite = Sprite.Create(texture, rect, pivot, template.pixelsPerUnit, 0, SpriteMeshType.FullRect, template.border);
        return Task.FromResult(sprite);
    }
}
