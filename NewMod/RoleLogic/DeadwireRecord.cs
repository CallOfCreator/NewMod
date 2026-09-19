using NewMod.Roles.NeutralRoles;

namespace NewMod.RoleLogic;

public enum DeadwireResponse : byte
{
    ReduceKillCooldown,
    TrackActor,
    Blink,
    JamActor,
    Barrier
}

public readonly record struct DeadwireRecord(byte ActorId, EnergyCategory Category)
{
    public static float ReduceCooldown(float remaining, float reduction, float minimum)
    {
        return System.Math.Min(remaining, System.Math.Max(minimum, remaining - reduction));
    }

    public string Description =>
        Category switch
        {
            EnergyCategory.Aggression => "Kill Boost",
            EnergyCategory.Intelligence => "Track",
            EnergyCategory.Mobility => "Blink",
            EnergyCategory.Control => "Jam",
            _ => "Shield"
        };

    public DeadwireResponse Response =>
        Category switch
        {
            EnergyCategory.Aggression => DeadwireResponse.ReduceKillCooldown,
            EnergyCategory.Intelligence => DeadwireResponse.TrackActor,
            EnergyCategory.Mobility => DeadwireResponse.Blink,
            EnergyCategory.Control => DeadwireResponse.JamActor,
            _ => DeadwireResponse.Barrier
        };
}