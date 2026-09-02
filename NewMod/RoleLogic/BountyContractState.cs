using System;

namespace NewMod.RoleLogic;

public enum BountyPhase : byte
{
    Escort,
    Collection
}

public enum BountyResolution : byte
{
    Ongoing,
    Win,
    Failed
}

public sealed class BountyContractState
{
    public BountyContractState(byte targetId)
    {
        TargetId = targetId;
    }

    public byte TargetId { get; }
    public float Progress { get; private set; }
    public BountyPhase Phase { get; private set; }

    public bool Advance(float seconds, float required)
    {
        if (Phase != BountyPhase.Escort)
            return false;

        Progress = Math.Min(required, Progress + seconds);
        if (Progress < required)
            return false;

        Phase = BountyPhase.Collection;
        return true;
    }

    public void BeginCollection()
    {
        Phase = BountyPhase.Collection;
    }

    public static BountyResolution ResolveCollection(bool killedByBounty, bool targetKilled, bool interrupted)
    {
        if (killedByBounty)
            return BountyResolution.Win;

        return targetKilled || interrupted ? BountyResolution.Failed : BountyResolution.Ongoing;
    }
}
