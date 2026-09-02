namespace NewMod.RoleLogic;

public enum SpecialistScanMode : byte
{
    Presence,
    Forensics,
    Vital
}

public sealed class SpecialistScanState
{
    public int Charges { get; private set; }
    public SpecialistScanMode Mode { get; private set; }

    public void Earn()
    {
        Charges++;
    }

    public SpecialistScanMode Cycle()
    {
        Mode = (SpecialistScanMode)(((int)Mode + 1) % 3);
        return Mode;
    }

    public bool TrySpend()
    {
        if (Charges == 0)
            return false;

        Charges--;
        return true;
    }
}
