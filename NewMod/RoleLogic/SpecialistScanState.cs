namespace NewMod.RoleLogic;

public enum SpecialistScanMode : byte
{
    Presence,
    Forensics,
    Disturbance
}

public class SpecialistScanState
{
    public int Charges;
    public SpecialistScanMode Mode;

    public void Earn()
    {
        if (Charges < 3)
            Charges++;
    }

    public SpecialistScanMode Cycle()
    {
        Mode = (SpecialistScanMode)(((int)Mode + 1) % 3);
        return Mode;
    }

    public bool Spend()
    {
        if (Charges == 0)
            return false;

        Charges--;
        return true;
    }
}