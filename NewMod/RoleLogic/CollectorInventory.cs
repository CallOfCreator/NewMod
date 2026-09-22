namespace NewMod.RoleLogic;

public enum CollectorFragmentKind : byte
{
    Violence,
    Ability,
    Fate
}

public enum CollectorManifestResult : byte
{
    None,
    Victory,
    Trace,
    Drift
}

public class CollectorInventory
{
    public readonly int[] Counts = new int[3];

    public void Add(CollectorFragmentKind kind)
    {
        Counts[(int)kind]++;
    }

    public int Count(CollectorFragmentKind kind)
    {
        return Counts[(int)kind];
    }

    public bool CanConvert(int cost)
    {
        return System.Array.Exists(Counts, count => count == 0) && System.Array.Exists(Counts, count => count > cost);
    }

    public bool Convert(int cost)
    {
        var missing = System.Array.FindIndex(Counts, count => count == 0);
        var donor = System.Array.FindIndex(Counts, count => count > cost);
        if (missing < 0 || donor < 0) return false;
        Counts[donor] -= cost;
        Counts[missing]++;
        return true;
    }

    public bool CanManifest => (Counts[0] > 0 && Counts[1] > 0 && Counts[2] > 0) || Counts[0] >= 2 || Counts[1] >= 2 || Counts[2] >= 2;

    public CollectorManifestResult Manifest()
    {
        if (Counts[0] > 0 && Counts[1] > 0 && Counts[2] > 0)
        {
            Counts[0]--;
            Counts[1]--;
            Counts[2]--;
            return CollectorManifestResult.Victory;
        }

        if (Counts[0] >= 2)
        {
            Counts[0] -= 2;
            return CollectorManifestResult.Trace;
        }

        if (Counts[1] >= 2)
        {
            Counts[1] -= 2;
            return CollectorManifestResult.Trace;
        }

        if (Counts[2] >= 2)
        {
            Counts[2] -= 2;
            return CollectorManifestResult.Drift;
        }

        return CollectorManifestResult.None;
    }
}