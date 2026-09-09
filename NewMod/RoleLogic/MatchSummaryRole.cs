namespace NewMod.RoleLogic;

public readonly record struct MatchSummaryRole(string Name, string Color, string Faction)
{
    public static MatchSummaryRole Select(MatchSummaryRole current, MatchSummaryRole previous, bool isDead)
    {
        return isDead ? previous : current;
    }
}