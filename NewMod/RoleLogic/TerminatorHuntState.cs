using System.Collections.Generic;

namespace NewMod.RoleLogic;

public sealed class TerminatorHuntState
{
    private readonly HashSet<byte> _attackers = [];

    public TerminatorHuntState(int armor)
    {
        Armor = armor;
    }

    public int Armor { get; private set; }
    public bool IsDefeated => Armor == 0;

    public bool TryHit(byte attackerId)
    {
        if (IsDefeated || !_attackers.Add(attackerId))
            return false;

        Armor--;
        return true;
    }
}
