namespace AGMA6134C21Calc.Models;

/// <summary>Blank casting method for the bronze wormgear, Table 3 (K_d).</summary>
public enum GearCastingMethod
{
    Sand,
    StaticChillOrForged,
    Centrifugal
}

/// <summary>Worm surface heat treatment, Table 1 (used only for the 1.2 d) scope hardness check).</summary>
public enum WormHeatTreat
{
    Carburized,
    FlameOrInductionHardened
}

/// <summary>Lubricant type covered by this standard, Table 7 (Z_oil-wear, Z_oil-mu).</summary>
public enum OilType
{
    Mineral,
    PAO,
    PAG
}

/// <summary>Drive type mounted on a shaft extension, Table 8 (overhung load factor K_O).</summary>
public enum OverhungDriveType
{
    SingleOrMultipleChain,
    TimingBelt,
    CutPinionRunWithCutGear,
    SingleOrMultipleVeeBelt,
    FlatBelt,
    VariablePitchPulley,
    OtherManualKo
}

/// <summary>Duration of service per day, Annex A Table A.1.</summary>
public enum ServiceDuration
{
    Occasional0p5Hr,
    LessThan3Hr,
    Hr3To10,
    OverHr10
}

/// <summary>Shock load classification, Annex A Table A.1 / A.3.1.</summary>
public enum ShockLoadClass
{
    UniformLoad,
    ModerateShock,
    HeavyShock,
    ExtremeShock
}

/// <summary>Ambient temperature band used by Table 9's lubricant viscosity recommendation.</summary>
public enum AmbientTemperatureBand
{
    Minus10CTo10C,
    Between10CAnd52C
}
