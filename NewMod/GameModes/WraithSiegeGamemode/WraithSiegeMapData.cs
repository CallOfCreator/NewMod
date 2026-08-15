using System.Collections.Generic;
using UnityEngine;

namespace NewMod.GameModes.WraithSiegeGamemode;

public enum WraithLane : byte
{
    Top,
    Mid,
    Bottom
}

public sealed class WraithSiegeMapDefinition(SystemTypes wraithBase, SystemTypes reviverBase, SystemTypes flag, SystemTypes[] top, SystemTypes[] mid, SystemTypes[] bottom)
{
    public SystemTypes WraithBase { get; } = wraithBase;
    public SystemTypes ReviverBase { get; } = reviverBase;
    public SystemTypes Flag { get; } = flag;

    public SystemTypes[] GetLane(WraithLane lane)
    {
        return lane switch
        {
            WraithLane.Top => top,
            WraithLane.Mid => mid,
            _ => bottom
        };
    }
}

public static class WraithSiegeMapData
{
    private const float SafeRadius = 0.34f;

    private static readonly Dictionary<SystemTypes, Vector2> RoomPointCache = [];

    private static int _cachedShipId = -1;

    private static readonly WraithSiegeMapDefinition[] Skeld =
    [
        new(SystemTypes.Reactor, SystemTypes.Nav, SystemTypes.Admin, [
            SystemTypes.Reactor,
            SystemTypes.UpperEngine,
            SystemTypes.MedBay,
            SystemTypes.Cafeteria
        ], [
            SystemTypes.Reactor,
            SystemTypes.Security,
            SystemTypes.Electrical,
            SystemTypes.Storage
        ], [
            SystemTypes.Reactor,
            SystemTypes.LowerEngine,
            SystemTypes.Storage
        ]),

        new(SystemTypes.Nav, SystemTypes.Reactor, SystemTypes.Storage, [
            SystemTypes.Nav,
            SystemTypes.Weapons,
            SystemTypes.Cafeteria,
            SystemTypes.Admin
        ], [
            SystemTypes.Nav,
            SystemTypes.Admin
        ], [
            SystemTypes.Nav,
            SystemTypes.Shields,
            SystemTypes.Comms
        ]),

        new(SystemTypes.LowerEngine, SystemTypes.Nav, SystemTypes.Cafeteria, [
            SystemTypes.LowerEngine,
            SystemTypes.Reactor,
            SystemTypes.UpperEngine,
            SystemTypes.MedBay
        ], [
            SystemTypes.LowerEngine,
            SystemTypes.Storage,
            SystemTypes.Admin
        ], [
            SystemTypes.LowerEngine,
            SystemTypes.Storage,
            SystemTypes.Shields,
            SystemTypes.Weapons
        ])
    ];

    private static readonly WraithSiegeMapDefinition[] Mira =
    [
        new(SystemTypes.Launchpad, SystemTypes.Greenhouse, SystemTypes.Office, [
            SystemTypes.Launchpad,
            SystemTypes.MedBay,
            SystemTypes.LockerRoom,
            SystemTypes.Laboratory,
            SystemTypes.Greenhouse
        ], [
            SystemTypes.Launchpad,
            SystemTypes.LockerRoom,
            SystemTypes.Laboratory
        ], [
            SystemTypes.Launchpad,
            SystemTypes.LockerRoom,
            SystemTypes.Admin,
            SystemTypes.Balcony
        ]),

        new(SystemTypes.Greenhouse, SystemTypes.Launchpad, SystemTypes.Cafeteria, [
            SystemTypes.Greenhouse,
            SystemTypes.Office,
            SystemTypes.Admin
        ], [
            SystemTypes.Greenhouse,
            SystemTypes.Laboratory,
            SystemTypes.LockerRoom
        ], [
            SystemTypes.Greenhouse,
            SystemTypes.Office,
            SystemTypes.Balcony
        ]),

        new(SystemTypes.Balcony, SystemTypes.Greenhouse, SystemTypes.Laboratory, [
            SystemTypes.Balcony,
            SystemTypes.Cafeteria,
            SystemTypes.Admin,
            SystemTypes.Office
        ], [
            SystemTypes.Balcony,
            SystemTypes.Cafeteria,
            SystemTypes.LockerRoom
        ], [
            SystemTypes.Balcony,
            SystemTypes.Admin,
            SystemTypes.Office,
            SystemTypes.Greenhouse
        ])
    ];

    private static readonly WraithSiegeMapDefinition[] Polus =
    [
        new(SystemTypes.Dropship, SystemTypes.Specimens, SystemTypes.Office, [
            SystemTypes.Dropship,
            SystemTypes.Electrical,
            SystemTypes.Weapons,
            SystemTypes.Comms
        ], [
            SystemTypes.Dropship,
            SystemTypes.Storage
        ], [
            SystemTypes.Dropship,
            SystemTypes.Laboratory,
            SystemTypes.Specimens
        ]),

        new(SystemTypes.Specimens, SystemTypes.Dropship, SystemTypes.Electrical, [
            SystemTypes.Specimens,
            SystemTypes.Laboratory
        ], [
            SystemTypes.Specimens,
            SystemTypes.Office,
            SystemTypes.Storage
        ], [
            SystemTypes.Specimens,
            SystemTypes.Office,
            SystemTypes.Comms,
            SystemTypes.Weapons
        ]),

        new(SystemTypes.Laboratory, SystemTypes.Weapons, SystemTypes.Office, [
            SystemTypes.Laboratory,
            SystemTypes.Electrical,
            SystemTypes.Storage
        ], [
            SystemTypes.Laboratory,
            SystemTypes.Specimens
        ], [
            SystemTypes.Laboratory,
            SystemTypes.Dropship,
            SystemTypes.Electrical,
            SystemTypes.Comms
        ])
    ];

    private static readonly WraithSiegeMapDefinition[] Airship =
    [
        new(SystemTypes.CargoBay, SystemTypes.Cockpit, SystemTypes.MainHall, [
            SystemTypes.CargoBay,
            SystemTypes.Records,
            SystemTypes.MeetingRoom,
            SystemTypes.GapRoom
        ], [
            SystemTypes.CargoBay,
            SystemTypes.Showers,
            SystemTypes.MainHall
        ], [
            SystemTypes.CargoBay,
            SystemTypes.Lounge,
            SystemTypes.Medical,
            SystemTypes.Electrical
        ]),

        new(SystemTypes.Cockpit, SystemTypes.CargoBay, SystemTypes.MeetingRoom, [
            SystemTypes.Cockpit,
            SystemTypes.VaultRoom,
            SystemTypes.GapRoom
        ], [
            SystemTypes.Cockpit,
            SystemTypes.Engine,
            SystemTypes.MainHall,
            SystemTypes.Records
        ], [
            SystemTypes.Cockpit,
            SystemTypes.Armory,
            SystemTypes.Kitchen,
            SystemTypes.ViewingDeck,
            SystemTypes.Engine
        ]),

        new(SystemTypes.Kitchen, SystemTypes.CargoBay, SystemTypes.Records, [
            SystemTypes.Kitchen,
            SystemTypes.Armory,
            SystemTypes.Cockpit,
            SystemTypes.VaultRoom
        ], [
            SystemTypes.Kitchen,
            SystemTypes.Engine,
            SystemTypes.MainHall,
            SystemTypes.Showers
        ], [
            SystemTypes.Kitchen,
            SystemTypes.Lounge,
            SystemTypes.CargoBay
        ])
    ];

    private static readonly WraithSiegeMapDefinition[] Fungle =
    [
        new(SystemTypes.Beach, SystemTypes.Highlands, SystemTypes.Jungle, [
            SystemTypes.Beach,
            SystemTypes.Lookout,
            SystemTypes.RecRoom
        ], [
            SystemTypes.Beach,
            SystemTypes.Kitchen,
            SystemTypes.SleepingQuarters
        ], [
            SystemTypes.Beach,
            SystemTypes.FishingDock,
            SystemTypes.MiningPit
        ]),

        new(SystemTypes.Jungle, SystemTypes.Beach, SystemTypes.RecRoom, [
            SystemTypes.Jungle,
            SystemTypes.Highlands,
            SystemTypes.Lookout
        ], [
            SystemTypes.Jungle,
            SystemTypes.SleepingQuarters,
            SystemTypes.Kitchen
        ], [
            SystemTypes.Jungle,
            SystemTypes.MiningPit,
            SystemTypes.FishingDock
        ]),

        new(SystemTypes.MiningPit, SystemTypes.Highlands, SystemTypes.Kitchen, [
            SystemTypes.MiningPit,
            SystemTypes.FishingDock,
            SystemTypes.Beach
        ], [
            SystemTypes.MiningPit,
            SystemTypes.Jungle,
            SystemTypes.SleepingQuarters
        ], [
            SystemTypes.MiningPit,
            SystemTypes.Jungle,
            SystemTypes.RecRoom
        ])
    ];

    public static byte LayoutIndex { get; private set; }

    private static WraithSiegeMapDefinition[] Layouts
    {
        get
        {
            var ship = ShipStatus.Instance;

            if (ship is AirshipStatus)
                return Airship;

            return ship.Type switch
            {
                ShipStatus.MapType.Hq => Mira,
                ShipStatus.MapType.Pb => Polus,
                ShipStatus.MapType.Fungle => Fungle,
                _ => Skeld
            };
        }
    }

    public static WraithSiegeMapDefinition Current => Layouts[LayoutIndex];

    public static Vector2 WraithSpawn => GetRoomPoint(Current.WraithBase);
    public static Vector2 ReviverSpawn => GetRoomPoint(Current.ReviverBase);
    public static Vector2 FlagPoint => GetRoomPoint(Current.Flag);

    public static byte PickRandomLayout()
    {
        return (byte)HashRandom.FastNext(Layouts.Length);
    }

    public static void SetLayout(byte index)
    {
        LayoutIndex = (byte)(index % Layouts.Length);
        EnsureRoomCache();
    }

    public static Vector2 GetWraithPlayerSpawn(byte playerId)
    {
        return GetPlayerSpawn(Current.WraithBase, playerId);
    }

    public static Vector2 GetReviverPlayerSpawn(byte playerId)
    {
        return GetPlayerSpawn(Current.ReviverBase, playerId);
    }

    public static Vector2[] ResolveLane(WraithLane lane, float flagRadius)
    {
        var roomIds = Current.GetLane(lane);
        var points = new Vector2[roomIds.Length + 1];

        for (var i = 0; i < roomIds.Length; i++)
            points[i] = GetRoomPoint(roomIds[i]);

        var flag = FlagPoint;
        var direction = points[^2] - flag;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.left;

        var preferred = flag + direction.normalized * (flagRadius + 0.4f);
        points[^1] = GetSafeNearby(preferred, flag);

        return points;
    }

    public static Vector2 GetRoomPoint(SystemTypes roomId)
    {
        EnsureRoomCache();

        if (RoomPointCache.TryGetValue(roomId, out var cached))
            return cached;

        var ship = ShipStatus.Instance;

        if (!ship.FastRooms.TryGetValue(roomId, out var room))
        {
            var fallback = GetSafeNearby(ship.InitialSpawnCenter, ship.InitialSpawnCenter);
            RoomPointCache[roomId] = fallback;
            return fallback;
        }

        if (!room.roomArea)
        {
            var fallback = GetSafeNearby(room.transform.position, ship.InitialSpawnCenter);
            RoomPointCache[roomId] = fallback;
            return fallback;
        }

        var area = room.roomArea;
        var center = (Vector2)area.bounds.center;

        if (IsSafePoint(center, area))
        {
            RoomPointCache[roomId] = center;
            return center;
        }

        var maxRadius = Mathf.Max(area.bounds.extents.x, area.bounds.extents.y);

        for (var radius = 0.35f; radius <= maxRadius; radius += 0.35f)
        {
            for (var i = 0; i < 20; i++)
            {
                var angle = i * Mathf.PI * 2f / 20f;
                var candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (!IsSafePoint(candidate, area))
                    continue;

                RoomPointCache[roomId] = candidate;
                return candidate;
            }
        }

        var safeFallback = GetSafeNearby(room.transform.position, ship.InitialSpawnCenter);
        RoomPointCache[roomId] = safeFallback;

        return safeFallback;
    }

    public static Vector2 GetSafeNearby(Vector2 preferred, Vector2 fallback)
    {
        if (IsSafePoint(preferred))
            return preferred;

        for (var radius = 0.25f; radius <= 2.5f; radius += 0.25f)
        {
            for (var i = 0; i < 20; i++)
            {
                var angle = i * Mathf.PI * 2f / 20f;
                var candidate = preferred + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (IsSafePoint(candidate))
                    return candidate;
            }
        }

        return fallback;
    }

    private static Vector2 GetPlayerSpawn(SystemTypes roomId, byte playerId)
    {
        var basePoint = GetRoomPoint(roomId);

        ShipStatus.Instance.FastRooms.TryGetValue(roomId, out var room);
        var area = room && room.roomArea ? room.roomArea : null;

        var startAngle = playerId * 47f * Mathf.Deg2Rad;
        var desired = basePoint + new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * 0.55f;

        if (IsSafePoint(desired, area))
            return desired;

        for (var ring = 0.4f; ring <= 1.2f; ring += 0.2f)
        {
            for (var i = 0; i < 16; i++)
            {
                var angle = startAngle + i * 22.5f * Mathf.Deg2Rad;
                var candidate = basePoint + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring;

                if (IsSafePoint(candidate, area))
                    return candidate;
            }
        }

        return basePoint;
    }

    private static bool IsSafePoint(Vector2 point, Collider2D roomArea = null)
    {
        if (roomArea && !roomArea.OverlapPoint(point))
            return false;

        if (HasSolidCollider(point, SafeRadius))
            return false;

        var openDirections = 0;

        if (!HasSolidCollider(point + Vector2.up * 0.55f, SafeRadius))
            openDirections++;

        if (!HasSolidCollider(point + Vector2.down * 0.55f, SafeRadius))
            openDirections++;

        if (!HasSolidCollider(point + Vector2.left * 0.55f, SafeRadius))
            openDirections++;

        if (!HasSolidCollider(point + Vector2.right * 0.55f, SafeRadius))
            openDirections++;

        return openDirections >= 2;
    }

    private static bool HasSolidCollider(Vector2 point, float radius)
    {
        foreach (var collider in Physics2D.OverlapCircleAll(point, radius, Constants.ShipOnlyMask))
        {
            if (collider && collider.enabled && !collider.isTrigger)
                return true;
        }

        return false;
    }

    private static void EnsureRoomCache()
    {
        var ship = ShipStatus.Instance;
        var shipId = ship.GetInstanceID();

        if (_cachedShipId == shipId)
            return;

        _cachedShipId = shipId;
        RoomPointCache.Clear();
    }
}