namespace NewMod.Roles.NeutralRoles;

public enum EnergyCategory : byte
{
    Aggression,
    Intelligence,
    Mobility,
    Control,
    Protection
}

public interface IEnergyAbility
{
    EnergyCategory Category { get; }
    bool CaptureOnClick => true;
}