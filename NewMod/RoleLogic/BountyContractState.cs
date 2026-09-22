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

public class BountyContractState
{
    public BountyContractState(byte targetId)
    {
        TargetId = targetId;
    }

    public float AwayTime;
    public byte TargetId { get; }
    public float Progress { get; private set; }
    public BountyPhase Phase { get; private set; }

    public bool Advance(float seconds, float required)
    {
        if (Phase != BountyPhase.Escort)
            return false;

        AwayTime = 0f;
        Progress = Math.Min(required, Progress + seconds);
        if (Progress < required)
            return false;

        Phase = BountyPhase.Collection;
        return true;
    }

    public void LoseContact(float seconds, float grace)
    {
        if (Phase != BountyPhase.Escort) return;
        var previous = AwayTime;
        AwayTime += seconds;
        var lost = Math.Max(0f, AwayTime - grace) - Math.Max(0f, previous - grace);
        Progress = Math.Max(0f, Progress - lost);
    }

    public bool CanCollect(float now, float startsAt, float endsAt)
    {
        return Phase == BountyPhase.Collection && now >= startsAt && now < endsAt;
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