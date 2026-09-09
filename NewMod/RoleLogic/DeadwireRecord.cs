using NewMod.Roles.NeutralRoles;

namespace NewMod.RoleLogic;

public enum DeadwireResponse : byte
{
    RefreshKill,
    TrackActor,
    Blink,
    JamActor,
    Barrier
}

public readonly record struct DeadwireRecord(byte ActorId, EnergyCategory Category)
{
    public DeadwireResponse Response =>
        Category switch
        {
            EnergyCategory.Aggression => DeadwireResponse.RefreshKill,
            EnergyCategory.Intelligence => DeadwireResponse.TrackActor,
            EnergyCategory.Mobility => DeadwireResponse.Blink,
            EnergyCategory.Control => DeadwireResponse.JamActor,
            _ => DeadwireResponse.Barrier
        };
}