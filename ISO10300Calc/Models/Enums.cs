namespace ISO10300Calc.Models;

/// <summary>Basic bevel gear tooth form (ISO 10300-1:2023, throughout). Straight/Zerol bevel
/// gears take K_F0 = 1.0 (10300-1, 8.4.4a) and their Y_BS bending-spiral-angle factor is not
/// validated by the standard (10300-3, 6.4.4) - this tool falls back to Y_BS = 1.0 for them.</summary>
public enum GearType
{
    StraightBevel,
    ZerolBevel,
    SpiralBevel
}

/// <summary>Source of the dynamic factor K_v (ISO 10300-1:2023, Clause 7): Method B (7.7.3,
/// closed-form mass/resonance model using measured single pitch deviation), Method C (7.7.4,
/// simpler empirical curve from an ISO accuracy grade), or a directly supplied value.</summary>
public enum DynamicFactorMode
{
    MethodB,
    MethodC,
    Manual
}

/// <summary>Source of the transverse load factors K_Halpha/K_Falpha (ISO 10300-1:2023, Clause
/// 9): Method B (9.3, closed-form using pitch deviation and mesh stiffness) or Method C (9.4,
/// table lookup by accuracy grade), or a directly supplied value.</summary>
public enum TransverseLoadFactorMode
{
    MethodB,
    MethodC,
    Manual
}

/// <summary>Mounting condition of pinion and wheel for the face load factor mounting table,
/// K_Hbeta-be (ISO 10300-1:2023, Table 4).</summary>
public enum MountingCondition
{
    NeitherMemberCantileverMounted,
    OneMemberCantileverMounted,
    BothMembersCantileverMounted
}

/// <summary>How the tooth contact pattern used to fix K_Hbeta-be was verified (ISO
/// 10300-1:2023, Table 4).</summary>
public enum ContactPatternVerification
{
    CheckedUnderFullLoad,
    CheckedUnderLightTestLoad,
    SampleGearSetEstimated
}

/// <summary>Profile crowning amount, which sets the load-sharing exponent e_LS (ISO
/// 10300-2:2023, Table 5) used by both Z_LS (pitting) and Y_LS = Z_LS^2 (bending).</summary>
public enum ProfileCrowning
{
    Low,
    High
}

/// <summary>Material family, which selects the Z_NT / Y_NT life-factor curve (ISO 10300-2:2023,
/// Figure 6 / Table 6 and ISO 10300-3:2023, Figure 5 / Table 5), the Y_delta,relT slip-layer
/// thickness rho' (ISO 10300-3:2023, Table 4), and the Y_R,relT surface-condition formula
/// family (ISO 10300-3:2023, 6.5.1). Case-hardened (Eh, IF) and through-hardened (St, V, GGG
/// perlitic/bainitic, GTS) steels share the same curves/formulas in both standards, so they
/// are grouped together; nitrided steel and grey/nodular cast iron likewise share a (lower)
/// family. A representative single rho' is used per family (ISO 10300-3, Table 4 actually
/// tabulates rho' per yield/tensile strength within each family - not modelled per-grade).</summary>
public enum MaterialFamily
{
    CaseOrThroughHardenedSteel,
    NonHardenedStructuralSteel,
    NitridedSteelOrCastIron
}

/// <summary>Hardness-ratio (work hardening) factor mode, Z_W (ISO 10300-2:2023, 8.3): equal
/// hardness (Z_W = 1.0), a through-hardened pair where the pinion is harder (8.3.3.2), or a
/// surface-hardened pinion run against a through-hardened wheel (8.3.3.1).</summary>
public enum HardnessRatioMode
{
    EqualHardness,
    ThroughHardenedHarderPinion,
    SurfaceHardenedPinion
}
