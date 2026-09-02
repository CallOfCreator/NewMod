namespace NewMod.RoleLogic;

public enum UsurperCrownPhase : byte
{
    Unclaimed,
    Claimed,
    Available,
    Held
}

public sealed class UsurperCrownState
{
    public byte TargetId { get; private set; } = byte.MaxValue;
    public UsurperCrownPhase Phase { get; private set; }

    public bool Claim(byte targetId)
    {
        if (Phase != UsurperCrownPhase.Unclaimed)
            return false;

        TargetId = targetId;
        Phase = UsurperCrownPhase.Claimed;
        return true;
    }

    public bool MakeCrownAvailable(byte targetId)
    {
        if (Phase != UsurperCrownPhase.Claimed || TargetId != targetId)
            return false;

        Phase = UsurperCrownPhase.Available;
        return true;
    }

    public bool Refund(byte targetId)
    {
        if (Phase != UsurperCrownPhase.Claimed || TargetId != targetId)
            return false;

        TargetId = byte.MaxValue;
        Phase = UsurperCrownPhase.Unclaimed;
        return true;
    }

    public bool TakeCrown()
    {
        if (Phase != UsurperCrownPhase.Available)
            return false;

        Phase = UsurperCrownPhase.Held;
        return true;
    }

    public bool SurvivedMeeting(bool alive)
    {
        return Phase == UsurperCrownPhase.Held && alive;
    }
}
