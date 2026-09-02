namespace NewMod.RoleLogic;

public enum EgoistChallengeResult : byte
{
    Failed,
    Win
}

public sealed class EgoistChallengeState
{
    public int Ego { get; private set; }
    public byte OpponentId { get; private set; } = byte.MaxValue;
    public bool Active => OpponentId != byte.MaxValue;

    public bool AddVotes(int votes, int required)
    {
        Ego = System.Math.Min(required, Ego + votes);
        return Ego >= required;
    }

    public void SetEgo(int ego)
    {
        Ego = ego;
    }

    public bool Start(byte opponentId)
    {
        if (Active)
            return false;

        OpponentId = opponentId;
        return true;
    }

    public EgoistChallengeResult Resolve(byte exiledId, byte egoistId)
    {
        var result = exiledId == OpponentId && exiledId != egoistId ? EgoistChallengeResult.Win : EgoistChallengeResult.Failed;
        Ego = 0;
        OpponentId = byte.MaxValue;
        return result;
    }
}
