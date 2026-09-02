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

public sealed class CollectorInventory
{
    private readonly int[] _counts = new int[3];

    public void Add(CollectorFragmentKind kind)
    {
        _counts[(int)kind]++;
    }

    public int Count(CollectorFragmentKind kind)
    {
        return _counts[(int)kind];
    }

    public bool CanManifest => _counts[0] > 0 && _counts[1] > 0 && _counts[2] > 0 || _counts[0] >= 2 || _counts[1] >= 2 || _counts[2] >= 2;

    public CollectorManifestResult Manifest()
    {
        if (_counts[0] > 0 && _counts[1] > 0 && _counts[2] > 0)
        {
            _counts[0]--;
            _counts[1]--;
            _counts[2]--;
            return CollectorManifestResult.Victory;
        }

        if (_counts[0] >= 2)
        {
            _counts[0] -= 2;
            return CollectorManifestResult.Trace;
        }

        if (_counts[1] >= 2)
        {
            _counts[1] -= 2;
            return CollectorManifestResult.Trace;
        }

        if (_counts[2] >= 2)
        {
            _counts[2] -= 2;
            return CollectorManifestResult.Drift;
        }

        return CollectorManifestResult.None;
    }
}
