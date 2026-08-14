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
            SystemTypes.Shields
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
            SystemTypes.Showers
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

    public static byte PickRandomLayout()
    {
        return (byte)HashRandom.FastNext(Layouts.Length);
    }

    public static void SetLayout(byte index)
    {
        LayoutIndex = (byte)(index % Layouts.Length);
    }

    public static Vector2 WraithSpawn => GetRoomPoint(Current.WraithBase);
    public static Vector2 ReviverSpawn => GetRoomPoint(Current.ReviverBase);
    public static Vector2 FlagPoint => GetRoomPoint(Current.Flag);

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

        points[^1] = flag + direction.normalized * (flagRadius + 0.4f);
        return points;
    }

    public static Vector2 GetRoomPoint(SystemTypes roomId)
    {
        var ship = ShipStatus.Instance;

        if (!ship.FastRooms.TryGetValue(roomId, out var room))
            return ship.InitialSpawnCenter;

        return room.roomArea ? room.roomArea.bounds.center : room.transform.position;
    }
}